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

        // LOGIC: Only increase stress if the player MENTIONS the item AND has it
        if (lowerInput.Contains("crowbar") && hasCrowbar)
        {
            stress.IncreaseStressLevel(25); 
        }
        else if (lowerInput.Contains("blood") && hasBloodSample)
        {
            stress.IncreaseStressLevel(40);
        }

        
        // Send the current stress level TO the AI so it knows how to act
        string context = $"[Nervousness: {stress.CurrentStressLevel}%] ";
        //string response = await nenjaminBrain.Generate(context + playerInput);
        
        //Debug.Log("NENJAMIN: " + response);
    }
}
