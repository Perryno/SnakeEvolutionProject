import os
import time
from snake_env import SnakeEnv
from q_agent import QLearningAgent


def play_trained_agent(max_steps=500):
    env = SnakeEnv()
    agent = QLearningAgent()

    model_path = "q_table.pkl"

    if not os.path.exists(model_path):
        print("Nessuna Q-table trovata. Allena prima l'agente.")
        return

    agent.load(model_path)
    agent.epsilon = 0.0

    state = env.reset()
    done = False
    steps = 0

    print("Inizio partita agente addestrato")
    print("=" * 30)

    env.render()

    while not done and steps < max_steps:
        action = agent.choose_action(state)
        state, reward, done = env.step(action)
        steps += 1

        print(f"Azione scelta: {action}, Reward: {reward}")
        env.render()

        time.sleep(0.3)

    print("Partita finita")
    print(f"Score finale: {env.score}")


if __name__ == "__main__":
    play_trained_agent()