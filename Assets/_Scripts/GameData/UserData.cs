using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "UserData", menuName = "ScriptableObjects/GameData/UserData"),]
public class UserData : ScriptableObject
{
    public const int NoQuestSelected = -1;
    public const int TutorialQuestIndex = 0;
    public const int Quest1Index = 1;
    public const int Quest2Index = 2;
    public const int Quest3Index = 3;
    public const int LastQuestIndex = Quest3Index;

    public int ID;
    public string Username;
    [Tooltip("Current game progression phase (e.g., 1, 2, 3)")]
    public int GamePhase = 1;
    [Tooltip("Selected quest index. -1 means no quest is selected yet.")]
    public int SelectedQuestIndex = NoQuestSelected;

    [Tooltip("Highest completed quest index. -1 means no quest has been completed yet.")]
    public int HighestCompletedQuestIndex = -1;

    [Tooltip("True after Quest 3 is completed and the run is finished.")]
    public bool IsGameFinished;

    [Tooltip("Total coins collected from completed quests.")]
    public int TotalCoins;

    [Tooltip("Coins waiting to be claimed by talking to the quest NPC.")]
    public int PendingQuestCoins;

    [Tooltip("Last quest index that generated a pending reward. -1 means no pending reward.")]
    public int PendingRewardQuestIndex = -1;

    [Header("Coin Reward Per Quest")]
    [SerializeField] private int tutorialCoinReward = 50;
    [SerializeField] private int quest1CoinReward = 100;
    [SerializeField] private int quest2CoinReward = 200;
    [SerializeField] private int quest3CoinReward = 300;

    [Header("Player Loadouts")]
    [Tooltip("ใส่ไฟล์ SkillLoadout ของผู้เล่นที่ต้องการให้ล้างค่าตอนเริ่มเกมใหม่")]
    public List<SkillLoadout> playerLoadoutsToReset = new List<SkillLoadout>();

    [Header("Player Skills")]
    [Tooltip("รายชื่อสกิลที่ผู้เล่นซื้อหรือปลดล็อคแล้ว")]
    public List<Skill> OwnedSkills = new List<Skill>();

    // เช็คว่ามีสกิลนี้หรือยัง
    public bool HasSkill(Skill skill)
    {
        return OwnedSkills.Contains(skill);
    }

    // เพิ่มสกิลเข้าตัวผู้เล่น
    public void UnlockSkill(Skill skill)
    {
        if (!HasSkill(skill))
        {
            OwnedSkills.Add(skill);
        }
    }

    // ฟังก์ชันสำหรับจ่ายเงิน
    public bool TrySpendCoins(int amount)
    {
        if (TotalCoins >= amount)
        {
            TotalCoins -= amount;
            return true;
        }
        return false;
    }

    [ContextMenu("Reset Game Progress")]
    public void ResetProgression()
    {
        GamePhase = 1;
        SelectedQuestIndex = NoQuestSelected;
        HighestCompletedQuestIndex = -1;
        IsGameFinished = false;
        TotalCoins = 0;
        PendingQuestCoins = 0;
        PendingRewardQuestIndex = -1;

        // 1. ลบสกิลที่มี
        if (OwnedSkills != null)
        {
            OwnedSkills.Clear();
        }

        // ==========================================
        // 2. ล้างช่องสวมใส่สกิล (Loadout)
        // ==========================================
        if (playerLoadoutsToReset != null)
        {
            foreach (var loadout in playerLoadoutsToReset)
            {
                if (loadout != null && loadout.EquippedSkills != null)
                {
                    loadout.EquippedSkills.Clear();

                    // บังคับให้ Unity เซฟค่าการล้างช่องลงไฟล์ด้วย (สำคัญมาก ไม่งั้นตอนกด Play ค่าอาจจะกลับมา)
#if UNITY_EDITOR
                    UnityEditor.EditorUtility.SetDirty(loadout);
#endif
                }
            }
        }
        // ==========================================

        Debug.Log("Game progress and loadouts have been reset via Inspector!");
    }

    public bool IsQuestCompleted(int questIndex)
    {
        return questIndex <= HighestCompletedQuestIndex;
    }

    public bool IsQuestUnlocked(int questIndex)
    {
        if (questIndex < TutorialQuestIndex || questIndex > LastQuestIndex)
        {
            return false;
        }

        // Only the immediate next quest is unlocked.
        return questIndex == HighestCompletedQuestIndex + 1;
    }

    public bool CanStartQuest(int questIndex)
    {
        if (IsGameFinished)
        {
            return false;
        }

        return IsQuestUnlocked(questIndex) && !IsQuestCompleted(questIndex);
    }

