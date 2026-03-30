import random
import pickle
import json
from collections import defaultdict
from snake_env import Direction


class QLearningAgent:
    def __init__(
        self,
        alpha=0.1,
        gamma=0.9,
        epsilon=1.0,
        epsilon_decay=0.999,
        epsilon_min=0.01
    ):
        self.alpha = alpha
        self.gamma = gamma
        self.epsilon = epsilon
        self.epsilon_decay = epsilon_decay
        self.epsilon_min = epsilon_min

        self.actions = [
            Direction.UP,
            Direction.DOWN,
            Direction.LEFT,
            Direction.RIGHT,
        ]

        self.q_table = defaultdict(lambda: {action: 0.0 for action in self.actions})

    def choose_action(self, state):
        if random.random() < self.epsilon:
            return random.choice(self.actions)

        state_actions = self.q_table[state]
        return max(state_actions, key=state_actions.get)

    def update_q_value(self, state, action, reward, next_state, done):
        current_q = self.q_table[state][action]

        if done:
            max_future_q = 0
        else:
            max_future_q = max(self.q_table[next_state].values())

        target = reward + self.gamma * max_future_q
        new_q = current_q + self.alpha * (target - current_q)

        self.q_table[state][action] = new_q

    def decay_epsilon(self):
        if self.epsilon > self.epsilon_min:
            self.epsilon *= self.epsilon_decay

            if self.epsilon < self.epsilon_min:
                self.epsilon = self.epsilon_min


    def save(self, file_path):
        data = {
            "q_table": dict(self.q_table),
            "alpha": self.alpha,
            "gamma": self.gamma,
            "epsilon": self.epsilon,
            "epsilon_decay": self.epsilon_decay,
            "epsilon_min": self.epsilon_min,
        }

        with open(file_path, "wb") as f:
            pickle.dump(data, f)

    def load(self, file_path):
        with open(file_path, "rb") as f:
            data = pickle.load(f)

        self.alpha = data["alpha"]
        self.gamma = data["gamma"]
        self.epsilon = data["epsilon"]
        self.epsilon_decay = data["epsilon_decay"]
        self.epsilon_min = data["epsilon_min"]

        loaded_q_table = data["q_table"]
        self.q_table = defaultdict(lambda: {action: 0.0 for action in self.actions}, loaded_q_table)

    def export_to_json(self, file_path):
        export_data = []

        for state, actions in self.q_table.items():
            state_key = ",".join(map(str, state))

            export_data.append({
                "state": state_key,
                "actions": {
                    action.name: value
                    for action, value in actions.items()
                }
            })

        with open(file_path, "w", encoding="utf-8") as f:
            json.dump(export_data, f, indent=2)