using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;

public class ReturnToStore : MonoBehaviour
{
    public TMP_Text returnPrompt;

    void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            SceneManager.LoadScene("JewleryStore");
        }
    }

    void Start()
    {
        if (returnPrompt != null)
            returnPrompt.text = "Press ESC to return to store";
    }
}