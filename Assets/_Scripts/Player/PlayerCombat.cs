using Unity.VisualScripting;
using UnityEngine;

public class PlayerCombat : Entity
{
    private PlayerState playerState;

    protected override void Die()
    {
        Debug.Log("Player Died");
    }

    public void SelectSkill(Skill skill)
    {
        selectedSkill = skill;
    }
    private void HandleTargetSkill()
    {

    }
}