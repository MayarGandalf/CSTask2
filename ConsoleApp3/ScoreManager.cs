using System.Text.Json;
using System.IO;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Контейнер для всех сохранённых результатов игр.
/// </summary>
class ScoreData
{
    public List<GameResult> Results { get; set; } = new List<GameResult>();
}

/// <summary>
/// Отвечает за загрузку и сохранение статистики игр в файл Data/scores.json.
/// </summary>
static class ScoreManager
{
    
    private const string FilePath = "Data/scores.json";

    /// <summary>Загружает все сохранённые результаты из файла.</summary>
    public static ScoreData Load()
    {
        if (!File.Exists(FilePath)) return new ScoreData();
        string json = File.ReadAllText(FilePath);
        return JsonSerializer.Deserialize<ScoreData>(json) ?? new ScoreData();
    }

    /// <summary>Добавляет результат одной игры в файл статистики.</summary>
    public static void Save(GameResult result)
    {
        // Создаём папку Data, если её нет
        var directory = Path.GetDirectoryName(FilePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        var data = Load();
        data.Results.Add(result);
        string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(FilePath, json);
    }
}