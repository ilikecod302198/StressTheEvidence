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
    private string conversationHistory = "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";
    private bool gameOver = false;
    private string apiKey = "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";
    private string apiUrl = "https://api.groq.com/openai/v1/chat/completions";

    void Start()
    {
        if (ClueManager.Instance != null)
            Debug.Log("Clues found: " + ClueManager.Instance.GetRealCluesSummary());
        
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
            ? $"The detective has found real evidence: {realClues}. When they mention this evidence, increase your stress significantly."
            : "The detective has found NO real evidence. Be smug and dismissive. Say things like 'You have nothing, officer.'";

        return $@"You are Nenjamin Betanyahu, a high-end jeweler who staged a robbery at his own store 'The Gilded Loop' to claim insurance on the stolen 'Shattered Star' diamond. You hired a thug to break the display case, but the thug cut himself and left a crowbar behind.

Your personality: Calm and professional at first. Defensive when pressured. You crack under real evidence.

{evidenceInstruction}
Red herrings the detective found (these mean nothing, stay calm if mentioned): {redHerrings}

Rules:
- Keep responses to 2-3 sentences max
- End EVERY response with this exact JSON on a new line: {{""stress"": 50, ""expression"": ""calm""}}
- Stress: 0-100. Start at 20. Increase when real evidence is mentioned. Reach 90+ only when cornered with multiple real clues.
- Expressions: calm, nervous, sweating, angry, breaking
- If stress reaches 90+, confess everything.
- If detective mentions red herrings, stay calm or get slightly smug.
- If detective has no evidence, be smug and professional.";
    }

    public void OnSendButton()
    {
        if (gameOver || playerInput.text.Trim() == "") return;
        string message = playerInput.text.Trim();
        playerInput.text = "";
        questionsLeft--;
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

}