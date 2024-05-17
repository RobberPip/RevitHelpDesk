namespace HelpDesk;

public class DbProvider
{
    private static readonly Dictionary<string, string> ConfigParameters = [];
    private NpgsqlConnection connection;
    private static string dbSchema;
    public DbProvider()
    {
        if (!ReadConfig())
        {
            Console.WriteLine("Error: no config file");
            return;
        }
        (connection, dbSchema) = InitDbConnection();
    }
    public ObservableCollection<string> GetTags()
    {
        try
        {
            ObservableCollection<string> tags = new ObservableCollection<string>();
            connection.Open();
            var query =
                $"SELECT tag_name FROM {dbSchema}.hd_tags";

            using (var command = new NpgsqlCommand(query, connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        tags.Add(reader.GetString(0));
                    }
                }
            }
            connection.Close();
            return tags;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Detailed description of the error: {ex}");
            throw;
        }
    }
    public async Task<Dictionary<int, (string, List<string>)>> GetContractorsAsync()
    {
        try
        {
            Dictionary<int, (string, List<string>)> contractorDictionary = new Dictionary<int, (string, List<string>)>();
            await connection.OpenAsync();
            var query =
                $"SET search_path = {dbSchema}; SELECT hd_contractors.id,name,tag_name FROM hd_contractors " +
                $"JOIN hd_contractors_tags ON contractor_id = hd_contractors.id " +
                $"JOIN hd_tags ON tag_id = hd_tags.id";

            using (var command = new NpgsqlCommand(query, connection))
            {
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        int id = reader.GetInt32(0);
                        string name = reader.GetString(1);
                        string tag = reader.GetString(2);
                        if (contractorDictionary.ContainsKey(id))
                        {
                            contractorDictionary[id].Item2.Add(tag);
                        }
                        else
                        {
                            contractorDictionary[id] = (name, new List<string> { tag });
                        }
                    }
                }
            }
            await connection.CloseAsync();
            return contractorDictionary;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Detailed description of the error: {ex}");
            throw;
        }
    }
    public async Task AddRequestAsync(RequestModel request)
    {
        try
        {
            int id = 0;
            string query;
            await connection.OpenAsync();
            if (request.IssueId != -1)
            {
                query =
                   $"INSERT INTO {dbSchema}.hd_request(issue_id,contractor_id) VALUES (@issue_id,(SELECT id FROM {dbSchema}.hd_contractors WHERE name = '{request.Contractor}'))";
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@issue_id", request.IssueId);
                    await command.ExecuteNonQueryAsync();
                }
            }
            query =
                $"SELECT id from {dbSchema}.hd_request WHERE issue_id = @issue_id";
            using (var command = new NpgsqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@issue_id", request.IssueId);
                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        id = reader.GetInt32(0);
                    }
                }
            }
            query = $"INSERT INTO {dbSchema}.hd_files(request_id,file) VALUES (@request_id,@file)";
            using (var command = new NpgsqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@request_id", id);
                foreach (FileDetailModel item in request.FileDetail)
                {
                    NpgsqlParameter fileParameter = command.Parameters.Add("@file", NpgsqlDbType.Bytea);
                    fileParameter.Value = item.FileBytes;
                    await command.ExecuteNonQueryAsync();
                    command.Parameters.Remove(fileParameter);
                }
            }


            await connection.CloseAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Detailed description of the error: {ex}");
            throw;
        }
    }
    private static (NpgsqlConnection, string) InitDbConnection()
    {
        NpgsqlConnection _connectionString = null;
        string _dbSchema = null;
        try
        {
            if (ConfigParameters.TryGetValue("db_address", out string dbAddress) &&
                ConfigParameters.TryGetValue("db_port", out string dbPort) &&
                ConfigParameters.TryGetValue("db_user", out string dbUsername) &&
                ConfigParameters.TryGetValue("db_password", out string dbPassword) &&
                ConfigParameters.TryGetValue("db_name", out string dbName) &&
                ConfigParameters.TryGetValue("db_schema", out string dbSchema))
            {
                _connectionString = new($"Server={dbAddress}; Port={dbPort}; User Id={dbUsername}; Password={dbPassword}; Database={dbName}; Timeout=30;");
                _dbSchema = $"\"{dbSchema}\"";
            }

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Detailed description of the error: {ex}");
            throw;
        }

        return (_connectionString, _dbSchema);
    }
    private static bool ReadConfig()
    {
        string exeDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        string configFilePath = Path.Combine(exeDirectory, "dbConfig.conf");

        if (!System.IO.File.Exists(configFilePath))
            return false;

        string[] propertyText;
        string proprtyName, propertyValue;
        char[] _separator = ['='];

        ConfigParameters.Clear();

        foreach (string configString in System.IO.File.ReadAllLines(configFilePath, Encoding.UTF8))
        {
            if (string.IsNullOrEmpty(configString) || configString.Trim()[0] == '#')
                continue;

            propertyText = configString.Split(_separator, StringSplitOptions.RemoveEmptyEntries);

            if (propertyText.Length < 2)
                continue;

            proprtyName = propertyText[0].Trim();
            propertyValue = propertyText[1].Trim();

            if (string.IsNullOrEmpty(proprtyName) || string.IsNullOrWhiteSpace(proprtyName) || string.IsNullOrEmpty(propertyValue) || string.IsNullOrWhiteSpace(propertyValue))
                continue;

            ConfigParameters.Add(proprtyName, propertyValue);
        }

        return ConfigParameters.Count > 0;
    }
    public void Dispose()
    {
        DisposeAsync().GetAwaiter().GetResult();
    }
    public async ValueTask DisposeAsync()
    {
        if (connection != null)
        {
            await connection.DisposeAsync();
            connection = null;
        }
    }
}
