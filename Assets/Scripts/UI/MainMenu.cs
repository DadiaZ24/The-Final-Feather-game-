using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("Panels")]
    public GameObject buttonPanel;
    public GameObject scoreboardPanel;
    void Awake()
    {
        ShowMainMenu();
    }

    public void PlayGame()
    {
        SceneManager.LoadScene("Game");
        AudioManager.Instance.StopAllSounds();
    }

    public void ShowScoreboard()
    {
        buttonPanel.SetActive(false);
        scoreboardPanel.SetActive(true);
    }

    public void ShowMainMenu()
    {
        buttonPanel.SetActive(true);
        scoreboardPanel.SetActive(false);
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game");
        Application.Quit();
    }
}
