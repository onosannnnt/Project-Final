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

    public LayerMask terrainLayer;
    public Rigidbody rb;
    public float groundDist;
    public SpriteRenderer sr;
    private bool interactRequested;

    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();
    }

    public bool IsWalking()
    {
        return isWalking;
    }

    void Update()
    {
        RaycastHit hit;
        Vector3 castPos = transform.position;
        // castPos.y += 1;

        if (Physics.Raycast(castPos, -transform.up, out hit, Mathf.Infinity, terrainLayer))
        {
            if (hit.collider != null)
            {
                Vector3 movePos = transform.position;
                movePos.y = hit.point.y + groundDist;
                transform.position = movePos;
            }
        }

        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");
        Vector3 moveDir = new Vector3(x, 0, y);
        rb.velocity = moveDir * moveSpeed;



        if (moveDir != Vector3.zero)
        {
            isWalking = true;
        }
        else
        {
            isWalking = false;
        }

        if (x != 0 && x < 0)
        {
            sr.flipX = true;
        }
        else if (x != 0 && x > 0)
        {
            sr.flipX = false;
        }

        if (gameInput.IsInteractPressed())
        {
            interactRequested = true;
        }

    }

    private void FixedUpdate()
    {
        if (interactRequested)
        {
            HandleInteraction();
            interactRequested = false; // รีเซ็ตค่าหลังจากใช้งานแล้ว
        }

    }

    [Header("Quest Configuration")]
    [Tooltip("The UserData to update before starting the quest.")]
    public UserData userData;
    [Tooltip("The array index of the quest to play (0 for Tutorial, 1 for Quest 1, etc.)")]
    public int selectedQuestIndex = 0;

    private void HandleInteraction()
    {
        // float interactDistance = 3f;

        // Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, interactDistance);
        // if (hit.collider != null)
        // {
        //     if (gameInput.IsInteractPressed())
        //     {
        //         Loader.Load(Loader.Scenes.Combat);
        //     }
        // } else if (gameInput.IsInteractPressed())
        // {
        //     // Debug.Log("ไม่มีอะไรให้โต้ตอบ");
        // }

        // หาตัว Manager ในฉากแล้วสั่ง Save ก่อนโหลดฉากใหม่
        SkillListManager skillManager = FindObjectOfType<SkillListManager>();
        if (skillManager != null)
        {
            skillManager.SaveLoadout();
        }

        if (userData == null)
        {
            Debug.LogWarning("UserData is not assigned in Movement. Cannot validate progression.");
            return;
        }

        if (!userData.TrySetSelectedQuest(selectedQuestIndex))
        {
            int fallbackQuestIndex = userData.SelectedQuestIndex;
            if (!userData.TrySetSelectedQuest(fallbackQuestIndex))
            {
                string reason = userData.GetQuestBlockedReason(selectedQuestIndex);
                Debug.LogWarning("Cannot start quest index " + selectedQuestIndex + ". " + reason);
                return;
            }

            Debug.Log("Quest index " + selectedQuestIndex + " is not available. Starting unlocked quest index " + fallbackQuestIndex + " instead.");
        }

        Loader.Load(Loader.Scenes.Combat);
    }
}

// public bool IsWalking()
// {
//     return isWalking;
// }
// private void HandleMovement()
// {
//     Vector2 inputVector = gameInput.GetMovementVectorNormalized();
//     Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y);

//     isWalking = inputVector != Vector2.zero;

//     if (isWalking)
//     {
//         if (moveDir.x > 0)
//             PlayerVisual.transform.rotation = Quaternion.Euler(0, 0, 0);     // facing right
//         else if (moveDir.x < 0)
//             PlayerVisual.transform.rotation = Quaternion.Euler(0, 180, 0);
//     }
//     transform.position += moveDir * moveSpeed * Time.fixedDeltaTime;
// }


