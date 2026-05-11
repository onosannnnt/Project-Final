using UnityEngine;

[CreateAssetMenu(fileName = "RepeatActionSkillBuff", menuName = "ScriptableObjects/Buff/RepeatActionSkillBuff")]
public class RepeatActionSkillBuff : Buff
{
    [Header("Repeat Action Settings")]
    [Tooltip("Chance to repeat the caster's action skill once immediately after use.")]
    [Range(0f, 100f)] public float RepeatChancePercent = 20f;

    [Tooltip("If true, consume 1 stack when the repeat triggers.")]
    public bool ConsumeStackOnTrigger = false;

    [Tooltip("If true, remove buff immediately when repeat triggers.")]
    public bool RemoveBuffOnTrigger = false;

    [Tooltip("If true, only skills with SkillType.Attack are allowed to repeat.")]
    public bool AttackSkillOnly = false;

    public bool CanRepeatSkill(Skill skill)
    {
        if (skill == null)
        {
            return false;
        }

        if (AttackSkillOnly && skill.skillType != SkillType.Attack)
        {
            return false;
        }

        return true;
    }

    public bool ShouldRepeat(ActiveBuff buffState)
    {
        if (buffState == null)
        {
            return false;
        }

        float chance = Mathf.Clamp(RepeatChancePercent, 0f, 100f);
        if (chance <= 0f)
        {
            return false;
        }

        return Random.Range(0f, 100f) <= chance;
    }

    public void OnRepeatTriggered(Entity owner, ActiveBuff buffState)
    {
        if (owner == null || owner.buffController == null || buffState == null)
        {
            return;
        }

        if (RemoveBuffOnTrigger)
        {
            owner.buffController.RemoveBuff(buffState);
            return;
        }

        if (ConsumeStackOnTrigger && buffState.CurrentStack > 0)
        {
            owner.buffController.ConsumeBuffStack(buffState, 1);
        }
    }
}
