using System;
using UnityEngine;

public class PlayerCombat : PlayerEntity
{
    [SerializeField] private UserData userData;
    [SerializeField] private bool registerAsMainPlayer = true;
    public static PlayerCombat instance;
    protected override void Awake()
    {
        base.Awake();
        // Keep backward compatibility for systems that still read PlayerCombat.instance,
        // but allow additional playable characters to exist.
        if (registerAsMainPlayer && instance == null)
        {
            instance = this;
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
