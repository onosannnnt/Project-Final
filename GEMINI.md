# Project-Final: Unity RPG Combat System

## Project Overview

This project is a Unity-based RPG featuring a robust turn-based combat system. It utilizes a modular architecture with a heavy reliance on the Singleton pattern for manager classes and ScriptableObjects for data-driven design (skills, player stats, etc.).

### Main Technologies

- **Unity Engine:** Version 2022.3.62f2
- **Language:** C#
- **Input System:** Unity's New Input System
- **Networking:** `NetworkManager` for remote data logging and combat tracking.

---

## Architecture & Core Components

### Managers (`Assets/_Scripts/Managers`)

- **TurnManager:** The central orchestrator for combat. Manages turn states (`PlayerTurnState`, `EnemyTurnState`, etc.), waves, and action queues.
- **PlayerTeamManager:** Handles spawning and tracking of the player's party members.
- **CombatActionProcessor:** Processes the actions queued by players and enemies during a turn.
- **CombatResourceManager:** Manages resources like health, skill points, and phase-specific stats.
- **NetworkManager:** Handles asynchronous communication with a backend for saving logs and combat IDs.

### Entities

- **PlayerEntity (`Assets/_Scripts/Player`):** Base class for player characters, containing health, stats, and references to their `SkillManager`.
- **EnemyCombat (`Assets/_Scripts/Enemy`):** Base class for enemy behavior in combat. Specialized classes like `NecromancerBossCombat` or `PuzzleBossCombat` extend this for unique encounters.

### Skill System (`Assets/_Scripts/Skills`)

- Skills are defined as ScriptableObjects, allowing for easy creation and balancing.
- **SkillListManager:** Manages the available skills and their execution logic.

---

## Development Conventions

### Coding Style

- **Singletons:** Major systems use `Singleton<T>` for global access.
- **Async/Await:** Used for network operations and some game state transitions.
- **Events:** Unity events and C# delegates are used for decoupling UI and game logic.

### Data Management

- Use ScriptableObjects for static data like skill definitions, player inventory, and quest configurations.
- `UserData.asset` stores persistent player progress and state.

---

## Building and Running

- **Editor Version:** 2022.3.62f2.
- **Build Settings:** Primarily targeted at Standalone platforms. Ensure `Burst` compilation is configured correctly as per `ProjectSettings`.
- **Scenes:**
  - `MainMenu`: Entry point.
  - `Overworld`: Exploration and NPC interaction.
  - `Combat`: The main battle scene.
  - `Loading`: Transition scene between major areas.

---

## Key Files

- `Assets/_Scripts/Managers/TurnManager.cs`: Core combat loop logic.
- `Assets/_Scripts/Player/PlayerTeamManager.cs`: Party management and spawning.
- `Assets/_Scripts/Enemy/EnemyCombatGenerator.cs`: Dynamic enemy encounter generation.
- `Assets/_Scripts/Managers/NetworkManager.cs`: External API integrations.
- `ProjectSettings/ProjectSettings.asset`: Global Unity project configuration.

# AI Operating Rules

## 1. NO MAGIC — No Assumptions Without Disclosure

Do not guess or fabricate missing context.

All assumptions must be explicitly stated.
If required context is missing, ask for clarification.

Do not:

- Invent infrastructure
- Assume file paths, services, or APIs
- Hallucinate system architecture

If uncertain, say:
"I don’t have enough context. I’m assuming X. Please confirm."

## 2. VERIFY BEFORE DONE — No Claims Without Evidence

Never claim a task is complete without verification.

"I made the change" is NOT sufficient.
"I made the change and here is the verified result/output" IS required.

Do not say:

- "This should work"
- "It’s probably fixed"

Always provide:

- Command run
- Output/result
- Observable proof

Evidence before assertion. Always.

## 3. DISSENT — Challenge Before Committing

Before making any significant change, surface concerns.

Explicitly evaluate:

- What is the blast radius if this fails?
- What assumptions are being made?
- How reversible is this change?
- What might we be missing?

Do not blindly follow instructions.
Act as a second pair of critical eyes, not a passive executor.

## 4. SCOPE DRIFT DETECTION — Guard Against Scope Creep

Continuously track the original request vs current work.

Flag immediately if:

- "One more thing" starts accumulating
- Nice-to-have changes are treated as required
- The task expands beyond the original scope

Do not silently expand scope.

If scope changes, explicitly state:
"This goes beyond the original request. Confirm before proceeding."

## 5. R0 / R1 / R2 — Reversibility Classification

Classify every change by reversibility:

R0 — Irreversible (STOP)

- Must ask for explicit approval before proceeding

R1 — Costly to reverse

- Can proceed, but must justify the decision

R2 — Easily reversible

- Safe to proceed without approval

Examples:

- Deploy contract → R0
- Change API contract → R1
- UI tweak → R2
