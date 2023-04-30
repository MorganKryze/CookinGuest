using System.Diagnostics;
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
        MySqlDataReader reader = Reader("SELECT * FROM Client");
        reader.Close();

        MainMenu:

        #region MainMenu
        if (utilisateur is Profil.NonDefini)
        {
            ClearContent();
            switch(ScrollingMenu("Bienvenue sur la plateforme CookinGuest !", new string[]{"Se connecter", "S'inscrire","Options", "Quitter"}))
            {
                case 0:
                    goto Connecter;
                case 1:
                    goto Inscription;
                case 2:
                    goto Options;
                case 3:
                    goto Quitter;
                default:
                    goto MainMenu;
            }
        }
        else if (utilisateur is Profil.Client)
        {
            ClearContent();
            reader = Reader($"SELECT Prenom, Nom, Age, Telephone, Domicile FROM Client WHERE Mail = '{email}'");
            WriteParagraph(new string[]{$"{reader.GetName(0),-20} {reader.GetName(1),-20} {reader.GetName(2),-20}"}, true);
            while (reader.Read())
                WriteParagraph(new string[]{$"{reader.GetString(0),-20} {reader.GetString(1),-20} {reader.GetString(2),-20}" }, false, CursorTop + 1);
            reader.Close();

            command = new MySqlCommand($"SELECT Prenom FROM Client WHERE Mail = '{email}'", connection);
            string? prenom = command.ExecuteScalar().ToString();
            switch(ScrollingMenu($"Bienvenue sur la plateforme CookinGuest {prenom} !", new string[]{"Acheter une recette", "Options", "Déconnexion"}, Placement.Center, CursorTop +2))
            {
                case 0:
                    ClearContent();
                    goto AcheterRecette;
                case 1:
                    ClearContent();
                    goto Options;
                case 2:
                    ClearContent();
                    utilisateur = Profil.NonDefini;
                    email = "";
                    mdp = "";
                    WriteBanner((" Projet BDD", "Accueil", "Réalisé par Dimitry et Clément "), true, true);
                    goto MainMenu;
                default:
                    ClearContent();
                    goto MainMenu;
            }
        }
        else if (utilisateur is Profil.Createur)
        {
            ClearContent();
            reader = Reader($"SELECT Prenom, Nom, Age, Telephone, Domicile FROM Client WHERE Mail = '{email}'");
            WriteParagraph(new string[] { $"{reader.GetName(0),-20} {reader.GetName(1),-20} {reader.GetName(2),-20}" }, true);
            while (reader.Read())
                WriteParagraph(new string[] { $"{reader.GetString(0),-20} {reader.GetString(1),-20} {reader.GetString(2),-20}" }, false, CursorTop + 1);
            reader.Close();
            switch(ScrollingMenu("Bienvenue sur la plateforme CookinGuest !", new string[]{"Acheter recette", "Créer recette", "Supprimer recette", "Options", "Déconnexion"}, Placement.Center, CursorTop + 2))
            {
                case 0:
                    goto AcheterRecette;
                case 1:
                    goto CreerRecette;
                case 2:
                    goto SupprimerRecette;
                case 3:
                    goto Options;
                case 4:
                    utilisateur = Profil.NonDefini;
                    email = "";
                    mdp = "";
                    WriteBanner((" Projet BDD", "Accueil", "Réalisé par Dimitry et Clément "), true, true);
                    goto MainMenu;
                default:
                    goto MainMenu;
            }
        }
        else if (utilisateur is Profil.Administrateur)
        {
            ClearContent();
            switch(ScrollingMenu("Bienvenue sur la plateforme CookinGuest !", new string[]{"Ajouter Client", "Supprimer Client", "Supprimer Recette","Supprimer Commande", "Ajouter Fournisseur", "Ajouter Produit", "Options", "Déconnexion"}))
            {
                case 0:
                    goto Inscription;
                case 1:
                    goto SupprimerClient;
                case 2:
                    goto SupprimerRecette;
                case 3:
                    goto SupprimerCommande;
                case 4:
                    goto AjouterFournisseur;
                case 5:
                    goto AjouterProduit;
                case 6:
                    goto Options;
                case 7:
                    utilisateur = Profil.NonDefini;
                    email = "";
                    mdp = "";
                    WriteBanner((" Projet BDD", "Accueil", "Réalisé par Dimitry et Clément "), true, true);
                    goto MainMenu;
                default:
                    goto MainMenu;
            }
        }
        else
            throw new Exception("The user profile is not defined.");
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
                        goto PassWord;
                    default:
                        goto MainMenu;
                }
            }
            else
            {
                if (email == "admin" && mdp == "staff")
                {
                    utilisateur = Profil.Administrateur;
                    WriteBanner((" Projet BDD", "Admin", "Réalisé par Dimitry et Clément "), true, true);
                    goto MainMenu;
                }
                else if (command.ExecuteScalar().ToString() == mdp)
                {
                    query = $"SELECT Createur FROM Client WHERE Mail = '{email}'";
                    command = new MySqlCommand(query, connection);
                    if (command.ExecuteScalar().ToString() == "True")
                    {
                        utilisateur = Profil.Createur;
                        WriteBanner((" Projet BDD", "Créateur", "Réalisé par Dimitry et Clément "), true, true);
                    }
                    else
                    {
                        utilisateur = Profil.Client;
                        WriteBanner((" Projet BDD", "Client", "Réalisé par Dimitry et Clément "), true, true);
                    }
                    goto MainMenu;
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

        Inscription:

        #region Inscription
        email = WritePrompt("Veuillez entrer votre email : ");
        query = $"SELECT Mail FROM Client WHERE Mail = '{email}'";
        command = Command(query);
        if (command.ExecuteScalar() is null)
        {
            mdp = WritePassword("Veuillez entrer votre mot de passe : ");
            string nom = WritePrompt("Veuillez entrer votre nom : ");
            string prenom = WritePrompt("Veuillez entrer votre prénom : ");
            int age = int.Parse(WritePrompt("Veuillez entrer votre âge : "));
            string telephone = WritePrompt("Veuillez entrer votre numéro de téléphone : ");
            string domicile = WritePrompt("Veuillez entrer votre adresse : ");
            switch(ScrollingMenu("Veuillez choisir votre profil", new string[]{"Client", "Créateur"}))
            {
                case 1:
                    query = $"INSERT INTO Client (Nom, Prenom, Age, Telephone, Domicile, Mail, PtsBonus, Solde, MDP, Createur) VALUES ('{nom}', '{prenom}', '{age}', '{telephone}', '{domicile}', '{email}', 0, 0, '{mdp}', true)";
                    if (utilisateur is not Profil.Administrateur)
                        utilisateur = Profil.Createur;
                    break;
                default:
                    query = $"INSERT INTO Client (Nom, Prenom, Age, Telephone, Domicile, Mail, PtsBonus, Solde, MDP, Createur) VALUES ('{nom}', '{prenom}', '{age}', '{telephone}', '{domicile}', '{email}', 0,0, '{mdp}', false)";
                    if (utilisateur is not Profil.Administrateur)
                        utilisateur = Profil.Client;
                    break;
            }       
            command = Command(query);
            command.ExecuteNonQuery();
            if (utilisateur == Profil.Createur)
                WriteBanner((" Projet BDD", "Créateur", "Réalisé par Dimitry et Clément "), true, true);
            else if (utilisateur == Profil.Client)
                WriteBanner((" Projet BDD", "Client", "Réalisé par Dimitry et Clément "), true, true);
            WriteParagraph(new string[]{" Votre compte a bien été créé ! ", " Presser une touche pour continuer... "});
            ReadKey(true);
            goto MainMenu;
        }
        else
        {
            switch (ScrollingMenu("Un compte existe déjà avec cet email, recommencer ?", new string[] { "Oui", "Non" }))
            {
                case 0:
                    goto Inscription;
                default:
                    goto MainMenu;
            }
        }
        #endregion

        SupprimerClient:

        #region SupprimerClient
        ClearContent();
        query = $"SELECT Nom, Prenom, Mail, Createur FROM Client;";
        reader = Reader(query);
        List<string> clients = new List<string>();
        List<string> mails = new List<string>();
        WriteParagraph(new string[] { "Veuillez sélectionner le compte client à supprimer :" }, false);
        while (reader.Read())
        {
            clients.Add($"{reader.GetString(0),-20} {reader.GetString(1),-20} {reader.GetString(2),-20} {reader.GetBoolean(3),-20}");
            mails.Add(reader.GetString(2));
        }
        string prompt = $"{reader.GetName(0),-20} {reader.GetName(1),-20} {reader.GetName(2),-20} {reader.GetName(3),-20}";
        reader.Close();
        int pos = ScrollingMenu(prompt, clients.ToArray(), Placement.Center, CursorTop + 2, false, 0, 0, true);
        if (pos == -1)
            goto MainMenu;
        int choixDel = ScrollingMenu("Voulez-vous vraiment supprimer ce compte ?", new string[] { "Oui", "Non" });
        if (choixDel == 1 || choixDel == -1)
            goto SupprimerClient;
        else
        { 
            // vérifier si le client est créateur
            query = $"SELECT Createur FROM Client WHERE Mail = '{mails[pos]}'";
            command = Command(query);
            if (command.ExecuteScalar().ToString() == "True")
            {
                //supprimer les produit dans QuantiteProduit où apparaît l'id de ses recettes
                query = $"DELETE FROM QuantiteProduit WHERE idRecette IN (SELECT idRecette FROM Recette WHERE idClient = (SELECT idClient FROM Client WHERE Mail = '{mails[pos]}'))";
                command = Command(query);
                command.ExecuteNonQuery();
                //supprimer ses recettes
                query = $"DELETE FROM Recette WHERE idClient = (SELECT idClient FROM Client WHERE Mail = '{mails[pos]}')"; 
                command = Command(query);
                command.ExecuteNonQuery();
            }
            //supprimer dans DetCommandes
            query = $"DELETE FROM DetCommande WHERE idCommande = (SELECT idCommande FROM Commande WHERE idClient = (SELECT idClient FROM Client WHERE Mail = '{mails[pos]}'))"; 
            command = Command(query);
            command.ExecuteNonQuery();
            //supprmier ses commandes 
            query = $"DELETE FROM Commande WHERE idClient = (SELECT idClient FROM Client WHERE Mail = '{mails[pos]}')"; 
            command = Command(query);
            command.ExecuteNonQuery();
            //supprimer le client
            query = $"DELETE FROM Client WHERE Mail = '{mails[pos]}'";
            command = Command(query);
            command.ExecuteNonQuery();
        }
        
        if (clients.Count > 0)
            goto SupprimerClient;
        else
            goto MainMenu;
        #endregion

        SupprimerCommande:

        #region SupprimerCommande
        ClearContent();
        query = $"SELECT idCommande, date, adresse, idClient FROM Commande;";
        reader = Reader(query);
        List<string> commandes = new List<string>();
        List<int> ids = new List<int>();
        WriteParagraph(new string[] { "Veuillez sélectionner la commande à supprimer :" }, false);
        while (reader.Read())
        {
            commandes.Add($"{reader.GetInt32(0),-20} {reader.GetDateTime(1),-20} {reader.GetString(2),-20} {reader.GetInt32(3),-20}");
            ids.Add(reader.GetInt32(0));
        }
        prompt = $"{reader.GetName(0),-20} {reader.GetName(1),-20} {reader.GetName(2),-20} {reader.GetName(3),-20}";
        reader.Close();
        pos = ScrollingMenu(prompt, commandes.ToArray(), Placement.Center, CursorTop + 2, false, 0, 0, true);
        if (pos == -1)
            goto MainMenu;
        choixDel = ScrollingMenu("Voulez-vous vraiment supprimer cette commande ?", new string[] { "Oui", "Non" }, Placement.Center, CursorTop + 1);
        if (choixDel == 1 || choixDel == -1)
            goto SupprimerCommande;
        else
        {
            //supprimer dans DetCommandes
            query = $"DELETE FROM DetCommande WHERE idCommande = {ids[pos]}";
            command = Command(query);
            command.ExecuteNonQuery();
            //supprimer la commande
            query = $"DELETE FROM Commande WHERE idCommande = {ids[pos]}";
            command = Command(query);
            command.ExecuteNonQuery();
        }
        if (commandes.Count > 0)
            goto SupprimerCommande;
        else
            goto MainMenu;
        #endregion        
        
        AjouterFournisseur:

        #region AjouterFournisseur
        ClearContent();
        string nomFournisseur = WritePrompt("Veuillez entrer le nom du fournisseur :");
        string numSiret = WritePrompt("Veuillez entrer le numéro de SIRET du fournisseur :");
        string adresseFournisseur = WritePrompt("Veuillez entrer l'adresse du fournisseur :");
        query = $"INSERT INTO Fournisseur (Nomfour, NumSiret, Adresse)VALUES ('{nomFournisseur}', '{numSiret}', '{adresseFournisseur}')";
        command = Command(query);
        command.ExecuteNonQuery();
        goto MainMenu;
        #endregion

        AjouterProduit:

        #region AjouterProduit
        ClearContent();
        string nomProduit = WritePrompt("Veuillez entrer le nom du produit :");
        string categorieProduit = WritePrompt("Veuillez entrer la catégorie du produit :");
        string unit = ScrollingMenuString("Veuillez entrer l'unité du produit :", new string[] { "kg", "L", "unité" });
        query = $"SELECT idFournisseur, NomFour FROM Fournisseur";
        reader = Reader(query);
        List<int> idFournisseurs = new List<int>();
        List<string> nomFournisseurs = new List<string>();
        while (reader.Read())
        {
            idFournisseurs.Add(reader.GetInt32(0));
            nomFournisseurs.Add(reader.GetString(1));
        }
        reader.Close();
        int idFournisseur = ScrollingMenu("Veuillez choisir le fournisseur du produit :", nomFournisseurs.ToArray());
        if (idFournisseur == -1)
            goto MainMenu;
        query = $"INSERT INTO Produit (Nom, CategorieProd, Unit, StockAnn, StockMin, StockMax, idFournisseur)VALUES ('{nomProduit}', '{categorieProduit}', '{unit}', 500, 50, 2000, {idFournisseurs[idFournisseur]})";
        command = Command(query);
        command.ExecuteNonQuery();
        goto MainMenu;
        #endregion

        AcheterRecette:

        #region AcheterRecette
        #region Affichage infos client
        ClearContent();
        reader = Reader($"SELECT Prenom, Nom, Domicile, PtsBonus FROM Client WHERE Mail = '{email}'");
        WriteParagraph(new string[] { $"{reader.GetName(0),-20} {reader.GetName(1),-20} {reader.GetName(2),-20} {reader.GetName(3),-20}" }, true);
        while (reader.Read())
            WriteParagraph(new string[] { $"{reader.GetString(0),-20} {reader.GetString(1),-20} {reader.GetString(2),-20} {reader.GetInt32(3),-20}" }, false, CursorTop + 1);
        reader.Close();
        #endregion

        #region Affichage recettes
        reader = Reader($"SELECT NomRec, CategorieRec, DescriptifRec, Prix, PtsBonus FROM Recette");
        WriteParagraph(new string[] { "Veuillez choisir la recette que vous souhaitez acheter parmi les propositions : " }, false, CursorTop + 2);
        List<string> recettes = new List<string>();
        while (reader.Read())
        {
            recettes.Add($"{reader.GetString(0),-20} {reader.GetString(1),-20} {reader.GetString(2),-50} {reader.GetInt32(3),-20} {reader.GetInt32(4),-20}");
        }
        
        int numRecette = ScrollingMenu($"{reader.GetName(0),-30} {reader.GetName(1),-30} {reader.GetName(2),-50} {reader.GetName(3),-25} {reader.GetName(4),-25}", recettes.ToArray(), Placement.Center, CursorTop + 2, true, 1500, 0, true);
        reader.Close();
        if (numRecette == -1)
            goto MainMenu;
        ClearContent();
        int nombreCommandes = (int)NumberSelector("Veuillez choisir le nombre de recettes que vous souhaitez acheter", 1f, 10f, 1f, 1f);
        if (nombreCommandes == -1)
            goto AcheterRecette;
        #endregion

        #region Ajouter Commande
        query = $"SELECT domicile FROM Client WHERE Mail = '{email}'";
        command = Command(query);
        string adresse = (string)command.ExecuteScalar();

        query = $"SELECT idClient FROM Client WHERE Mail = '{email}'";
        command = Command(query);
        int idClient = (int)command.ExecuteScalar();

        query = $"INSERT INTO Commande (date, adresse, idClient) VALUES ('{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', '{adresse}', {idClient})";
        command = Command(query);
        command.ExecuteNonQuery();

        query = $"INSERT INTO DetCommande(idRecette, idCommande, QCommande) VALUES ({numRecette + 1}, (SELECT MAX(idCommande) FROM Commande), {nombreCommandes})";
        command = Command(query);
        command.ExecuteNonQuery();
        #endregion

        #region Ajouter Points Bonus
        // augmenter les pts bonus du client qui a proposé la recette 
        query = $"UPDATE Client SET PtsBonus = PtsBonus + (SELECT PtsBonus FROM Recette WHERE NomRec = (SELECT NomRec FROM Recette WHERE idRecette = {numRecette + 1})) WHERE idClient = (SELECT idClient FROM Recette WHERE idRecette = {numRecette + 1})";
        command = Command(query);
        command.ExecuteNonQuery();

        query = $"UPDATE Client SET Solde = Solde + 2 * (SELECT PtsBonus FROM Recette WHERE NomRec = (SELECT NomRec FROM Recette WHERE idRecette = {numRecette + 1})) WHERE idClient = (SELECT idClient FROM Recette WHERE idRecette = {numRecette + 1})";
        command = Command(query);
        command.ExecuteNonQuery();
        #endregion

        goto MainMenu;
        #endregion

        CreerRecette:

        #region CreerRecette
        string nomRecette = WritePrompt("Veuillez entrer le nom de la recette :");
        string categorieRecette = ScrollingMenuString("Veuillez choisir la catégorie de la recette", new string[] { "Entrée", "Plat", "Dessert" });
        if (categorieRecette == "Retour")
            goto MainMenu;
        string descriptifRecette = WritePrompt("Veuillez entrer le descriptif de la recette :");


        Ingredients:

        query = $"SELECT Nom FROM Produit;";
        List<string> ingredients = new List<string>();
        reader = Reader(query);
        while (reader.Read())
            ingredients.Add(reader.GetString(0));
        reader.Close();
        ingredients.Add("Terminer la création");
        List<string> ingredientsRecette = new List<string>();
        List<int> quantitesRecette = new List<int>();
        int numIngredient = 0;
        while (ingredients.Count > 1 || numIngredient != ingredients.Count - 1)
        {
            ClearContent();
            numIngredient = ScrollingMenu("Veuillez choisir un ingrédient à ajouter à la recette", ingredients.ToArray(), Placement.Center, -1, false, 0, numIngredient);
            if (numIngredient == -1)
            {
                int choix = ScrollingMenu("Voulez-vous vraiment quitter la création de la recette ?", new string[] { "Oui", "Non" });
                if (choix == 0)
                    goto MainMenu;
                else
                    goto Ingredients;
            }
            else if (numIngredient == ingredients.Count - 1)
                break;
            else
            {
                int quantiteIngredient = (int)NumberSelector("Veuillez entrer la quantité de cet ingrédient :", 0f, 10f, 0f, 1f, CursorTop + 2);
                if (quantiteIngredient == -1)
                    quantiteIngredient = 1;
                ingredientsRecette.Add(ingredients[numIngredient]);
                quantitesRecette.Add(quantiteIngredient);
                ingredients.RemoveAt(numIngredient);
            }
        }
        ClearContent();
        float prixRecette = (int)NumberSelector("Veuillez entrer le prix de la recette :", 0f, 20f, 0f, 1f);
        int ptsBonusRecette = (int)NumberSelector("Veuillez entrer le nombre de points bonus de la recette :", 0f, 30f, 0f, 1f);


        query = $"INSERT INTO Recette (NomRec, CategorieRec, DescriptifRec, Prix, PtsBonus, idClient) VALUES ('{nomRecette}', '{categorieRecette}', '{descriptifRecette}', {prixRecette}, {ptsBonusRecette}, (SELECT idClient FROM Client WHERE Mail = '{email}'));";
        command = Command(query);
        command.ExecuteNonQuery();

        query = $"SELECT MAX(idRecette) FROM Recette";
        command = Command(query);
        int idRecette = (int)command.ExecuteScalar();

        for (int i = 0; i < ingredientsRecette.Count; i++)
        {
            query = $"INSERT INTO QuantiteProduit (idProduit, idrecette, QNeeded) VALUES ((SELECT idProduit FROM Produit WHERE Nom = '{ingredientsRecette[i]}'), {idRecette}, {quantitesRecette[i]})";
            command = Command(query);
            command.ExecuteNonQuery();
        }
        goto MainMenu;
        #endregion

        SupprimerRecette:

        #region SupprimerRecette
        if (utilisateur is Profil.Createur)
            query = $"SELECT NomRec FROM Recette WHERE idClient = (SELECT idClient FROM Client WHERE Mail = '{email}')";
        else
            query = $"SELECT NomRec FROM Recette";
        List<string> recettesDel = new List<string>();
        reader = Reader(query);
        while (reader.Read())
            recettesDel.Add(reader.GetString(0));
        recettesDel.Add("Retour");
        WriteParagraph(new string[]{"Veuillez choisir la recette à supprimer"});
        int numRecetteDel = ScrollingMenu("Nom de Recette".BuildString(20), recettesDel.ToArray(), Placement.Center, CursorTop + 2, true, 1500, 0, true);
        reader.Close();
        if (numRecetteDel == recettesDel.Count - 1 || numRecetteDel == -1)
            goto MainMenu;
        else
        {
            int choixDelCom = ScrollingMenu("Voulez-vous vraiment supprimer cette recette ?", new string[] { "Oui", "Non" });
            if (choixDelCom == 1 || choixDelCom == -1)
                goto SupprimerRecette;
            query = $"DELETE FROM Recette WHERE NomRec = '{recettesDel[numRecetteDel]}'";
            command = Command(query);
            command.ExecuteNonQuery();

            query = $"DELETE FROM QuantiteProduit WHERE idRecette = (SELECT idRecette FROM Recette WHERE NomRec = '{recettesDel[numRecetteDel]}')";
            command = Command(query);
            command.ExecuteNonQuery();
        }
        goto SupprimerRecette;
        #endregion

        Options:

        #region Options
        ClearContent();
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