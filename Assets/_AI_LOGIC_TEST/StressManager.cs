using System.Diagnostics;
using System.Runtime.ExceptionServices;
using UnityEngine;

public class StressManager : MonoBehaviour
{
    [Header("Suspect Profile")]
    public string suspectName = "Nenjamin Betanyahu";
    public int CurrentStressLevel = 0;
    public int MaxStressLevel = 100;

    void Start()
    {
        Debug.Log("SYSTEM: " + suspectName + " is ready for questioning");
    }

    public void IncreaseStressLevel(int amount)
    {
        CurrentStressLevel += amount;
        //keeping stress b/w 0 and max
        CurrentStressLevel = Mathf.Clamp(CurrentStressLevel, 0, MaxStressLevel);

        Debug.Log(suspectName + "Stress Level: " + CurrentStressLevel + '%');

        if (CurrentStressLevel >= MaxStressLevel)
        {
            BreakSuspect();
        }
    } 

    void BreakSuspect()
    {
        Debug.Log("VICTORY: Sal has broken! He is now telling the truth.");
        // Later, we will trigger the AI to change its prompt here
    }
}
    

