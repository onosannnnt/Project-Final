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

    private int selectedIndex = -1;

    private void Start()
    {
        // Condition: Only open if build not chosen AND no quests completed (-1)
        if (userData != null && !userData.HasChosenStarterBuild && userData.HighestCompletedQuestIndex == -1)
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
        if (confirmButton != null) confirmButton.interactable = false;

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
    }

    private void OnConfirmButtonClicked()
    {
        if (selectedIndex < 0 || selectedIndex >= buildData.Count) return;

        // Give trial skills to player (not auto-assigned to loadout)
        if (userData != null)
        {
            userData.TrialSkills.Clear();
            userData.AddTrialSkills(buildData[selectedIndex].starterSkills);
            userData.HasChosenStarterBuild = true;

            // In Editor, mark as dirty to save the change
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(userData);
#endif
        }

        ClosePanel();
    }

    private void ClosePanel()
    {
        if (panelRoot != null) panelRoot.SetActive(false);
        GameInput.SetInputLock(false);
    }
}
