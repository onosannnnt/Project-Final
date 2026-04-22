using UnityEngine;
using TMPro; // สำหรับจัดการ TextMeshPro
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NameHandler : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField nameInputField;
    public Button startButton;
    [SerializeField] private UserData userData;

    [Header("Scene Settings")]
    public string nextSceneName = "GameScene";

    void Start()
    {
        // ปิดการกดปุ่ม Start ไว้ก่อนจนกว่าจะพิมพ์ชื่อ (ตัวเลือกเสริม)
        if (startButton != null)
            startButton.interactable = false;

        // เพิ่ม Listener ตรวจสอบการพิมพ์
        nameInputField.onValueChanged.AddListener(ValidateInput);
    }

    void ValidateInput(string input)
    {
        // ถ้าพิมพ์มากกว่า 0 ตัวอักษร ให้กดปุ่มได้
        startButton.interactable = input.Length > 0;
    }

    public async void StartGame()
    {
        string playerName = nameInputField.text;

        if (!string.IsNullOrEmpty(playerName))
        {
            // บันทึกชื่อเก็บไว้ (ใช้ PlayerPrefs เพื่อดึงไปใช้ใน Scene อื่นได้ง่าย)
            PlayerPrefs.SetString("PlayerName", playerName);
            userData.ResetProgression();
            userData.Username = playerName;
            UserResponse response = await NetworkManager.SavePlayerData(userData);
            userData.ID = response.ID;
            userData.Username = response.Username;
            PlayerPrefs.Save();


            // // Debug.Log("Player Name Saved: " + playerName);

            // เปลี่ยน Scene
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogWarning("Please enter a name!");
        }
    }
}
