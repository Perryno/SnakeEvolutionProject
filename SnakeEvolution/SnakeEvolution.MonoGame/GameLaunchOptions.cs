namespace SnakeEvolution.MonoGame
{
    public class GameLaunchOptions
    {
        public string PolicyJsonPath { get; init; } = string.Empty;
        public string EpisodeLabel { get; init; } = "N/A";
        public bool AutoExitOnGameOver { get; init; }
    }
}
