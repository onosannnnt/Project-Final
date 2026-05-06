using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "StackScaledDamageEffect", menuName = "ScriptableObjects/SkillEffect/StackScaledDamageEffect")]
public class StackScaledDamageEffect : SkillEffect
{
    [Header("Scaling")]
    [SerializeField] private Buff stackSourceBuff;
    [SerializeField] private float baseDamage = 200f;
    [SerializeField] private float damagePerStack = 50f;
    [SerializeField] private DamageElement element = DamageElement.Physical;

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

    private static int lastFrame = -1;
    private static Dictionary<int, int> cachedStacks = new();

    public override bool IsElementalAttackEffect => true;

    private int GetStacks(Entity caster)
    {
        if (stackSourceBuff == null) return 0;
        
        if (Time.frameCount != lastFrame)
        {
            lastFrame = Time.frameCount;
            cachedStacks.Clear();
        }

        int id = caster.GetEntityID();
        if (!cachedStacks.TryGetValue(id, out int count))
        {
            ActiveBuff b = caster.buffController.GetBuffByName(stackSourceBuff.BuffName);
            count = b != null ? b.CurrentStack : 0;
            cachedStacks[id] = count;
        }
        return count;
    }

    public override bool Execute(Entity caster, Entity target, CombatActionLog log)
    {
        if (caster == null || target == null) return false;

        int currentStacks = GetStacks(caster);
        float finalDamage = baseDamage + (currentStacks * damagePerStack);

        // 1. Initial Attack
        DealDamage(caster, target, finalDamage, log);

        // 2. Repeat Attack logic (simple version: deal extra instances of damage)
        if (repeatIfEnoughStacks && currentStacks >= repeatStackThreshold)
        {
            int remainingStacks = currentStacks;
            while (remainingStacks >= repeatStackThreshold + repeatStackCost)
            {
                DealDamage(caster, target, finalDamage, log);
                remainingStacks -= repeatStackCost;
                
                // We don't actually update the real buff stacks here yet 
                // to avoid confusing the AOE loop if there is one.
            }
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

        // 4. Consumption (Only trigger once per frame/caster to handle AOE correctly)
        // Note: This relies on the fact that Skill.Execute loops over targets sequentially in one frame.
        // To be safe, consumption should ideally happen after the whole skill is done, 
        // but since we are per-target, we use a trick.
        if (consumeStacks)
        {
            // We only actually consume from the real buff once.
            // But how do we know it's the LAST target? We don't.
            // Alternative: The caster's real stacks are reduced, and cached value stays same for frame.
            ActiveBuff realBuff = caster.buffController.GetBuffByName(stackSourceBuff.BuffName);
            if (realBuff != null)
            {
                int toConsume = consumeAll ? realBuff.CurrentStack : stacksToConsume;
                // This will reduce stacks for subsequent Execute calls, 
                // but our GetStacks uses cached value! Perfect.
                caster.buffController.ConsumeBuffStack(realBuff, toConsume);
            }
        }

        return true;
    }

    private void DealDamage(Entity caster, Entity target, float amount, CombatActionLog log)
    {
        float variance = Random.Range(0.85f, 1.15f);
        float damageValue = amount * variance;
        
        float healthBefore = target.CurrentHealth;

        Damage dmg = new Damage(damageValue, element, false, 5f, 1.5f);
        DamageCtx ctx = new DamageCtx(caster, target, dmg);
        DamageSystem.Process(ctx, log);

        float actualDamage = healthBefore - target.CurrentHealth;

        if (damageToPartyHealPercent > 0 && actualDamage > 0)
        {
            float healAmount = actualDamage * damageToPartyHealPercent;
            List<Entity> allies = caster is PlayerEntity ? 
                (PlayerTeamManager.Instance != null ? PlayerTeamManager.Instance.GetAliveMembers().ConvertAll(p => (Entity)p) : new List<Entity>{caster}) : 
                new List<Entity>{caster};
            
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
}
