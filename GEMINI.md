e# Project Final - Unity Combat System

A turn-based RPG combat system built in Unity 2022.3. It features a modular architecture driven by ScriptableObjects for stats, skills, and status effects.

## Project Overview

- **Engine:** Unity 2022.3.62f2
- **Genre:** Turn-based RPG / Combat Simulator
- **Core Systems:**
  - **Combat:** Speed-based turn management via `TurnManager`.
  - **Stats:** ScriptableObject-driven entity stats (`EntitiesBaseStat`).
  - **Skills:** Composable skill system using `Skill` and `SkillEffect` subclasses.
  - **Buffs:** Extensible status effect system (`BuffManager`, `BuffEffect`).
  - **UI:** Combat feedback including damage numbers, action queues, and skill selection.

## Directory Structure

- `Assets/_Scripts/Managers/`: Core system managers (Turn, Skill, Buff, etc.).
- `Assets/_Scripts/BaseScriptableObject/`: Data definitions and effect implementations.
  - `SkillUseEffect.cs/`: (Directory) Specialized skill effects like `BloodRageEffect`, `TauntEffect`, etc.
- `Assets/_Scripts/Utils/`: Shared utilities, enums, and the base `Entity` class.
- `Assets/_Scripts/Player/` & `Assets/_Scripts/Enemy/`: Specialized entity implementations.
- `Assets/_Scripts/UI/`: Combat and Overworld UI systems.
- `Assets/Scenes/`: Game scenes (`MainMenu`, `Loading`, `Overworld`, `Combat`).
- `Assets/GameData/`: ScriptableObject instances for game configuration.

## Building and Running

- **Unity Version:** Use Unity 2022.3.62f2.
- **Main Scenes:**
  1. `MainMenu`: Initial entry point.
  2. `Overworld`: World exploration and quest initiation.
  3. `Combat`: Direct combat testing.
- **Typical Flow:** Start from `MainMenu` or `Loading` to ensure all singleton managers (like `UserData`) are properly initialized.

## Development Conventions

### 1. Entity Architecture

All combatants must inherit from `Entity.cs`.

- **Stats:** Uses `EntitiesBaseStat`. Call `Clone()` in `Awake` to prevent modifying assets.
- **Health/SP:** Managed in `Entity`, triggers events `OnHealthChanged` and `OnSPChanged`.
- **Buffs:** All entities have a `BuffManager` to handle status effects and stat modifiers.

### 2. ScriptableObject-Driven Design

- **Stats:** Define base stats in `EntitiesBaseStat` (Note: file named `EnititiesStat.cs`). Supports legacy core stats and a list of `AdditionalStats` for flexibility.
- **Skills:** `Skill` assets define target types, costs, and a list of `SkillEffect` objects.
- **Effects:** Inherit from `SkillEffect` to create new combat behaviors. Specialized effects are organized under the `SkillUseEffect.cs/` directory.

### 3. Turn Management (`TurnManager`)

Operates in a state machine:

- `PlayerTurnState`: Players select skills and targets.
- `SpeedCompareState`: Sorts all actions by the `ActionSpeed` stat.
- `ActionState`: Executes actions sequentially from the `actionQueue`.
- Supports shared or individual Skill Point (SP) pools.

### 4. Coding Style

- **Naming:** PascalCase for methods/properties, camelCase for private fields.
- **Inspector:** Use `[SerializeField]` for private fields that need to be exposed.
- **Composition:** Prefer adding new `SkillEffect` types over modifying the core `Skill` class.

## Key Files

- `Assets/_Scripts/Utils/Entity.cs`: The foundation for all players and enemies.
- `Assets/_Scripts/Managers/TurnManager.cs`: The central combat loop.
- `Assets/_Scripts/BaseScriptableObject/Skill.cs`: Defines how skills are structured.
- `Assets/_Scripts/BaseScriptableObject/EnititiesStat.cs`: Defines base statistics.
- `Assets/_Scripts/Utils/Enum/PlayerType.cs`: Contains central enums like `StatType` and `SkillType`.
