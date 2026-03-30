using SnakeEvolution.Core.Models;

namespace SnakeEvolution.Core.Game
{
    public class Snake
    {
        private readonly List<Position> _segments = [];

        public IReadOnlyList<Position> Segments => _segments;
        public Position Head => _segments[0];
        public Direction Direction { get; private set; }

        public Snake(Position head, Direction direction, int initialLength = 3)
        {
            Direction = direction;
            _segments.Add(head);

            Position tailDelta = direction switch
            {
                Direction.Up => new Position(0, 1),
                Direction.Down => new Position(0, -1),
                Direction.Left => new Position(1, 0),
                _ => new Position(-1, 0)
            };

            for (int i = 1; i < initialLength; i++)
            {
                Position previous = _segments[i - 1];
                _segments.Add(new Position(previous.x + tailDelta.x, previous.y + tailDelta.y));
            }
        }

        public void SetDirection(Direction newDirection)
        {
            bool isOpposite = (Direction == Direction.Up && newDirection == Direction.Down)
                || (Direction == Direction.Down && newDirection == Direction.Up)
                || (Direction == Direction.Left && newDirection == Direction.Right)
                || (Direction == Direction.Right && newDirection == Direction.Left);

            if (!isOpposite)
            {
                Direction = newDirection;
            }
        }

        public Position GetNextHeadPosition()
        {
            return Direction switch
            {
                Direction.Up => new Position(Head.x, Head.y - 1),
                Direction.Down => new Position(Head.x, Head.y + 1),
                Direction.Left => new Position(Head.x - 1, Head.y),
                _ => new Position(Head.x + 1, Head.y)
            };
        }

        public void Move(bool grow = false)
        {
            _segments.Insert(0, GetNextHeadPosition());

            if (!grow)
            {
                _segments.RemoveAt(_segments.Count - 1);
            }
        }

        public bool Occupies(Position position)
        {
            return _segments.Contains(position);
        }

        public bool IsBody(Position position)
        {
            for (int i = 1; i < _segments.Count; i++)
            {
                if (_segments[i] == position)
                {
                    return true;
                }
            }

            return false;
        }

        public bool WouldCollide(Position nextPosition, bool grow)
        {
            int checkCount = grow ? _segments.Count : _segments.Count - 1;

            for (int i = 0; i < checkCount; i++)
            {
                if (_segments[i] == nextPosition)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
