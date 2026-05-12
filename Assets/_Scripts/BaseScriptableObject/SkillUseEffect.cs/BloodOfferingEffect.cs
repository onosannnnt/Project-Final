using UnityEngine;

[CreateAssetMenu(fileName = "BloodOfferingEffect", menuName = "ScriptableObjects/SkillEffect/BloodOfferingEffect")]
public class BloodOfferingEffect : SkillEffect
{
    public BloodOfferingBuff BuffTemplate;
    public float MaxConsumptionPercent = 0.3f;

    public override bool Execute(Entity caster, Entity target, CombatActionLog log, SkillStyle style = SkillStyle.None)
    {
        if (caster == null || BuffTemplate == null) return false;

        float maxHP = caster.GetStat(StatType.MaxHealth);
        float toConsume = Mathf.Min(caster.CorruptedHealth, maxHP * MaxConsumptionPercent);
        
        bool consumed = caster.TryConsumeCorruptedHealth(toConsume);
        
        caster.buffController.AddBuff(BuffTemplate);
        
        // Find the newly added buff to set custom state
        foreach(var buff in caster.buffController.GetAllBuffs())
        {
            if (buff.Data == BuffTemplate)
            {
                if (toConsume > 0) buff.CustomState["DidConsume"] = true;
                break;
            }
        }
        
        return true;
    }
}
