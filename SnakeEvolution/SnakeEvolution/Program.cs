using SnakeEvolution.Core.Game;
using SnakeEvolution.Core.Models;

int width = 30;
int height = 15;
int tickMs = 170;

Snake snake = new Snake(new Position(width / 2, height / 2), Direction.Right, 3);
Random random = new Random();
Position food = SpawnFood(width, height, snake, random);
int score = 0;
bool gameOver = false;

Console.CursorVisible = false;
Render(width, height, snake, food, score);

while (!gameOver)
{
    HandleInput(snake);

    Position nextHead = snake.GetNextHeadPosition();
    bool willGrow = nextHead == food;

    bool hitsWall = nextHead.x < 0 || nextHead.x >= width || nextHead.y < 0 || nextHead.y >= height;
    bool hitsSelf = snake.WouldCollide(nextHead, willGrow);

    if (hitsWall || hitsSelf)
    {
        gameOver = true;
        continue;
    }

    snake.Move(willGrow);

    if (willGrow)
    {
        score++;
        food = SpawnFood(width, height, snake, random);
    }

    Render(width, height, snake, food, score);
    Thread.Sleep(tickMs);
}

Console.SetCursorPosition(0, height + 3);
Console.WriteLine($"Game Over. Punteggio finale: {score}");
Console.WriteLine("Premi un tasto per uscire...");
Console.ReadKey(true);

static void HandleInput(Snake snake)
{
    while (Console.KeyAvailable)
    {
        ConsoleKey key = Console.ReadKey(true).Key;

        if (key == ConsoleKey.UpArrow || key == ConsoleKey.W)
        {
            snake.SetDirection(Direction.Up);
        }
        else if (key == ConsoleKey.DownArrow || key == ConsoleKey.S)
        {
            snake.SetDirection(Direction.Down);
        }
        else if (key == ConsoleKey.LeftArrow || key == ConsoleKey.A)
        {
            snake.SetDirection(Direction.Left);
        }
        else if (key == ConsoleKey.RightArrow || key == ConsoleKey.D)
        {
            snake.SetDirection(Direction.Right);
        }
    }
}

static Position SpawnFood(int width, int height, Snake snake, Random random)
{
    Position candidate;

    do
    {
        candidate = new Position(random.Next(0, width), random.Next(0, height));
    }
    while (snake.Occupies(candidate));

    return candidate;
}

static void Render(int width, int height, Snake snake, Position food, int score)
{
    Console.SetCursorPosition(0, 0);
    Console.WriteLine($"Punteggio: {score}".PadRight(width + 2));

    for (int y = 0; y < height; y++)
    {
        for (int x = 0; x < width; x++)
        {
            Position current = new Position(x, y);

            if (current == snake.Head)
            {
                Console.Write('O');
            }
            else if (snake.IsBody(current))
            {
                Console.Write('o');
            }
            else if (current == food)
            {
                Console.Write('*');
            }
            else
            {
                Console.Write('.');
            }
        }

        Console.WriteLine();
    }
}
