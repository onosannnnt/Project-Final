using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Movement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private GameInput gameInput;
    [SerializeField] private GameObject PlayerVisual;

    private bool isWalking;

    private void FixedUpdate()
    {
        HandleInteraction();
        HandleMovement();

    }

    public bool IsWalking()
    {
        return isWalking;
    }
    private void HandleMovement()
    {
        Vector2 inputVector = gameInput.GetMovementVectorNormalized();
        Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y);

        isWalking = inputVector != Vector2.zero;

        if (isWalking)
        {
            if (moveDir.x > 0)
                PlayerVisual.transform.rotation = Quaternion.Euler(0, 0, 0);     // facing right
            else if (moveDir.x < 0)
                PlayerVisual.transform.rotation = Quaternion.Euler(0, 180, 0);
        }
        transform.position += moveDir * moveSpeed * Time.fixedDeltaTime;
    }

    private void HandleInteraction()
    {
        float interactDistance = 3f;

        Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, interactDistance);
        if (hit.collider != null)
        {
            if (gameInput.IsInteractPressed())
            {
                Loader.Load(Loader.Scenes.Combat);
            }
        }
    }
}
