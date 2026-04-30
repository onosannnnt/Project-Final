using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestButtonSlotView : MonoBehaviour
{
    [Range(UserData.TutorialQuestIndex, UserData.LastQuestIndex)]
    public int questIndex;

    public Button selectButton;
    public Image slotBackground;
    public TMP_Text questNameText;
    public TMP_Text rewardText;
    public TMP_Text statusText;

    [Header("Prefab Visuals (Optional)")]
    public GameObject selectedQuestBubble;
    public GameObject completedQuestBubble;
    public GameObject completedTextObject;
    public GameObject rewardGroupObject;
    public GameObject lockLayer;

    private void Awake()
    {
        BindMissingReferences();
    }

    private void Reset()
    {
        BindMissingReferences();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        BindMissingReferences();
    }
#endif

    public void BindMissingReferences()
    {
        if (selectButton == null)
        {
            selectButton = GetComponent<Button>();
        }

        if (slotBackground == null)
        {
            slotBackground = GetComponent<Image>();
            if (slotBackground == null)
            {
                slotBackground = GetImageInChild("QuestBubble");
            }
        }

        if (questNameText == null)
        {
            questNameText = GetTextInChild("QuestName");
        }

        if (rewardText == null)
        {
            rewardText = GetTextInChild("Amount");
            if (rewardText == null)
            {
                rewardText = GetTextInChild("Reward");
            }
        }

        if (statusText == null)
        {
            statusText = GetTextInChild("Status");
        }

        if (selectedQuestBubble == null)
        {
            selectedQuestBubble = FindChildGameObject("SelectedQuestBubble");
        }

        if (completedQuestBubble == null)
        {
            completedQuestBubble = FindChildGameObject("CompletedQuestBubble");
        }

        if (completedTextObject == null)
        {
            completedTextObject = FindChildGameObject("Success");
        }

        if (rewardGroupObject == null)
        {
            rewardGroupObject = FindChildGameObject("RewardGroup");
            if (rewardGroupObject == null && rewardText != null && rewardText.transform.parent != null)
            {
                rewardGroupObject = rewardText.transform.parent.gameObject;
            }
        }

        if (lockLayer == null)
        {
            lockLayer = FindChildGameObject("LockLayer");
        }

        if (selectButton != null && selectButton.targetGraphic == null)
        {
            if (slotBackground != null)
            {
                selectButton.targetGraphic = slotBackground;
            }
            else if (selectedQuestBubble != null)
            {
                Image selectedBubbleImage = selectedQuestBubble.GetComponent<Image>();
                if (selectedBubbleImage != null)
                {
                    selectButton.targetGraphic = selectedBubbleImage;
                }
            }
        }
    }

    private TMP_Text GetTextInChild(string childName)
    {
        Transform child = FindChildRecursive(transform, childName);
        return child != null ? child.GetComponent<TMP_Text>() : null;
    }

    private Image GetImageInChild(string childName)
    {
        Transform child = FindChildRecursive(transform, childName);
        return child != null ? child.GetComponent<Image>() : null;
    }

    private GameObject FindChildGameObject(string childName)
    {
        Transform child = FindChildRecursive(transform, childName);
        return child != null ? child.gameObject : null;
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
