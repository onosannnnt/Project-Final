# Skill Fixes - Runtime Verification Guide

## Overview
Three skills have been fixed for incorrect buff/debuff application and stack consumption:
- **Chain Bloom** (EL AttackSkill - Skill 24)
- **Tangleweed** (CE SupportSkill - Skill 48)
- **Focused Mind** (CE SupportSkill - Skill 49)

---

## Fix 1: Chain Bloom - Stack Consumption

### Issue
Frenzy Stacks were not being consumed when repeating attack

### Root Cause
`StackScaledDamageEffect.asset` had `consumeStacks: 0` (disabled)

### Fix Applied
- **File**: `Assets/_Scripts/Skills/EL/AttackSkill/AOE/24Chain Bloom/StackScaledDamageEffect.asset`
- **Change**: `consumeStacks: 0` → `consumeStacks: 1`

### Expected Behavior After Fix
1. Cast Chain Bloom on any target
2. If caster has Frenzy Stacks >= 6:
   - Combat log should show TWO damage instances:
     - First attack: Deal 200 damage
     - Second attack: Deal 200 damage (repeat)
   - Caster's Frenzy Stack count should DECREASE by 6 after the attack
   - Both attacks should hit the same target (AOE skill)

### Verification Checklist
- [ ] Frenzy Stacks visible on caster in buff panel
- [ ] With 6+ stacks, attack repeats (two damage numbers appear)
- [ ] Stack count decreases by 6 after attack
- [ ] Combat log shows "StackScaledDamageEffect" consumed 6 stacks
- [ ] UI status panel updates correctly
- [ ] With < 6 stacks, attack does NOT repeat

### Code Reference
- `StackScaledDamageEffect.cs` line ~24: Checks `consumeStacks` flag
- `CombatActionLog.cs` line ~100: BuffEffectLogs track stack changes
- `BuffManager.cs`: Tracks active buff stack counts

---

## Fix 2: Tangleweed - Missing Speed Debuff + Wrong Effect Trigger

### Issues
1. Debuff had no stat modifier (modifiers array was empty)
2. Effect was not being applied to target (ExecuteOnce: 1 prevented target-specific effects)

### Root Causes
1. `Buff.asset` had empty modifiers array
2. `ForecastUtilityEffect.asset` had `ExecuteOnce: 1` but `applyBuffsToParty: 0`

### Fixes Applied
- **File 1**: `Assets/_Scripts/Skills/CE/SupportSkill/48Tangleweed/Buff.asset`
  - Added `BuffName: Tangleweed`
  - Added `Description: Reduce Speed by 30%`
  - Changed `buffType: 0` → `buffType: 1` (Debuff)
  - Added StatModifier: `Stat: 4 (ActionSpeed), Type: 1 (Percent), Value: -0.3`

- **File 2**: `Assets/_Scripts/Skills/CE/SupportSkill/48Tangleweed/ForecastUtilityEffect.asset`
  - Changed `ExecuteOnce: 1` → `ExecuteOnce: 0`

### Expected Behavior After Fix
1. Cast Tangleweed on target enemy
2. Target should receive "Tangleweed" debuff immediately
3. Debuff panel shows:
   - Name: "Tangleweed"
   - Type: Red icon (Debuff)
   - Duration: 3 turns
   - Effect: "Reduce Speed by 30%"
4. Target's ActionSpeed stat should be reduced by 30% for 3 turns
5. Combat log should show:
   - Buff applied to target
   - Stat modifier: ActionSpeed -30%

### Verification Checklist
- [ ] Debuff appears on target in buff panel (red background)
- [ ] Debuff name shows "Tangleweed"
- [ ] Duration shows "3"
- [ ] Target's speed is visibly reduced in action order
- [ ] Combat log shows BuffEffectLog with:
  - BuffName: "Tangleweed"
  - BuffType: "Debuff"
  - statModifiers[0]: Stat="ActionSpeed", Type="Percent", Value="-0.3"
