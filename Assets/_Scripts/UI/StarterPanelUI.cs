using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class StarterPanelUI : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private UserData userData;

    [Header("Manual Build Mapping")]
    [Tooltip("Order these to match the buttons list below")]
    [SerializeField] private List<StarterBuild> buildData; 
    [SerializeField] private List<Button> buildButtons; 
    
    [Header("UI Panels")]
    [Tooltip("The actual UI Panel GameObject (the one you set to Inactive)")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button finalizeButton; // New: Confirm & Proceed button

    [Header("Progression Feedback")]
    [SerializeField] private GameObject shopPromptUI; // New: UI to tell player to go to shop

    private int selectedIndex = -1;

    private void Start()
    {
        // Condition: Only open if build not yet chosen OR if tutorial is NOT finalized.
        // Replayable loop: Open if chosen but not finalized.
        if (userData != null && !userData.IsTutorialFinalized)
        {
            OpenStarterPanel();
        }
        else
        {
            if (panelRoot != null) panelRoot.SetActive(false);
        }
    }

    public void OpenStarterPanel()
    {
        if (panelRoot == null) return;

        panelRoot.SetActive(true);
        InitializeLogic();
        GameInput.SetInputLock(true);
    }

    private void InitializeLogic()
    {
        if (confirmButton != null) confirmButton.interactable = (selectedIndex >= 0);
        if (finalizeButton != null) finalizeButton.interactable = userData.HasChosenStarterBuild;

        for (int i = 0; i < buildButtons.Count; i++)
        {
            int index = i;
            if (i < buildButtons.Count)
            {
                buildButtons[index].onClick.RemoveAllListeners();
                buildButtons[index].onClick.AddListener(() => {
                    selectedIndex = index;
                    if (confirmButton != null) confirmButton.interactable = true;
                });
            }
        }

        if (confirmButton != null)
        {
            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(OnConfirmButtonClicked);
        }

        if (finalizeButton != null)
        {
            finalizeButton.onClick.RemoveAllListeners();
            finalizeButton.onClick.AddListener(OnFinalizeButtonClicked);
        }
    }

    private void OnConfirmButtonClicked()
    {
        if (selectedIndex < 0 || selectedIndex >= buildData.Count) return;

        // Replayable loop: Clear old trial skills and give new ones
        if (userData != null)
        {
            userData.ClearAllLoadouts(); // Clear equipped skills first
            userData.RemoveTrialSkills(); 
            userData.AddTrialSkills(buildData[selectedIndex].starterSkills);
            userData.HasChosenStarterBuild = true;

            if (finalizeButton != null) finalizeButton.interactable = true;

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(userData);
#endif
        }

        // Close to let player test the build in the tutorial quest
        ClosePanel();
    }

    private void OnFinalizeButtonClicked()
    {
        if (userData == null || !userData.HasChosenStarterBuild) return;

        // 1. Grant Tutorial Reward (Currency)
        int reward = userData.GetQuestCoinReward(UserData.TutorialQuestIndex);
        userData.AddCoins(reward);

        // 2. Mark Tutorial as Completed in progression
        userData.HighestCompletedQuestIndex = UserData.TutorialQuestIndex;

        // 3. Mark Tutorial as Finalized (Locks the replayable loop)
        userData.IsTutorialFinalized = true;

        // 4. Remove Trial Skills (Player must now buy permanent skills)
        userData.ClearAllLoadouts();
        userData.RemoveTrialSkills();

        // 5. Persistence
        PlayerPrefs.SetInt("TutorialFinalized", 1);
        PlayerPrefs.Save();

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(userData);
#endif

        // 6. Visual Feedback: Prompt to go to shop for Quest 1
        if (shopPromptUI != null) shopPromptUI.SetActive(true);

        // 7. Refresh NPCs (Main NPCs like Shop NPC should now show up)
        RefreshAllNpcs();

        ClosePanel();
    }

    private void RefreshAllNpcs()
    {
        // Find all InteractableNpc components (including inactive ones)
        InteractableNpc[] allNpcs = Resources.FindObjectsOfTypeAll<InteractableNpc>();
        foreach (var npc in allNpcs)
        {
            // Only refresh NPCs that are part of the active scene
            if (npc.gameObject.scene.name != null)
            {
                npc.RefreshVisibility();
            }
        }
    }

    private void ClosePanel()
    {
        if (panelRoot != null) panelRoot.SetActive(false);
        GameInput.SetInputLock(false);
    }
}
