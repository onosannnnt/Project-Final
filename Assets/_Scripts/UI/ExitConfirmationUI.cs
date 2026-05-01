using UnityEngine;
using UnityEngine.UI;

public class ExitConfirmationUI : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject exitPanel;
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject objectivePanel;

    [Header("Buttons")]
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;

    private void Start()
    {
        if (exitPanel != null) 
            exitPanel.SetActive(false);

        if (confirmButton != null) 
            confirmButton.onClick.AddListener(QuitGame);
        
        if (cancelButton != null) 
            cancelButton.onClick.AddListener(ClosePanel);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (exitPanel != null && exitPanel.activeSelf)
            {
                ClosePanel();
            }
            else
            {
                // Only open if:
                // 1. No blocking panels are open (IsInputLocked is false)
                // 2. We didn't JUST unlock input this frame (e.g. closing another panel with Esc)
                if (!GameInput.IsInputLocked && !GameInput.InputUnlockedThisFrame)
                {
                    OpenPanel();
                }
            }
        }
    }

    private void OpenPanel()
    {
        if (exitPanel != null)
        {
            exitPanel.SetActive(true);
            GameInput.SetInputLock(true);
        }
    }

    private void ClosePanel()
    {
        if (exitPanel != null)
        {
            exitPanel.SetActive(false);
            GameInput.SetInputLock(false);
        }
    }

    private void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
