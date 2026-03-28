using UnityEngine;
using LLMUnity;
using TMPro;
public class AiManager : MonoBehaviour
{
    public LLMClient nenjaminBrain;
    public StressManager stress;
    
    [Header("Inventory State")]
    public bool hasCrowbar = false;
    public bool hasBloodSample = false;

    public async void SendToNenjamin(string playerInput)
    {
        string lowerInput = playerInput.ToLower();
        float pressureBonus = 0;

        // LOGIC: Only increase stress if the player MENTIONS the item AND has it
        if (lowerInput.Contains("crowbar") && hasCrowbar)
        {
            pressureBonus = 30f; 
            Debug.Log("LOGIC: You trapped him with the Crowbar!");
        }
        else if (lowerInput.Contains("blood") && hasBloodSample)
        {
            pressureBonus = 40f;
            Debug.Log("LOGIC: The DNA evidence is breaking him.");
        }

        if (pressureBonus > 0) stress.AddStress(pressureBonus);

        // Send the current stress level TO the AI so it knows how to act
        string context = $"[Nervousness: {stress.currentStress}%] ";
        string response = await nenjaminBrain.Chat(context + playerInput);
        
        Debug.Log("NENJAMIN: " + response);
    }
}
