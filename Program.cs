using MySql.Data.MySqlClient;
using System.Text.Json;
using static System.ConsoleColor;
using static CookinGuest.ConsoleVisuals;
using static System.Console;

namespace Cookinguest;

public class Program
{
    #region Variables
    public static MySqlConnection connection = InitializeConnection();
    public static MySqlCommand command = new MySqlCommand("", connection);
    //public static MySqlDataReader reader = new MySqlDataReader("");
    public static Profil utilisateur = Profil.NonDefini;
    public static string email = "";
    public static string mdp = "";

    public enum Profil
    {
        NonDefini,
        Client,
        Createur,
        Administrateur
    }
    #endregion

    #region Methods
    public static void Main()
    {
        connection.Open();
        WriteFullScreen(default);

        MainMenu:

        #region MainMenu
        switch(ScrollingMenu("Bienvenue sur la plateforme CookinGuest !", new string[]{"Se connecter", "S'inscrire","Options", "Quitter"}))
        {
            case 0:
                goto Connecter;
            case 1:
                Console.Clear();
                Console.WriteLine("S'inscrire");
                break;
            case 2:
                goto Options;
            case 3:
                goto Quitter;
        }
        #endregion

        Connecter:

        #region Connecter
        email = WritePrompt("Veuillez entrer votre email : ");
        string query = $"SELECT Mail FROM Client WHERE Mail = '{email}'";
        command = new MySqlCommand(query, connection);
        if (command.ExecuteScalar() is null && email != "admin")
        {
            switch (ScrollingMenu("Aucune correspondance dans la base de données, recommencer ?", new string[] { "Oui", "Non" }))
            {
                case 0:
                    goto Connecter;
                default:
                    goto MainMenu;
            }
        }
        else
        {
            PassWord:
            mdp = WritePassword("Veuillez entrer votre mot de passe : ");
            query = $"SELECT MDP FROM Client WHERE Mail = '{email}'";
            command = new MySqlCommand(query, connection);
            if (command.ExecuteScalar() is null && mdp != "staff")
            {
                switch (ScrollingMenu("Aucune correspondance dans la base de données, recommencer ?", new string[] { "Oui", "Non" }))
                {
                    case 0:
                        goto Connecter;
                    default:
                        goto MainMenu;
                }
            }
            else
            {
                if (email == "admin" && mdp == "staff")
                {
                    utilisateur = Profil.Administrateur;
                    goto MainMenu;
                }
                else if (command.ExecuteScalar().ToString() == mdp)
                {
                    query = $"SELECT Createur FROM Client WHERE Mail = '{email}'";
                    command = new MySqlCommand(query, connection);
                    if (command.ExecuteScalar().ToString() == "1")
                        utilisateur = Profil.Createur;
                    else
                        utilisateur = Profil.Client;
                }
                else 
                {
                    switch (ScrollingMenu("Mot de passe incorrect, recommencer ?", new string[] { "Oui", "Non" }))
                    {
                        case 0:
                            goto PassWord;
                        default:
                            goto Connecter;
                    }
                }

            }
        }
        #endregion

        Options:

        #region Options
        switch (ScrollingMenu("Veuillez sélectionner une option", new string[]{
                    "Changer de couleur",
                    "Changer description",
                    "Retour"}))
        {
            case 0:
                switch (ScrollingMenu("Veuillez choisir la nouvelle couleur de l'interface", new string[]{
                            "Blanc",
                            "Rouge",
                            "Magenta",
                            "Jaune",
                            "Vert",
                            "Bleu",
                            "Cyan"}))
                                {
                                    case 0:
                                        ChangeFont(White);
                                        break;
                                    case 1:
                                        ChangeFont(Red);
                                        break;
                                    case 2:
                                        ChangeFont(Magenta);
                                        break;
                                    case 3:
                                        ChangeFont(Yellow);
                                        break;
                                    case 4:
                                        ChangeFont(Green);
                                        break;
                                    case 5:
                                        ChangeFont(Blue);
                                        break;
                                    case 6:
                                        ChangeFont(Cyan);
                                        break;
                                    case -1:
                                        goto Options;
                                }
                goto MainMenu;
            case 1:
                string description = WritePrompt("Veuillez entrer votre nouvelle description : ");
                WriteBanner((" Projet BDD", description, "Réalisé par Dimitry et Clément "), true, true);
                goto MainMenu;
            default:
                goto MainMenu;
        }
        #endregion

        Quitter:

        #region Quitter
        connection.Close();
        ProgramExit();
        #endregion
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
        return new MySqlConnection($"server={dico["server"]};port={dico["port"]};database={dico["database"]};user={dico["user"]};password={dico["password"]};"); 
    }

    public static MySqlCommand Command(string query) => new MySqlCommand(query, connection);
    public static MySqlDataReader Reader(string query) => Command(query).ExecuteReader();
    #endregion
}