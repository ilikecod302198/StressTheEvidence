using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class ClueObject : MonoBehaviour
{
    public string clueName = "Crowbar";
    [TextArea] public string clueDescription = "A heavy crowbar with dried blood on the end. Someone left this behind.";
    public bool isRealClue = true;

    public GameObject inspectUI;
    public TMP_Text inspectText;
    public GameObject pressEPrompt;

    private bool playerNearby = false;
    private bool collected = false;

    void Update()
    {
        if (playerNearby && !collected)
        {
            pressEPrompt.SetActive(true);

            if (Keyboard.current.eKey.wasPressedThisFrame)
            {
                collected = true;
                ClueManager.Instance.AddClue(clueName, isRealClue);
                inspectText.text = clueDescription;
                pressEPrompt.SetActive(false);
                inspectUI.SetActive(true);
                Invoke("HideUI", 3f);
            }
        }
    }

    void HideUI() { inspectUI.SetActive(false); }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerNearby = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;
            pressEPrompt.SetActive(false);
        }
    }
}