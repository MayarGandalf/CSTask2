using System.Text.Json;
using System.IO;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Container for all saved game results.
/// </summary>
class ScoreData
{
    public List<GameResult> Results { get; set; } = new List<GameResult>();
}

/// <summary>
/// Responsible for loading and saving game statistics to the file Data/scores.json.
/// </summary>
static class ScoreManager
{
    private const string FilePath = "Data/scores.json";

    /// <summary>Loads all saved results from the file.</summary>
    public static ScoreData Load()
    {
        if (!File.Exists(FilePath)) return new ScoreData();
        string json = File.ReadAllText(FilePath);
        return JsonSerializer.Deserialize<ScoreData>(json) ?? new ScoreData();
    }

    /// <summary>Adds the result of a single game to the statistics file.</summary>
    public static void Save(GameResult result)
    {
        // Create the Data directory if it does not exist
        var directory = Path.GetDirectoryName(FilePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        var data = Load();
        data.Results.Add(result);
        string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(FilePath, json);
    }
}