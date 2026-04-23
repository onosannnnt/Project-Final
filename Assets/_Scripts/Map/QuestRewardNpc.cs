using UnityEngine;

public class QuestRewardNpc : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private UserData userData;

    [Header("Quest Panel")]
    [SerializeField] private QuestSelectionUI questSelectionUI;

    [Header("Interaction")]
    [SerializeField] private bool openQuestPanelOnTalk = true;
    [SerializeField] private bool claimRewardOnTalk;
    [SerializeField] private KeyCode fallbackInteractKey = KeyCode.E;
    [SerializeField] private string playerTag = "Player";

    [Header("Talk Prompt Sprite")]
    [SerializeField] private bool showTalkPromptSprite = true;
    [SerializeField] private GameObject talkPromptSprite;
    [SerializeField] private bool usePlayerPromptSprite;
    [SerializeField] private string playerPromptChildName = "ChangeMapIcon";
    [SerializeField] private bool billboardPromptSprite = true;

    [Header("NPC Sprite Billboard")]
    [SerializeField] private bool billboardNpcSprite = true;
    [SerializeField] private Transform npcSpriteTransform;

    private bool isPlayerInRange;
    private GameInput playerGameInput;
    private int playerColliderCount;
    private GameObject activePromptSprite;

    private void Awake()
    {
        TryResolveNpcSpriteTransform();

        if (talkPromptSprite != null)
        {
            talkPromptSprite.SetActive(false);
        }
    }

    private void Update()
    {
        UpdateNpcSpriteBillboard();

        if (!isPlayerInRange)
        {
            return;
        }

        UpdatePromptSpriteBillboard();

        if (IsInteractPressed())
        {
            OnTalkToNpc();
        }
    }

    private bool IsInteractPressed()
    {
        if (playerGameInput != null)
        {
            return playerGameInput.IsInteractPressed();
        }

        return Input.GetKeyDown(fallbackInteractKey);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsPlayerCollider(other))
        {
            return;
        }

        playerColliderCount++;
        isPlayerInRange = playerColliderCount > 0;

        if (playerGameInput == null)
        {
            playerGameInput = other.GetComponentInParent<GameInput>();
        }

        TryShowPromptSprite(other);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsPlayerCollider(other))
        {
            return;
        }

        playerColliderCount = Mathf.Max(0, playerColliderCount - 1);
        isPlayerInRange = playerColliderCount > 0;
        if (!isPlayerInRange)
        {
            playerGameInput = null;
            HidePromptSprite();
        }
    }

    private void OnDisable()
    {
        isPlayerInRange = false;
        playerColliderCount = 0;
        playerGameInput = null;
        HidePromptSprite();
    }

    private bool IsPlayerCollider(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            return true;
        }

        Transform root = other.transform.root;
        return root != null && root.CompareTag(playerTag);
    }

    private void TryShowPromptSprite(Collider playerCollider)
    {
        if (!showTalkPromptSprite)
        {
            return;
        }

        if (activePromptSprite == null)
        {
            activePromptSprite = ResolvePromptSprite(playerCollider);
        }

        if (activePromptSprite != null)
        {
            activePromptSprite.SetActive(true);
        }
    }

    private GameObject ResolvePromptSprite(Collider playerCollider)
    {
        if (talkPromptSprite != null)
        {
            return talkPromptSprite;
        }

        if (!usePlayerPromptSprite || string.IsNullOrEmpty(playerPromptChildName) || playerCollider == null)
        {
            return null;
        }

        Transform fromCollider = FindChildRecursive(playerCollider.transform, playerPromptChildName);
        if (fromCollider != null)
        {
            return fromCollider.gameObject;
        }

        Transform root = playerCollider.transform.root;
        Transform fromRoot = FindChildRecursive(root, playerPromptChildName);
        return fromRoot != null ? fromRoot.gameObject : null;
    }

    private void HidePromptSprite()
    {
        if (activePromptSprite != null)
        {
            activePromptSprite.SetActive(false);
            activePromptSprite = null;
        }
    }

    private void UpdatePromptSpriteBillboard()
    {
        if (!billboardPromptSprite || activePromptSprite == null)
        {
            return;
        }

        if (Camera.main == null)
        {
            return;
        }

        Transform spriteTransform = activePromptSprite.transform;
        spriteTransform.rotation = Quaternion.LookRotation(spriteTransform.position - Camera.main.transform.position);
    }

    private void UpdateNpcSpriteBillboard()
    {
        if (!billboardNpcSprite || npcSpriteTransform == null)
        {
            return;
        }

        if (Camera.main == null)
        {
            return;
        }

        npcSpriteTransform.rotation = Quaternion.LookRotation(npcSpriteTransform.position - Camera.main.transform.position);
    }

    private void TryResolveNpcSpriteTransform()
    {
        if (npcSpriteTransform != null)
        {
            return;
        }

        SpriteRenderer spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            npcSpriteTransform = spriteRenderer.transform;
        }
    }

    private static Transform FindChildRecursive(Transform root, string targetName)
    {
        if (root == null)
        {
            return null;
        }

        if (root.name == targetName)
        {
            return root;
        }

        for (int i = 0; i < root.childCount; i++)
        {
            Transform found = FindChildRecursive(root.GetChild(i), targetName);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }

    // Call this from your NPC talk interaction event.
    public void OnTalkToNpc()
    {
        if (claimRewardOnTalk)
        {
            ClaimPendingQuestReward();
        }

        if (openQuestPanelOnTalk)
        {
            OpenQuestPanel();
        }
    }

    public void OpenQuestPanel()
    {
        if (questSelectionUI == null)
        {
            Debug.LogWarning("QuestSelectionUI is not assigned on QuestRewardNpc.");
            return;
        }

        questSelectionUI.OpenPanel();
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
