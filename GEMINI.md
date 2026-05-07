# Project-Final: Unity RPG Combat System

## Project Overview

This project is a Unity-based RPG featuring a modular turn-based combat system. It emphasizes a data-driven design using ScriptableObjects for skills, entity stats, and game configurations. The architecture relies on Singleton patterns for core managers and a state-machine approach for combat flow.

### Main Technologies

- **Unity Engine:** Version 2022.3.62f2
- **Language:** C#
- **Input System:** Unity's New Input System (`com.unity.inputsystem`)
- **Rendering:** Universal Render Pipeline (URP)
- **UI:** TextMeshPro and standard Unity UI (uGUI)
- **Networking:** `NetworkManager` using `HttpClient` for asynchronous data logging and combat tracking.

---

## Architecture & Core Components

### Managers (`Assets/_Scripts/Managers`)

- **TurnManager:** The central orchestrator of the combat loop. It uses a state machine (`TurnState`) to transition between player turns, enemy turns, action execution, and combat resolution (Win/Lose).
- **CombatActionProcessor:** Processes actions queued by entities. It handles resource validation (SP), target verification, and invokes skill effects.
- **CombatResourceManager:** Manages combat-specific resources, including health, skill points (shared or individual), and phase-based stat modifiers.
- **NetworkManager:** A persistent singleton that handles communication with a backend API for saving player data, combat logs, and skill loadouts.
- **BuffManager:** Manages active status effects on entities, processing their logic each turn.

### Entity System (`Assets/_Scripts/Utils/Entity.cs`, `Assets/_Scripts/Player/`, `Assets/_Scripts/Enemy/`)

- **Entity:** The base class for all combatants. Handles stats, health (including "Corrupted Health" mechanics), SP, buffs, and damage processing.
- **PlayerEntity:** Abstract extension of `Entity` for player-controlled characters. Handles input, targeting states, and visual highlighting.
- **EnemyCombat:** Base class for enemy entities, implementing AI logic for action selection.

### Skill & Effect System (`Assets/_Scripts/BaseScriptableObject/`)

- **Skill:** A ScriptableObject defining a skill's properties (cost, target type, target count).
- **SkillUseEffect:** Modular, pluggable effects (e.g., `DamageEffect`, `HealEffect`, `BuffEffect`) that are assigned to skills. This allows for complex skill behaviors without hardcoding logic for every variation.

---

## Development Conventions

### Coding Style & Patterns

- **Singletons:** Core systems use `Singleton<T>` or `SingletonPersistent<T>` for global access. Implementation is found in `Assets/_Scripts/Utils/Singletron.cs`.
- **Data-Driven Design:** Stats, skills, and quests are defined as ScriptableObjects. Always use `Clone()` when modifying ScriptableObject data at runtime to avoid mutating project assets.
- **Async/Await:** Used for non-blocking network operations and certain game state transitions.
- **Events:** C# delegates and events (e.g., `OnHealthChanged`) are used to decouple core logic from UI and secondary systems.

### Combat Flow

1. **PlayerTurnState:** Players select skills and targets for each party member.
2. **SpeedCompareState:** Actions are sorted based on entity speed.
3. **ActionState:** Actions are executed sequentially via `CombatActionProcessor`.
4. **Resolution:** Check for win/loss conditions or proceed to the next wave.

### Data Management

- `UserData.asset`: Stores persistent player state and progress.
- `LoadEnv.apiKey`: Environment variable used by `NetworkManager` for the backend URL.

---

## Key Files

- `Assets/_Scripts/Managers/TurnManager.cs`: Core combat state machine.
- `Assets/_Scripts/Managers/CombatActionProcessor.cs`: Action execution logic.
- `Assets/_Scripts/Utils/Entity.cs`: Base combatant logic and stat management.
- `Assets/_Scripts/BaseScriptableObject/Skill.cs`: Definition of the skill system.
- `Assets/_Scripts/Managers/NetworkManager.cs`: External API integration.

### Mandatory Validation

- **Compilation Check:** Before finishing any task, verify that the project compiles.
- **Symbol Integrity:** If a method or property is removed/renamed, perform a workspace-wide search to ensure all references are updated.

## Building and Running

- **Unity Version:** Ensure 2022.3.62f2 is used to avoid serialization or package issues.
- **Input:** Uses the New Input System. Input actions are defined in `Assets/_Scripts/Player/PlayerInputActions.inputactions`.
- **Environment:** A `.env` file or `LoadEnv` setup is required for backend connectivity.

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
