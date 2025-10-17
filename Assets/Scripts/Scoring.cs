using UnityEngine;

public class Scoring : MonoBehaviour
{
	private int score = 0;
	public static Scoring Instance;

	void Awake() //inicializa tudo que tem aqui antes do Start
	{
		Instance = this;
	}
	
	public void AddScore(int nbr)
	{
		score += nbr;
		Debug.Log($"{score}");
		//update UI call
	}
}
