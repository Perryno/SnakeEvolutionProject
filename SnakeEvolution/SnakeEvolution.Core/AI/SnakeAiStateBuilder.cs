using SnakeEvolution.Core.Game;
using SnakeEvolution.Core.Models;

namespace SnakeEvolution.Core.AI
{
    public static class SnakeAiStateBuilder
    {
        public static string BuildStateKey(
            Snake snake,
            Position food,
            int gridWidth,
            int gridHeight)
        {
            Position head = snake.Head;
            Direction direction = snake.Direction;

            bool movingUp = direction == Direction.Up;
            bool movingDown = direction == Direction.Down;
            bool movingLeft = direction == Direction.Left;
            bool movingRight = direction == Direction.Right;

            Position posStraight;
            Position posLeft;
            Position posRight;

            if (movingUp)
            {
                posStraight = new Position(head.x, head.y - 1);
                posLeft = new Position(head.x - 1, head.y);
                posRight = new Position(head.x + 1, head.y);
            }
            else if (movingDown)
            {
                posStraight = new Position(head.x, head.y + 1);
                posLeft = new Position(head.x + 1, head.y);
                posRight = new Position(head.x - 1, head.y);
            }
            else if (movingLeft)
            {
                posStraight = new Position(head.x - 1, head.y);
                posLeft = new Position(head.x, head.y + 1);
                posRight = new Position(head.x, head.y - 1);
            }
            else
            {
                posStraight = new Position(head.x + 1, head.y);
                posLeft = new Position(head.x, head.y - 1);
                posRight = new Position(head.x, head.y + 1);
            }

            int dangerStraight = WouldCollide(snake, posStraight, gridWidth, gridHeight) ? 1 : 0;
            int dangerLeft = WouldCollide(snake, posLeft, gridWidth, gridHeight) ? 1 : 0;
            int dangerRight = WouldCollide(snake, posRight, gridWidth, gridHeight) ? 1 : 0;

            int foodUp = food.y < head.y ? 1 : 0;
            int foodDown = food.y > head.y ? 1 : 0;
            int foodLeft = food.x < head.x ? 1 : 0;
            int foodRight = food.x > head.x ? 1 : 0;

            return string.Join(",",
                dangerStraight,
                dangerLeft,
                dangerRight,
                movingUp ? 1 : 0,
                movingDown ? 1 : 0,
                movingLeft ? 1 : 0,
                movingRight ? 1 : 0,
                foodUp,
                foodDown,
                foodLeft,
                foodRight);
        }

        private static bool WouldCollide(
            Snake snake,
            Position nextPosition,
            int gridWidth,
            int gridHeight)
        {
            if (nextPosition.x < 0 || nextPosition.x >= gridWidth)
            {
                return true;
            }

            if (nextPosition.y < 0 || nextPosition.y >= gridHeight)
            {
                return true;
            }

            var segments = snake.Segments;

            for (int i = 0; i < segments.Count - 1; i++)
            {
                if (segments[i] == nextPosition)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
