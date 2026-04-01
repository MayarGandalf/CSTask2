using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Основной класс игры «Слова». Управляет ходами, проверкой слов, таймером и завершением игры.
/// </summary>
class Game
{
    // Словарь локализованных сообщений (русский/английский)
    private Dictionary<string, string> messages;

    private Player player1;
    private Player player2;
    private Player currentPlayer; // игрок, совершающий ход
    private Player otherPlayer;    // второй игрок

    private List<string> usedWords = new List<string>(); // использованные за игру слова
    private string baseWord;       // базовое слово, из букв которого составляются ответы
    private int timer;             // время на ход в секундах

    /// <summary>
    /// Запускает игру: выбор языка, ввод имён, таймер, базовое слово, игровой цикл.
    /// </summary>
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

    /// <summary>Выводит строку на консоль.</summary>
    private void Print(string text = "") => Console.WriteLine(text);

    /// <summary>Считывает строку с консоли, удаляя лишние пробелы.</summary>
    private string Read() => Console.ReadLine()?.Trim() ?? "";

    /// <summary>Предлагает пользователю выбрать язык и возвращает соответствующий словарь сообщений.</summary>
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

    /// <summary>Возвращает словарь сообщений на английском языке.</summary>
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

    /// <summary>Возвращает словарь сообщений на русском языке.</summary>
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

    /// <summary>Запрашивает имя игрока.</summary>
    /// <param name="n">Номер игрока (1 или 2).</param>
    private string AskName(int n)
    {
        Print(string.Format(messages["enterName"], n));
        return Read();
    }

    /// <summary>Запрашивает время на ход в секундах (положительное целое число).</summary>
    private int AskTimer()
    {
        while (true)
        {
            Print(messages["seconds"]);
            if (int.TryParse(Read(), out int t) && t > 0) return t;
            Print(messages["invalidSeconds"]);
        }
    }

    /// <summary>Запрашивает базовое слово (длиной от 8 до 30 букв).</summary>
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

    /// <summary>Основной игровой цикл. Пока игра не закончена, обрабатывает ходы игроков.</summary>
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

    /// <summary>Считывает слово с консоли с ограничением по времени.</summary>
    /// <returns>Введённое слово в нижнем регистре или null, если время истекло.</returns>
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

    /// <summary>Проверяет, является ли слово допустимым по правилам игры.</summary>
    /// <param name="word">Проверяемое слово.</param>
    /// <returns>true, если слово не пустое, не использовалось ранее и может быть составлено из букв базового слова.</returns>
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

    /// <summary>Меняет местами текущего и другого игрока.</summary>
    private void SwapPlayers()
    {
        var t = currentPlayer;
        currentPlayer = otherPlayer;
        otherPlayer = t;
    }

    /// <summary>Завершает игру, объявляет победителя и сохраняет результат.</summary>
    /// <param name="winner">Игрок-победитель.</param>
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

    /// <summary>Обрабатывает команды, введённые с префиксом '/'.</summary>
    /// <param name="cmd">Команда (например, /show-words, /score, /total-score).</param>
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

    /// <summary>Обработчик нажатия Ctrl+C. Сохраняет результат как поражение текущего игрока и завершает приложение.</summary>
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