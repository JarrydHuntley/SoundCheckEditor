using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Web;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using System;
using UnityEngine.UIElements;
using Codice.Utils;

public class SoundCheckEditor : EditorWindow
{
    // SoundCheckEditor : FreeSound�� API Key�� ����Ͽ�, ����Ƽ ������ ������ Freesound�� �����ϴ� License Free�� ���带 �˻��ϰ� play�غ� �� �ִ� ���

    private string searchQuery = ""; // �˻��� �Է� �ʵ�
    private List<SoundResult> soundResults = new List<SoundResult>(); // �˻� ��� ����Ʈ
    private static readonly HttpClient client = new HttpClient(); // HttpClient �̱��� ���. HttpClient�� �ν��Ͻ�ȭ�ϴ� ����� ���� ũ�� ������, ������ �� �����ϴ� ���� ����
    private Vector2 scrollPosition;//��ũ�� ��ġ 

    private string nextPageUrl = null;//���� ������ url ����
    private string prevPageUrl = null;//���� ������ url ����

    private string apiKey;//apikey�� ������ ���� �Է��� �� �ִ�.
    private bool needSearch = false;//�˻��� �ʿ����� ���θ� ����

    //---250318_������� ǥ�ñ� ����
    private bool isSearching = false;//�˻� ������ ����
    private float progress = 0.0f;//���� ���� (0 ~ 1)
    private string progressMessage = "";//���� ���� �޽���
    //---250318_����� �̸���� ��� ����
    private AudioSource audioSource;//����� ����� ���� �ҽ�
    private AudioClip currentAudioClip;//���� ��� ���� AudioClip

    [MenuItem("Tools/SoundCheckEditor")]
    public static void ShowWindow()
    {
        GetWindow<SoundCheckEditor>("Sound Check Editor");
    }

    private void OnEnable()// ������â�� Ȱ��ȭ�Ǹ� EditorPrefs���� ����� apikey�� �ҷ��´�.
    {
        apiKey = EditorPrefs.GetString("FreesoundAPIKey", "");//�⺻���� �� ���ڿ�
        EditorApplication.update += Update;//������Ʈ �޼��带 ���

        if(audioSource == null)//����� �ҽ� �ʱ�ȭ
        {
            GameObject audioSourceObject = new GameObject("AudioSourceObject");
            audioSource = audioSourceObject.AddComponent<AudioSource>();
            audioSourceObject.hideFlags = HideFlags.HideAndDontSave;
        }
    }

    private void OnDisable()
    {
        EditorApplication.update-=Update;//������Ʈ �޼��� ����
    }

    private void Update()
    {
        if(needSearch)// �˻� ��û ó��
        {
            needSearch = false;//�˻� �� ���� ����
            SearchSound(searchQuery);
        }
        if(isSearching)//���� ���� �� ���� UI ������Ʈ
        {
            Repaint();
        }
    }

    private void OnGUI()
    {
        GUIStyle firstCenteredLabel = new GUIStyle(EditorStyles.boldLabel);//��� ���Ŀ� GUI��Ÿ�� 
        firstCenteredLabel.alignment = TextAnchor.MiddleCenter;
        firstCenteredLabel.fontSize = 15;

        GUIStyle centeredLabel = new GUIStyle(EditorStyles.boldLabel);//��� ���Ŀ� GUI��Ÿ�� 
        centeredLabel.alignment = TextAnchor.MiddleCenter;

        // GUI �׸���
        GUILayout.Label("Search for Sound Effect You Want\n", firstCenteredLabel);
        GUILayout.Label("�� This system uses the API from https://freesound.org ��\n", centeredLabel);
        GUILayout.Label("�� To use this tool, You need to get the API Key of freesounds issued. ��\n", centeredLabel);

        DrawApiKeyField();//apiŰ �Է� �� ���� ui
        
        searchQuery = EditorGUILayout.TextField("Search Sound Effect", searchQuery);// �˻��� �Է�

        if (GUILayout.Button("Search") && !string.IsNullOrEmpty(apiKey))//Search��ư Ŭ�� �� + apikey �Է� �� ��ġ���� ȣ��
        {
            
            needSearch=true;//�˻��� �ʿ����� ǥ��
        }
        if(isSearching)//���� ǥ�ñ� ǥ��
        {
            EditorGUI.ProgressBar(GUILayoutUtility.GetRect(position.width, 20.0f), progress, progressMessage);
            Repaint();
        }

        if (soundResults != null && soundResults.Count > 0)// �˻� ����� ���� ���� ��ũ�� ��� ��� ����� ǥ���Ѵ�.
        {
            DrawSoundResults();
            DrawPaginationButtons();
        }
        else if(!needSearch)//�˻��� ���� �ʾ��� ��
        {
            GUILayout.Label("No Results Found.");
        }

        DrawStopPreviewButton();
    }

