using UnityEngine;

public class player_animator : MonoBehaviour
{
    [SerializeField] private Movement movement;
    private const string isWalking = "IsWalking";
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    private void Update()
    {
        animator.SetBool(isWalking, movement.IsWalking());
    }
}
