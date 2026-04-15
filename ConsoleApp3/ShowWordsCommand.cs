/// <summary>
/// Displays all words used so far in the current game.
/// </summary>
class ShowWordsCommand : ICommandHandler
{
    public void Execute(Game game)
    {
        game.Print(game.messages["used"]);
        foreach (var w in game.usedWords)
            game.Print(w);
    }
}