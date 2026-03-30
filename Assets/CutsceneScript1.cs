using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CutsceneManager : MonoBehaviour
{
    public UnityEngine.UI.Image displayImage;
    public Sprite[] images;
    public float timePerImage = 3f;
    public string nextScene = "JewleryStore";

    private int currentIndex = 0;

    void Start()
    {
        if (images.Length > 0)
            displayImage.sprite = images[0];
        Invoke("NextImage", timePerImage);
    }

    void NextImage()
    {
        currentIndex++;
        if (currentIndex < images.Length)
        {
            displayImage.sprite = images[currentIndex];
            Invoke("NextImage", timePerImage);
        }
        else
        {
            SceneManager.LoadScene(nextScene);
        }
    }
}