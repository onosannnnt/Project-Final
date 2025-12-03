using UnityEditor;
using UnityEngine.SceneManagement;
public static class Loader
{
    public enum Scenes
    {
        Overworld,
        Combat,
        Loading
    }

    private static Scenes targetScene;

    public static void Load(Scenes scene)
    {
        targetScene = scene;
        SceneManager.LoadScene(Scenes.Loading.ToString());
    }
    public static void LoadCallback()
    {
        SceneManager.LoadScene(targetScene.ToString());
    }
}