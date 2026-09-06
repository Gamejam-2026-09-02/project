using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
public class TypewriterText : MonoBehaviour, IPointerClickHandler
{
    public GameObject body;

    [Header("文本")]
    [TextArea(3, 10)]
    public List<string> texts = new();
    [Header("打字速度")]
    [SerializeField]
    public float charInterval = 0.05f;
    [Header("结束后是否循环")]
    [SerializeField]
    private bool loop = false;
    private TextMeshProUGUI textUI;
    private int currentIndex = 0;
    private Coroutine typingCoroutine;
    private bool isTyping;
    private string currentText;
    private bool hasFinished = false;
    private void Awake()
    {
        textUI = GetComponent<TextMeshProUGUI>();
    }
    private void Start()
    {
        if (texts.Count > 0)
        {
            ShowText(currentIndex);
        }
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        ClickNext();
    }
    private void ClickNext()
    {
        if (hasFinished)
            return;

        // 当前正在打字，直接完成
        if (isTyping)
        {
            CompleteCurrentText();
            return;
        }
        // 下一句
        currentIndex++;
        if (currentIndex >= texts.Count)
        {
            if (loop)
            {
                currentIndex = 0;
            }
            else
            {
                hasFinished = true;
                GameStateController.Instance.ResumeGame();
                body.SetActive(false);
                return;
            }
        }
        ShowText(currentIndex);
    }
    private void ShowText(int index)
    {
        currentText = texts[index];
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeText());
    }
    private IEnumerator TypeText()
    {
        isTyping = true;
        textUI.text = "";
        for (int i = 0; i < currentText.Length; i++)
        {
            textUI.text += currentText[i];
            yield return new WaitForSecondsRealtime(charInterval);
        }
        isTyping = false;
    }
    private void CompleteCurrentText()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);
        textUI.text = currentText;
        isTyping = false;
    }
}