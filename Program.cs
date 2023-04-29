using MySql.Data.MySqlClient;
using System.Text.Json;
namespace Cookinguest;

public class Program
{
    public static MySqlConnection connection = InitializeConnection();
    public static void Main()
    {
        connection.Open();

    }

    public static MySqlConnection InitializeConnection()
    {
        string jsonString = File.ReadAllText("settings.json");

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };
        Dictionary<string,string>? dico = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonString, options);
        if (dico is null)
            throw new NullReferenceException("The dictionary is null.");
        return new MySqlConnection($"server={dico["server"]};port={dico["port"]};database={dico["database"]};user={dico["user"]};password={dico["passord"]};"); 
    }
}