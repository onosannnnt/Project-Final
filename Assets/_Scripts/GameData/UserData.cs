using UnityEngine;

[CreateAssetMenu(fileName = "UserData", menuName = "ScriptableObjects/GameData/UserData"),]
public class UserData : ScriptableObject
{
    public int ID;
    public string Username;
}