- [ ] After 3 turns, debuff automatically removes
- [ ] Target's speed returns to normal

### Code Reference
- `ForecastUtilityEffect.cs` line ~36: Execute() only runs ApplyTargetEffects if !ExecuteOnce
- `ForecastUtilityEffect.cs` line ~90: ApplyTargetEffects applies buffs to specific target
- `Buff.cs` line ~50: modifiers list is applied via BuffEffectData

---

## Fix 3: Focused Mind - Buff Not Applied (ExecuteOnce Issue)

### Issue
Target does not receive critical rate buff

### Root Cause
`ForecastUtilityEffect.asset` had `ExecuteOnce: 1` but `applyBuffsToParty: 0`
- When ExecuteOnce=true, only OnSkillStarted() executes globally
- ApplyGlobalEffects() checks applyBuffsToParty before applying
- Since applyBuffsToParty=0, buffs were skipped entirely

### Fix Applied
- **File**: `Assets/_Scripts/Skills/CE/SupportSkill/49Focused Mind/ForecastUtilityEffect.asset`
  - Changed `ExecuteOnce: 1` → `ExecuteOnce: 0`

### Expected Behavior After Fix
1. Cast Focused Mind on target ally
2. Target should receive "Focused Mind" buff immediately
3. Buff panel shows:
   - Name: "Focused Mind"
   - Type: Green icon (Buff)
   - Duration: 3 turns
   - Effect: "Increase critical rate by 50%"
4. When target attacks, critical hit chance should increase by 50%
5. Combat log should show:
   - Buff applied to target
   - BonusCritChance: 0.5

### Verification Checklist
- [ ] Buff appears on target in buff panel (green background)
- [ ] Buff name shows "Focused Mind"
- [ ] Duration shows "3"
- [ ] Target's next attack has increased crit chance:
  - Look for critical hits in combat log (marked with ★ or "CRIT")
  - With 50% bonus, should see more crits than before
- [ ] Combat log shows BuffEffectLog with:
  - BuffName: "Focused Mind"
  - BuffType: "Buff"
  - BonusCritChance: 0.5
- [ ] After 3 turns, buff automatically removes
- [ ] Target's crit chance returns to normal

### Code Reference
- `GenericStatModifierBuff.cs` line ~15: OnApply creates GenericDamageModifier
- `GenericDamageModifier.cs` line ~44: Modify() applies BonusCritChance
- `ForecastUtilityEffect.cs` line ~36: Execute() gates on !ExecuteOnce

---

## Runtime Testing Steps

### Prerequisites
1. Open Combat.unity scene
2. Set up a test battle with players and enemies
3. Ensure EL/CE/RV characters are available
4. Open Console (Ctrl+Grave or View → Editor Logs)

### Test Sequence

#### Test 1: Chain Bloom Stack Consumption
```
1. Add Frenzy Stacks to EL character (manually or via another skill)
2. Ensure character has 6+ stacks
3. Cast Chain Bloom on an enemy
4. Check combat log for:
   - Two damage instances listed
   - StackScaledDamageEffect consuming 6 stacks
5. Verify stack count on character buff panel decreased by 6
6. Repeat with < 6 stacks and verify NO repeat attack
```

#### Test 2: Tangleweed Debuff Application
```
1. Cast Tangleweed on an enemy
2. Check enemy buff panel for "Tangleweed" debuff (RED)
3. Verify action order shows enemy with reduced speed
4. Check combat log for BuffEffectLog with:
   - "AppliedTarget": enemy name
   - "BuffName": "Tangleweed"
   - "Duration": 3
   - "statModifiers": [{"Stat": "ActionSpeed", "Type": "Percent", "Value": "-0.3"}]
5. Let battle continue for 3 turns
6. Verify debuff disappears and enemy speed returns to normal
```

