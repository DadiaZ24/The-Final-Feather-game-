using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public static MainMenu Instance;
    [Header("Panels")]
    public GameObject buttonPanel;
    public GameObject scoreboardPanel;
    public GameObject nicknamePanel;

    [Header("Elements")]
    public TMP_InputField nicknameInput;

    public static string PlayerNickname { get; private set; }

    void Awake()
    {
        if (!Instance)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        buttonPanel.SetActive(false);
        scoreboardPanel.SetActive(false);
        nicknamePanel.SetActive(true);
    }

    public void OnConfirmNickname()
    {
        string nickname = nicknameInput.text.Trim();

        if (string.IsNullOrEmpty(nickname))
        {
            return;
        }

        PlayerNickname = nickname;
        ShowMainMenu();
    }

    public void PlayGame()
    {
        SceneManager.LoadScene("Game");
        AudioManager.Instance.StartGameSounds();
    }


    public void ShowScoreboard()
    {
        nicknamePanel.SetActive(false);
        buttonPanel.SetActive(false);
        scoreboardPanel.SetActive(true);
    }

    public void ShowMainMenu()
    {
        nicknamePanel.SetActive(false);
        buttonPanel.SetActive(true);
        scoreboardPanel.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
