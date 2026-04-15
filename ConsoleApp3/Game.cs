using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Main game class for the "Words" game. Manages turns, word validation, timer, and game over.
/// </summary>
class Game
{
    // Constants for base word length restrictions
    private const int MinBaseWordLength = 8;
    private const int MaxBaseWordLength = 30;

    // Predefined message dictionaries (created only once)
    private static readonly Dictionary<string, string> EnglishMessages = new Dictionary<string, string>
    {
        {"welcome","Words Game"},
        {"enterName","Enter name for Player {0}:"},
        {"seconds","Seconds per move:"},
        {"invalidSeconds","Invalid number"},
        {"enterBase",$"Enter base word ({MinBaseWordLength}-{MaxBaseWordLength} letters):"},
        {"invalidBase","Invalid word"},
        {"turn","{0}'s turn"},
        {"timeUp","{0} ran out of time!"},
        {"invalidWord","Invalid word!"},
        {"winner","Winner: {0}"},
        {"used","Used words:"},
        {"gameOver","Game over"}
    };

    private static readonly Dictionary<string, string> RussianMessages = new Dictionary<string, string>
    {
        {"welcome","Игра Слова"},
        {"enterName","Введите имя игрока {0}:"},
        {"seconds","Секунд на ход:"},
        {"invalidSeconds","Неверное число"},
        {"enterBase",$"Введите базовое слово ({MinBaseWordLength}-{MaxBaseWordLength} букв):"},
        {"invalidBase","Неверное слово"},
        {"turn","Ход игрока {0}"},
        {"timeUp","{0} не успел!"},
        {"invalidWord","Неверное слово!"},
        {"winner","Победитель: {0}"},
        {"used","Использованные слова:"},
        {"gameOver","Игра окончена"}
    };

    // Dictionary of localized messages currently in use
    internal Dictionary<string, string> messages;

    internal Player player1;
    internal Player player2;
    private Player currentPlayer; // player whose turn it is
    private Player otherPlayer;   // the other player

    internal List<string> usedWords = new List<string>(); // words used during the game
    private string baseWord;       // base word from which letters answers are composed
    private int timer;             // time per turn in seconds

    // Command handlers using full Strategy pattern
    private Dictionary<string, ICommandHandler> commandHandlers;

    /// <summary>
    /// Starts the game: language selection, name input, timer, base word, game loop.
    /// </summary>
    public void Start()
    {
        messages = ChooseLanguage();
        InitializeCommands();

        Print(messages["welcome"]);

        player1 = new Player { Name = AskName(1) };
        player2 = new Player { Name = AskName(2) };

        timer = AskTimer();
        baseWord = AskBaseWord();

        currentPlayer = player1;
        otherPlayer = player2;

        Console.CancelKeyPress += OnExit;

        Run();

        Print(messages["gameOver"]);
    }

    /// <summary>Initializes the dictionary that maps command strings to strategy objects.</summary>
    private void InitializeCommands()
    {
        commandHandlers = new Dictionary<string, ICommandHandler>
        {
            ["/show-words"] = new ShowWordsCommand(),
            ["/score"] = new ShowScoreCommand(),
            ["/total-score"] = new ShowTotalScoreCommand()
        };
    }

    /// <summary>Prints a string to the console.</summary>
    internal void Print(string text = "") => Console.WriteLine(text);

    /// <summary>Reads a trimmed string from the console.</summary>
    private string Read() => Console.ReadLine()?.Trim() ?? "";

    /// <summary>Asks the user to choose a language and returns the corresponding message dictionary.</summary>
    private Dictionary<string, string> ChooseLanguage()
    {
        while (true)
        {
            Print("Choose language / Выберите язык:");
            Print("1 - English");
            Print("2 - Русский");

            string choice = Read();

            if (choice == "1") return EnglishMessages;
            if (choice == "2") return RussianMessages;

            Print("Invalid choice / Неверный выбор");
        }
    }

