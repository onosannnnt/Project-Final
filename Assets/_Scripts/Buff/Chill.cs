using UnityEngine;

[CreateAssetMenu(fileName = "Buff", menuName = "ScriptableObjects/Buff/Chill")]
public class Chill : Buff
{
    public override void OnApply(Entity owner)
    {
        base.OnApply(owner);
        Buff chilledBuff = owner.buffController.GetBuffByName("Chill");
        Debug.Log($"{owner.gameObject.name} has {chilledBuff.Stack} stacks of Chilled and is freeze {chilledBuff.Stack >= Threshold}");
        if (chilledBuff.Stack >= Threshold)
        {
            owner.buffController.removeBuff(this);
            Debug.Log("Applied freeze");
        }
    }

}