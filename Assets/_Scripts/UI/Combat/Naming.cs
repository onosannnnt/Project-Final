using System;
using TMPro;
using UnityEngine;
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

        if (string.IsNullOrEmpty(username))
        {
            Debug.LogWarning("Please enter a name!");
            return;
        }

        if (userData != null)
        {
            userData.ResetProgression();
            userData.Username = username;
        }

        try
        {
            UserResponse response = await NetworkManager.SavePlayerData(new CreateUserRequest
            {
                username = username
            });

            if (response != null && userData != null)
            {
                int resolvedId = response.GetId();
                string resolvedName = response.GetUsername();

                if (resolvedId > 0)
                {
                    userData.ID = resolvedId;
                }

                if (!string.IsNullOrEmpty(resolvedName))
                {
                    userData.Username = resolvedName;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Create user failed, using fallback ID. {e.Message}");
            if (userData != null)
            {
                userData.ID = UnityEngine.Random.Range(1000, 9999);
            }
        }

        if (userData != null)
        {
            PlayerPrefs.SetInt("UserId", userData.ID);
            PlayerPrefs.SetString("PlayerName", userData.Username);
            PlayerPrefs.Save();
        }


        Loader.Load(Loader.Scenes.Overworld);
    }
}
