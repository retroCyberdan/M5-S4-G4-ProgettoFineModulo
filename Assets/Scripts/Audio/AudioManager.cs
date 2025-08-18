using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources Settings")]
    [SerializeField] private AudioSource _musicSource;
    [SerializeField] private AudioSource _sfxSource;

    [Header("Scenes Clips")]
    [SerializeField] private SceneMusic[] _sceneMusics;

    [Header("Victory/Game Over Clips")]
    [SerializeField] private AudioClip _victorySound;
    [SerializeField] private AudioClip _gameOverSound;

    [Header("SFXs Clips")]
    [SerializeField] private AudioClip _clickSound;
    [SerializeField] private AudioClip damageSound;

    [Header("Fade Settings")]
    [SerializeField] private float _fadeTime = 1.5f;
    [SerializeField] private float _clickFadeTime = 0.05f;

    private Coroutine _fadeCoroutine;
    private Coroutine _clickFadeCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        foreach (SceneMusic sceneMusic in _sceneMusics)
        {
            if (currentScene.name == sceneMusic.sceneName)
            {
                PlayMusic(sceneMusic.musicClip);
                break;
            }
        }
    }
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        foreach (SceneMusic sceneMusic in _sceneMusics)
        {
            if (scene.name == sceneMusic.sceneName)
            {
                PlayMusic(sceneMusic.musicClip);
                return;
            }
        }
    }

    #region Music Methods
    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (clip == null) return;

        if (_musicSource.clip == clip && _musicSource.isPlaying) return;

        if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);

        _fadeCoroutine = StartCoroutine(FadeMusic(clip, loop));
    }

    private IEnumerator FadeMusic(AudioClip newClip, bool loop)
    {
        float startVolume = _musicSource.volume;
        float t = 0f;

        // fade out
        while (t < _fadeTime)
        {
            t += Time.deltaTime;
            _musicSource.volume = Mathf.Lerp(startVolume, 0f, t / _fadeTime);
            yield return null;
        }

        _musicSource.Stop();
        _musicSource.clip = newClip;
        _musicSource.loop = loop;
        _musicSource.Play();

        // fade in
        t = 0f;
        while (t < _fadeTime)
        {
            t += Time.deltaTime;
            _musicSource.volume = Mathf.Lerp(0f, startVolume, t / _fadeTime);
            yield return null;
        }

        _musicSource.volume = startVolume;
    }

    public void StopMusic()
    {
        _musicSource.Stop();
    }

    public void PlayVictorySound()
    {
        if (_victorySound != null) AudioSource.PlayClipAtPoint(_victorySound, Camera.main.transform.position);
    }

    public void PlayGameOverSound()
    {
        if (_gameOverSound != null) AudioSource.PlayClipAtPoint(_gameOverSound, Camera.main.transform.position);
    }
    #endregion

    #region SFX Methods
    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;

        _sfxSource.PlayOneShot(clip);
    }

    public void PlayClickSound()
    {
        if (_clickSound == null) return;

        if (_clickFadeCoroutine != null) StopCoroutine(_clickFadeCoroutine);

        _clickFadeCoroutine = StartCoroutine(FadeClick());
    }
    private IEnumerator FadeClick()
    {
        float baseVolume = _sfxSource.volume;

        _sfxSource.clip = _clickSound;
        _sfxSource.volume = 0f;
        _sfxSource.Play();

        float t = 0f;
        while (t < _clickFadeTime)
        {
            t += Time.deltaTime;
            _sfxSource.volume = Mathf.Lerp(0f, baseVolume, t / _clickFadeTime);
            yield return null;
        }

        _sfxSource.volume = 1f;

        yield return new WaitForSeconds(_clickSound.length - _clickFadeTime);

        _sfxSource.volume = baseVolume;
        //_sfxSource.Stop();
    }

    public void PlayDamageSound()
    {
        if (damageSound == null) return;
        _sfxSource.PlayOneShot(damageSound);
    }

    public void SetMusicVolume(float volume)
    {
        _musicSource.volume = Mathf.Clamp01(volume);
    }

    public void SetSFXVolume(float volume)
    {
        _sfxSource.volume = Mathf.Clamp01(volume);
    }
    #endregion
}