    private void DrawStopPreviewButton()// �̸���� ���� ��ư�� �׸��� �޼���.
    {
        if(audioSource.isPlaying)
        {
            GUIStyle stopButtonStyle = new GUIStyle(GUI.skin.button);
            stopButtonStyle.normal.textColor = Color.green;
            stopButtonStyle.fontStyle = FontStyle.Bold;
            
            if(GUILayout.Button("Stop Preview", stopButtonStyle))
            {
                StopPreview();
            }
        }
    }

    private void StopPreview()//�̸���� �������
    {
        if(audioSource.isPlaying)
        {
            audioSource.Stop();//����� ����
            audioSource.clip = null;//���� Ŭ�� �ʱ�ȭ
            currentAudioClip = null;//���� ����� Ŭ�� �ʱ�ȭ
            EditorUtility.DisplayDialog("Preview Stopped", "The preview has been stopped.", "OK");
        }
    }

    private async void SearchSound(string query)// Freesound�� apiŰ�� �˻� ������ ����Ͽ� api��û�� ���� ���带 �˻�. �Է��� �����Ϳ� ���� �������� URL ����,ȣ��
    {
        CheckApiKeyVaildation();
        if(string.IsNullOrEmpty(query))
        {
            EditorUtility.DisplayDialog("Empty Query", "Please enter Query in blank.", "OK");
            return;
        }
        string encodedQuery = HttpUtility.UrlEncode(query);// ������ URL���·� ���۵ǹǷ�, Ư�����ڳ� ������ ���Ե� ��츦 ����� URL���ڵ��� �ʿ�.
                                                           // C#�� System.Web�� �ִ� HttpUtility.UrlEncode�� ���. �̸� ���� �������� ��� ����, �˻� ��� ����, ���� ���� �� ����.
                                                           //���۱��� �����ο� cc0���̼����� ���͸�, api���信 ������ url�� �����Ͽ� ��û�ϱ� ���� ������ �Ķ���� �߰�
        string url =  $"https://freesound.org/apiv2/search/text/?query={encodedQuery}&filter=license:(\"Creative Commons 0\")&fields=id,name,previews,username";

        var request = new HttpRequestMessage(HttpMethod.Get, url);// HttpRequestMessage ��ü ���� �� ���� ��� ����
        request.Headers.Add("Authorization", $"Token {apiKey}");
        await FetchSoundData(request); // �־��� url�� freesound �����͸� �˻��ϴ� �޼���. ������ �̵� �� ���
    }

    private async Task FetchSoundData(HttpRequestMessage request) //�־��� URL�� freesound �����͸� �񵿱������� �������� �޼���.
    {
        try
        {   isSearching = true;//�˻� ����. HTTP ��û ���ķ� ������¸� ������Ʈ.
            progress = 0.0f;
            progressMessage = "Sendig Request...";

            HttpResponseMessage response = await client.SendAsync(request);//�񵿱������� HTTP GET ��û�� ���� �� ������ �޴´�. ������ URL�� ���.
            progress = 0.5f;
            progressMessage = "Receiving data...";

            if (response.IsSuccessStatusCode) // ��û�� �����ߴ��� Ȯ��
            {
                string jsonResponse = await response.Content.ReadAsStringAsync();//������ JSON ���ڿ��� �д´�.
                progress = 0.8f;
                progressMessage = "Parsing data...";

                soundResults = ParseSoundResults(jsonResponse);//JSON ���ڿ��� �Ľ��Ͽ� �˻� ��� ����Ʈ�� ������Ʈ
                progress = 1f;
                progressMessage = "Complete!";
            }
            else
            {
                HandleApiError(response);
            }
        }
        catch (HttpRequestException e)//��û ���� ��
        {
            Debug.LogError($"Error fetching sound data: {e.Message}");
            EditorUtility.DisplayDialog("API Request Error", $"Error fetching sound data: {e.Message}", "OK");
        }
        finally
        {
            isSearching = false; // �˻� ����
            progress = 0f;
            progressMessage = "";
        }
    }

    private async void SearchSoundWithUrl(string url)// �־��� URL�� ����Ͽ� Freesound API�� ȣ��. ������ �ѱ�� ����� ���� ���.
    {   //SearchSound�޼������ ���� : SearchSoundWithUrl�� �ܺο��� �����Ǵ� URL ���. SearchSound�� ���� URL����.
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("Authorization", $"Token {apiKey}");
        await FetchSoundData(request);
    }

