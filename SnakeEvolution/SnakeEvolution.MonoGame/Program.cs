using SnakeEvolution.MonoGame;

GameLaunchOptions options = ParseArgs(args);

using var game = new Game1(options);
game.Run();

static GameLaunchOptions ParseArgs(string[] args)
{
    string policyJsonPath = string.Empty;
    string episodeLabel = "N/A";
    bool autoExitOnGameOver = false;

    for (int i = 0; i < args.Length; i++)
    {
        string arg = args[i];

        if (arg == "--policy-json" && i + 1 < args.Length)
        {
            policyJsonPath = args[++i];
        }
        else if (arg == "--episode-label" && i + 1 < args.Length)
        {
            episodeLabel = args[++i];
        }
        else if (arg == "--auto-exit")
        {
            autoExitOnGameOver = true;
        }
    }

    return new GameLaunchOptions
    {
        PolicyJsonPath = policyJsonPath,
        EpisodeLabel = episodeLabel,
        AutoExitOnGameOver = autoExitOnGameOver
    };
}
