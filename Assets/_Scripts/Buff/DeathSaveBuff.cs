using UnityEngine;

[CreateAssetMenu(fileName = "DeathSaveBuff", menuName = "ScriptableObjects/Buff/DeathSaveBuff")]
public class DeathSaveBuff : Buff
{
    [Header("Death Save")]
    [Range(0f, 100f)]
    public float AvoidDeathChance = 25f;

    [Tooltip("Extra chance per stack of this buff (beyond the first stack).")]
    public float AvoidDeathChancePerStack = 0f;

    [Tooltip("If enabled, consume 1 stack when the death save triggers.")]
    public bool ConsumeStackOnTrigger = true;

    [Header("Rest-of-Turn Immunity")]
    [Tooltip("If enabled, enemies that trigger the death save will ignore all hits for the rest of their turn.")]
    public bool GrantEnemyImmunityForRestOfTurn = true;

    private const string ImmunityStateKey = "DeathSaveBuff_ImmuneThisTurn";

    public float GetChance(ActiveBuff buffState)
    {
        if (buffState == null)
        {
            return 0f;
        }

        float bonus = Mathf.Max(0, buffState.CurrentStack - 1) * AvoidDeathChancePerStack;
        return Mathf.Clamp(AvoidDeathChance + bonus, 0f, 100f);
    }

    public void SetRestOfTurnImmunity(ActiveBuff buffState)
    {
        if (buffState == null)
        {
            return;
        }

        buffState.CustomState[ImmunityStateKey] = true;
    }

    public bool HasRestOfTurnImmunity(ActiveBuff buffState)
    {
        if (buffState == null)
        {
            return false;
        }

        if (buffState.CustomState.TryGetValue(ImmunityStateKey, out object value) && value is bool active)
        {
            return active;
        }

        return false;
    }

    public override void OnTurnEnd(Entity owner, ActiveBuff buffState)
    {
        base.OnTurnEnd(owner, buffState);

        if (buffState == null)
        {
            return;
        }

        if (buffState.CustomState.ContainsKey(ImmunityStateKey))
        {
            buffState.CustomState.Remove(ImmunityStateKey);
        }
    }
}
