using UnityEngine;

public class QuestCombatTrigger : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private UserData userData;

    [Header("Interaction")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private KeyCode fallbackInteractKey = KeyCode.F;

    [Header("Prompt Sprite")]
    [SerializeField] private GameObject promptSprite;
    [SerializeField] private bool usePlayerPromptSprite;
    [SerializeField] private string playerPromptChildName = "ChangeMapIcon";
    [SerializeField] private bool usePromptBillboard = true;
    [SerializeField] private BillboardToCamera.BillboardMode promptBillboardMode = BillboardToCamera.BillboardMode.FaceCameraPosition;

    [Header("Before Combat")]
    [SerializeField] private bool saveSkillLoadoutBeforeCombat = true;

    private int playerColliderCount;
    private bool isPlayerInside;
    private bool isLoadingCombat;
    private GameInput playerGameInput;
    private GameObject activePromptSprite;

    private void Awake()
    {
        if (promptSprite != null)
        {
            ConfigurePromptBillboard(promptSprite);
            promptSprite.SetActive(false);
        }
    }

    private void OnEnable()
    {
        isLoadingCombat = false;
    }

    private void Update()
    {
        if (!isPlayerInside || isLoadingCombat)
        {
            return;
        }

        if (IsInteractPressed())
        {
            TryEnterCombatFromActiveQuest();
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
        isPlayerInside = playerColliderCount > 0;

        if (playerGameInput == null)
        {
            playerGameInput = other.GetComponentInParent<GameInput>();
        }

        if (activePromptSprite == null)
        {
            activePromptSprite = ResolvePromptSprite(other);
        }

        ConfigurePromptBillboard(activePromptSprite);

        SetPromptVisible(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsPlayerCollider(other))
        {
            return;
        }

        playerColliderCount = Mathf.Max(0, playerColliderCount - 1);
        isPlayerInside = playerColliderCount > 0;

        if (!isPlayerInside)
        {
            playerGameInput = null;
            SetPromptVisible(false);
            activePromptSprite = null;
        }
    }

    private void OnDisable()
    {
        playerColliderCount = 0;
        isPlayerInside = false;
        playerGameInput = null;
        SetPromptVisible(false);
        activePromptSprite = null;
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

    private void TryEnterCombatFromActiveQuest()
    {
        if (userData == null)
        {
            Debug.LogWarning("UserData is not assigned on QuestCombatTrigger.");
            return;
        }

        if (!userData.HasSelectedQuest())
        {
            Debug.LogWarning("No quest selected. Accept a quest first before entering combat.");
            return;
        }

        int selectedQuestIndex = userData.SelectedQuestIndex;
        if (!userData.CanStartQuest(selectedQuestIndex))
        {
            string reason = userData.GetQuestBlockedReason(selectedQuestIndex);
            Debug.LogWarning("Selected quest is not startable. " + reason);
            userData.ClearSelectedQuest();
            return;
        }

        if (saveSkillLoadoutBeforeCombat)
        {
            SkillListManager skillManager = FindObjectOfType<SkillListManager>();
            if (skillManager != null)
            {
                skillManager.SaveLoadout();
            }
        }

        isLoadingCombat = true;
        Loader.Load(Loader.Scenes.Combat);
    }

    private void SetPromptVisible(bool visible)
    {
        if (activePromptSprite != null)
        {
            activePromptSprite.SetActive(visible);
            return;
        }

        if (promptSprite != null)
        {
            promptSprite.SetActive(visible);
        }
    }

    private void ConfigurePromptBillboard(GameObject targetObject)
    {
        if (targetObject == null)
        {
            return;
        }

        BillboardToCamera billboard = targetObject.GetComponent<BillboardToCamera>();

        if (usePromptBillboard)
        {
            if (billboard == null)
            {
                billboard = targetObject.AddComponent<BillboardToCamera>();
            }

            billboard.SetMode(promptBillboardMode);
            billboard.enabled = true;
            return;
        }

        if (billboard != null)
        {
            billboard.enabled = false;
        }
    }

    private GameObject ResolvePromptSprite(Collider playerCollider)
    {
        if (promptSprite != null)
        {
            return promptSprite;
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

    private static Transform FindChildRecursive(Transform root, string childName)
    {
        if (root == null)
        {
            return null;
        }

        if (root.name == childName)
        {
            return root;
        }

        for (int i = 0; i < root.childCount; i++)
        {
            Transform found = FindChildRecursive(root.GetChild(i), childName);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }
}
