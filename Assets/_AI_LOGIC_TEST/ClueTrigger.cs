using UnityEngine;

public class ClueTrigger : MonoBehaviour
{
    public string clueType; // Set to "Crowbar" or "Blood" in Inspector
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            AIManager ai = Object.FindAnyObjectByType<AIManager>();
            
            if (clueType == "Crowbar") ai.hasCrowbar = true;
            if (clueType == "Blood") ai.hasBloodSample = true;

            Debug.Log("SYSTEM: Picked up " + clueType + ". You can now use this in chat.");
            gameObject.SetActive(false);
        }
    }
}
