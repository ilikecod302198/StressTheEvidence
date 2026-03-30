using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;

public class ExitTrigger : MonoBehaviour
{
    public TMP_Text exitPrompt;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            exitPrompt.enabled = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && exitPrompt != null)
        exitPrompt.enabled = false;
    }

    void Update()
    {
        if (exitPrompt.enabled && Keyboard.current.spaceKey.wasPressedThisFrame)
            SceneManager.LoadScene("InterrogationScene");
    }
}