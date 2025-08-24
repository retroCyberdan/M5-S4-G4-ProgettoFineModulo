using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeartUIManager : MonoBehaviour
{
    public static HeartUIManager Instance { get; private set; }

    [Header("Heart Settings")]
    [SerializeField] private GameObject _heartPrefab;
    [SerializeField] private int _maxHearts = 3;

    private List<Image> _hearts = new List<Image>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        // Creiamo i cuori come child di questo GameObject
        for (int i = 0; i < _maxHearts; i++)
        {
            GameObject heart = Instantiate(_heartPrefab, transform);
            _hearts.Add(heart.GetComponent<Image>());
        }

        UpdateHearts(_maxHearts); // <- inizializza tutti i cuori visibili
    }

    public void UpdateHearts(int currentAttempts)
    {
        for (int i = 0; i < _hearts.Count; i++)
        {
            if (_hearts[i] != null)
                _hearts[i].enabled = i < currentAttempts; // <- visibile solo se i < tentativi rimasti
        }
    }
}
