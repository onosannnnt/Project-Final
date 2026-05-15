using UnityEngine;

[CreateAssetMenu(fileName = "CorruptHealthEffect", menuName = "ScriptableObjects/SkillEffect/CorruptHealthEffect")]
public class CorruptHealthEffect : SkillEffect
{
    public enum CorruptAction { Increase, Decrease }

    [SerializeField] private CorruptAction action = CorruptAction.Increase;
    [SerializeField] private float flatAmount;
    [SerializeField, Range(0f, 1f)] private float percentOfMaxHP;
    [SerializeField] private bool targetCaster = true;

    public override bool Execute(Entity caster, Entity target, CombatActionLog log, SkillStyle style = SkillStyle.None)
    {
        Entity effectTarget = targetCaster ? caster : target;
        if (effectTarget == null) return false;

        float maxHP = effectTarget.GetStat(StatType.MaxHealth);
        float amount = Mathf.Abs(flatAmount + (maxHP * percentOfMaxHP));

        if (action == CorruptAction.Increase)
        {
            effectTarget.GainCorruptedHealth(amount);
        }
        else
        {
            effectTarget.RemoveCorruptedHealth(amount);
        }

        return true;
    }
}
