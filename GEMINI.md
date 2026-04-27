# Project Final - Unity Combat System

A turn-based RPG combat system built in Unity 2022.3. It features a highly modular architecture driven by ScriptableObjects for stats, skills, and effects.

## Project Overview
- **Engine:** Unity 2022.3.62f2
- **Genre:** Turn-based RPG / Combat Simulator
- **Core Systems:**
    - **Combat:** Speed-based turn management (`TurnManager`).
    - **Stats:** ScriptableObject-driven entities (`EntitiesBaseStat`).
    - **Skills:** Composable effects (`Skill`, `SkillEffect`).
    - **Buffs:** Extensible status effect system (`BuffManager`, `BuffEffect`).
    - **Networking:** Basic combat logging via `NetworkManager`.

## Directory Structure
- `Assets/_Scripts/Managers/`: Core game loop and system managers (Turn, Skill, Buff, etc.).
- `Assets/_Scripts/BaseScriptableObject/`: Data definitions for Skills, Stats, and Effects.
- `Assets/_Scripts/Utils/`: Shared utilities, enums, and the base `Entity` class.
- `Assets/_Scripts/Player/` & `Assets/_Scripts/Enemy/`: Specialized entity implementations.
- `Assets/_Scripts/UI/`: Combat and Overworld UI systems.
- `Assets/Scenes/`: Game scenes including `MainMenu`, `Overworld`, and `Combat`.

## Building and Running
- **Editor:** Use Unity 2022.3.62f2.
- **Scenes:**
    1. Start from `MainMenu` or `Loading` for the full flow.
    2. `Overworld` for exploration.
    3. `Combat` for direct combat testing.
- **Dependencies:**
    - TextMesh Pro (UI)
    - Unity Mathematics
    - DOTween (inferred/common, though not explicitly checked, TMPro is definitely used)

## Development Conventions
### 1. Entity Architecture
All combatants (Players and Enemies) must inherit from `Entity.cs`. This base class handles:
- Health and SP management.
- Damage/Healing logic with modifiers (`IDamageModifier`).
- Buff/Debuff processing via `BuffManager`.
- Stat calculation including flat and percentage modifiers.

### 2. ScriptableObject-Driven Design
- **Stats:** Define base stats in `EntitiesBaseStat`. Use the `Clone()` method in `Awake` to ensure runtime modifications don't affect the asset.
- **Skills:** Create new skills as `Skill` assets. Skills are composed of multiple `SkillEffect` objects (e.g., `DamageEffect`, `HealEffect`, `BuffEffect`).
- **Effects:** Inherit from `SkillEffect` to create new types of combat interactions.

### 3. Turn Management
The `TurnManager` operates in states:
- `PlayerTurnState`: Players select targets and skills.
- `SpeedCompareState`: Actions are sorted by the `ActionSpeed` stat.
- `ActionState`: Actions are executed sequentially.

### 4. Coding Style
- Follow standard C# Unity conventions (PascalCase for methods/properties, camelCase for private fields).
- Use `[SerializeField]` for inspector exposure.
- Prefer composition (Effects) over complex inheritance for skill logic.

## Key Files
- `Assets/_Scripts/Utils/Entity.cs`: The core base class for all combatants.
- `Assets/_Scripts/Managers/TurnManager.cs`: The "brain" of the combat system.
- `Assets/_Scripts/BaseScriptableObject/Skill.cs`: Defines how skills are executed.
- `Assets/_Scripts/Utils/Enum/PlayerType.cs`: Contains central enums like `StatType` and `SkillType`.
