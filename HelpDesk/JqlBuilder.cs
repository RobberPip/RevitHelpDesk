<<<<<<< HEAD
﻿using System.Net.Http;
=======
﻿using Newtonsoft.Json;
>>>>>>> 4c2db5e (first version)

namespace HelpDesk;

public class JqlBuilder
{
<<<<<<< HEAD
    public static async Task<int> SendPostRequest(RequestModel _request)
=======
    static string GenerateAuthHeader()
    {
        return "Bearer Nzc0MjY3OTg4NjAwOlnxFu73cGYyz8gHplHWCopUodIq";
    }
    public static async Task<long> SendPostRequest(RequestModel _request)
>>>>>>> 4c2db5e (first version)
    {

        //TODO Вынести в конфиг
        string jiraApiHost = "https://jira.arogroup.ru/rest/api/2";
        string url = $"{jiraApiHost}/issue";
<<<<<<< HEAD
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
=======
        string labelsString = string.Join(",", _request.Tags.Select(tag => "\"" + tag + "\""));
        string jsonContent = @"
        {
            ""update"": {},
            ""fields"": {
                ""issuetype"": { ""id"": ""10209"" }, 
                ""project"": { ""id"": ""11501"" }, 
                ""summary"": """ + _request.Theme + @""",
                ""description"": """ + _request.Description + @""",
                ""labels"": [" + labelsString + @"],
                ""customfield_10704"": """ + _request.UID + @"""
            }
                
        }";
>>>>>>> 4c2db5e (first version)

        using (HttpClient client = new HttpClient())
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);
<<<<<<< HEAD
            request.Headers.Add("Authorization", authorization);
=======
            request.Headers.Add("Authorization", GenerateAuthHeader());
>>>>>>> 4c2db5e (first version)
            request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                JObject jsonResponse = JObject.Parse(await response.Content.ReadAsStringAsync());
                string issueId = jsonResponse["id"].ToString();
<<<<<<< HEAD
                return int.Parse(issueId);
=======
                return long.Parse(issueId);
>>>>>>> 4c2db5e (first version)
            }
            return -1;
        }
    }
<<<<<<< HEAD
}

=======
    public static async Task<string> GetRequests(string UID)
    {
        string jiraApiHost = $"https://jira.arogroup.ru/rest/api/2/search?jql=project=AISHD AND \"UID\" ~ {UID}";
        return await GetResponse(jiraApiHost);
    }
    public static async Task<List<RequestModel>> GetNameRequests(string UID)
    {
        JObject jObject = JObject.Parse(await GetRequests(UID));
        JArray issues = (JArray)jObject["issues"];
        List<RequestModel> result = new List<RequestModel>();
        foreach (JObject issue in issues)
        {
            RequestModel model = new RequestModel()
            {
                Theme = (string)issue["fields"]["summary"],
                IssueId = (long)issue["id"],
                Status = (string)issue["fields"]["status"]["name"]
            };
            result.Add(model);
        }
        return result;
    }
    public static async Task<RequestModel> GetRequestComment(RequestModel model)
    {
        string jiraApiHost = $"https://jira.arogroup.ru/rest/api/2/issue/{model.IssueId}/comment";
        JObject jObject = JObject.Parse(await GetResponse(jiraApiHost));
        JArray comments = (JArray)jObject["comments"];
        foreach (JObject comment in comments)
        {
            long commentId = (long)comment["id"];
            string authorName = (string)comment["author"]["displayName"];
            string body = (string)comment["body"];
            AuthorModel author = new AuthorModel()
            {
                Name = authorName,
                Comment = body
            };
            model.Comments.Add(commentId, author);
        }
        return model;
    }
    private static async Task<string> GetResponse(string jiraApiHost)
    {
        using (HttpClient client = new HttpClient())
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, jiraApiHost);
            request.Headers.Add("Authorization", GenerateAuthHeader());
            HttpResponseMessage response = await client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                return responseBody;
            }
            return null;
        }
    }
    public static async Task<int> SendPostComment(RequestModel _request)
    {
        string jiraApiHost = $"https://jira.arogroup.ru/rest/api/2/issue/{_request.IssueId}/comment";
        string jsonContent = JsonConvert.SerializeObject(new { body = _request.CommentMessage });


        using (HttpClient client = new HttpClient())
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, jiraApiHost);
            request.Headers.Add("Authorization", GenerateAuthHeader());
            request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                return 1;
            }
            return -1;
        }
    }
}
>>>>>>> 4c2db5e (first version)
