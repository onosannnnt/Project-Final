using UnityEngine;

public class QuestRewardNpc : MonoBehaviour
{
    [SerializeField] private UserData userData;

    // Call this from your NPC talk interaction event.
    public void OnTalkToNpc()
    {
        ClaimPendingQuestReward();
    }

    public void ClaimPendingQuestReward()
    {
        if (userData == null)
        {
            Debug.LogWarning("UserData is not assigned on QuestRewardNpc.");
            return;
        }

        int claimedCoins;
        int rewardQuestIndex;
        if (userData.TryClaimPendingQuestCoins(out claimedCoins, out rewardQuestIndex))
        {
            string questName = userData.GetQuestDisplayName(rewardQuestIndex);
            Debug.Log("Claimed quest reward from " + questName + ": +" + claimedCoins + " coins. Total coins: " + userData.TotalCoins + ".");
        }
        else
        {
            Debug.Log("No pending quest coin reward to claim.");
        }
    }
}
