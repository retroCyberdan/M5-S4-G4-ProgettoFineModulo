using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public enum CAPTURE_MODE { GAME_OVER, RESPAWN, HYBRID }
public enum RESPAWN_TYPE { TELEPORT, RELOAD_SCENE }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }  // <- gestisco il GameManager in singleton

    [Header("Capture Mode Settings")]
    [SerializeField] private CAPTURE_MODE _captureMode = CAPTURE_MODE.RESPAWN;
    [SerializeField] private int _maxAttempts = 3;

    [Header("Respawn Settings")]
    [SerializeField] private RESPAWN_TYPE _respawnType = RESPAWN_TYPE.TELEPORT;
    [SerializeField] private Transform _respawnPoint;

    [Header("UI Settings")]
    [SerializeField] private GameObject _gameOverUI;
    [SerializeField] private GameObject _victoryUI;
    [SerializeField] private Transform _restartPoint;

    private int _attemptsLeft;
    private bool _isRespawn = false;
    private GameObject _player;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        _attemptsLeft = _maxAttempts;
        TextUIManager.Instance?.UpdateAttemptsUI(_attemptsLeft, _captureMode == CAPTURE_MODE.HYBRID);
        _restartPoint = _respawnPoint;

        if (_gameOverUI != null) _gameOverUI.SetActive(false);
        if (_victoryUI != null) _victoryUI.SetActive(false);
    }

    #region Capture Modes Methods
    public void OnPlayerCaught(GameObject caughtPlayer)
    {
        if (_isRespawn) return;

        _player = caughtPlayer;
        _isRespawn = true;

        switch (_captureMode)
        {
            case CAPTURE_MODE.GAME_OVER:
                GameOverUI();
                break;

            case CAPTURE_MODE.RESPAWN:
                StartCoroutine(RespawnCoroutine());
                break;

            case CAPTURE_MODE.HYBRID:
                if (_attemptsLeft > 1)
                {
                    _attemptsLeft--;
                    TextUIManager.Instance?.UpdateAttemptsUI(_attemptsLeft, true);
                    TextUIManager.Instance?.AnimateAttemptsText();
                    StartCoroutine(RespawnCoroutine());
                }
                else
                {
                    _attemptsLeft = 0;
                    TextUIManager.Instance?.UpdateAttemptsUI(_attemptsLeft, true);
                    GameOverUI();
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

    #region UI
    private void GameOverUI()
    {
        if (_gameOverUI != null) _gameOverUI.SetActive(true);
        if (_player != null)
        {
            var controller = _player.GetComponent<PlayerAgentController>();
            if (controller != null) controller.enabled = false;
        }

        AudioManager.Instance?.PlayGameOverSound();
    }

    public void VictoryUI()
    {
        if (_victoryUI != null) _victoryUI.SetActive(true);
        if (_player != null)
        {
            var controller = _player.GetComponent<PlayerAgentController>();
            if (controller != null) controller.enabled = false;
        }

        AudioManager.Instance?.PlayVictorySound();
    }

    public void RestartLevel()
    {
        //if (_player != null && _restartPoint != null)
        //{
        //    NavMeshAgent agent = _player.GetComponent<NavMeshAgent>();
        //    if (agent != null)
        //    {
        //        agent.ResetPath();
        //        agent.Warp(_restartPoint.position);
        //    }
        //    else
        //    {
        //        _player.transform.position = _restartPoint.position;
        //        _player.transform.rotation = _restartPoint.rotation;
        //    }

        //    EnemyAgentBehaviour[] enemies = FindObjectsOfType<EnemyAgentBehaviour>();
        //    foreach (var enemy in enemies)
        //    {
        //        enemy.PlayerTarget = _player.transform;
        //    }

        //    _attemptsLeft = _maxAttempts;
        //    TextUIManager.Instance?.UpdateAttemptsUI(_attemptsLeft, true);

        //    if (_gameOverUI != null)
        //        _gameOverUI.SetActive(false);

        //    _isRespawn = false;

        //    var controller = _player.GetComponent<PlayerAgentController>();
        //    if (controller != null)
        //        controller.enabled = true;
        //}

        SceneManager.LoadScene("ProgettoFineModulo");
    }

    public void BackToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
    #endregion
}