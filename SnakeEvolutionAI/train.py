from pathlib import Path
import subprocess
import sys

from snake_env import SnakeEnv
from q_agent import QLearningAgent


def train(
    episodes=5000,
    max_steps_per_episode=900,
    model_path="q_table.pkl",
    json_path="q_table.json",
):
    env = SnakeEnv(width=15, height=10)
    agent = QLearningAgent()

    model_file = Path(model_path)
    json_file = Path(json_path)

    if model_file.exists():
        agent.load(str(model_file))
        print(f"Q-table caricata da {model_file}")
    else:
        print("Nessuna Q-table trovata, training da zero")

    scores = []
    best_score = 0

    print(
        f"Training standard avviato | episodes={episodes}, "
        f"max_steps={max_steps_per_episode}, grid={env.width}x{env.height}"
    )

    for episode in range(1, episodes + 1):
        state = env.reset()
        done = False
        steps = 0

        while not done and steps < max_steps_per_episode:
            action = agent.choose_action(state)
            next_state, reward, done = env.step(action)
            agent.update_q_value(state, action, reward, next_state, done)
            state = next_state
            steps += 1

        agent.decay_epsilon()

        episode_score = env.score
        scores.append(episode_score)
        best_score = max(best_score, episode_score)

        if episode % 100 == 0:
            last_100_scores = scores[-100:]
            avg_score = sum(last_100_scores) / len(last_100_scores)
            print(
                f"Episodio {episode:4d} | "
                f"Score medio (ultimi 100): {avg_score:6.2f} | "
                f"Best score: {best_score:3d} | "
                f"Epsilon: {agent.epsilon:.4f}"
            )

    agent.save(str(model_file))
    agent.export_to_json(str(json_file))

    print(f"Q-table salvata in {model_file}")
    print(f"Q-table esportata in {json_file}")

    launch_trained_run(json_file.resolve())


def launch_trained_run(policy_json_path):
    repo_root = Path(__file__).resolve().parents[1]
    monogame_csproj = repo_root / "SnakeEvolution" / "SnakeEvolution.MonoGame" / "SnakeEvolution.MonoGame.csproj"
    monogame_workdir = repo_root / "SnakeEvolution"

    if not monogame_csproj.exists():
        print(f"Progetto MonoGame non trovato: {monogame_csproj}")
        return

    print("\n=== AVVIO PARTITA CON AGENTE ADDESTRATO ===")

    command = [
        "dotnet",
        "run",
        "--project",
        str(monogame_csproj),
        "--",
        "--policy-json",
        str(policy_json_path),
        "--episode-label",
        "Trained-Policy",
    ]

    result = subprocess.run(command, cwd=str(monogame_workdir), check=False)

    if result.returncode != 0:
        raise subprocess.CalledProcessError(result.returncode, command)


if __name__ == "__main__":
    try:
        train()
    except subprocess.CalledProcessError as exc:
        print(f"Errore durante l'avvio MonoGame: comando fallito con exit code {exc.returncode}")
        sys.exit(exc.returncode)
