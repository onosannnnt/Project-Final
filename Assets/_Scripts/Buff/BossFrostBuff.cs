using UnityEngine;

[CreateAssetMenu(fileName = "BossIceBuff", menuName = "ScriptableObjects/Buff/BossIceBuff")]
public class BossFrostBuff : Buff
{
    [Header("Ice Scaling Settings (Flat Heal)")]
    public float BaseHealFlat = 50f;
    public float StepHealFlat = 50f;
    public float MaxHealFlat = 300f;

    public override void OnApply(Entity owner, ActiveBuff buffState)
    {
        base.OnApply(owner, buffState);
        buffState.CustomState["turnsActive"] = 0;
        buffState.CustomState["currentHeal"] = BaseHealFlat;
    }

    public override void OnTurnEnd(Entity owner, ActiveBuff buffState)
    {
        base.OnTurnEnd(owner, buffState);

        // 1. ฮีลตัวเองตอนจบเทิร์น
        float currentHeal = (float)buffState.CustomState["currentHeal"];

        // ==========================================
        // TODO: เรียกฟังก์ชันฮีลของเกมคุณตรงนี้ เช่น:
        // owner.Stats.Heal(currentHeal);
        // ==========================================
        Debug.Log($"<color=cyan>[Ice Buff]</color> บอสฟื้นฟูเลือด {currentHeal} หน่วย!");

        // 2. อัปเดตสเกลทุกๆ 2 เทิร์น
        int turns = (int)buffState.CustomState["turnsActive"] + 1;
        buffState.CustomState["turnsActive"] = turns;

        if (turns % 2 == 0)
        {
            float newValue = Mathf.Min(MaxHealFlat, currentHeal + StepHealFlat);
            buffState.CustomState["currentHeal"] = newValue;
            Debug.Log($"<color=cyan>[Ice Buff]</color> พลังน้ำแข็งแกร่งขึ้น! เทิร์นหน้าจะฮีล {newValue} หน่วย");
        }
    }
}