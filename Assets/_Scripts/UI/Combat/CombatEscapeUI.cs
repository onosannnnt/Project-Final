using UnityEngine;
using UnityEngine.UI;

public class CombatEscapeUI : MonoBehaviour
{
    public static CombatEscapeUI Instance { get; private set; }

    [Header("UI Panels")]
    [SerializeField] private GameObject escapePanel;

    [Header("Buttons")]
    [SerializeField] private Button leaveButton;
    [SerializeField] private Button stayButton;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (escapePanel != null)
            escapePanel.SetActive(false);

        if (leaveButton != null)
            leaveButton.onClick.AddListener(LeaveBattle);

        if (stayButton != null)
            stayButton.onClick.AddListener(ClosePanel);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (escapePanel != null && escapePanel.activeSelf)
            {
                ClosePanel();
            }
            else
            {
                // Only open if no other UI is blocking
                // We don't use GameInput.IsInputLocked here to allow escaping from sub-menus if desired,
                // but checking it would prevent opening multiple panels.
                if (!GameInput.IsInputLocked)
                {
                    OpenPanel();
                }
            }
        }
    }

    public void OpenPanel()
    {
        if (escapePanel != null && !escapePanel.activeSelf)
        {
            escapePanel.SetActive(true);
            GameInput.SetInputLock(true); 
        }
    }

    private void ClosePanel()
    {
        if (escapePanel != null)
        {
            escapePanel.SetActive(false);
            GameInput.SetInputLock(false);
        }
    }

    private void LeaveBattle()
    {
        // Cleanup Logic
        TurnManager turnManager = TurnManager.Instance;
        if (turnManager != null && turnManager.UserData != null)
        {
            // Clear the active quest so the player can re-select it or choose another
            turnManager.UserData.ClearSelectedQuest();
        }

        GameInput.SetInputLock(false); // Ensure input is unlocked when leaving
        Loader.Load(Loader.Scenes.Overworld);
    }
}
