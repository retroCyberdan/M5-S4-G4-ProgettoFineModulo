using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private AudioClip _mainMenuMusic;

    private void Start()
    {
        AudioManager.Instance.PlayMusic(_mainMenuMusic);
    }

    public void StartGame()
    {
        SceneManager.LoadScene("ProgettoFineModulo");
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
