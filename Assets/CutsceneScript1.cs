using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CutsceneManager : MonoBehaviour
{
    public UnityEngine.UI.Image displayImage;
    public Sprite[] images;
    public AudioClip[] audioClips;
    public float timePerImage = 3f;
    public string nextScene = "MainMenu";

    private int currentIndex = 0;
    private AudioSource audioSource;

    void Start()
        {
            audioSource = GetComponent<AudioSource>();
            if (images.Length > 0)
            {
                displayImage.sprite = images[0];
                PlayAudio(0);
            }
            Invoke("NextImage", timePerImage);
        }

        void NextImage()
        {
            currentIndex++;
            if (currentIndex < images.Length)
            {
                displayImage.sprite = images[currentIndex];
                PlayAudio(currentIndex);
                Invoke("NextImage", timePerImage);
            }
            else
            {
                SceneManager.LoadScene(nextScene);
            }
        }

        void PlayAudio(int index)
        {
            if (audioSource != null && audioClips.Length > index && audioClips[index] != null)
                audioSource.PlayOneShot(audioClips[index]);
        }
}