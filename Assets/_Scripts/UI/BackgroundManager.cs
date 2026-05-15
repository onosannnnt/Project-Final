using UnityEngine;
using UnityEngine.UI;

public class BackgroundManager : MonoBehaviour
{
    [Header("Targets")]
    [Tooltip("UI Image to change (optional)")]
    [SerializeField] private Image uiBackgroundImage;
    [Tooltip("SpriteRenderer to change (optional, for world backgrounds)")]
    [SerializeField] private SpriteRenderer worldBackgroundRenderer;

    [Header("Background Sprites")]
    [SerializeField] private Sprite quest1Sprite;
    [SerializeField] private Sprite quest2Wave12Sprite;
    [SerializeField] private Sprite quest2Wave3Sprite;
    [SerializeField] private Sprite quest3Sprite;

    [Header("Data Sources (optional - will try to resolve automatically)")]
    [SerializeField] private UserData userData;
    [SerializeField] private TurnManager turnManager;

    private int lastQuest = int.MinValue;
    private int lastWave = int.MinValue;
    private Sprite lastAppliedSprite;

    private void Start()
    {
        if (turnManager == null && TurnManager.Instance != null)
        {
            turnManager = TurnManager.Instance;
        }

        if (userData == null && turnManager != null)
        {
            userData = turnManager.UserData;
        }

        UpdateBackground(true);
    }

    private void Update()
    {
        UpdateBackground(false);
    }

    private void UpdateBackground(bool force)
    {
        int questIndex = userData != null ? userData.SelectedQuestIndex : UserData.NoQuestSelected;
        int wave = turnManager != null ? turnManager.currentWave : 1;

        if (!force && questIndex == lastQuest && wave == lastWave)
        {
            return;
        }

        lastQuest = questIndex;
        lastWave = wave;

        Sprite chosen = null;
        if (questIndex == UserData.Quest1Index)
        {
            chosen = quest1Sprite;
        }
        else if (questIndex == UserData.Quest2Index)
        {
            chosen = (wave <= 2) ? quest2Wave12Sprite : quest2Wave3Sprite;
        }
        else if (questIndex == UserData.Quest3Index)
        {
            chosen = quest3Sprite;
        }

        if (chosen == lastAppliedSprite)
        {
            return;
        }

        lastAppliedSprite = chosen;

        if (uiBackgroundImage != null)
        {
            uiBackgroundImage.sprite = chosen;
        }

        if (worldBackgroundRenderer != null)
        {
            worldBackgroundRenderer.sprite = chosen;
        }
    }
}
