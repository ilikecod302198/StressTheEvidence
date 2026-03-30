using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.InputSystem;

public class InterrogationManager : MonoBehaviour
{
    public TMP_Text conversationLog;
    public TMP_InputField playerInput;
    public TMP_Text questionsLeftText;
    public TMP_Text suspectNameText;
    public GameObject winScreen;
    public GameObject loseScreen;

    private int questionsLeft = 7;
    private string conversationHistory = "";
    private bool gameOver = false;
    private string apiKey = "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";
    private string apiUrl = "https://api.groq.com/openai/v1/chat/completions";

    public SpriteRendered nejaminSprite;
    public Sprite[] expressions; // 0: calm, 1: nervous, 2: sweating, 3: breaking
    void Start()
    {   
        Debug.Log("ClueManager exists: " + (ClueManager.Instance != null));
        Debug.Log("Questions left: " + ClueManager.Instance?.questionsLeft);

        if (ClueManager.Instance != null)
        {   
            questionsLeft = ClueManager.Instance.questionsLeft;
            Debug.Log("Clues found: " + ClueManager.Instance.GetRealCluesSummary());
        }
        UpdateQuestionsUI();
        conversationLog.text = "Nenjamin sits across from you, calm and composed.\n\n";

        // Auto focus input field
        playerInput.ActivateInputField();
        playerInput.Select();    
    }

    string BuildSystemPrompt()
{
    string realClues = ClueManager.Instance != null ? ClueManager.Instance.GetRealCluesSummary() : "none";
    string redHerrings = ClueManager.Instance != null ? ClueManager.Instance.GetRedHerringSummary() : "none";
    bool hasEvidence = ClueManager.Instance != null && ClueManager.Instance.HasRealClues();

    string evidenceInstruction = hasEvidence
        ? $"The detective has found real evidence: {realClues}. Only increase stress if they ask a SMART, specific question about this evidence. Simply stating 'I found the crowbar' is not enough — they must connect it to you meaningfully, e.g. 'The crowbar has your fingerprints on it' or 'Why was the crowbar hidden in your vents?'"
        : "The detective has found NO real evidence. Be smug, cool and professional. Deflect everything. Say things like 'Is that all you have, officer?' or 'I think we're done here.'";

    return $@"You are Nenjamin Betanyahu, a high-end jeweler who staged a robbery at his own store 'The Gilded Loop' to claim insurance on the stolen 'Shattered Star' diamond. You hired a thug to break the display case, but the thug cut himself and left a crowbar behind in the vents.

Your personality: Calm and professional at first. Defensive when pressured with real evidence AND smart questions. You only crack when cornered with specific, intelligent accusations.

{evidenceInstruction}
Red herrings found by detective (mean nothing, stay calm or get slightly smug): {redHerrings}

STRICT RULES:
- Speak ONLY in dialogue. Never describe actions or thoughts.
- NEVER use asterisks like *sweats* or *nervous*. NEVER say 'I am sweating' or 'my hands are shaking'.
- Show stress through your words and tone only. Example: 'I... that's not relevant.' instead of '*fidgets nervously*'
- Keep responses to 2-3 sentences max.
- Only increase stress when detective asks something specific and intelligent about real evidence.
- Stress: 0-100. Start at 15. Only reach 90+ when cornered with multiple smart questions about real clues.
- Confess only if stress reaches 90+.
- End EVERY response on a new line with exactly: {{""stress"": 20, ""expression"": ""calm""}}
- Expressions: calm, nervous, sweating, angry, breaking";
}

    public void OnSendButton()
    {
        if (gameOver || playerInput.text.Trim() == "") return;
        string message = playerInput.text.Trim();
        playerInput.text = "";
        questionsLeft--;
        if (ClueManager.Instance != null)
        {
            ClueManager.Instance.questionsLeft = questionsLeft;
        }
        UpdateQuestionsUI();
        conversationLog.text += $"<b>Detective:</b> {message}\n\n";
        StartCoroutine(GetAIResponse(message));
    }

