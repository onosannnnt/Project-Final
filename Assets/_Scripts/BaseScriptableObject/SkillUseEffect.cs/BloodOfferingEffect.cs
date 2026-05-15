using UnityEngine;

[CreateAssetMenu(fileName = "BloodOfferingEffect", menuName = "ScriptableObjects/SkillEffect/BloodOfferingEffect")]
public class BloodOfferingEffect : SkillEffect
{
    public BloodOfferingBuff BuffTemplate;
    public float MaxConsumptionPercent = 0.3f;

    public override bool Execute(Entity caster, Entity target, CombatActionLog log, SkillStyle style = SkillStyle.None)
    {
        if (caster == null || target == null || BuffTemplate == null) return false;

        float maxHP = caster.GetStat(StatType.MaxHealth);
        float toConsume = Mathf.Min(caster.CorruptedHealth, maxHP * MaxConsumptionPercent);
        
        bool consumed = caster.TryConsumeCorruptedHealth(toConsume);
        
        target.buffController.AddBuff(BuffTemplate);
        
        // Find the newly added buff to set custom state
        foreach(var buff in target.buffController.GetAllBuffs())
        {
            if (buff.Data == BuffTemplate)
            {
                if (toConsume > 0) buff.CustomState["DidConsume"] = true;
                buff.CustomState["CasterEntity"] = caster;
                
                if (buff.CustomState.TryGetValue("BloodOfferingMod", out object modObj) && modObj is IDamageModifier mod)
                {
                    if (caster != target) caster.AddModifier(mod, EntityModifierType.Incoming);
                }
                break;
            }
        }
        
        return true;
    }
}
