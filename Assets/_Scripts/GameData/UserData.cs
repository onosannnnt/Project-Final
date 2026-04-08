using UnityEngine;

[CreateAssetMenu(fileName = "UserData", menuName = "ScriptableObjects/GameData/UserData"),]
public class UserData : ScriptableObject
{
    public int ID;
    public string Username;
    [Tooltip("Current game progression phase (e.g., 1, 2, 3)")]
    public int GamePhase = 1;
    [Tooltip("The index of the quest currently being played (e.g., 0 for Tutorial, 1 for Quest 1)")]
    public int SelectedQuestIndex = 0;
}
