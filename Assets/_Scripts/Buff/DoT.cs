using System.Runtime.InteropServices;
using UnityEngine;

[CreateAssetMenu(fileName = "DOT", menuName = "ScriptableObjects/Buff/DOT")]

public class DOT : Buff
{
    //TODO: ทำส่วนที่เพิ่มลด damage จาก dot เช่น inc dot, inc ignite, inc bleed etc.

    public float value;
    public StatScale statScale;
    public ModifierType damageModifierType;
    public DamageType damageType;
    public override void OnTurnStart(Entity owner, CombatActionLog log)
    {
        float totalDamage = 0f;
        StatType scalingStat = Utils.GetScalingStat(statScale);
        float baseStat = owner.GetStat(scalingStat);


        if (StackCalculationType == StackMultiplierType.Linear)
        {
            if (damageModifierType == ModifierType.Flat)
            {
                totalDamage = value * Stack;
            }
            else if (damageModifierType == ModifierType.Percent)
            {
                totalDamage = baseStat * (1 + value * Stack);
            }
        }
        else if (StackCalculationType == StackMultiplierType.DiminishingReturn)
        {
            float effectiveStack = Stack / (Stack + Mathf.Max(Threshold, 1));

            if (damageModifierType == ModifierType.Flat)
            {
                totalDamage = value * effectiveStack;
            }
            else if (damageModifierType == ModifierType.Percent)
            {
                totalDamage = baseStat * (1 + value * effectiveStack);
            }
        }
        Debug.Log($"{owner.gameObject.name} takes {totalDamage} {damageType} damage from {name} DOT.");
        Damage damage = new Damage(damageType, totalDamage);
        DamageCtx ctx = new DamageCtx(owner, owner, damage);
        DamageSystem.Process(ctx, log);
    }

}