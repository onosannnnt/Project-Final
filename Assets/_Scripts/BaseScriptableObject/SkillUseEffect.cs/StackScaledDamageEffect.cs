using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "StackScaledDamageEffect", menuName = "ScriptableObjects/SkillEffect/StackScaledDamageEffect")]
public class StackScaledDamageEffect : SkillEffect
{
    [Header("Scaling")]
    [SerializeField] private Buff stackSourceBuff;
    [SerializeField] private float baseDamage = 200f;
    [SerializeField] private float damagePerStack = 50f;
    [SerializeField] private DamageElement Element = DamageElement.Physical;

    [Header("Consumption")]
    [SerializeField] private bool consumeStacks = false;
    [SerializeField] private bool consumeAll = true;
    [SerializeField] private int stacksToConsume = 1;

    [Header("Extra Effects")]
    [SerializeField] private List<Buff> debuffsToApply;
    [SerializeField, Range(0f, 1f)] private float damageToPartyHealPercent = 0f;

    [Header("Repeat Attack")]
    [SerializeField] private bool repeatIfEnoughStacks = false;
    [SerializeField] private int repeatStackThreshold = 5;
    [SerializeField] private int repeatStackCost = 2;

    private float _finalBaseDamage;
    private int _repeatCount;

    public override bool IsElementalAttackEffect => true;

    public override void OnSkillStarted(Entity caster, List<Entity> targets, CombatActionLog log)
    {
        if (caster == null || stackSourceBuff == null) return;

        ActiveBuff activeStacks = caster.buffController.GetBuffByName(stackSourceBuff.BuffName);
        int stackCount = activeStacks != null ? activeStacks.CurrentStack : 0;

        _finalBaseDamage = baseDamage + (stackCount * damagePerStack);
        
        _repeatCount = 0;
        if (repeatIfEnoughStacks && stackCount >= repeatStackThreshold)
        {
            int remaining = stackCount;
            while (remaining >= repeatStackThreshold + repeatStackCost)
            {
                _repeatCount++;
                remaining -= repeatStackCost;
            }
        }

        if (consumeStacks && activeStacks != null)
        {
            int toConsume = consumeAll ? activeStacks.CurrentStack : stacksToConsume;
            
            // Add repeated attack costs to total consumption
            if (repeatIfEnoughStacks)
            {
                toConsume += (_repeatCount * repeatStackCost);
                if (activeStacks.Data.MaxStack > 0 && toConsume > activeStacks.Data.MaxStack)
                {
                    // Protection against consuming more than possible, though unlikely here
                }
            }

            caster.buffController.ConsumeBuffStack(activeStacks, toConsume);
        }
    }

    public override bool Execute(Entity caster, Entity target, CombatActionLog log, SkillStyle style = SkillStyle.None)
    {
        if (caster == null || target == null) return false;

        // 1. Initial Attack
        DealDamage(caster, target, _finalBaseDamage, log, style);

        // 2. Repeat Attacks
        for (int i = 0; i < _repeatCount; i++)
        {
            DealDamage(caster, target, _finalBaseDamage, log, style);
        }

        // 3. Apply Debuffs
        if (debuffsToApply != null && debuffsToApply.Count > 0)
        {
            foreach (var debuff in debuffsToApply)
            {
                target.buffController.AddBuff(debuff);
                log.AddBuffEffectLog(new BuffEffectLog()
                {
                    AppliedTargetID = target.GetEntityID(),
                    AppliedTarget = target.Stats.EntityName,
                    Buff = new BuffEffectData(debuff)
                });
            }
        }

        return true;
    }

    private void DealDamage(Entity caster, Entity target, float amount, CombatActionLog log, SkillStyle style)
    {
        float variance = Random.Range(0.85f, 1.15f);
        float damageValue = amount * variance;
        
        float healthBefore = target.CurrentHealth;

        Damage dmg = new Damage(damageValue, Element, false, 0.05f, 1.5f);
        DamageCtx ctx = new DamageCtx(caster, target, dmg, style, log);
        DamageSystem.Process(ctx, log);

        float actualDamage = healthBefore - target.CurrentHealth;

        if (damageToPartyHealPercent > 0 && actualDamage > 0)
        {
            float healAmount = actualDamage * damageToPartyHealPercent;
            List<Entity> allies = GetAliveAllies(caster);
            
            foreach(var ally in allies)
            {
                ally.Heal(healAmount);
                log.AddHealEffectLog(new HealEffectLog()
                {
                    AppliedTarget = ally.Stats.EntityName,
                    AppliedTargetID = ally.GetEntityID(),
                    HealAmount = healAmount
                });
            }
        }
    }

    private List<Entity> GetAliveAllies(Entity caster)
    {
        if (caster is PlayerEntity)
        {
            return PlayerTeamManager.Instance != null ? PlayerTeamManager.Instance.GetAliveMembers().ConvertAll(p => (Entity)p) : new List<Entity> { caster };
        }
        return new List<Entity> { caster };
    }
}
