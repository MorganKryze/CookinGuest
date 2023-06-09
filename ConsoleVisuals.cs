﻿using static System.Console;
using static System.Threading.Thread;
using static System.IO.File;
using static System.ConsoleColor;
using static System.ConsoleKey;

using System.Collections.Generic;
using System.Threading.Tasks;

namespace CookinGuest;

/// <summary> The <see cref="ConsoleVisuals"/> classe contains all the visual elements for a console app. </summary>
public static class ConsoleVisuals
{
    #region Private attributes
    private const string titlePath = "title.txt";
    private static string[] titleContent = ReadAllLines(titlePath);
    private static int initialWindowWidth = WindowWidth;
    private static int intialWindowHeight = WindowHeight;
    private static (ConsoleColor, ConsoleColor) colorPanel = (White, Black);
    private static (ConsoleColor, ConsoleColor) initialColorPanel = (colorPanel.Item1, colorPanel.Item2);
    private static (ConsoleColor, ConsoleColor) terminalColorpanel = (ForegroundColor, BackgroundColor);
    #endregion

    #region Private properties
    public static (string, string, string) defaultHeader = (" Projet BDD", "Accueil", "Réalisé par Dimitry et Clément ");
    public static (string, string, string) defaultFooter = (" [ESC] Retour", "[Z|↑] Monter   [S|↓] Descendre", "[ENTRER] Sélectionner ");
    private static int TitleHeight => titleContent.Length;
    private static int HeaderHeigth => TitleHeight ;
    private static int FooterHeigth => WindowHeight - 1;
    private static int ContentHeigth => HeaderHeigth + 2;
    private static bool WindowManipulated => WindowWidth != initialWindowWidth || WindowHeight != intialWindowHeight;
    #endregion

    #region Enums
    /// <summary> The <see cref="Placement"/> enum defines the placement of a string in the console. </summary>
    public enum Placement
    {
        /// <summary> The string is placed at the left of the console. </summary>
        Left,
        /// <summary> The string is placed at the center of the console. </summary>
        Center,
        /// <summary> The string is placed at the right of the console. </summary>
        Right
    }
    #endregion

    #region Low level methods
    /// <summary> this method changes the font color of the console. </summary>
    public static void ChangeFont(ConsoleColor newfont)
    {
        colorPanel.Item1 = newfont;
    }
    private static void TryNegative(bool negative = false)
    {
        ForegroundColor = negative ? colorPanel.Item2 : colorPanel.Item1;
        BackgroundColor = negative ? colorPanel.Item1 : colorPanel.Item2;
    }
    private static void WritePositionnedString(string str, Placement position = Placement.Center, bool negative = false, int line = -1, bool chariot = false)
	{
        TryNegative(negative);
		if (line < 0) 
            line = Console.CursorTop;
		if (str.Length < Console.WindowWidth) 
            switch (position)
		    {
		    	case (Placement.Left): 
                    SetCursorPosition(0, line); 
                    break;
		    	case (Placement.Center): 
                    SetCursorPosition((WindowWidth - str.Length) / 2, line); 
                    break;
		    	case (Placement.Right): 
                    SetCursorPosition(WindowWidth - str.Length, line); 
                    break;
		    }
		else 
            SetCursorPosition(0, line);
		if (chariot) 
            WriteLine(str);
        else 
            Write(str);
        TryNegative(default);
	}
    private static void ClearLine(int line)
	{
		TryNegative(default);
		WritePositionnedString("".PadRight(Console.WindowWidth), Placement.Left, default, line);
	}
    /// <summary> This method clears a specified part of the console. </summary>
    /// <param name="startIndex"> The index of the first line to clear. </param>
    /// <param name="length"> The number of lines to clear. </param>
    public static void ClearPanel(int startIndex = -1, int length = 1)
    {
        if (startIndex == -1)
            startIndex = ContentHeigth;
        for (int i = startIndex; i < startIndex + length; i++)
            ClearLine(i);
    }
    /// <summary> This method clears the content of the console. </summary>
    public static void ClearContent()
    {
        for (int i = ContentHeigth - 1; i < FooterHeigth; i++)
            ClearLine(i);
    }
    private static void ClearAll()
    {
        colorPanel = terminalColorpanel;
        for (int i = 0; i < WindowHeight; i++)
            ContinuousPrint("".PadRight(WindowWidth), i, default, 100, 10);
        Clear();
        colorPanel = (White, Black);
    }
    private static void ContinuousPrint(string text, int line, bool negative = false, int stringTime = 2000, int endStringTime = 1000)
    {
        int t_interval = (int)(stringTime / text.Length);
        for (int i = 0; i <= text.Length; i++)
        {
            string continuous = "";
            for(int j = 0; j < i; j++) 
                continuous += text[j];
            continuous = continuous.PadRight(text.Length);
            WritePositionnedString(continuous.BuildString(WindowWidth, Placement.Center), default, negative, line);

            if(i != text.Length)
                Sleep(t_interval);

            if(KeyAvailable)
            {
                ConsoleKeyInfo keyPressed = ReadKey(true);
                if(keyPressed.Key == Enter || keyPressed.Key == Escape)
                {
                    i = text.Length;
                    break;
                }
            }
        }
        WritePositionnedString(text.BuildString(WindowWidth, Placement.Center), default, negative, line);
        Sleep(endStringTime);
    }
    private static bool IsScreenUpdated()
    {
        if (WindowManipulated || colorPanel != initialColorPanel)
        {
            WriteFullScreen(true);
            initialWindowWidth = WindowWidth;
            initialColorPanel = (colorPanel.Item1, colorPanel.Item2);
            return true;
        }
        return false;
    }
    #endregion

