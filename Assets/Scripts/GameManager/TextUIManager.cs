using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextUIManager : MonoBehaviour
{
    public static TextUIManager Instance { get; private set; } // <- gestisco questa UI in singleton

    [Header("UI Settings")]
    [SerializeField] private TextMeshProUGUI _attemptsText;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void UpdateAttemptsUI(int attemptsLeft, bool show)
    {
        if (_attemptsText == null)
        {
            Debug.LogWarning("⚠️ il Testo non è collegato nell'Inspector!");
            return;
        }
        _attemptsText.text = show ? "Tentativi: " + attemptsLeft : "";
        Debug.Log("UI aggiornata: " + _attemptsText.text);
    }

    public void AnimateAttemptsText()
    {
        if (_attemptsText != null)
            StartCoroutine(AnimateTextCoroutine());
    }

    private IEnumerator AnimateTextCoroutine()
    {
        Vector3 originalScale = _attemptsText.rectTransform.localScale;
        Color originalColor = _attemptsText.color;

        for (int i = 0; i < 2; i++)
        {
            _attemptsText.color = Color.red;
            _attemptsText.rectTransform.localScale = originalScale * 1.2f;
            yield return new WaitForSeconds(0.15f);

            _attemptsText.color = originalColor;
            _attemptsText.rectTransform.localScale = originalScale;
            yield return new WaitForSeconds(0.15f);
        }
    }
}