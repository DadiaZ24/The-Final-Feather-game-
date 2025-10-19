using TMPro;
using UnityEngine;

public class InfoUI : MonoBehaviour
{
    private TMP_Text stageText;
    private TMP_Text timeText;
    private TMP_Text scoreText;
    public static InfoUI Instance;
    void Awake()
    {
        stageText = transform.Find("StagePanel/Text").GetComponent<TMP_Text>();
        timeText = transform.Find("TimePanel/Text").GetComponent<TMP_Text>();
        scoreText = transform.Find("ScorePanel/Text").GetComponent<TMP_Text>();
        Instance = this;
        stageText.text = 1.ToString();
        scoreText.text = 0.ToString();
    }

    void Update()
    {
        float time = Time.time;
        int hours = Mathf.FloorToInt(time / 3600f);
        int minutes = Mathf.FloorToInt((time % 3600f) / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);

        timeText.text = string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, seconds);
    }
    public void UpdateUI(int stage, int score)
    {
        stageText.text = stage.ToString();
        scoreText.text = score.ToString();
    }
}
