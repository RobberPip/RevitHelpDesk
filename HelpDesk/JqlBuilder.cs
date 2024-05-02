using System.Net.Http;

namespace HelpDesk;

public class JqlBuilder
{
    public static async Task<int> SendPostRequest(RequestModel _request)
    {

        //TODO Вынести в конфиг
        string jiraApiHost = "https://jira.arogroup.ru/rest/api/2";
        string url = $"{jiraApiHost}/issue";
        string authorization = "Bearer Nzc0MjY3OTg4NjAwOlnxFu73cGYyz8gHplHWCopUodIq";
        string labelsString = string.Join(",", _request.Tags.Select(tag => "\"" + tag + "\""));

        string jsonContent = @"
                {
                    ""update"": {},
                    ""fields"": {
                        ""issuetype"": { ""id"": ""10209"" }, 
                        ""project"": { ""id"": ""11501"" }, 
                        ""summary"": """ + _request.Theme + @""",
                        ""description"": """ + _request.Description + @""",
                        ""labels"": [" + labelsString + @"]
                    }
                        
                }";

        using (HttpClient client = new HttpClient())
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Add("Authorization", authorization);
            request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                JObject jsonResponse = JObject.Parse(await response.Content.ReadAsStringAsync());
                string issueId = jsonResponse["id"].ToString();
                return int.Parse(issueId);
            }
            return -1;
        }
    }
}

