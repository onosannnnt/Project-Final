using UnityEngine;

[CreateAssetMenu(fileName = "BossFireBuff", menuName = "ScriptableObjects/Buff/BossFireBuff")]
public class BossFireBuff : Buff
{
    [Header("Fire Scaling Settings")]
    public float BaseDamagePercent = 0.10f; // เริ่มมาตีแรงขึ้น 10% เลย
    public float StepPercent = 0.10f;       // เพิ่มทีละ 10%
    public float MaxPercent = 0.50f;        // ตันที่ 50%

    public override void OnApply(Entity owner, ActiveBuff buffState)
    {
        base.OnApply(owner, buffState);
        buffState.CustomState["turnsActive"] = 0;
        buffState.CustomState["currentBonus"] = BaseDamagePercent;

        FireDamageModifier modifier = new FireDamageModifier(buffState);
        buffState.CustomState["modifier"] = modifier;
        owner.AddModifier(modifier, EntityModifierType.Outgoing);
    }

    public override void OnTurnEnd(Entity owner, ActiveBuff buffState)
    {
        base.OnTurnEnd(owner, buffState);

        // นับเทิร์น
        int turns = (int)buffState.CustomState["turnsActive"] + 1;
        buffState.CustomState["turnsActive"] = turns;

        // สเกลทุกๆ 2 เทิร์น
        if (turns % 2 == 0)
        {
            float current = (float)buffState.CustomState["currentBonus"];
            float newValue = Mathf.Min(MaxPercent, current + StepPercent);
            buffState.CustomState["currentBonus"] = newValue;
            Debug.Log($"<color=red>[Fire Buff]</color> บอสโกรธขึ้น! ดาเมจโบนัสเพิ่มเป็น {newValue * 100}%");
        }
    }

    public override void OnRemove(Entity owner, ActiveBuff buffState)
    {
        base.OnRemove(owner, buffState);
        if (buffState.CustomState.TryGetValue("modifier", out object mod))
        {
            owner.RemoveModifier((FireDamageModifier)mod, EntityModifierType.Outgoing);
        }
    }

    private class FireDamageModifier : IDamageModifier
    {
        private ActiveBuff buffRef;
        public FireDamageModifier(ActiveBuff buffRef) { this.buffRef = buffRef; }
        public void Modify(DamageCtx ctx)
        {
            float bonus = (float)buffRef.CustomState["currentBonus"];
            ctx.Damage.Amount += (ctx.Damage.Amount * bonus);
        }
    }
}