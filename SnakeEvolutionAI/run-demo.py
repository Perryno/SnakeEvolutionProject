from pathlib import Path
import subprocess
import sys

from demo_training import train_with_checkpoints


def run_demo():
    episodes = 3000
    checkpoint_interval = 300
    max_steps_per_episode = 900

    checkpoint_items = train_with_checkpoints(
        episodes=episodes,
        checkpoint_interval=checkpoint_interval,
        max_steps_per_episode=max_steps_per_episode,
        checkpoints_dir="demo_artifacts/checkpoints",
    )

    repo_root = Path(__file__).resolve().parents[1]
    monogame_csproj = repo_root / "SnakeEvolution" / "SnakeEvolution.MonoGame" / "SnakeEvolution.MonoGame.csproj"
    monogame_workdir = repo_root / "SnakeEvolution"

    if not monogame_csproj.exists():
        raise FileNotFoundError(f"Progetto MonoGame non trovato: {monogame_csproj}")

    print("\n=== DEMO AUTOMATICA ===")
    print("Ogni checkpoint viene lanciato in MonoGame con una partita autonoma.\n")

    for episode, checkpoint_json in checkpoint_items:
        print(f"[DEMO] Avvio episodio {episode} con policy: {checkpoint_json}")

        command = [
            "dotnet",
            "run",
            "--project",
            str(monogame_csproj),
            "--",
            "--policy-json",
            checkpoint_json,
            "--episode-label",
            str(episode),
            "--auto-exit",
        ]

        result = subprocess.run(command, cwd=str(monogame_workdir), check=False)

        if result.returncode == 130:
            print("[DEMO] Stop anticipato richiesto dall'utente (ESC).")
            break

        if result.returncode != 0:
            raise subprocess.CalledProcessError(result.returncode, command)

    print("\nDemo completata.")


if __name__ == "__main__":
    try:
        run_demo()
    except subprocess.CalledProcessError as exc:
        print(f"Errore durante la demo: comando fallito con exit code {exc.returncode}")
        sys.exit(exc.returncode)