    private List<SoundResult> ParseSoundResults(string json)//freesound API ������ �Ľ��Ͽ� �˻� ����� ��ȯ�ϴ� �޼���
    {
        if (string.IsNullOrEmpty(json))//api ���� ������ ��ȿ�� ���� --> json�Ľ� ���� api ������ empty�� ��츦 ����
        {
            Debug.LogError("API response is empty.");
            EditorUtility.DisplayDialog("API Error", "The API response is empty.", "OK");
            return new List<SoundResult>();
        }
        try
        {
            //JSON ���ڿ��� FreesoundSearchResult ��ü�� ��ȯ.
            var searchResults = JsonConvert.DeserializeObject<FreesoundSearchResult>(json);
            if (searchResults == null || searchResults.results == null || searchResults?.results == null)// �˻� ����� ���ų� json ������ ��ȿ���� ���� ��� ��µǴ� ����
            {
                Debug.LogError("No search results found or invalid JSON structure.");
                return new List<SoundResult>();//�� ����Ʈ�� ��ȯ��� �Ѵ�.
            }
            //������������ ���� �������� URL ����
            nextPageUrl = CleanUrl(searchResults.next);
            prevPageUrl = CleanUrl(searchResults.previous);

            List<SoundResult> results = new List<SoundResult>();

            foreach (var sound in searchResults.results)// �˻� ��� ����Ʈ�� ��ȸ�Ͽ�, SoundResult ��ü�� ��ȯ �� ����Ʈ�� �߰�
            {
                results.Add(new SoundResult
                {
                    id = sound.id,
                    name = sound.name,
                    license = sound.license,
                    username = sound.username,
                    previews = sound.previews
                });
            }
            return results;//��ȯ�� �˻� ��� ����Ʈ ��ȯ
        }
        catch(JsonException ex)//JSON �Ľ� �����Ͱ� ��ȿ���� �ʰų� ������ �ٸ� ����� ����ó��
        {
            Debug.LogError($"JSON Parsing Error : {ex.Message}");
            EditorUtility.DisplayDialog("JSON Error", "Failed to parse the API response. Please Check the data format", "OK");
            return new List<SoundResult>();// �� ����Ʈ�� ��ȯ.
        }
        catch(Exception ex)//�Ϲ� ���ܿ� ���� ó��
        {
            Debug.LogError($"Unexepted Error : {ex.Message}");
            EditorUtility.DisplayDialog("Error", "An unexpected error ocurred while processing the data", "OK");
            return new List<SoundResult>();//�� ����Ʈ�� ��ȯ.
        }

    }

