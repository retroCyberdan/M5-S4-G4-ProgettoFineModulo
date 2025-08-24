using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject _gameOverUI;
    [SerializeField] private GameObject _victoryUI;

    [Header("Auto-Find UI")]
    [SerializeField] private bool _autoFindUIElements = true;

    void Awake()
    {
        GameManager.OnGameInitialized += OnGameInitialized;
        GameManager.OnAttemptsChanged += OnAttemptsChanged;
        GameManager.OnGameOver += OnGameOver;
        GameManager.OnVictory += OnVictory;

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        GameManager.OnGameInitialized -= OnGameInitialized;
        GameManager.OnAttemptsChanged -= OnAttemptsChanged;
        GameManager.OnGameOver -= OnGameOver;
        GameManager.OnVictory -= OnVictory;

        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        InitializeUI();
    }

    #region Scene Management
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(InitializeUIAfterSceneLoad());
    }

    private IEnumerator InitializeUIAfterSceneLoad()
    {
        yield return new WaitForEndOfFrame();
        InitializeUI();
    }
    #endregion

    #region UI Initialization
    private void InitializeUI()
    {
        if (_autoFindUIElements)
        {
            RefreshUIReferences();
        }

        HideAllUI(); // <- nasconde tutte le UI all'inizio

        Debug.Log($"[UIManager] UI initialized - GameOver: {_gameOverUI != null}, Victory: {_victoryUI != null}");
    }

    private void RefreshUIReferences()
    {
        // cerca GameOver UI
        if (_gameOverUI == null)
        {
            GameObject gameOverObj = GameObject.FindGameObjectWithTag("GameOverUI");
            if (gameOverObj == null)
            {
                gameOverObj = GameObject.Find("GameOverUI");
            }
            _gameOverUI = gameOverObj;
        }

        // cerca Victory UI
        if (_victoryUI == null)
        {
            GameObject victoryObj = GameObject.FindGameObjectWithTag("VictoryUI");
            if (victoryObj == null)
            {
                victoryObj = GameObject.Find("VictoryUI");
            }
            _victoryUI = victoryObj;
        }

        Debug.Log($"[UIManager] UI References refreshed - GameOver: {_gameOverUI != null}, Victory: {_victoryUI != null}");
    }
    #endregion

    #region Event Handlers
    private void OnGameInitialized()
    {
        Debug.Log("[UIManager] Game initialized, hiding all UI");
        HideAllUI();
    }

    private void OnAttemptsChanged(int attemptsLeft, bool showAttempts)
    {
        Debug.Log($"[UIManager] Attempts changed: {attemptsLeft}, Show: {showAttempts}");

        TextUIManager.Instance?.UpdateAttemptsUI(attemptsLeft, showAttempts); // <- aggiorna la UI dei tentativi

        if (showAttempts)
        {
            TextUIManager.Instance?.AnimateAttemptsText(); // <- anima il testo se necessario
        }
    }

    private void OnGameOver()
    {
        Debug.Log("[UIManager] Showing Game Over UI");
        ShowGameOverUI();
    }

    private void OnVictory()
    {
        Debug.Log("[UIManager] Showing Victory UI");
        ShowVictoryUI();
    }
    #endregion

    #region UI Display Methods
    private void ShowGameOverUI()
    {
        HideAllUI();

        if (_gameOverUI == null)
        {
            RefreshUIReferences();
        }

        if (_gameOverUI != null)
        {
            _gameOverUI.SetActive(true);
            Debug.Log("[UIManager] Game Over UI displayed");
        }
        else
        {
            Debug.LogWarning("[UIManager] GameOver UI reference is null! Cannot show Game Over screen.");
        }
    }

    private void ShowVictoryUI()
    {
        HideAllUI();

        if (_victoryUI == null)
        {
            RefreshUIReferences();
        }

        if (_victoryUI != null)
        {
            _victoryUI.SetActive(true);
            Debug.Log("[UIManager] Victory UI displayed");
        }
        else
        {
            Debug.LogWarning("[UIManager] Victory UI reference is null! Cannot show Victory screen.");
        }
    }

    private void HideAllUI()
    {
        if (_gameOverUI != null) _gameOverUI.SetActive(false);
        if (_victoryUI != null) _victoryUI.SetActive(false);
    }
    #endregion

    #region Button Methods (da richiamare da OnAction)
    public void OnRestartButtonClicked()
    {
        Debug.Log("[UIManager] Restart button clicked");
        HideAllUI();
        GameManager.Instance?.RestartLevel();
    }

    public void OnMainMenuButtonClicked()
    {
        Debug.Log("[UIManager] Main menu button clicked");
        HideAllUI();
        GameManager.Instance?.BackToMainMenu();
    }
    #endregion

    #region Public Methods
    public void ForceUIRefresh()
    {
        RefreshUIReferences();
        Debug.Log("[UIManager] UI References manually refreshed");
    }

    public void ShowGameOver()
    {
        ShowGameOverUI();
    }

    public void ShowVictory()
    {
        ShowVictoryUI();
    }

    public void HideUI()
    {
        HideAllUI();
    }

    public void DebugUIState()
    {
        Debug.Log($"[UIManager Debug] GameOverUI: {_gameOverUI != null} (Active: {(_gameOverUI != null ? _gameOverUI.activeSelf : false)}), " +
                  $"VictoryUI: {_victoryUI != null} (Active: {(_victoryUI != null ? _victoryUI.activeSelf : false)})");
    }
    #endregion

    #region Public Getters
    public bool IsGameOverUIActive => _gameOverUI != null && _gameOverUI.activeSelf;
    public bool IsVictoryUIActive => _victoryUI != null && _victoryUI.activeSelf;
    public bool HasValidUIReferences => _gameOverUI != null && _victoryUI != null;
    #endregion
}