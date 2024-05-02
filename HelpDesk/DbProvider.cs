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
    public async Task<ObservableCollection<string>> GetTagsAsync()
    {
        try
        {
            ObservableCollection<string> tags = new ObservableCollection<string>();
            await connection.OpenAsync();
            var query =
                $"SET search_path = {dbSchema}; SELECT tag_name FROM hd_tags";

            using (var command = new NpgsqlCommand(query, connection))
            {
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        tags.Add(reader.GetString(0));
                    }
                }
            }
            await connection.CloseAsync();
            return tags;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Detailed description of the error: {ex}");
            throw;
        }
    }
    public async Task AddRequestAsync(int issue_id)
    {
        try
        {   
            await connection.OpenAsync();
            var query =
                $"SET search_path = {dbSchema}; INSERT INTO hd_request(issue_id) VALUES (@issue_id)";

            using (var command = new NpgsqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@issue_id", issue_id);
                await command.ExecuteNonQueryAsync();
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
        string configFilePath = $@"{AppContext.BaseDirectory}dbConfig.conf";

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