    IEnumerator GetAIResponse(string playerMessage)
    {
    conversationLog.text += "<i>Nenjamin is thinking...</i>\n\n";
    conversationHistory += $"\nDetective: {playerMessage}";

    string body = "{\"model\":\"llama-3.3-70b-versatile\",\"messages\":[{\"role\":\"system\",\"content\":\"" + EscapeJson(BuildSystemPrompt()) + "\"},{\"role\":\"user\",\"content\":\"" + EscapeJson(conversationHistory) + "\"}],\"max_tokens\":300}";

    var request = new UnityWebRequest(apiUrl, "POST");
    byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(body);
    request.uploadHandler = new UploadHandlerRaw(bodyRaw);
    request.downloadHandler = new DownloadHandlerBuffer();
    request.SetRequestHeader("Content-Type", "application/json");
    request.SetRequestHeader("Authorization", "Bearer " + apiKey);

    yield return request.SendWebRequest();

    if (request.result != UnityWebRequest.Result.Success)
    {
        conversationLog.text += $"<color=red>[Error: {request.error} - {request.downloadHandler.text}]</color>\n\n";
        yield break;
    }

    string fullReply = ParseContent(request.downloadHandler.text);
    int stress = ParseStress(fullReply);
    string displayReply = System.Text.RegularExpressions.Regex.Replace(
        fullReply, @"\{""stress"".*?\}", "").Trim();

    conversationHistory += $"\nNenjamin: {displayReply}";

    conversationLog.text = conversationLog.text.Replace("<i>Nenjamin is thinking...</i>\n\n", "");
    conversationLog.text += $"<b>Nenjamin:</b> {displayReply}\n\n";

    Debug.Log("Stress: " + stress);
    
    string expression = ParseExpression(request.downloadHandler.text);
    UpdateExpression(expression);
    
    if (stress >= 90)
    {
        winScreen.SetActive(true);
        gameOver = true;
    }
    else if (questionsLeft <= 0)
    {
        loseScreen.SetActive(true);
        gameOver = true;
    }
    }
    string ParseContent(string json)
    {
        int start = json.IndexOf("\"content\": \"") + 12;
        if (start < 12) start = json.IndexOf("\"content\":\"") + 11;
        if (start < 11) return "[No response]";
        int end = json.IndexOf("\"", start);
        if (end < 0) return "[Parse error]";
        return json.Substring(start, end - start)
            .Replace("\\n", "\n")
            .Replace("\\\"", "\"");
    }

    int ParseStress(string text)
    {
        var match = System.Text.RegularExpressions.Regex.Match(text, "\"stress\"\\s*:\\s*(\\d+)");
        if (match.Success) return int.Parse(match.Groups[1].Value);
        return 20;
    }

    void UpdateQuestionsUI()
    {
        questionsLeftText.text = $"Questions left: {questionsLeft}";
    }

    string EscapeJson(string s)
    {
        return s.Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\n", "\\n")
                .Replace("\r", "");
    }

    void Update()
    {
    if (Keyboard.current.enterKey.wasPressedThisFrame)
        OnSendButton();
    
    if (!playerInput.isFocused)
        playerInput.ActivateInputField();
    }   

    void UpdateExpression(string expression)
    {
    if (nenjaminSprite == null) return;
    
    int index = expression switch
    {
        "nervous"  => 1,
        "sweating" => 2,
        "angry"    => 2,
        "breaking" => 3,
        _          => 0  // calm default
    };
    
    if (expressions.Length > index)
        nenjaminSprite.sprite = expressions[index];
    }

    string ParseExpression(string json)
    {
        var match = System.Text.RegularExpressions.Regex.Match(json, @"""expression""\s*:\s*""(\w+)""");
        if (match.Success) return match.Groups[1].Value;
        return "calm";
    }

}