#### Test 3: Focused Mind Buff Application
```
1. Cast Focused Mind on an ally
2. Check ally buff panel for "Focused Mind" buff (GREEN)
3. Have the ally attack 5-10 times
4. Count critical hits and compare to baseline
5. Check combat log for BuffEffectLog with:
   - "AppliedTarget": ally name
   - "BuffName": "Focused Mind"
   - "Duration": 3
   - BonusCritChance: 0.5 (visible in modifiers)
6. Let battle continue for 3 turns
7. Verify buff disappears and crit rate returns to normal
```

### Logging Output Expected

Each action should generate CombatActionLog entries. Look for patterns like:

```
[BuffEffectLog]
- AppliedTarget: "Enemy Name"
- Buff:
  - BuffName: "Tangleweed"
  - BuffType: "Debuff"
  - Duration: 3
  - statModifiers[0]:
    - Stat: "ActionSpeed"
    - Type: "Percent"
    - Value: -0.3
```

### Console Debug Output

Enable Debug.Log in relevant files:
- `ForecastUtilityEffect.cs` line ~90: Uncomment to see when buffs apply
- `StackScaledDamageEffect.cs`: Add logging for stack consumption
- `BuffManager.cs`: Add logging for buff lifecycle events

---

## Troubleshooting

### Issue: Tangleweed debuff doesn't appear after casting
**Solution**: Check that `ExecuteOnce: 0` was applied correctly
- Open `Tangleweed.asset` and verify SkillEffects contains ForecastUtilityEffect
- Check ForecastUtilityEffect.asset: `ExecuteOnce: 0`, `applyBuffsToParty: 0`

### Issue: Focused Mind buff doesn't apply
**Solution**: Verify ExecuteOnce flag change
- Open `Focused Mind.asset` and check SkillEffects
- Verify ForecastUtilityEffect.asset: `ExecuteOnce: 0`, `applyBuffsToParty: 0`
- Check that GenericStatModifierBuff.asset is referenced correctly

### Issue: Chain Bloom doesn't repeat even with 6+ stacks
**Solution**: Check StackScaledDamageEffect configuration
- Verify `consumeStacks: 1` in asset file
- Check `repeatIfEnoughStacks: 1`
- Verify `repeatStackThreshold: 6` and `repeatStackCost: 6`
- Ensure Frenzy Stacks buff exists and is active on caster

---

## Files Modified Summary

| File | Changes | Status |
|------|---------|--------|
| StackScaledDamageEffect.asset | consumeStacks: 0→1 | ✓ Applied |
| Tangleweed/Buff.asset | Added speed modifier, updated metadata | ✓ Applied |
| Tangleweed/ForecastUtilityEffect.asset | ExecuteOnce: 1→0 | ✓ Applied |
| Focused Mind/ForecastUtilityEffect.asset | ExecuteOnce: 1→0 | ✓ Applied |

---

## Architecture Notes

### Stack Consumption (Chain Bloom)
- `StackScaledDamageEffect.Execute()` reads consumeStacks flag
- If true and stacks >= repeatStackThreshold, effect repeats
- BuffManager removes stacks via RemoveBuff() or ConsumeStacks()
- CombatActionLog tracks buff changes

### Buff Application (Tangleweed, Focused Mind)
- `ForecastUtilityEffect` is a SkillEffect that applies buffs to targets
- When ExecuteOnce=false, Execute() runs for EACH target
- Execute() calls ApplyTargetEffects() → AddBuff()
- BuffEffectData logs the buff application with all modifiers
- BuffManager.AddBuff() triggers Buff.OnApply() lifecycle method

### Stat Modifiers (Tangleweed Speed)
- StatModifier objects stored in Buff.modifiers[] list
- Stat: enum value (4=ActionSpeed, 1=MaxHealth, 2=MaxSkillPoint, etc.)
- Type: enum value (0=Flat, 1=Percent)
- Value: float (can be negative for reductions)
- Applied when Entity.GetStat() calculates modified value

