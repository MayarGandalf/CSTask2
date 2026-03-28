using System.Text.Json;
using System.IO;
using System.Collections.Generic;
using System.Linq;

class ScoreData
{
    public List<GameResult> Results { get; set; } = new List<GameResult>();
}

class ScoreManager
{
    private const string FilePath = "scores.json";

    public static ScoreData Load()
    {
        if (!File.Exists(FilePath)) return new ScoreData();
        string json = File.ReadAllText(FilePath);
        return JsonSerializer.Deserialize<ScoreData>(json) ?? new ScoreData();
    }

    public static void Save(GameResult result)
    {
        var data = Load();
        data.Results.Add(result);
        string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(FilePath, json);
    }
}
