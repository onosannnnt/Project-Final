using System.Collections.Generic;
using UnityEngine;

public class CombatResourceManager : MonoBehaviour
{
    [Header("Skill Point Restoration per Phase")]
    [SerializeField] private int phase1SPRestore = 10;
    [SerializeField] private int phase2SPRestore = 20;
    [SerializeField] private int phase3SPRestore = 30;

    [Header("Team Resource Settings")]
    [SerializeField] private bool useSharedPlayerSkillPointPool = true;
    public bool UseSharedPlayerSkillPointPool => useSharedPlayerSkillPointPool;

    private UserData userData;

    public void Initialize(UserData data)
    {
        userData = data;
    }

    public int GetCurrentPhase()
    {
        if (userData != null) return userData.GamePhase;
        return 1;
    }

    public void RestoreSkillPointsByPhase(List<GameObject> allEnemies)
    {
        int currentPhase = GetCurrentPhase();
        int spToRestore = 0;

        switch (currentPhase)
        {
            case 1: spToRestore = phase1SPRestore; break;
            case 2: spToRestore = phase2SPRestore; break;
            case 3: spToRestore = phase3SPRestore; break;
            default: spToRestore = 0; break;
        }

        if (spToRestore > 0)
        {
            if (useSharedPlayerSkillPointPool)
            {
                RestoreSharedPlayerSkillPoints(spToRestore);
            }
            else
            {
                if (PlayerTeamManager.Instance != null)
                {
                    foreach (var member in PlayerTeamManager.Instance.GetAliveMembers())
                    {
                        member.SetSP(spToRestore);
                    }
                }
            }

            foreach (GameObject enemyObj in allEnemies)
            {
                EnemyCombat enemy = enemyObj.GetComponent<EnemyCombat>();
                if (enemy != null && !enemy.IsDead())
                {
                    enemy.SetSP(spToRestore);
                }
            }
        }
    }

    public void SyncSharedPlayerSkillPoints()
    {
        if (!useSharedPlayerSkillPointPool) return;
        int currentShared = GetSharedPlayerCurrentSkillPoints();
        SetSharedPlayerSkillPoints(currentShared);
    }

    public bool TryConsumeSharedPlayerSkillPoints(int cost)
    {
        if (!useSharedPlayerSkillPointPool) return false;
        int currentShared = GetSharedPlayerCurrentSkillPoints();
        if (currentShared < cost) return false;

        SetSharedPlayerSkillPoints(currentShared - cost);
        return true;
    }

    public void RestoreSharedPlayerSkillPoints(int amount)
    {
        int currentShared = GetSharedPlayerCurrentSkillPoints();
        SetSharedPlayerSkillPoints(currentShared + amount);
    }

    public int GetSharedPlayerCurrentSkillPoints()
    {
        if (PlayerTeamManager.Instance == null) return 0;

        foreach (var member in PlayerTeamManager.Instance.ActiveTeamMembers)
        {
            if (member != null) return member.CurrentSP;
        }

        return 0;
    }

    public int GetSharedPlayerMaxSkillPoints()
    {
        if (PlayerTeamManager.Instance == null) return 0;

        int minMaxSp = int.MaxValue;
        foreach (var member in PlayerTeamManager.Instance.ActiveTeamMembers)
        {
            if (member == null) continue;
            int memberMaxSp = Mathf.RoundToInt(member.GetStat(StatType.MaxSkillPoint));
            minMaxSp = Mathf.Min(minMaxSp, memberMaxSp);
        }

        return minMaxSp == int.MaxValue ? 0 : minMaxSp;
    }

    public void SetSharedPlayerSkillPoints(int value)
    {
        if (PlayerTeamManager.Instance == null) return;

        int maxShared = GetSharedPlayerMaxSkillPoints();
        int clampedShared = Mathf.Clamp(value, 0, maxShared);

        foreach (var member in PlayerTeamManager.Instance.ActiveTeamMembers)
        {
            if (member == null) continue;
            member.SetSPInternal(clampedShared, true);
        }
    }

    public void ApplyPhaseStats(int currentWave)
    {
        int phase = GetCurrentPhase();
        if (PlayerTeamManager.Instance == null || PlayerTeamManager.Instance.ActiveTeamMembers.Count == 0) return;

        // Apply to all active players
        foreach (var member in PlayerTeamManager.Instance.ActiveTeamMembers)
        {
            if (member == null || member.Stats == null) continue;

            switch (phase)
            {
                case 1:
                    member.Stats.SetBase(StatType.MaxHealth, 1000);
                    member.Stats.SetBase(StatType.MaxSkillPoint, 100);
                    break;
                case 2:
                    member.Stats.SetBase(StatType.MaxHealth, 2000);
                    member.Stats.SetBase(StatType.MaxSkillPoint, 200);
                    break;
                case 3:
                    member.Stats.SetBase(StatType.MaxHealth, 3000);
                    member.Stats.SetBase(StatType.MaxSkillPoint, 300);
                    break;
                default:
                    member.Stats.SetBase(StatType.MaxHealth, 3000);
                    member.Stats.SetBase(StatType.MaxSkillPoint, 300);
                    break;
            }
        }

        // Heal to full HP/SP to reflect the new max values if it's the start of battle
        // Or if it's a tutorial quest and not the last wave
        bool isTutorial = false;
        if (EnemyGenerator.Instance != null)
        {
            QuestEnemies currentQuest = EnemyGenerator.Instance.GetCurrentQuest();
            if (currentQuest != null && currentQuest.isTutorial)
            {
                isTutorial = true;
            }
        }

        bool isStartOfBattle = (currentWave == 1);

        if (isStartOfBattle || isTutorial)
        {
            foreach (var member in PlayerTeamManager.Instance.ActiveTeamMembers)
            {
                if (member == null) continue;
                member.Heal(member.GetStat(StatType.MaxHealth));
                member.SetSPInternal((int)member.GetStat(StatType.MaxSkillPoint), true);
            }
        }
    }
}
