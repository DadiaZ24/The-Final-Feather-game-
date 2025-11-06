using UnityEngine;

public class Scoring : MonoBehaviour
{
	private long score = 0;
	public static Scoring Instance;

	void Awake() //inicializa tudo que tem aqui antes do Start
	{
		Instance = this;
	}

	public long GetScore()
	{
		return score;
	}

	public void AddScore(long nbr)
	{
		score += nbr;
		InfoUI.Instance.UpdateScore(score);
	}
}