    private void DrawSoundResults()// �˻������ �����ִ� �޼���
    {
        int maxResultToShow = 10;//UI �� �������� �������� �ִ� ���� ����
        int resultToDisplay = Mathf.Min(soundResults.Count, maxResultToShow);

        //��ũ�� ������ ������ ����. ���̸� �����ϰ� �� ������ �߰��Ͽ� next,prev��ư�� �������� �ʰ� ��ġ
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Width(position.width), GUILayout.Height(300));
        for (int i = 0; i < resultToDisplay; i++)
        {
            GUILayout.Space(20);
            GUILayout.Label(soundResults[i].name);// ���� �̸��� 8�������� ǥ��

            if(GUILayout.Button("Play Preview"))//�˻� ��� UI�� �̸���� ��ư �߰�
            {
                if (soundResults[i].previews != null)
                {
                    string previewUrl = soundResults[i].previews.preview_hq_mp3; // ��ǰ�� MP3 ���
                    PlayPreview(previewUrl);
                }
                else
                {
                    Debug.LogError("No preview URL found.");
                    EditorUtility.DisplayDialog("Preview Error", "No preview URL found for this sound.", "OK");
                }
            }
            if (GUILayout.Button("Open in Browser"))//��ư Ŭ�� �� ���� �������� �̵�
            {
                // Freesound�� ����� ������ URL ����: https://freesound.org/people/{username}/sounds/{id}/
                 string url = $"https://freesound.org/people/{soundResults[i].username}/sounds/{soundResults[i].id}/";
                Application.OpenURL(url);//url�� �� ���������� �� �� ����.
            }
        }
        EditorGUILayout.EndScrollView();
    }

    private void DrawPaginationButtons()//������ �ѱ�� ��ư ��� �޼���
    {
            //�� ���� �߰� : �˻� ����� ������ �ѱ�� ��ư���̿� ���� ���� Ȯ��
            GUILayout.Space(20);//��ư�� �˻� ��� ���̿� 20�ȼ��� ���� �߰�

            //������ �ѱ�� ��ư
            EditorGUILayout.BeginHorizontal();
            {
                // ���� ������ ��ư: prevPageUrl�� null�̸� ��Ȱ��ȭ
                EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(prevPageUrl));
                if (GUILayout.Button("< Previous Page", GUILayout.Height(30))) // ��ư ũ�� ����
                {
                    SearchSoundWithUrl(prevPageUrl);
                }
                EditorGUI.EndDisabledGroup();

                // ���� ������ ��ư: nextPageUrl�� null�̸� ��Ȱ��ȭ
                EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(nextPageUrl));
                if (GUILayout.Button("Next Page >", GUILayout.Height(30))) // ��ư ũ�� ����
                {
                    SearchSoundWithUrl(nextPageUrl);
                }
                EditorGUI.EndDisabledGroup();
            }
            EditorGUILayout.EndHorizontal();
    }

    private string CleanUrl(string url)// next, prev ������ url ���� �� �ʿ���� �κ��� ����
    {
        if (string.IsNullOrEmpty(url)) return url;
        return url.Replace("&weights=", "");
    }

    private void DrawApiKeyField() //����ڰ� ������ â���� ���� ������ APIŰ�� �Է��ϰ� ������ �� �ֵ��� ����
    {   
        GUILayout.Label("Freesound API Key", EditorStyles.boldLabel);
        string newApiKey = EditorGUILayout.TextField("API Key", apiKey);
        if (newApiKey != apiKey)
        {
            apiKey = newApiKey;
        }

        if (GUILayout.Button("Save API Key"))//���̺� ��ư Ŭ�� �� apikey�� EditorPrefs�� ����
        {
            EditorPrefs.SetString("FreesoundAPIKey", apiKey);
            EditorUtility.DisplayDialog("API Key Saved", "Your API Key has been saved successfully.", "OK");
        }
    }

    private void HandleApiError(HttpResponseMessage response)//apiŰ ���� �ڵ鸵�� ���� �޼���
    {
        string errorMessage = $"API Error : {response.ReasonPhrase}";
        if(response.StatusCode==System.Net.HttpStatusCode.Unauthorized)//�������� ��
        {
            errorMessage = "Unauthorized: Your API Key might be invalid or expired.";
        }
        else if(response.StatusCode == System.Net.HttpStatusCode.Forbidden)//�߸��� ���� ��
        {
            errorMessage = "Forbidden: You do not have permission to access this resource.";
        }
        Debug.LogError(errorMessage);
        EditorUtility.DisplayDialog("API Error", errorMessage, "OK");
    }

    private async void PlayPreview(string previewUrl)//��Ͽ� ��µ� ���� ���ҽ��� �̸���� ����� �����ϴ� �޼���.
    {
        CheckApiKeyVaildation();
        CheckUrlValidation(previewUrl);
        try
        {
            if(audioSource.isPlaying)//��� ���� ���� ����� ����.
            {
                StopPreview();
            }
            using(UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(previewUrl, audioType:AudioType.MPEG))
            {
                www.SetRequestHeader("Authorization", $"Token {apiKey}");//apiŰ�� ����� �߰�. freesound�� �̸���⵵ apikey�� �䱸�ϹǷ�.

                var operation = www.SendWebRequest();
                while(!operation.isDone)
                {
                    await Task.Yield();//�񵿱� ���.
                }

                if(www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError($"Error loading audio : {www.error}");
                    EditorUtility.DisplayDialog("Audio Error", $"Failed to load audio : {www.error}", "OK");
                }
                else
                {
                    currentAudioClip = DownloadHandlerAudioClip.GetContent(www);

                    if(currentAudioClip != null)
                    {
                        if(audioSource.isPlaying)//���� ������̸� ����.
                        {
                            audioSource.Stop();
                        }
                        audioSource.clip = currentAudioClip;
                        audioSource.Play();
                    }
                }
            }
        }
        catch(Exception ex)
        {
            Debug.LogError($"Error playing preview : {ex.Message}");
            EditorUtility.DisplayDialog("Error", $"Failed to play preview : {ex.Message}", "OK");
        }
    }

    private void CheckApiKeyVaildation()//API Ű ��ȿ�� �˻� �޼���.
    {
        if(string.IsNullOrEmpty(apiKey))
        {
            EditorUtility.DisplayDialog("API Key Missing", "Please enter and save your API Key first.", "OK");
            return;
        }
    }
    private void CheckUrlValidation(string url)//url ��ȿ�� �˻� �޼���.
    {
        if(string.IsNullOrEmpty(url))//url�� null�̰ų� ������� ���
        {
            Debug.LogError($"{url} is empty.");
            return;
        }
    }
 }

