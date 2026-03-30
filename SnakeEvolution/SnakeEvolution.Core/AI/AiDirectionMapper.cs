using SnakeEvolution.Core.Models;

namespace SnakeEvolution.Core.AI
{
    public static class AiDirectionMapper
    {
        public static Direction? ParseAction(string? action)
        {
            return action switch
            {
                "UP" => Direction.Up,
                "DOWN" => Direction.Down,
                "LEFT" => Direction.Left,
                "RIGHT" => Direction.Right,
                _ => null
            };
        }
    }
}
