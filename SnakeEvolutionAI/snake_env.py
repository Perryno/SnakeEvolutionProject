import random
from dataclasses import dataclass
from enum import Enum

class Direction(Enum):
    UP = 0
    DOWN = 1
    LEFT = 2
    RIGHT = 3


@dataclass(frozen=True)
class Position:
    x: int
    y: int


class SnakeEnv:
    def __init__(self, width: int = 15, height: int = 10, seed=None):
        self.width = width
        self.height = height
        self.rng = random.Random(seed)

        self.snake = []
        self.direction = Direction.RIGHT
        self.food = None
        self.score = 0
        self.done = False

    def reset(self):
        center_x = self.width // 2
        center_y = self.height // 2

        self.snake = [
            Position(center_x, center_y),
            Position(center_x - 1, center_y),
            Position(center_x - 2, center_y),
        ]
        self.direction = Direction.RIGHT
        self.score = 0
        self.done = False

        self._spawn_food()

        return self.get_state()
    
    def _spawn_food(self):
        while True:
            candidate = Position(
                self.rng.randint(0, self.width - 1),
                self.rng.randint(0, self.height - 1)
            )

            if candidate not in self.snake:
                self.food = candidate
                break

    def get_next_head_position(self):
        head = self.snake[0]

        if self.direction == Direction.UP:
            return Position(head.x, head.y - 1)

        elif self.direction == Direction.DOWN:
            return Position(head.x, head.y + 1)

        elif self.direction == Direction.LEFT:
            return Position(head.x - 1, head.y)

        elif self.direction == Direction.RIGHT:
            return Position(head.x + 1, head.y)
    
    def move(self, grow=False):
        new_head = self.get_next_head_position()
        self.snake.insert(0, new_head)

        if not grow:
            self.snake.pop()
    
    def would_collide(self, next_position, grow=False):
    # collisione con i muri
        if next_position.x < 0 or next_position.x >= self.width:
            return True

        if next_position.y < 0 or next_position.y >= self.height:
            return True

        # collisione con il corpo
        body_to_check = self.snake if grow else self.snake[:-1]

        if next_position in body_to_check:
            return True

        return False
    
    def set_direction(self, new_direction):
        opposite_directions = {
            Direction.UP: Direction.DOWN,
            Direction.DOWN: Direction.UP,
            Direction.LEFT: Direction.RIGHT,
            Direction.RIGHT: Direction.LEFT,
        }

        if new_direction != opposite_directions[self.direction]:
            self.direction = new_direction

    def step(self, action):
        if self.done:
            return self.get_state(), 0, self.done

        self.set_direction(action)

        next_head = self.get_next_head_position()
        will_eat = next_head == self.food

        if self.would_collide(next_head, grow=will_eat):
            self.done = True
            reward = -10
            return self.get_state(), reward, self.done

        if will_eat:
            self.move(grow=True)
            self.score += 1
            self._spawn_food()
            reward = 10
        else:
            self.move(grow=False)
            reward = -0.1

        return self.get_state(), reward, self.done
    
    def get_state(self):
        head = self.snake[0]

        # direzione attuale
        moving_up = self.direction == Direction.UP
        moving_down = self.direction == Direction.DOWN
        moving_left = self.direction == Direction.LEFT
        moving_right = self.direction == Direction.RIGHT

        # posizioni relative: davanti, sinistra, destra
        if moving_up:
            pos_straight = Position(head.x, head.y - 1)
            pos_left = Position(head.x - 1, head.y)
            pos_right = Position(head.x + 1, head.y)

        elif moving_down:
            pos_straight = Position(head.x, head.y + 1)
            pos_left = Position(head.x + 1, head.y)
            pos_right = Position(head.x - 1, head.y)

        elif moving_left:
            pos_straight = Position(head.x - 1, head.y)
            pos_left = Position(head.x, head.y + 1)
            pos_right = Position(head.x, head.y - 1)

        elif moving_right:
            pos_straight = Position(head.x + 1, head.y)
            pos_left = Position(head.x, head.y - 1)
            pos_right = Position(head.x, head.y + 1)

        danger_straight = self.would_collide(pos_straight)
        danger_left = self.would_collide(pos_left)
        danger_right = self.would_collide(pos_right)

        food_up = self.food.y < head.y
        food_down = self.food.y > head.y
        food_left = self.food.x < head.x
        food_right = self.food.x > head.x

        return (
            int(danger_straight),
            int(danger_left),
            int(danger_right),
            int(moving_up),
            int(moving_down),
            int(moving_left),
            int(moving_right),
            int(food_up),
            int(food_down),
            int(food_left),
            int(food_right),
        )
    
    def render(self):
        grid = [["." for _ in range(self.width)] for _ in range(self.height)]

        for segment in self.snake[1:]:
            grid[segment.y][segment.x] = "S"

        head = self.snake[0]
        grid[head.y][head.x] = "H"

        if self.food is not None:
            grid[self.food.y][self.food.x] = "F"

        for row in grid:
            print(" ".join(row))

        print(f"Score: {self.score}")
        print(f"Direction: {self.direction}")
        print("-" * 30)

if __name__ == "__main__":
    env = SnakeEnv()
    env.reset()

    print("Snake:", env.snake)
    print("Direction:", env.direction)
    print("Food:", env.food)
    print("State:", env.get_state())
