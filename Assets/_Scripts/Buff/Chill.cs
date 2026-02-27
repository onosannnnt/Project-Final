using UnityEngine;

[CreateAssetMenu(fileName = "Buff", menuName = "ScriptableObjects/Buff/Chill")]
public class Chill : Buff
{
    [SerializeField] private Buff FreezeBuff;
    public override void OnApply(Entity owner)
    {
        base.OnApply(owner);
        Buff chilledBuff = owner.buffController.GetBuffByName("Chill");
        Debug.Log(owner.gameObject.name + " has " + chilledBuff.Stack + " stacks of Chill.");
        if (chilledBuff == null) return;
        if (chilledBuff.Stack >= Threshold)
        {
            Debug.Log(owner.gameObject.name + " has reached the threshold for Chill buff. Removing Chill and applying Freeze.");
            owner.buffController.RemoveBuff(chilledBuff);
            owner.buffController.AddBuff(FreezeBuff);
        }
    }

}