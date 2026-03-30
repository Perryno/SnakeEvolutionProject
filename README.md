# Snake Evolution AI

Educational project that trains a Snake agent with Q-learning (Python) and runs gameplay in MonoGame (C#).

## Repository Structure

- `SnakeEvolutionAI/` -> Python training and demo scripts
- `SnakeEvolution/` -> MonoGame application used to visualize the trained policy

## What It Does

- **Demo mode**: trains with checkpoints every 300 episodes and shows automatic gameplay for each checkpoint.
- **Standard mode**: runs full training (default 5000 episodes), then opens MonoGame and plays using the trained policy.

## Requirements

- Python 3.13+
- .NET 8 SDK

## Quick Start

Open a terminal in `SnakeEvolutionAI/`.

### 1) Demo mode (`run-demo`)

```powershell
python .\run-demo.py
```

### 2) Standard training + MonoGame playback

```powershell
python .\train.py
```

This will:

1. Train the Q-table for 5000 episodes.
2. Save `q_table.pkl` and `q_table.json`.
3. Launch MonoGame with the trained JSON policy.

## Output Artifacts

- `SnakeEvolutionAI/q_table.pkl`
- `SnakeEvolutionAI/q_table.json`
- `SnakeEvolutionAI/demo_artifacts/checkpoints/`

## Notes

- During demo mode, MonoGame auto-closes after each game (`--auto-exit`).
- During standard mode, MonoGame starts once with the final trained policy.
