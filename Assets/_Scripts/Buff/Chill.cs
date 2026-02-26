using UnityEngine;

[CreateAssetMenu(fileName = "Buff", menuName = "ScriptableObjects/Buff/Chill")]
public class Chill : Buff
{
    [SerializeField] private Buff FreezeBuff;
    public override void OnApply(Entity owner)
    {
        base.OnApply(owner);
        Buff chilledBuff = owner.buffController.GetBuffByName("Chill");
        if (chilledBuff == null) return;
        // if (chilledBuff.Stack >= Threshold)
        // {
        //     owner.buffController.RemoveBuff(chilledBuff);
        //     owner.buffController.AddBuff(FreezeBuff);
        // }
    }

}