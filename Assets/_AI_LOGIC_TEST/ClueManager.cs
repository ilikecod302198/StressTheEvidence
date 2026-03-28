using UnityEngine;
using System.Collections.Generic;

public class ClueManager : MonoBehaviour
{
    public static ClueManager Instance;
    public List<string> realClues = new List<string>();
    public List<string> redHerrings = new List<string>();

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else Destroy(gameObject);
    }

    public void AddClue(string clue, bool isReal)
    {
        if (isReal)
        {
            if (!realClues.Contains(clue)) realClues.Add(clue);
        }
        else
        {
            if (!redHerrings.Contains(clue)) redHerrings.Add(clue);
        }
    }

    public string GetRealCluesSummary()
    {
        if (realClues.Count == 0) return "none";
        return string.Join(", ", realClues);
    }

    public string GetRedHerringSummary()
    {
        if (redHerrings.Count == 0) return "none";
        return string.Join(", ", redHerrings);
    }

    public bool HasRealClues()
    {
        return realClues.Count > 0;
    }
}
