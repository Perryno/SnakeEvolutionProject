from pathlib import Path
import csv
from snake_env import SnakeEnv
from q_agent import QLearningAgent


def evaluate_greedy_policy(
    agent,
    width,
    height,
    episodes=12,
    max_steps_per_episode=900,
    seed_base=5000,
):
    scores = []

    for i in range(episodes):
        env = SnakeEnv(width=width, height=height, seed=seed_base + i)
        state = env.reset()
        done = False
        steps = 0

        while not done and steps < max_steps_per_episode:
            state_actions = agent.q_table[state]
            action = max(state_actions, key=state_actions.get)
            state, _, done = env.step(action)
            steps += 1

        scores.append(env.score)

    average_score = sum(scores) / len(scores)
    max_score = max(scores)

    return average_score, max_score


def train_with_checkpoints(
    episodes=3000,
    checkpoint_interval=300,
    max_steps_per_episode=900,
    checkpoints_dir="demo_artifacts/checkpoints",
):
    env = SnakeEnv(width=15, height=10)
    agent = QLearningAgent()

    checkpoints_path = Path(checkpoints_dir)
    checkpoints_path.mkdir(parents=True, exist_ok=True)

    scores = []
    checkpoint_json_paths = []
    best_score = 0
    metrics_rows = []

    print(
        f"Training DEMO avviato | episodes={episodes}, "
        f"checkpoint_interval={checkpoint_interval}, "
        f"max_steps={max_steps_per_episode}, "
        f"grid={env.width}x{env.height}"
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

        if episode % checkpoint_interval == 0:
            checkpoint_stem = f"q_table_ep{episode:04d}"
            checkpoint_pkl = checkpoints_path / f"{checkpoint_stem}.pkl"
            checkpoint_json = checkpoints_path / f"{checkpoint_stem}.json"

            agent.save(str(checkpoint_pkl))
            agent.export_to_json(str(checkpoint_json))

            checkpoint_json_paths.append((episode, str(checkpoint_json.resolve())))

            recent_scores = scores[-checkpoint_interval:]
            recent_avg = sum(recent_scores) / len(recent_scores)
            eval_avg, eval_max = evaluate_greedy_policy(
                agent,
                width=env.width,
                height=env.height,
                episodes=12,
                max_steps_per_episode=max_steps_per_episode,
                seed_base=5000 + episode,
            )

            metrics_rows.append(
                {
                    "episode": episode,
                    "epsilon": f"{agent.epsilon:.6f}",
                    "train_recent_avg": f"{recent_avg:.3f}",
                    "train_best_so_far": best_score,
                    "eval_greedy_avg": f"{eval_avg:.3f}",
                    "eval_greedy_max": eval_max,
                    "checkpoint_json": checkpoint_json.name,
                }
            )

            print(
                f"[CHECKPOINT] Episodio {episode} salvato | "
                f"avg training ultimi {checkpoint_interval}: {recent_avg:.2f} | "
                f"eval greedy avg: {eval_avg:.2f} | "
                f"eval greedy max: {eval_max} | "
                f"json: {checkpoint_json.name}"
            )

    latest_pkl = checkpoints_path / "q_table_latest.pkl"
    latest_json = checkpoints_path / "q_table_latest.json"
    agent.save(str(latest_pkl))
    agent.export_to_json(str(latest_json))

    metrics_csv = checkpoints_path / "checkpoint_metrics.csv"
    with metrics_csv.open("w", encoding="utf-8", newline="") as f:
        writer = csv.DictWriter(
            f,
            fieldnames=[
                "episode",
                "epsilon",
                "train_recent_avg",
                "train_best_so_far",
                "eval_greedy_avg",
                "eval_greedy_max",
                "checkpoint_json",
            ],
        )
        writer.writeheader()
        writer.writerows(metrics_rows)

    print(f"Training DEMO completato. Checkpoint generati: {len(checkpoint_json_paths)}")
    print(f"Ultimo modello demo: {latest_json}")
    print(f"Metriche checkpoint demo: {metrics_csv}")

    return checkpoint_json_paths
