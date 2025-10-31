using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Scoreboard : MonoBehaviour
{
    public Transform contentParent;
    public GameObject scoreRowPrefab;

    private List<PlayerScore> scores = new();

    void Start()
    {
        scores.Add(new PlayerScore { username = "Alex", score = 9999999999 });
        scores.Add(new PlayerScore { username = "Nuno", score = 9999999998 });
        scores.Add(new PlayerScore { username = "DadiaZ", score = 9999999996 });
        scores.Add(new PlayerScore { username = "Melissa", score = 9999999997 });
        scores.Add(new PlayerScore { username = "asgagasg", score = 9999999998 });
        scores.Add(new PlayerScore { username = "sagvaascva", score = 51512 });
        scores.Add(new PlayerScore { username = "savasgsagas", score = 1561 });
        scores.Add(new PlayerScore { username = "sagasvasvsa", score = 165156156 });
        scores.Add(new PlayerScore { username = "safgsavsavasaa", score = 52352 });
        scores.Add(new PlayerScore { username = "ascascsacasca", score = 6623 });
        scores.Add(new PlayerScore { username = "sgbavasvavaas", score = 886 });

        scores.Sort((a, b) => b.score.CompareTo(a.score));

        PopulateScoreboard();
    }

    void PopulateScoreboard()
    {
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
}

[System.Serializable]
public class PlayerScore
{
    public string username;
    public long score;
}