using System.Linq;

/// <summary>
/// Displays the head-to-head score between the two current players.
/// </summary>
class ShowScoreCommand : ICommandHandler
{
    public void Execute(Game game)
    {
        var data = ScoreManager.Load();

        int p1 = data.Results.Count(r => r.Winner == game.player1.Name);
        int p2 = data.Results.Count(r => r.Winner == game.player2.Name);

        game.Print($"{game.player1.Name}: {p1}");
        game.Print($"{game.player2.Name}: {p2}");
    }
}