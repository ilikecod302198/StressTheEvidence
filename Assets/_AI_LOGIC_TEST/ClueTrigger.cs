using UnityEngine;

public class ClueTrigger : MonoBehaviour
{
    [Header("Evidence Settings")]
    public string clueName = "Evidence";
    public int stressImpact = 20;

    // This is an 'Interrupt' triggered by Physics
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the thing hitting the clue is the Player
        if (other.CompareTag("Player"))
        {
            Debug.Log("LOGIC: Player found the " + clueName);

            // Find the StressManager in the scene
            StressManager sm = Object.FindAnyObjectByType<StressManager>();
            
            if (sm != null)
            {
                sm.IncreaseStressLevel(stressImpact);
            }

            // 'Destroy' the object (it disappears from the room)
            gameObject.SetActive(false);
        }
    }
}
