using UnityEngine;
using UnityEngine.SceneManagement;

public class CutsceneManager : MonoBehaviour
{
    public string nextScene = "JewleryStore";
    public float displayTime = 5f;

    void Start()
    {
        Invoke("LoadNextScene", displayTime);
    }

    void LoadNextScene()
    {
        SceneManager.LoadScene(nextScene);
    }
}