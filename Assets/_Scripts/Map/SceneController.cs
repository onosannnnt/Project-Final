using UnityEngine;
using UnityEngine.SceneManagement; // สำคัญมาก: ต้องมีบรรทัดนี้เพื่อจัดการ Scene

public class SceneController : MonoBehaviour
{
    // เปลี่ยน Scene โดยใช้ชื่อ (เข้าใจง่ายที่สุด)
    // public void LoadSceneByName(string sceneName)
    // {
    //     SceneManager.LoadScene(sceneName);
    // }

    // เปลี่ยน Scene โดยใช้เลข Index (เร็วและจัดการง่ายในโปรเจกต์ใหญ่)
    public void LoadSceneByIndex(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }

    // สำหรับปุ่มออกจากเกม
    public void QuitGame()
    {
// // Debug.Log("Game is exiting..."); // ไว้เช็คใน Console เพราะใน Editor เกมจะไม่ปิดจริง
        // ถ้ากำลังรันอยู่ใน Unity Editor
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            // ถ้าเป็นเกมที่ Build ออกมาแล้ว (.exe / .apk)
            Application.Quit();
        #endif
    }
}
