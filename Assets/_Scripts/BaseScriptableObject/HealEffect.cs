
using UnityEngine;

[CreateAssetMenu(fileName = "HealEffect", menuName = "ScriptableObjects/SkillEffect/HealEffect")]
public class HealEffect : SkillEffect
{
    public float HealAmount;
    public StatScale healScale;
    public ModifierType healModifierType;

    public override void Execute(Entity caster, Entity target, CombatActionLog log)
    {
        Debug.Log(caster.gameObject.name + " used HealEffect on " + target.gameObject.name);
        float finalHeal = HealAmount;
        switch (healModifierType)
        {
            case ModifierType.Flat:
                finalHeal = HealAmount;
                break;
            case ModifierType.Percent:
                StatType scalingStat = Utils.GetScalingStat(healScale);
                float baseStat = caster.GetStat(scalingStat);
                finalHeal = baseStat * HealAmount;
                break;
        }
        caster.Heal(finalHeal);
        log.AddHealEffectLog(new HealEffectLog()
        {
            AppliedTarget = caster.Stats.EntityName,
            AppliedTargetID = caster.GetEntityID(),
            HealAmount = finalHeal
        });
    }
}