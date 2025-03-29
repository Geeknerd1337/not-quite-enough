using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Perception.Engine;

public class DialogBox : MonoBehaviour
{
    public TMP_Text Text;
    public TMP_Text Speaker;
    public DialogData DialogData;
    public float DialogSpeed = 0.05f; // Time between characters
    public int CharacterSkip = 1; // How many characters to reveal at once
    public SoundObject DialogSound;

    // Animation parameters
    public float SlideDistance = 100f;
    public float AnimationDuration = 0.5f;

    private int CurrentDialogIndex;
    private int CurrentLineIndex;
    private bool isTyping = false;
    private bool isComplete = false;
    private RectTransform rectTransform;
    private Coroutine typeCoroutine;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        // Start off-screen
        Vector2 startPos = rectTransform.anchoredPosition;
        rectTransform.anchoredPosition = new Vector2(startPos.x, startPos.y - SlideDistance);
    }

    public void StartDialog()
    {
        CurrentDialogIndex = 0;
        CurrentLineIndex = 0;
        Text.text = "";
        Speaker.text = DialogData.dialogLines[CurrentDialogIndex].speakerName;
        StartCoroutine(SlideIn());
    }

    private IEnumerator SlideIn()
    {
        Vector2 startPos = rectTransform.anchoredPosition;
        Vector2 endPos = new Vector2(startPos.x, startPos.y + SlideDistance);
        float elapsed = 0f;

        while (elapsed < AnimationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / AnimationDuration;
            rectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            yield return null;
        }

        StartTyping();
    }

    private void StartTyping()
    {
        if (CurrentDialogIndex >= DialogData.dialogLines.Length)
        {
            StartCoroutine(EndDialog());
            return;
        }

        Speaker.text = DialogData.dialogLines[CurrentDialogIndex].speakerName;
        typeCoroutine = StartCoroutine(TypeText());
    }

    private IEnumerator TypeText()
    {
        isTyping = true;
        isComplete = false;
        string fullText = DialogData.dialogLines[CurrentDialogIndex].dialogText;
        Text.text = "";

        for (int i = 0; i < fullText.Length; i += CharacterSkip)
        {
            Text.text = fullText.Substring(0, Mathf.Min(i + CharacterSkip, fullText.Length));
            if (DialogSound != null)
            {
                DialogSound.Play();
            }
            yield return new WaitForSeconds(DialogSpeed);
        }

        isTyping = false;
        isComplete = true;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetMouseButtonDown(0))
        {
            if (isTyping)
            {
                // Skip to end of current line
                if (typeCoroutine != null)
                    StopCoroutine(typeCoroutine);
                Text.text = DialogData.dialogLines[CurrentDialogIndex].dialogText;
                isTyping = false;
                isComplete = true;
            }
            else if (isComplete)
            {
                // Progress to next line
                CurrentDialogIndex++;
                StartTyping();
            }
        }
    }

    private IEnumerator EndDialog()
    {
        Vector2 startPos = rectTransform.anchoredPosition;
        Vector2 endPos = new Vector2(startPos.x, startPos.y - SlideDistance);
        float elapsed = 0f;

        while (elapsed < AnimationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / AnimationDuration;
            rectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            yield return null;
        }

        Destroy(gameObject);
    }
}
