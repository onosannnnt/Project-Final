using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTeleport : MonoBehaviour
{
    public KeyCode interactKey = KeyCode.E;

    private GameObject playerIcon; // ตัวแปรเก็บ Sprite บนหัวผู้เล่น
    private bool isPlayerInArea = false;

    void Update()
    {
        if (isPlayerInArea)
        {
            // ทำให้ Icon หันหน้าเข้าหากล้องตลอดเวลา (Billboard)
            if (playerIcon != null)
            {
                playerIcon.transform.rotation = Quaternion.LookRotation(playerIcon.transform.position - Camera.main.transform.position);
            }

            if (Input.GetKeyDown(interactKey))
            {
                Debug.Log("Loading Scene: Map 2");
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInArea = true;
            
            // ค้นหา Sprite ที่ชื่อ "ChangeMapIcon" ในตัวผู้เล่น
            Transform iconTransform = other.transform.Find("ChangeMapIcon");
            if (iconTransform != null)
            {
                playerIcon = iconTransform.gameObject;
                playerIcon.SetActive(true); // เปิดไฟ
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInArea = false;
            if (playerIcon != null)
            {
                playerIcon.SetActive(false); // ปิดไฟ
                playerIcon = null;
            }
        }
    }
}