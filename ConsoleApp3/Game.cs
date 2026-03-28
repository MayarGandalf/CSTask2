using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

class Game
{
    private Dictionary<string, string> messages;

    private Player player1;
    private Player player2;
    private Player currentPlayer;
    private Player otherPlayer;

    private List<string> usedWords = new List<string>();
    private string baseWord;
    private int timer;

    public void Start()
    {
        messages = ChooseLanguage();

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

    private void Print(string text = "") => Console.WriteLine(text);

    private string Read() => Console.ReadLine()?.Trim() ?? "";

    private Dictionary<string, string> ChooseLanguage()
    {
        while (true)
        {
            Print("Choose language / Выберите язык:");
            Print("1 - English");
            Print("2 - Русский");

            string choice = Read();

            if (choice == "1") return CreateEnglish();
            if (choice == "2") return CreateRussian();

            Print("Invalid choice / Неверный выбор");
        }
    }

    private Dictionary<string, string> CreateEnglish() => new Dictionary<string, string>
    {
        {"welcome","Words Game"},
        {"enterName","Enter name for Player {0}:"},
        {"seconds","Seconds per move:"},
        {"invalidSeconds","Invalid number"},
        {"enterBase","Enter base word (8-30 letters):"},
        {"invalidBase","Invalid word"},
        {"turn","{0}'s turn"},
        {"timeUp","{0} ran out of time!"},
        {"invalidWord","Invalid word!"},
        {"winner","Winner: {0}"},
        {"used","Used words:"},
        {"gameOver","Game over"}
    };

    private Dictionary<string, string> CreateRussian() => new Dictionary<string, string>
    {
        {"welcome","Игра Слова"},
        {"enterName","Введите имя игрока {0}:"},
        {"seconds","Секунд на ход:"},
        {"invalidSeconds","Неверное число"},
        {"enterBase","Введите базовое слово (8-30 букв):"},
        {"invalidBase","Неверное слово"},
        {"turn","Ход игрока {0}"},
        {"timeUp","{0} не успел!"},
        {"invalidWord","Неверное слово!"},
        {"winner","Победитель: {0}"},
        {"used","Использованные слова:"},
        {"gameOver","Игра окончена"}
    };

    private string AskName(int n)
    {
        Print(string.Format(messages["enterName"], n));
        return Read();
    }

    private int AskTimer()
    {
        while (true)
        {
            Print(messages["seconds"]);
            if (int.TryParse(Read(), out int t) && t > 0) return t;
            Print(messages["invalidSeconds"]);
        }
    }

    private string AskBaseWord()
    {
        while (true)
        {
            Print(messages["enterBase"]);
            string w = Read().ToLower();

            if (w.Length >= 8 && w.Length <= 30 && w.All(char.IsLetter)) return w;

            Print(messages["invalidBase"]);
        }
    }

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

    private void SwapPlayers()
    {
        var t = currentPlayer;
        currentPlayer = otherPlayer;
        otherPlayer = t;
    }

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

    private void HandleCommand(string cmd)
    {
        if (cmd == "/show-words")
        {
            Print(messages["used"]);
            foreach (var w in usedWords)
                Print(w);
        }
        else if (cmd == "/score")
        {
            var data = ScoreManager.Load();

            int p1 = data.Results.Count(r => r.Winner == player1.Name);
            int p2 = data.Results.Count(r => r.Winner == player2.Name);

            Print($"{player1.Name}: {p1}");
            Print($"{player2.Name}: {p2}");
        }
        else if (cmd == "/total-score")
        {
            var data = ScoreManager.Load();

            var grouped = data.Results
                .GroupBy(r => r.Winner)
                .Select(g => new { Name = g.Key, Wins = g.Count() });

            foreach (var g in grouped)
                Print($"{g.Name}: {g.Wins}");
        }
    }

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