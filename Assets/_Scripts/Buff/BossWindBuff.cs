using UnityEngine;

[CreateAssetMenu(fileName = "BossWindBuff", menuName = "ScriptableObjects/Buff/BossWindBuff")]
public class BossWindBuff : Buff
{
    [Header("Wind Scaling Settings")]
    public float BaseMissChance = 5f; // เริ่มที่ 5%
    public float StepMissChance = 5f; // เพิ่มทีละ 5%
    public float MaxMissChance = 30f; // ตันที่ 30%

    public override void OnApply(Entity owner, ActiveBuff buffState)
    {
        base.OnApply(owner, buffState);
        buffState.CustomState["turnsActive"] = 0;
        buffState.CustomState["currentMissChance"] = BaseMissChance;

        WindDodgeModifier modifier = new WindDodgeModifier(buffState);
        buffState.CustomState["modifier"] = modifier;
        owner.AddModifier(modifier, EntityModifierType.Incoming);
    }

    public override void OnTurnEnd(Entity owner, ActiveBuff buffState)
    {
        base.OnTurnEnd(owner, buffState);

        int turns = (int)buffState.CustomState["turnsActive"] + 1;
        buffState.CustomState["turnsActive"] = turns;

        if (turns % 2 == 0)
        {
            float current = (float)buffState.CustomState["currentMissChance"];
            float newValue = Mathf.Min(MaxMissChance, current + StepMissChance);
            buffState.CustomState["currentMissChance"] = newValue;
            Debug.Log($"<color=green>[Wind Buff]</color> พายุแรงขึ้น! โอกาสหลบหลีกเพิ่มเป็น {newValue}%");
        }
    }

    public override void OnRemove(Entity owner, ActiveBuff buffState)
    {
        base.OnRemove(owner, buffState);
        if (buffState.CustomState.TryGetValue("modifier", out object mod))
        {
            owner.RemoveModifier((WindDodgeModifier)mod, EntityModifierType.Incoming);
        }
    }

    private class WindDodgeModifier : IDamageModifier
    {
        private ActiveBuff buffRef;
        public WindDodgeModifier(ActiveBuff buffRef) { this.buffRef = buffRef; }
        public void Modify(DamageCtx ctx)
        {
            float dodgeChance = (float)buffRef.CustomState["currentMissChance"];
            if (Random.Range(0f, 100f) < dodgeChance)
            {
                ctx.Damage.Amount = 0; // ปรับดาเมจเป็น 0
                Debug.Log("💨 บอสหลบการโจมตีได้ด้วยพลังแห่งลม!");
            }
        }
    }
}