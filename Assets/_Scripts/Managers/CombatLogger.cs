using UnityEngine;

public class CombatLogger : MonoBehaviour
{
    public void ShowLog(CombatActionLog log)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        // 1. หัวข้อ: [Turn X] ใคร ทำอะไร
        sb.Append($"<color=#4EC9B0>{log.CasterName}</color> ใช้ <b>{log.SkillName}</b> ➜ ");

        // 2. รวบรวมผลลัพธ์ (Damage, Heal, Buff) มาต่อท้ายในบรรทัดเดียว
        bool hasEffect = false;

        // เช็ค Damage
        if (log.DamageEffectLogs != null && log.DamageEffectLogs.Count > 0)
        {
            foreach (var dmg in log.DamageEffectLogs)
            {
                sb.Append($"[<color=red>{dmg.AppliedTarget} -{dmg.Damage}</color>] ");
            }
            hasEffect = true;
        }

        // เช็ค Heal
        if (log.HealEffectLogs != null && log.HealEffectLogs.Count > 0)
        {
            foreach (var heal in log.HealEffectLogs)
            {
                sb.Append($"[<color=green>{heal.AppliedTarget} +{heal.HealAmount}</color>] ");
            }
            hasEffect = true;
        }

        // เช็ค Buff/Debuff
        if (log.BuffEffectLogs != null && log.BuffEffectLogs.Count > 0)
        {
            foreach (var buff in log.BuffEffectLogs)
            {
                sb.Append($"[<color=yellow>{buff.AppliedTarget} {buff.Buff}</color>] ");
            }
            hasEffect = true;
        }

        if (!hasEffect) sb.Append("<i>(ไม่มีผลกระทบ)</i>");

        // ปริ้นท์ออก Console บรรทัดเดียวจบ!
        Debug.Log(sb.ToString());
    }
}
