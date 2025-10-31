using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Scoreboard : MonoBehaviour
{
    private static readonly WaitForSeconds _waitForSeconds0_5 = new(0.5f);
    public Transform contentParent;
    public GameObject scoreRowPrefab;
    void Start()
    {
        StartCoroutine(WaitForFirebase());
    }

    void OnScoresLoaded(List<PlayerScore> scores)
    {
        scores.Sort((a, b) => b.score.CompareTo(a.score));

        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < scores.Count; i++)
        {
            GameObject row = Instantiate(scoreRowPrefab, contentParent);
            row.transform.Find("RankText").GetComponent<TextMeshProUGUI>().text = (i + 1).ToString();
            row.transform.Find("UsernameText").GetComponent<TextMeshProUGUI>().text = scores[i].username;
            row.transform.Find("ScoreText").GetComponent<TextMeshProUGUI>().text = scores[i].score.ToString();
        }
    }

    private IEnumerator WaitForFirebase()
    {
        while (!FirebaseManager.IsReady)
            yield return _waitForSeconds0_5;
        FirebaseManager.Instance.GetScores(OnScoresLoaded);
    }

    public void OnReloadGame()
    {
        Debug.Log("CLICKED!!!");
        MainMenu.Instance.PlayGame();
    }

    public void OnQuitGame()
    {
        Application.Quit();
    }
}

[System.Serializable]
public class PlayerScore
{
    public string username;
    public long score;

    public PlayerScore(string username, long score)
    {
        this.username = username;
        this.score = score;
    }
}