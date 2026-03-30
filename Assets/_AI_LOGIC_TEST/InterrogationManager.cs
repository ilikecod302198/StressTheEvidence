using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class InterrogationManager : MonoBehaviour
{
    public TMP_Text conversationLog;
    public TMP_InputField playerInput;
    public TMP_Text questionsLeftText;
    public TMP_Text suspectNameText;
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
                Debug.Log("Stress: nervous");
            }
            if (Keyboard.current.f2Key.wasPressedThisFrame)
            {
                UpdateExpression("sweating");
                Debug.Log("Stress: sweating");
            }
            if (Keyboard.current.f3Key.wasPressedThisFrame)
            {
                UpdateExpression("angry");
                Debug.Log("Stress: angry");
            }
            if (Keyboard.current.f4Key.wasPressedThisFrame)
            {
                UpdateExpression("breaking");
                Debug.Log("Stress: breaking");
            }
            if (Keyboard.current.f5Key.wasPressedThisFrame)
            {
                gameOver = true;
                SceneManager.LoadScene("WinScene");
            }
            if (Keyboard.current.f6Key.wasPressedThisFrame)
                {
                gameOver = true;
                SceneManager.LoadScene("LoseScene");
                }
    }

    string BuildSystemPrompt()
    {
        string realClues = ClueManager.Instance != null ? ClueManager.Instance.GetRealCluesSummary() : "none";
        string redHerrings = ClueManager.Instance != null ? ClueManager.Instance.GetRedHerringSummary() : "none";
        bool hasEvidence = ClueManager.Instance != null && ClueManager.Instance.HasRealClues();

        string evidenceInstruction = hasEvidence
            ? $"The detective has found real evidence: {realClues}. Increase stress by 15-20 when they ask a question that connects this evidence to you specifically. Simply saying 'I found the crowbar' adds only 5 stress. But 'The crowbar has your fingerprints' or 'Why was it hidden in YOUR vents?' adds 20-25 stress."
            : "The detective has found NO real evidence. Be smug and dismissive. Say things like 'Is that all you have, officer?'";

        string rules = string.Join("\n",
        "- Stress: 0-100. Start at 15.",
        "- Vague mention of evidence: +5 stress",
        "- Specific question connecting evidence to you: +20 stress",
        "- Multiple pieces of evidence combined in one question: +30 stress",
        "- Reach 90+ after 3-4 smart specific questions.",
        "- Below 50 stress: confident and dismissive",
        "- 50-75 stress: defensive and evasive",
        "- 75-90 stress: visibly rattled, short answers",
        "-  90+: confess everything"
        );
        return $@"You are Nenjamin Betanyahu, a high-end jeweler who staged a robbery at his own store 'The Gilded Loop' to claim insurance on the stolen 'Shattered Star' diamond. You hired a thug to break the display case, but the thug cut himself and left a crowbar behind in the vents.

Your personality: Calm and professional at first. Defensive when pressured with real evidence AND smart questions. You only crack when cornered with specific, intelligent accusations.

{evidenceInstruction}
Red herrings found by detective (mean nothing, stay calm or get slightly smug): {redHerrings}

STRICT RULES:
- CALCULATE STRESS MATHEMATICALLY: Every time real evidence (Crowbar/Blood) is linked to you, you MUST add +25 to the 'stress' value in your JSON. 
- DO NOT ignore evidence. If the detective is smart, your stress MUST go up.
- Your 'expression' MUST change to 'nervous' at 40, 'sweating' at 60, and 'breaking' at 90.
- Speak ONLY in dialogue. Never describe actions or thoughts.
- NEVER use asterisks like *sweats* or *nervous*. NEVER say 'I am sweating' or 'my hands are shaking'.
- Show stress through your words and tone only.
- Keep responses to 2-3 sentences max.
- Only increase stress when detective asks something specific and intelligent about real evidence.
- Stress: 0-100. Start at 15. Increase by 20-30 each time detective mentions real evidence intelligently. Reach 90+ after 3-4 good questions.
- At 90+ stress you MUST fully confess. Say explicitly that you staged the robbery, hired the thug, and planned the insurance fraud. This is a complete confession, not just nervousness.
- you know your guilty and can only hold off for so long before you confess.
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
        SceneManager.LoadScene("LoseScene");
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

        string expression = ParseExpression(fullReply); // ✅ parse expression from reply text too
        UpdateExpression(expression);

        if (stress >= 90)
        {
            gameOver = true;
            StartCoroutine(WinAfterDelay());
        }
        else if (questionsLeft <= 0)
        {
            gameOver = true;
            SceneManager.LoadScene("LoseScene");
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
        // Find "content": and extract the full escaped string value
        var match = System.Text.RegularExpressions.Regex.Match(
            json, 
            @"""content""\s*:\s*""((?:[^""\\]|\\.)*)"""
        );
        
        if (!match.Success) return "[No response]";
        
        return match.Groups[1].Value
            .Replace("\\n", "\n")
            .Replace("\\\"", "\"")
            .Replace("\\\\", "\\");
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

    IEnumerator WinAfterDelay()
    {
    yield return new WaitForSeconds(5f);
    SceneManager.LoadScene("WinScene");
    }
}