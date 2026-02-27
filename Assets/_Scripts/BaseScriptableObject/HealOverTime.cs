using UnityEngine;

[CreateAssetMenu(fileName = "HealOverTimeEffect", menuName = "ScriptableObjects/Buff/HealEffectOverTime")]

public class HealOverTimeEffect : Buff
{
    public float healAmount;
    public StatScale healScale;
    public ModifierType healModifierType;

    public override void OnTurnStart(Entity owner)
    {
        float totalHeal = 0f;
        StatType scalingStat = Utils.GetScalingStat(healScale);
        float baseStat = owner.GetStat(scalingStat);

        if (StackCalculationType == StackMultiplierType.Linear)
        {
            if (healModifierType == ModifierType.Flat)
            {
                totalHeal = healAmount * Stack;
            }
            else if (healModifierType == ModifierType.Percent)
            {
                totalHeal = baseStat * healAmount * Stack;
            }
        }
        else if (StackCalculationType == StackMultiplierType.DiminishingReturn)
        {
            float effectiveStack = Stack / (Stack + Mathf.Max(Threshold, 1));

            if (healModifierType == ModifierType.Flat)
            {
                totalHeal = healAmount * effectiveStack;
            }
            else if (healModifierType == ModifierType.Percent)
            {
                totalHeal = baseStat * healAmount * effectiveStack;
            }
        }
        Debug.Log($"{owner.gameObject.name} heals {totalHeal} HP from {name} HOT.");
        owner.Heal(totalHeal);
    }
}