    public bool TrySetSelectedQuest(int questIndex)
    {
        if (!CanStartQuest(questIndex))
        {
            return false;
        }

        SelectedQuestIndex = questIndex;
        return true;
    }

    public bool HasSelectedQuest()
    {
        return CanStartQuest(SelectedQuestIndex);
    }

    public void ClearSelectedQuest()
    {
        SelectedQuestIndex = NoQuestSelected;
    }

    public bool TryCompleteSelectedQuest(out bool didAdvancePhase, out bool didFinishGame)
    {
        int ignoredCoins;
        return TryCompleteSelectedQuest(out didAdvancePhase, out didFinishGame, out ignoredCoins);
    }

    public bool TryCompleteSelectedQuest(out bool didAdvancePhase, out bool didFinishGame, out int earnedCoins)
    {
        didAdvancePhase = false;
        didFinishGame = false;
        earnedCoins = 0;

        int questIndex = SelectedQuestIndex;
        if (!CanStartQuest(questIndex))
        {
            return false;
        }

        HighestCompletedQuestIndex = questIndex;

        if (questIndex == Quest1Index)
        {
            if (GamePhase < 2)
            {
                GamePhase = 2;
                didAdvancePhase = true;
            }
        }
        else if (questIndex == Quest2Index)
        {
            if (GamePhase < 3)
            {
                GamePhase = 3;
                didAdvancePhase = true;
            }
        }
        else if (questIndex == Quest3Index)
        {
            IsGameFinished = true;
            didFinishGame = true;
        }

        earnedCoins = GetQuestCoinReward(questIndex);
        AddPendingQuestCoins(earnedCoins, questIndex);

        ClearSelectedQuest();
        return true;
    }

    public int GetQuestCoinReward(int questIndex)
    {
        switch (questIndex)
        {
            case TutorialQuestIndex:
                return tutorialCoinReward;
            case Quest1Index:
                return quest1CoinReward;
            case Quest2Index:
                return quest2CoinReward;
            case Quest3Index:
                return quest3CoinReward;
            default:
                return 0;
        }
    }

    public void AddCoins(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        TotalCoins += amount;
    }

    public void AddPendingQuestCoins(int amount, int questIndex)
    {
        if (amount <= 0)
        {
            return;
        }

        PendingQuestCoins += amount;
        PendingRewardQuestIndex = questIndex;
    }

    public bool HasPendingQuestReward()
    {
        return PendingQuestCoins > 0;
    }

    public bool TryClaimPendingQuestCoins(out int claimedCoins)
    {
        int ignoredQuestIndex;
        return TryClaimPendingQuestCoins(out claimedCoins, out ignoredQuestIndex);
    }

    public bool TryClaimPendingQuestCoins(out int claimedCoins, out int rewardQuestIndex)
    {
        claimedCoins = 0;
        rewardQuestIndex = -1;

        if (PendingQuestCoins <= 0)
        {
            return false;
        }

        claimedCoins = PendingQuestCoins;
        rewardQuestIndex = PendingRewardQuestIndex;

        AddCoins(claimedCoins);

        PendingQuestCoins = 0;
        PendingRewardQuestIndex = -1;
        return true;
    }

    public int GetCurrentUnlockedQuestIndex()
    {
        return Mathf.Clamp(HighestCompletedQuestIndex + 1, TutorialQuestIndex, LastQuestIndex);
    }

    public int GetCurrentInProgressQuestIndex()
    {
        if (HasSelectedQuest())
        {
            return SelectedQuestIndex;
        }

        return NoQuestSelected;
    }

    public string GetQuestDisplayName(int questIndex)
    {
        switch (questIndex)
        {
            case NoQuestSelected:
                return "None";
            case TutorialQuestIndex:
                return "Tutorial";
            case Quest1Index:
                return "Quest 1";
            case Quest2Index:
                return "Quest 2";
            case Quest3Index:
                return "Quest 3";
            default:
                return "Unknown Quest";
        }
    }

    public string GetQuestBlockedReason(int questIndex)
    {
        if (IsGameFinished)
        {
            return "Game is already finished.";
        }

        if (questIndex < TutorialQuestIndex || questIndex > LastQuestIndex)
        {
            return "Quest index is out of range.";
        }

        if (IsQuestCompleted(questIndex))
        {
            return "Quest is already completed and cannot be replayed.";
        }

        if (!IsQuestUnlocked(questIndex))
        {
            return "Quest is locked. Complete the current quest first.";
        }

        return string.Empty;
    }
}
