// score_manager.cs
using System.Text.Json;
using System.IO;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Контейнер для всех сохранённых результатов игр.
/// </summary>
class ScoreData
{
    /// <summary>Список завершённых игр.</summary>
    public List<GameResult> Results { get; set; } = new List<GameResult>();
}

/// <summary>
/// Отвечает за загрузку и сохранение статистики игр в файл scores.json.
/// </summary>
static class ScoreManager
{
    private const string FilePath = "scores.json";

    /// <summary>Загружает все сохранённые результаты из файла.</summary>
    /// <returns>Объект ScoreData с имеющимися данными (если файла нет — возвращается пустой список).</returns>
    public static ScoreData Load()
    {
        if (!File.Exists(FilePath)) return new ScoreData();
        string json = File.ReadAllText(FilePath);
        return JsonSerializer.Deserialize<ScoreData>(json) ?? new ScoreData();
    }

    /// <summary>Добавляет результат одной игры в файл статистики.</summary>
    /// <param name="result">Результат игры, который нужно сохранить.</param>
    public static void Save(GameResult result)
    {
        var data = Load();
        data.Results.Add(result);
        string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(FilePath, json);
    }
}