    #region Mid level methods
    /// <summary> This method prints the title of the console app. </summary>
    public static void WriteTitle()
    {
        Clear();
        SetCursorPosition(0, 0);
        foreach (string line in titleContent)
        {
            WritePositionnedString(line.BuildString(WindowWidth, Placement.Center));
            WriteLine("");
        } 
    }
    /// <summary> This method prints a banner in the console. </summary>
    /// <param name="banner"> The banner to print. </param>
    /// <param name="header"> If true, the banner is printed at the top of the console. If false, the banner is printed at the bottom of the console. </param>
    /// <param name="straight"> If true, the title is not continuously printed. </param>
    public static void WriteBanner((string, string, string)? banner = null, bool header = true, bool straight = false)
	{
        (string, string, string) newBanner = banner ??= header ? defaultHeader : defaultFooter;

		TryNegative(true);
		string strBanner = newBanner.Item2.BuildString(Console.WindowWidth, Placement.Center, true);
		strBanner = strBanner.Substring(0, strBanner.Length - newBanner.Item3.Length) + newBanner.Item3;
		strBanner = newBanner.Item1 + strBanner.Substring(newBanner.Item1.Length);
        if (straight) 
            WritePositionnedString(strBanner, default, true, header ? HeaderHeigth : FooterHeigth);
        else
		    ContinuousPrint(strBanner, header ? HeaderHeigth : FooterHeigth, true);
		TryNegative();
	}
    /// <summary> This method prints a full screen in the console. </summary>
    /// <param name="straight"> If true, the title is not continuously printed. </param>
    /// <param name="header"> The header of the screen. </param>
    /// <param name="footer"> The footer of the screen. </param>
    public static void WriteFullScreen(bool straight = false, (string, string, string)? header = null, (string, string, string)? footer = null)
    {
        if (header is null)
            header = defaultHeader;
        if (footer is null)
            footer = defaultFooter;
        CursorVisible = false;
        WriteTitle();
        WriteBanner(header, true, straight);
        WriteBanner(footer, false, straight);
        ClearContent();
        if (!straight) 
            LoadingScreen("[ Chargement ... ]");
        ClearContent();
    }
    #endregion
    // dots : •••••••••••
    #region High level methods
    /// <summary> This method prints a message in the console and gets a string written by the user. </summary>
    /// <param name="message"> The message to print. </param>
    /// <param name="line"> The line where the message will be printed. </param>
    /// <returns> The string written by the user. </returns>
    public static string WritePrompt(string message, int line = -1)
    {
        if (line == -1) 
            line = ContentHeigth;
        if (!IsScreenUpdated())
            ClearPanel(line, 3);

        ContinuousPrint(message.BuildString(message.Length, Placement.Center), line, default, 1500, 50);
        string prompt = "";
        do
        {
            ClearLine(line + 1);
            Write("{0," + ((WindowWidth / 2) - (message.Length / 2) + 2) + "}", "> ");
            CursorVisible = true;
            prompt = ReadLine() ?? "";
            CursorVisible = false;
        } while (prompt is "");
        ClearPanel(line, 3);
        return prompt;
    }
    public static string WritePassword(string message, int line = -1)
    {
        if (line == -1)
            line = ContentHeigth;
        if (!IsScreenUpdated())
            ClearPanel(line, 3);

        ContinuousPrint(message.BuildString(message.Length, Placement.Center), line, default, 1500, 50);
        string prompt = "";
        do
        {
            while (true)
            {
                ClearLine(line + 1);
                Write("{0," + ((WindowWidth / 2) - (message.Length / 2) + 2) + "}", "> ");
                Write(new string('•', prompt.Length));
                CursorVisible = true;
                ConsoleKeyInfo key = Console.ReadKey(true);

                if (key.Key == ConsoleKey.Enter)
                {
                    break;
                }
                else if (key.Key == ConsoleKey.Backspace)
                {
                    if (prompt.Length > 0)
                    {
                        prompt = prompt.Remove(prompt.Length - 1);
                        Write("\b \b");
                    }
                }
                else if (key.KeyChar != '\u0000' && key.Key != ConsoleKey.Escape)
                    prompt += key.KeyChar;
            CursorVisible = false;
            }
            CursorVisible = false;
        } while (prompt is "");
        ClearPanel(line, 3);
        return prompt;
    }
    /// <summary> This method prints a paragraph in the console. </summary>
    /// <param name="text"> The lines of the paragraph. </param>
    /// <param name="negative"> If true, the paragraph is printed in the negative colors. </param>
    /// <param name="line"> The height of the paragraph. </param>
    public static void WriteParagraph(IEnumerable<string> text, bool negative = false, int line = -1, Placement placement = Placement.Center)
	{
        IsScreenUpdated();
        if (line == -1)
            line =  ContentHeigth;
            ClearPanel(line, text.Count());

        TryNegative(negative);
		int maxLength = text.Count() > 0 ? text.Max(s => s.Length) : 0;
		foreach (string str in text)
		{
			WritePositionnedString(str.BuildString(maxLength, Placement.Center), placement, negative, line++);
			if (line >= WindowHeight - 1) 
                break;
		}
        TryNegative(default);
        //ClearPanel(line, text.Count());
	}
    /// <summary> This method prints a menu in the console and gets the choice of the user. </summary>
    /// <param name="question"> The question to print. </param>
    /// <param name="choices"> The choices of the menu. </param>
    /// <param name="line"> The line where the menu is printed. </param>
    /// <returns> The choice of the user. </returns>
    public static int ScrollingMenu(string question, string[] choices, Placement location = Placement.Center, int line = -1, bool clear = true, int delay = 1500, int posdefaut = 0, bool negative = false)
    {
        IsScreenUpdated();
        if (line == -1)
            line = ContentHeigth;

        int currentPosition = posdefaut;
        int maxLength = choices.Count() > 0 ? choices.Max(s => s.Length) : 0;
        for (int i = 0; i < choices.Length; i++) 
            choices[i] = choices[i].PadRight(maxLength);

        ContinuousPrint(question, line, negative, delay, 50);
        while (true)
        {
            string[] currentChoice = new string[choices.Length];
            for (int i = 0; i < choices.Length; i++)
            {
                if (i == currentPosition)
                {
                    currentChoice[i] = $" ▶ {choices[i]}  ";
                    WritePositionnedString(currentChoice[i], location, true, line + 2 + i);
                    continue;
                }
                currentChoice[i] = $"   {choices[i]}  ";
                WritePositionnedString(currentChoice[i], location, false, line + 2 + i);
            }
            switch (ReadKey(true).Key)
            {
                case UpArrow: case Z: 
                    if (currentPosition == 0) 
                        currentPosition = choices.Length - 1; 
                    else if (currentPosition > 0)
                        currentPosition--; 
                        break;
                case DownArrow: case S: 
                    if (currentPosition == choices.Length - 1) 
                        currentPosition = 0;  
                    else if (currentPosition < choices.Length - 1) 
                        currentPosition++; 
                        break;
                case Enter: 
                    if (clear)
                        ClearPanel(line, choices.Length + 2);
                    return currentPosition;
                case Escape:
                    if (clear) 
                        ClearPanel(line, choices.Length + 2);
                    return -1;
            }
        }
    }
    public static string ScrollingMenuString(string question, string[] choices, Placement location = Placement.Center, int line = -1)
    {
        IsScreenUpdated();
        if (line == -1)
            line = ContentHeigth;
        string[] propositions = new string[choices.Length];
        Array.Copy(choices, propositions, choices.Length);
        int currentPosition = 0;
        int maxLength = propositions.Count() > 0 ? propositions.Max(s => s.Length) : 0;
        for (int i = 0; i < propositions.Length; i++) 
            propositions[i] = propositions[i].PadRight(maxLength);

        ContinuousPrint(question, line, default, 1500, 50);
        while (true)
        {
            string[] currentChoice = new string[propositions.Length];
            for (int i = 0; i < propositions.Length; i++)
            {
                if (i == currentPosition)
                {
                    currentChoice[i] = $" ▶ {propositions[i]}  ";
                    WritePositionnedString(currentChoice[i], location, true, line + 2 + i);
                    continue;
                }
                currentChoice[i] = $"   {propositions[i]}  ";
                WritePositionnedString(currentChoice[i], location, false, line + 2 + i);
            }
            switch (ReadKey(true).Key)
            {
                case UpArrow: case Z: 
                    if (currentPosition == 0) 
                        currentPosition = propositions.Length - 1; 
                    else if (currentPosition > 0)
                        currentPosition--; 
                        break;
                case DownArrow: case S: 
                    if (currentPosition == propositions.Length - 1) 
                        currentPosition = 0;  
                    else if (currentPosition < propositions.Length - 1) 
                        currentPosition++; 
                        break;
                case Enter: 
                    ClearPanel(line, propositions.Length + 2);
                    return choices[currentPosition];
                case Escape: 
                    ClearPanel(line, propositions.Length + 2);
                    return "Retour";
            }
        }
    }
    /// <summary> This method prints a menu in the console and gets the choice of the user. </summary>
    /// <param name="question"> The question to print. </param>
    /// <param name="min"> The minimum value of the number. </param>
    /// <param name="max"> The maximum value of the number. </param>
    /// <param name="start"> The starting value of the number. </param>
    /// <param name="step"> The step of the number. </param>
    /// <param name="line"> The line where the menu is printed. </param>
    /// <returns> The number chose of the user. </returns>
    public static float NumberSelector(string question, float min, float max, float start = 0,float step = 100, int line = -1)
    {
        IsScreenUpdated();
        if (line == -1)
            line = ContentHeigth;
        float currentNumber = start;
        ContinuousPrint(question, line, default, 1500, 50);
        while (true)
        {
            WritePositionnedString($" ▶ {(float)Math.Round(currentNumber, 1)} ◀ ", Placement.Center, true, line + 2);
            
            switch (ReadKey(true).Key)
            {
                case UpArrow: case Z: 
                    if (currentNumber + step <= max)
                        currentNumber += step;
                    else if (currentNumber + step > max)
                        currentNumber = min;
                    break;
                case DownArrow: case S: 
                    if (currentNumber - step >= min)
                        currentNumber -= step;
                    else if (currentNumber - step < min)
                        currentNumber = max;
                        break;
                case Enter: 
                    ClearPanel(line, 4);
                    return currentNumber;
                case Escape: 
                    ClearPanel(line, 4);
                    return -1;
            }
            Sleep(1);
            ClearLine(line +2);
        }
    }
    
