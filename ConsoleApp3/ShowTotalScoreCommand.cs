using System.Linq;

/// <summary>
/// Displays the total wins for all players ever recorded.
/// </summary>
class ShowTotalScoreCommand : ICommandHandler
{
    public void Execute(Game game)
    {
        var data = ScoreManager.Load();

        var grouped = data.Results
            .GroupBy(r => r.Winner)
            .Select(g => new { Name = g.Key, Wins = g.Count() });

        foreach (var g in grouped)
            game.Print($"{g.Name}: {g.Wins}");
    }
}