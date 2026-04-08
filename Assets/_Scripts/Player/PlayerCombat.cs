using System;
using UnityEngine;

public class PlayerCombat : PlayerEntity
{
    [SerializeField] private UserData userData;
    public static PlayerCombat instance;
    protected override void Awake()
    {
        base.Awake();
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void SetUserData(UserData data)
    {
        userData = data;
        ID = data.ID;
        Stats.EntityName = data.Username;

    }
    public UserData GetUserData()
    {
        return userData;
    }
    protected override void Die()
    {
// // Debug.Log("Player Died");
    }
    public override void TakeDamage(Damage damage)
    {
        base.TakeDamage(damage);
        // HealthbarUI.Instance.UpdateHealthBar();
    }
    public void SelectSkill(Skill skill)
    {
        selectedSkill = skill;
    }
}
