using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework.Internal;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private static readonly WaitForSeconds _waitForSeconds1 = new(1f);
    public static GameManager Instance;

    [Header("Stage Settings")]
    public int currentStage = 1;
    public int enemiesCount = 1;
    public int enemiesPerStage = 1;
    public float spawnRadius = 15f;
    public float minSpawnDistance = 8f;
    public float timeBetweenStages = 3f;

    [Header("References")]
    public GameObject[] enemyPrefabs;
    public Transform player;
    public Transform[] spawnPoints;
    public GameObject perkMenuUI;
    public GameObject spawner;
    [Tooltip("The value to change dificulty about the armor reduce damage")]
    public int k = 20;


    private List<GameObject> aliveEnemies = new();

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        StartCoroutine(StartStage());
    }


    // Logica deste sistema: Vamos spawnar o numero de inimigos da stage. Quando estiverem todos mortos, esperamos 1 segundo e mostramos as escolhas.
    IEnumerator StartStage()
    {

        SpawnEnemies(enemiesCount);

        yield return new WaitUntil(() => aliveEnemies.Count == 0);

        yield return _waitForSeconds1;

        ShowPerkSelection();
    }

    void SpawnEnemies(int nbr)
    {
        // Penso que seja facil de entender. Instanciamos o prefab do inimigo na posição random.
        for (int i = 0; i < nbr; i++)
        {
            int id = Random.Range(0, enemyPrefabs.Length);
            Vector3 spawnPos = GetRandomSpawnPosition();
            GameObject enemy = Instantiate(enemyPrefabs[id], spawnPos, Quaternion.identity);
            aliveEnemies.Add(enemy);

            var health = enemy.GetComponentInChildren<EnemyAI>();
            // Talvez seja algo diferente do que vêm normalmente. Mas criamos o evento OnDeath no lado do EnemyHealthSystem.
            // Quando o inimigo atinge 0 de vida, damos Invoke deste evento e ele vai por sua vez chamar a função OnEnemyDeath.
            if (health)
                health.OnDeath += () => OnEnemyDeath(enemy);
        }
    }

    void OnEnemyDeath(GameObject enemy)
    {
        aliveEnemies.Remove(enemy);
    }

    // Vai colocar random positions no spawn. Temos que ajustar mais tarde isto
    Vector3 GetRandomSpawnPosition()
    {
        if (!spawner.transform) return Vector3.zero;

        float sqMin = minSpawnDistance * minSpawnDistance;
        float sqMax = spawnRadius * spawnRadius;

        float r = Mathf.Sqrt(Random.Range(sqMin, sqMax));
        float ang = Random.Range(0f, Mathf.PI * 2f);
        Vector3 offset = new Vector3(Mathf.Cos(ang), 0f, Mathf.Sin(ang)) * r;
        Vector3 samplePos = spawner.transform.position + offset;

        if (NavMesh.SamplePosition(samplePos, out NavMeshHit hit, 20f, NavMesh.AllAreas))
            return hit.position;

        return Vector3.zero;
    }

    void ShowPerkSelection()
    {
        var perkUI = perkMenuUI.GetComponent<PerkUI>();
        if (perkUI)
            perkUI.ShowMenu();
    }

    public void OnPerkSelected()
    {
        currentStage++;
        InfoUI.Instance.UpdateStage(currentStage);
        enemiesCount += enemiesPerStage;
        StartCoroutine(StartStage());
    }

    public void FinishGame()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        AudioManager.Instance.StopMusic();
        AudioManager.Instance.PlayMusic(AudioManager.Instance.gameOverMusic);
        SceneManager.LoadScene("GameOver");
    }

}
