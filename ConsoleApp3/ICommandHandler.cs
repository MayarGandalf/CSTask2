/// <summary>
/// Defines a command that can be executed within the game context.
/// </summary>
interface ICommandHandler
{
    /// <summary>Executes the command logic using the provided game instance.</summary>
    void Execute(Game game);
}