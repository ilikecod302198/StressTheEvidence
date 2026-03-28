using UnityEngine;

public class ClueTrigger : MonoBehaviour
{
    public string clueType = "Crowbar"; // Set to "Crowbar" or "Blood" in Inspector
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            AiManager ai = Object.FindAnyObjectByType<AiManager>();
            
            if (clueType == "Crowbar") {
                ai.hasCrowbar = true;
            }
            else if (clueType == "Blood") {
                ai.hasBloodSample = true;
            }

            Debug.Log("SYSTEM: Picked up " + clueType + ". You can now use this in chat.");
            gameObject.SetActive(false);
        }
    }
}
