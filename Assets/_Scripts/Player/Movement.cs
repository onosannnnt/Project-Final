using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private GameInput gameInput;
    [SerializeField] private GameObject PlayerVisual;

    private bool isWalking;

    private void FixedUpdate()
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

    public bool IsWalking()
    {
        return isWalking;
    }
}
