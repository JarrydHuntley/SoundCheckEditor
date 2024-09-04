using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

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

    [MenuItem("Tools/SoundCheckEditor")]
    public static void ShowWindow()
    {
        GetWindow<SoundCheckEditor>("Sound Check Editor");
    }

    private void OnEnable()// ������â�� Ȱ��ȭ�Ǹ� EditorPrefs���� ����� apikey�� �ҷ��´�.
    {
        apiKey = EditorPrefs.GetString("FreesoundAPIKey", "");//�⺻���� �� ���ڿ�
        EditorApplication.update += Update;//������Ʈ �޼��带 ���
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
    }

    private void OnGUI()
    {
        // GUI �׸���
        GUILayout.Label("Search for Sound Effect You Want(���ϴ� ���� ����Ʈ�� �˻��ϼ���)\n", EditorStyles.boldLabel);
        GUILayout.Label("This system uses the API from https://freesound.org/.(�� �ý����� https://freesound.org/�� API�� ����մϴ�.)\n", EditorStyles.whiteLabel);

        DrawApiKeyField();//apiŰ �Է� �� ���� ui
        
        searchQuery = EditorGUILayout.TextField("Search Sound Effect", searchQuery);// �˻��� �Է�

        if (GUILayout.Button("Search") && !string.IsNullOrEmpty(apiKey))//Search��ư Ŭ�� �� + apikey �Է� �� ��ġ���� ȣ��
        {
            //SearchSound(searchQuery);
            needSearch=true;//�˻��� �ʿ����� ǥ��
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
    }

    private async void SearchSound(string query)// Freesound�� apiŰ�� �˻� ������ ����Ͽ� api��û�� ���� ���带 �˻�. �Է��� �����Ϳ� ���� �������� URL ����,ȣ��
    {
        if (string.IsNullOrEmpty(apiKey))//����ó��
        {
            EditorUtility.DisplayDialog("API Key Missing", "Please enter and save your API Key first.", "OK");
            return;
        }
        string url = $"https://freesound.org/apiv2/search/text/?query={query}";
        var request = new HttpRequestMessage(HttpMethod.Get, url);// HttpRequestMessage ��ü ���� �� ���� ��� ����
        request.Headers.Add("Authorization", $"Token {apiKey}");
        await FetchSoundData(request); // �־��� url�� freesound �����͸� �˻��ϴ� �޼���. ������ �̵� �� ���
    }

    private async Task FetchSoundData(HttpRequestMessage request) //�־��� URL�� freesound �����͸� �񵿱������� �������� �޼���.
    {
        try
        {
            HttpResponseMessage response = await client.SendAsync(request);//�񵿱������� HTTP GET ��û�� ���� �� ������ �޴´�. ������ URL�� ���.
            if (response.IsSuccessStatusCode) // ��û�� �����ߴ��� Ȯ��
            {
                string jsonResponse = await response.Content.ReadAsStringAsync();//������ JSON ���ڿ��� �д´�.
                soundResults = ParseSoundResults(jsonResponse);//JSON ���ڿ��� �Ľ��Ͽ� �˻� ��� ����Ʈ�� ������Ʈ
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
    }

    private async void SearchSoundWithUrl(string url)// �־��� URL�� ����Ͽ� Freesound API�� ȣ��. ������ �ѱ�� ����� ���� ���.
    {   //SearchSound�޼������ ���� : SearchSoundWithUrl�� �ܺο��� �����Ǵ� URL ���. SearchSound�� ���� URL����.
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("Authorization", $"Token {apiKey}");
        await FetchSoundData(request);
    }

    private List<SoundResult> ParseSoundResults(string json)//freesound API ������ �Ľ��Ͽ� �˻� ����� ��ȯ�ϴ� �޼���
    {
        //Debug.Log($"Received JSON: {json}");
        var searchResults = JsonConvert.DeserializeObject<FreesoundSearchResult>(json);//JSON ���ڿ��� FreesoundSearchResult ��ü�� ��ȯ.

        if (searchResults == null || searchResults.results == null)// �˻� ����� ���ų� json ������ ��ȿ���� ���� ��� ��µǴ� ����
        {
            Debug.LogError("No search results found or invalid JSON structure.");
            return new List<SoundResult>();//�� ����Ʈ�� ��ȯ��� �Ѵ�.
        }
        //������������ ���� �������� URL ����
        nextPageUrl = CleanUrl(searchResults.next);
        prevPageUrl = CleanUrl(searchResults.previous);

       // Debug.Log($"Next Page URL: {nextPageUrl}");
       // Debug.Log($"Previous Page URL: {prevPageUrl}");

        List<SoundResult> results = new List<SoundResult>();

        foreach (var sound in searchResults.results)// �˻� ��� ����Ʈ�� ��ȸ�Ͽ�, SoundResult ��ü�� ��ȯ �� ����Ʈ�� �߰�
        {
            results.Add(new SoundResult
            {
                id = sound.id,
                name = sound.name,
                license = sound.license,
                username = sound.username
            });
        }
        return results;//��ȯ�� �˻� ��� ����Ʈ ��ȯ
    }

    private void DrawSoundResults()// �˻������ �����ִ� �޼���
    {
        int maxResultToShow = 10;//UI �� �������� �������� �ִ� ���� ����
        int resultToDisplay = Mathf.Min(soundResults.Count, maxResultToShow);

        //��ũ�� ������ ������ ����. ���̸� �����ϰ� �� ������ �߰��Ͽ� next,prev��ư�� �������� �ʰ� ��ġ
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Width(position.width), GUILayout.Height(300));
        for (int i = 0; i < resultToDisplay; i++)
        {
            GUILayout.Label(soundResults[i].name);// ���� �̸��� 8�������� ǥ��
            if (GUILayout.Button("Play"))//play��ư Ŭ�� �� ���� �������� �̵�
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
 }