    /// <summary> This method prints a loading screen in the console. </summary>
    /// <param name="text"> The text to print. </param>
    public static void LoadingScreen(string text)
    {
        if(!IsScreenUpdated())
            ClearPanel(ContentHeigth, 3);
        WritePositionnedString(text.BuildString(WindowWidth, Placement.Center), default, default, ContentHeigth, true);
        string loadingBar = "";
            for(int j = 0; j < text.Length; j++) 
                loadingBar += '█';
        ContinuousPrint(loadingBar, ContentHeigth + 2);
    }
    /// <summary> This method exits the program. </summary>
    public static void ProgramExit()
    {
        LoadingScreen("[ Sortie du programme ... ]");
        ClearAll();
        CursorVisible = true;
        Environment.Exit(0);
    }
    #endregion

    #region Extensions  
    /// <summary> This method builds a string with a specific size and a specific placement. </summary>
    /// <param name="str"> The string to build. </param>
    /// <param name="size"> The size of the string. </param>
    /// <param name="position"> The placement of the string. </param>
    /// <param name="truncate"> If true, the string is truncated if it is too long. </param>
    /// <returns> The built string. </returns>
    public static string BuildString(this string str, int size, Placement position = Placement.Center, bool truncate = true)
	{
		int padding = size - str.Length;
        if (truncate && padding < 0) 
            switch (position)
		    {
		    	case (Placement.Left): 
                    return str.Substring(0, size);
		    	case (Placement.Center): 
                    return str.Substring((- padding) / 2, size);
		    	case (Placement.Right): 
                    return str.Substring(- padding, size);
		    }
        else 
		    switch (position)
		    {
		    	case (Placement.Left):
		    		return str.PadRight(size);
		    	case (Placement.Center):
		    		return str.PadLeft(padding / 2 + padding % 2 + str.Length).PadRight(padding + str.Length);
		    	case (Placement.Right):
		    		return str.PadLeft(size);
		    }
		return str;
	}
    #endregion
}