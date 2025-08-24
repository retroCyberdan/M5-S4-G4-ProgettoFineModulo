using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using System;

public enum CAPTURE_MODE { GAME_OVER, RESPAWN, HYBRID }
public enum RESPAWN_TYPE { TELEPORT, RELOAD_SCENE }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Capture Mode Settings")]
    [SerializeField] private CAPTURE_MODE _captureMode = CAPTURE_MODE.RESPAWN;
    [SerializeField] private int _maxAttempts = 3;

    [Header("Respawn Settings")]
    [SerializeField] private RESPAWN_TYPE _respawnType = RESPAWN_TYPE.TELEPORT;
    [SerializeField] private Transform _respawnPoint;

    private int _attemptsLeft;
    private bool _isRespawn = false;
    private GameObject _player;
    private bool _isReinitialized = true;

    #region UI Manager
    public static event Action<int, bool> OnAttemptsChanged;
    public static event Action OnGameInitialized;
    public static event Action OnGameOver;
    public static event Action OnVictory;
    #endregion

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Start()
    {
        InitializeGame();
    }

    #region Scene Initialization
    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        _isReinitialized = true;
        StartCoroutine(ReinitializeAfterSceneLoad());
    }

    private IEnumerator ReinitializeAfterSceneLoad()
    {
        yield return new WaitForEndOfFrame();

        if (_isReinitialized)
        {
            InitializeGame();
            _isReinitialized = false;
        }
    }

    private void InitializeGame()
    {
        // reset delle variabili di gioco
        _attemptsLeft = _maxAttempts;
        _isRespawn = false;
        _player = null;

        // trova il respawn point
        RefreshRespawnPoint();

        // notifica l'UIManager dell'inizializzazione
        OnGameInitialized?.Invoke();
        OnAttemptsChanged?.Invoke(_attemptsLeft, _captureMode == CAPTURE_MODE.HYBRID);

        Debug.Log($"[GameManager] Game initialized - Attempts: {_attemptsLeft}");
    }

    private void RefreshRespawnPoint()
    {
        if (_respawnPoint == null)
        {
            GameObject respawnObj = GameObject.FindGameObjectWithTag("RespawnPoint");
            if (respawnObj != null)
            {
                _respawnPoint = respawnObj.transform;
            }
        }
    }
    #endregion

    #region Capture Modes Methods
    public void OnPlayerCaught(GameObject caughtPlayer)
    {
        if (_isRespawn) return;

        _player = caughtPlayer;
        _isRespawn = true;

        Debug.Log($"[GameManager] Player caught! Mode: {_captureMode}, Attempts left: {_attemptsLeft}");

        switch (_captureMode)
        {
            case CAPTURE_MODE.GAME_OVER:
                TriggerGameOver();
                break;

            case CAPTURE_MODE.RESPAWN:
                StartCoroutine(RespawnCoroutine());
                break;

            case CAPTURE_MODE.HYBRID:
                if (_attemptsLeft > 1)
                {
                    _attemptsLeft--;
                    OnAttemptsChanged?.Invoke(_attemptsLeft, true);
                    StartCoroutine(RespawnCoroutine());
                }
                else
                {
                    _attemptsLeft = 0;
                    OnAttemptsChanged?.Invoke(_attemptsLeft, true);
                    TriggerGameOver();
                }
                break;
        }
    }

    private IEnumerator RespawnCoroutine()
    {
        _isRespawn = true;
        yield return null;

        if (_respawnType == RESPAWN_TYPE.RELOAD_SCENE)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        else
        {
            if (_player == null || _respawnPoint == null)
            {
                _isRespawn = false;
                yield break;
            }

            NavMeshAgent agent = _player.GetComponent<NavMeshAgent>();
            if (agent != null)
            {
                agent.ResetPath();
                agent.Warp(_respawnPoint.position);
            }
            else
            {
                _player.transform.position = _respawnPoint.position;
                _player.transform.rotation = _respawnPoint.rotation;
            }

            EnemyAgentBehaviour[] enemies = FindObjectsOfType<EnemyAgentBehaviour>();
            foreach (var enemy in enemies)
            {
                enemy.PlayerTarget = _player.transform;
            }
        }

        _isRespawn = false;
    }
    #endregion

    #region Game State Methods
    private void TriggerGameOver()
    {
        Debug.Log("[GameManager] Triggering Game Over");

        // disabilita il controller del player in caso di sconfitta
        if (_player != null)
        {
            var controller = _player.GetComponent<PlayerAgentController>();
            if (controller != null) controller.enabled = false;
        }

        // notifica l'UIManager
        OnGameOver?.Invoke();

        // a questo punto riproduce l'audio
        AudioManager.Instance?.PlayGameOverSound();
    }

    public void TriggerVictory()
    {
        Debug.Log("[GameManager] Triggering Victory");

        // disabilita il controller del player
        if (_player == null)
        {
            _player = GameObject.FindGameObjectWithTag("Player");
        }

        if (_player != null)
        {
            var controller = _player.GetComponent<PlayerAgentController>();
            if (controller != null) controller.enabled = false;
        }

        // notifica l'UIManager
        OnVictory?.Invoke();

        // a questo punto riproduce l'audio
        AudioManager.Instance?.PlayVictorySound();
    }
    #endregion

    #region Restart/Main Menu
    public void RestartLevel()
    {
        Debug.Log("[GameManager] Restarting level...");
        SceneManager.LoadScene("ProgettoFineModulo");
    }

    public void BackToMainMenu()
    {
        Debug.Log("[GameManager] Going back to main menu...");
        SceneManager.LoadScene("MainMenu");
    }
    #endregion

    #region Debug Methods
    public void DebugCurrentState()
    {
        Debug.Log($"[GameManager Debug] Attempts: {_attemptsLeft}, IsRespawn: {_isRespawn}, " +
                  $"Player: {_player != null}, RespawnPoint: {_respawnPoint != null}");
    }
    #endregion

    #region Public Getters
    public int AttemptsLeft => _attemptsLeft;
    public CAPTURE_MODE CaptureMode => _captureMode;
    public bool IsRespawnMode => _isRespawn;
    #endregion
}