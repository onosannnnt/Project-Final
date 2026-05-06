using UnityEngine;

[CreateAssetMenu(fileName = "CorruptHealthEffect", menuName = "ScriptableObjects/SkillEffect/CorruptHealthEffect")]
public class CorruptHealthEffect : SkillEffect
{
    [SerializeField] private float flatAmount;
    [SerializeField] private float percentOfMaxHP;
    [SerializeField] private bool targetCaster = true;

    public override bool Execute(Entity caster, Entity target, CombatActionLog log)
    {
        Entity effectTarget = targetCaster ? caster : target;
        if (effectTarget == null) return false;

        float maxHP = effectTarget.GetStat(StatType.MaxHealth);
        float amount = flatAmount + (maxHP * (percentOfMaxHP / 100f));

        if (amount > 0)
        {
            effectTarget.GainCorruptedHealth(amount);
        }
        else if (amount < 0)
        {
            effectTarget.RemoveCorruptedHealth(-amount);
        }

        return true;
    }
}
