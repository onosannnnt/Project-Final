using UnityEngine;

[CreateAssetMenu(fileName = "BossLightningBuff", menuName = "ScriptableObjects/Buff/BossLightningBuff")]
public class BossLightningBuff : Buff
{
    [Header("Lightning Scaling Settings")]
    public float BaseExtraSPPercent = 0.10f; // เริ่มที่ 10%
    public float StepExtraSPPercent = 0.10f; // เพิ่มทีละ 10%
    public float MaxExtraSPPercent = 0.50f;  // ตันที่ 50%

    // ✅ ให้โค้ดฝั่ง Player ดึงตัวแปรนี้ไปใช้บวกค่า SP 
    public static float GlobalSpPenaltyPercent = 0f;

    public override void OnApply(Entity owner, ActiveBuff buffState)
    {
        base.OnApply(owner, buffState);
        buffState.CustomState["turnsActive"] = 0;
        buffState.CustomState["currentPenalty"] = BaseExtraSPPercent;

        GlobalSpPenaltyPercent = BaseExtraSPPercent;
    }

    public override void OnTurnEnd(Entity owner, ActiveBuff buffState)
    {
        base.OnTurnEnd(owner, buffState);

        int turns = (int)buffState.CustomState["turnsActive"] + 1;
        buffState.CustomState["turnsActive"] = turns;

        if (turns % 2 == 0)
        {
            float current = (float)buffState.CustomState["currentPenalty"];
            float newValue = Mathf.Min(MaxExtraSPPercent, current + StepExtraSPPercent);
            buffState.CustomState["currentPenalty"] = newValue;

            GlobalSpPenaltyPercent = newValue;
            Debug.Log($"<color=yellow>[Lightning Buff]</color> ไฟฟ้าลัดวงจร! ผู้เล่นใช้สกิลแพงขึ้น {newValue * 100}%");
        }
    }

    public override void OnRemove(Entity owner, ActiveBuff buffState)
    {
        base.OnRemove(owner, buffState);
        GlobalSpPenaltyPercent = 0f; // รีเซ็ตคืนเมื่อบัฟโดนลบ
    }
}