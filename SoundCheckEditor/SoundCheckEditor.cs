using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public class SoundCheckEditor : EditorWindow
{
    // SoundCheckEditor : FreeSound�� API Key�� ����Ͽ�, ����Ƽ ������ ������ Freesound�� �����ϴ� License Free�� ���带 �˻��ϰ� play�غ� �� �ִ� ���
    // ���ǻ��� : Json ������ ó���� ���� Newtonsoft json 3.2.1������ ����Ʈ�ؾ� �ϴµ�, ������ �߻��ϴ� ��찡 ����. �̷� ���� asset���� �� plugin ������ ���� Newtonsoft.json dll������ �־��ָ� �ذ�

    private string searchQuery = ""; // �˻��� �Է� �ʵ�
    private List<SoundResult> soundResults = new List<SoundResult>(); // �˻� ��� ����Ʈ
    private static readonly HttpClient client = new HttpClient(); // HttpClient �̱��� ���. HttpClient�� �ν��Ͻ�ȭ�ϴ� ����� ���� ũ�� ������, ������ �� �����ϴ� ���� ����
    private Vector2 scrollPosition;//��ũ�� ��ġ 


    [MenuItem("Tools/SoundCheckEditor")]
    public static void ShowWindow()
    {
        GetWindow<SoundCheckEditor>("Sound Check Editor");
    }

    private void OnGUI()
    {
        // GUI �׸���
        GUILayout.Label("Search for Sound Effect You Want(���ϴ� ���� ����Ʈ�� �˻��ϼ���)\n", EditorStyles.boldLabel);
        GUILayout.Label("This system uses the API from https://freesound.org/.(�� �ý����� https://freesound.org/�� API�� ����մϴ�.)\n", EditorStyles.whiteLabel);
        searchQuery = EditorGUILayout.TextField("Search Sound Effect", searchQuery);// �˻��� �Է�

        if (GUILayout.Button("Search"))//Search��ư Ŭ�� �� ��ġ���� ȣ��
        {
            SearchSound(searchQuery);
        }

        if (soundResults != null && soundResults.Count >0)// �˻� ����� ���� ���� ��ũ�� ��� ��� ����� ǥ���Ѵ�.
        {
            //��ũ�� ������ ������ ����. ���� ������ â�� �ʺ�� ���̿� �°�.
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Width(position.width), GUILayout.Height(position.height - 50));//50:�ϴ� ��ư ����);

            foreach (var result in soundResults)//�˻���� ����Ʈ�� foreach�� ��ȸ. �� �׸��� ǥ��
            {
                GUILayout.Label(result.name);// ���� �̸� ǥ��

                if (GUILayout.Button("Play"))//play��ư Ŭ�� �� ���� �������� �̵�
                {
                    // Freesound�� ����� ������ URL ����: https://freesound.org/people/{username}/sounds/{id}/
                    string url = $"https://freesound.org/people/{result.username}/sounds/{result.id}/";
                    Application.OpenURL(url);//url�� �� ���������� �� �� ����.
                }
            }
            EditorGUILayout.EndScrollView();
        }
        else
        {
            GUILayout.Label("No Results Found.");
        }
    }

    private async void SearchSound(string query)
        // Freesound�� apiŰ�� �˻� ������ ����Ͽ� api��û�� ���� URL�� �����Ѵ�.
    {
        string apiKey = ""; // Freesound�� API Ű
        string url = $"https://freesound.org/apiv2/search/text/?query={query}&token={apiKey}";

        try
        {
            //�񵿱������� HTTP GET ��û�� ���� �� ������ �޴´�. ������ URL�� ���.
            HttpResponseMessage response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();// ��û�� �����ߴ��� Ȯ��
            string jsonResponse = await response.Content.ReadAsStringAsync();//������ JSON ���ڿ��� �д´�.

            soundResults = ParseSoundResults(jsonResponse);//JSON ���ڿ��� �Ľ��Ͽ� �˻� ��� ����Ʈ�� ������Ʈ
            Debug.Log("Sound Data fetched successfully");
        }
        catch (HttpRequestException e)//��û ���� ��
        {
            Debug.LogError($"Error fetching sound data: {e.Message}");
        }
    }

    private List<SoundResult> ParseSoundResults(string json)
    {
       // Debug.Log($"Received JSON: {json}");

        var searchResults = JsonConvert.DeserializeObject<FreesoundSearchResult>(json);//JSON ���ڿ��� FreesoundSearchResult ��ü�� ��ȯ.

        if (searchResults == null || searchResults.results == null)
        {
            // �˻� ����� ���ų� json ������ ��ȿ���� ���� ��� ��µǴ� ����
            Debug.LogError("No search results found or invalid JSON structure.");
            return new List<SoundResult>();//�� ����Ʈ�� ��ȯ��� �Ѵ�.
        }

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

  




}
