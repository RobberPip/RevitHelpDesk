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
<<<<<<< HEAD
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
=======
    //TODO Перенести в хранимые процедуры БД
    public async Task AddRequestAsync(long issue_id, RequestModel request)
    {
        try
        {
            int id = 0;
            string query;
            await connection.OpenAsync();
            if (issue_id != -1)
            {
                query =
                   $"SET search_path = {dbSchema}; INSERT INTO hd_request(issue_id,advance_info) VALUES (@issue_id,@advance_info)";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@issue_id", issue_id);
                    command.Parameters.Add("@advance_info", NpgsqlDbType.Bytea).Value = request.fileBytes != null ? request.fileBytes : DBNull.Value;
                    await command.ExecuteNonQueryAsync();
                }
            }
            issue_id = request.IssueId;
            query =
                $"SET search_path = {dbSchema}; SELECT id from hd_request WHERE issue_id = @issue_id";
            using (var command = new NpgsqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@issue_id", issue_id);
                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        id = reader.GetInt32(0);
                    }
                }
            }
            query = $"SET search_path = {dbSchema}; INSERT INTO hd_files(request_id,file) VALUES (@request_id,@file)";
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


>>>>>>> 4c2db5e (first version)
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
<<<<<<< HEAD
        string configFilePath = $@"{AppContext.BaseDirectory}dbConfig.conf";
=======
        string exeDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        string configFilePath = Path.Combine(exeDirectory, "dbConfig.conf");
>>>>>>> 4c2db5e (first version)

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
