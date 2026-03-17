using System.Runtime.CompilerServices;
using TMPro;
using UnityEditor.Compilation;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NameingUI : Singleton<NameingUI>
{
    [SerializeField] private Button startButton;
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private UserData userData;

    private void Start()
    {
        startButton.onClick.AddListener(OnStartButtonClicked);
    }
    private async void OnStartButtonClicked()
    {
        string username = nameInputField.text;
        userData.Username = username;
        UserResponse response = await NetworkManager.SavePlayerData(userData);
        userData.ID = response.ID;
        userData.Username = response.Username;



        Loader.Load(Loader.Scenes.Overworld);
    }
}