    /// <summary>Asks for a player's name.</summary>
    /// <param name="n">Player number (1 or 2).</param>
    private string AskName(int n)
    {
        Print(string.Format(messages["enterName"], n));
        return Read();
    }

    /// <summary>Asks for the time per turn in seconds (positive integer).</summary>
    private int AskTimer()
    {
        while (true)
        {
            Print(messages["seconds"]);
            if (int.TryParse(Read(), out int t) && t > 0) return t;
            Print(messages["invalidSeconds"]);
        }
    }

    /// <summary>Asks for a base word (length between defined constants).</summary>
    private string AskBaseWord()
    {
        while (true)
        {
            Print(messages["enterBase"]);
            string w = Read().ToLower();

            if (w.Length >= MinBaseWordLength && w.Length <= MaxBaseWordLength && w.All(char.IsLetter)) return w;

            Print(messages["invalidBase"]);
        }
    }

    /// <summary>Main game loop. Processes player turns until the game ends.</summary>
    private void Run()
    {
        while (true)
        {
            Print();
            Print(string.Format(messages["turn"], currentPlayer.Name));

            string input = GetWordWithTimer();

            if (input == null)
            {
                Print(string.Format(messages["timeUp"], currentPlayer.Name));
                EndGame(otherPlayer);
                break;
            }

            if (input.StartsWith("/"))
            {
                HandleCommand(input);
                continue;
            }

            if (!IsValid(input))
            {
                Print(messages["invalidWord"]);
                EndGame(otherPlayer);
                break;
            }

            usedWords.Add(input);
            SwapPlayers();
        }
    }

    /// <summary>Reads a word from the console with a time limit.</summary>
    /// <returns>The entered word in lowercase, or null if time expired.</returns>
    private string GetWordWithTimer()
    {
        var tcs = new TaskCompletionSource<bool>();

        using (var t = new Timer(_ => tcs.TrySetResult(true), null, timer * 1000, Timeout.Infinite))
        {
            var task = Task.Run(() => Console.ReadLine());
            int index = Task.WaitAny(task, tcs.Task);
            return index == 0 ? task.Result?.Trim().ToLower() : null;
        }
    }

    /// <summary>Checks whether a word is valid according to game rules.</summary>
    /// <param name="word">The word to check.</param>
    /// <returns>true if the word is not empty, hasn't been used before, and can be formed from the base word's letters.</returns>
    private bool IsValid(string word)
    {
        if (string.IsNullOrWhiteSpace(word)) return false;
        if (usedWords.Contains(word)) return false;

        var letters = baseWord.ToList();

        foreach (char c in word)
        {
            if (!letters.Contains(c)) return false;
            letters.Remove(c);
        }

        return true;
    }

    /// <summary>Swaps the current and the other player.</summary>
    private void SwapPlayers()
    {
        var t = currentPlayer;
        currentPlayer = otherPlayer;
        otherPlayer = t;
    }

    /// <summary>Ends the game, announces the winner, and saves the result.</summary>
    /// <param name="winner">The winning player.</param>
    private void EndGame(Player winner)
    {
        Print(string.Format(messages["winner"], winner.Name));

        ScoreManager.Save(new GameResult
        {
            Player1 = player1.Name,
            Player2 = player2.Name,
            Winner = winner.Name
        });
    }

    /// <summary>Handles commands entered with a '/' prefix using the Strategy pattern.</summary>
    /// <param name="cmd">Command string (e.g., "/show-words").</param>
    private void HandleCommand(string cmd)
    {
        if (commandHandlers.TryGetValue(cmd, out ICommandHandler handler))
        {
            handler.Execute(this);
        }
        // Unknown commands are silently ignored
    }

    /// <summary>Handler for Ctrl+C press. Saves the result as a loss for the current player and exits.</summary>
    private void OnExit(object sender, ConsoleCancelEventArgs e)
    {
        e.Cancel = true;

        Print("Game interrupted!");

        ScoreManager.Save(new GameResult
        {
            Player1 = player1.Name,
            Player2 = player2.Name,
            Winner = otherPlayer.Name
        });

        Environment.Exit(0);
    }
}