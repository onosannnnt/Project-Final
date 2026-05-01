using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour
{
    private PlayerInputActions playerInputActions;
    public static bool IsInputLocked { get; private set; }
    public static bool InputUnlockedThisFrame { get; private set; }

    void Awake()
    {
        playerInputActions = new PlayerInputActions();
        playerInputActions.Enable();
    }
    void OnDestroy()
    {
        playerInputActions.Disable();
    }

    private void LateUpdate()
    {
        InputUnlockedThisFrame = false;
    }

    public static void SetInputLock(bool isLocked)
    {
        if (IsInputLocked && !isLocked)
        {
            InputUnlockedThisFrame = true;
        }
        IsInputLocked = isLocked;
    }

    public Vector2 GetMovementVectorNormalized()
    {
        if (IsInputLocked) return Vector2.zero;

        Vector2 inputVector = playerInputActions.Player.Movement.ReadValue<Vector2>();
        inputVector = inputVector.normalized;
        return inputVector;
    }
    public bool IsInteractPressed()
    {
        if (IsInputLocked) return false;
        return playerInputActions.Player.Interact.triggered;
    }
}
