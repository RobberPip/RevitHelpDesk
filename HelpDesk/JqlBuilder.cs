using Newtonsoft.Json;

namespace HelpDesk;

public class JqlBuilder
{
    static string GenerateAuthHeader()
    {
        return "Bearer Nzc0MjY3OTg4NjAwOlnxFu73cGYyz8gHplHWCopUodIq";
    }
    public static async Task<long> SendPostRequest(RequestModel _request)
    {
        string jiraApiHost = "https://jira.arogroup.ru/rest/api/2";
        string url = $"{jiraApiHost}/issue";
        string[] labelsArray = _request.Tags.Select(tag => tag).ToArray();
        var jsonContentObject = new
        {
            fields = new
            {
                issuetype = new { id = "10209" },
                project = new { id = "11501" },
                summary = _request.Theme,
                description = _request.Description ?? "",
                labels = labelsArray.Length > 0 ? labelsArray : null,
                customfield_10704 = _request.UID,
                assignee = _request.Contractor != null ? new { name = _request.Contractor } : null
            }
        };


        // Преобразуем объект в JSON строку
        string jsonContent = JsonConvert.SerializeObject(jsonContentObject);


        using (HttpClient client = new HttpClient())
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Add("Authorization", GenerateAuthHeader());
            request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                JObject jsonResponse = JObject.Parse(await response.Content.ReadAsStringAsync());
                string issueId = jsonResponse["id"].ToString();
                return long.Parse(issueId);
            }
            return -1;
        }
    }
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
        foreach (var jToken in issues)
        {
            var issue = (JObject)jToken;
            RequestModel model = new RequestModel()
            {
                Key = (string)issue["key"],
                Theme = (string)issue["fields"]?["summary"],
                IssueId = (long)issue["id"],
                Status = (string)issue["fields"]?["status"]?["name"],
                Contractor = issue["fields"] is JObject fields 
                             && fields.TryGetValue("assignee", out var assignee) 
                             && assignee is JObject assigneeObject 
                             && assigneeObject.TryGetValue("displayName", out var displayName)
                    ? (string)displayName
                    : "Общий"

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
    public static async Task<(int TasksCount, bool Success)> GetActiveTasks(string contractor)
    {
        string jiraApiHost = $"https://jira.arogroup.ru/rest/api/2/search?jql=";
        string filter = $"project = AISHD AND (resolution is EMPTY OR resolution not in (Done, Готово)) AND assignee = {contractor}";
        try
        {
            JObject jObject = JObject.Parse(await GetResponse(jiraApiHost + filter));
            int countRequests = (int)jObject["total"];
            bool success = jObject["warningMessages"] == null || !jObject["warningMessages"].Any();
            return (countRequests, success);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error fetching tasks for contractor {contractor}: {ex.Message}");
            return (0, false);
        }
    }
}