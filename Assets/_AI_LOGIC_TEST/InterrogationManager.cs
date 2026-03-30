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
    
    public UnityEngine.UI.Image nenjaminImage;
    public Sprite[] expressions;

    private int questionsLeft = 7;
    private string conversationHistory = "";
    private bool gameOver = false;
    private string apiKey = "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";
    private string apiUrl = "https://api.groq.com/openai/v1/chat/completions";

    void Start()
    {
        if (ClueManager.Instance != null)
        {
            questionsLeft = ClueManager.Instance.questionsLeft;
            Debug.Log("Clues found: " + ClueManager.Instance.GetRealCluesSummary());
        }
        UpdateQuestionsUI();
        conversationLog.text = "Nenjamin sits across from you, calm and composed.\n\n";
        playerInput.ActivateInputField();
        playerInput.Select();
    }

    void Update()
    {
        if (Keyboard.current.enterKey.wasPressedThisFrame)
            OnSendButton();

        if (!playerInput.isFocused)
            playerInput.ActivateInputField();

        if (Keyboard.current.f1Key.wasPressedThisFrame)
        {
            UpdateExpression("nervous");
            Debug.Log("Stress forced to nervous");
        }
        if (Keyboard.current.f2Key.wasPressedThisFrame)
        {
            UpdateExpression("sweating");
            Debug.Log("Stress forced to sweating");
        }
        if (Keyboard.current.f3Key.wasPressedThisFrame)
        {
            UpdateExpression("breaking");
            winScreen.SetActive(true);
            gameOver = true;
        }

        if (Keyboard.current.f4Key.wasPressedThisFrame)
        {
            UpdateExpression("breaking");
            winScreen.SetActive(true);
            gameOver = true;
        }
    }

    string BuildSystemPrompt()
    {
        string realClues = ClueManager.Instance != null ? ClueManager.Instance.GetRealCluesSummary() : "none";
        string redHerrings = ClueManager.Instance != null ? ClueManager.Instance.GetRedHerringSummary() : "none";
        bool hasEvidence = ClueManager.Instance != null && ClueManager.Instance.HasRealClues();

        string evidenceInstruction = hasEvidence
            ? $"The detective has found real evidence: {realClues}. Only increase stress if they ask a SMART, specific question about this evidence. Simply stating 'I found the crowbar' is not enough — they must connect it to you meaningfully."
            : "The detective has found NO real evidence. Be smug, cool and professional. Deflect everything.";

        return $@"You are Nenjamin Betanyahu, a high-end jeweler who staged a robbery at his own store 'The Gilded Loop' to claim insurance on the stolen 'Shattered Star' diamond. You hired a thug to break the display case, but the thug cut himself and left a crowbar behind in the vents.

Your personality: Calm and professional at first. Defensive when pressured with real evidence AND smart questions. You only crack when cornered with specific, intelligent accusations.

{evidenceInstruction}
Red herrings found by detective (mean nothing, stay calm or get slightly smug): {redHerrings}

STRICT RULES:
- Speak ONLY in dialogue. Never describe actions or thoughts.
- NEVER use asterisks like *sweats* or *nervous*. NEVER say 'I am sweating' or 'my hands are shaking'.
- Show stress through your words and tone only.
- Keep responses to 2-3 sentences max.
- Only increase stress when detective asks something specific and intelligent about real evidence.
- Stress: 0-100. Start at 15. Increase by 20-30 each time detective mentions real evidence intelligently. Reach 90+ after 3-4 good questions.
- Confess only if stress reaches 90+.
- You are a powerful, well-connected man who believes he is above suspicion. You occasionally reference your status, your lawyers, and your many years of public service. You are witty and condescending.
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
        ClueManager.Instance.questionsLeft = questionsLeft;
    UpdateQuestionsUI();
    
    if (questionsLeft <= 0)
    {
        loseScreen.SetActive(true);
        gameOver = true;
        return;
    }
    
    conversationLog.text += $"<b>Detective:</b> {message}\n\n";
    StartCoroutine(GetAIResponse(message));
    }

    IEnumerator GetAIResponse(string playerMessage)
    {
        conversationLog.text = $"<b>Detective:</b> {playerMessage}\n\n<i>Nenjamin is thinking...</i>";
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

        conversationLog.text = $"<b>Detective:</b> {playerMessage}\n\n<b>Nenjamin:</b> {displayReply}";

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

    void UpdateExpression(string expression)
    {
        if (nenjaminImage == null) return;
    
        int index = expression switch
        {
            "nervous"  => 1,
            "sweating" => 2,
            "angry"    => 3,
            "breaking" => 4,
            _          => 0
        };
    
        if (expressions.Length > index)
            nenjaminImage.sprite = expressions[index];
    }

    string ParseExpression(string json)
    {
        var match = System.Text.RegularExpressions.Regex.Match(json, @"""expression""\s*:\s*""(\w+)""");
        if (match.Success) return match.Groups[1].Value;
        return "calm";
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
}