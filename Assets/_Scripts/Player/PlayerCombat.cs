using UnityEngine;

public class PlayerCombat : Entity
{
    protected override void Die()
    {
        Debug.Log("Player Died");
    }

}