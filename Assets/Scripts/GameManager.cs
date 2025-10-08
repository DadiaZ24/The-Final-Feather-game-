using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GameManager : MonoBehaviour
{
    private static WaitForSeconds _waitForSeconds1 = new WaitForSeconds(1f);
    public static GameManager Instance;

    [Header("Stage Settings")]
    public int currentStage = 1;
    public int enemiesPerStage = 10;
    public float spawnRadius = 15f;
    public float timeBetweenStages = 3f;

    [Header("References")]
    public GameObject enemyPrefab;
    public Transform player;
    public Transform[] spawnPoints;
    public GameObject perkMenuUI;

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

        SpawnEnemies(enemiesPerStage);

        yield return new WaitUntil(() => aliveEnemies.Count == 0);

        yield return _waitForSeconds1;

        ShowPerkSelection();
    }

    void SpawnEnemies(int nbr)
    {
        // Penso que seja facil de entender. Instanciamos o prefab do inimigo na posição random.
        for (int i = 0; i < nbr; i++)
        {
            Vector3 spawnPos = GetRandomSpawnPosition();
            GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
            aliveEnemies.Add(enemy);

            var health = enemy.GetComponentInChildren<EnemyHealthSystem>();
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
        float planeMinX = -50f;
        float planeMaxX = 50f;
        float planeMinZ = -50f;
        float planeMaxZ = 50f;

        Vector3 randomPos = new(
            Random.Range(planeMinX, planeMaxX),
            10f,
            Random.Range(planeMinZ, planeMaxZ)
        );

        if (NavMesh.SamplePosition(randomPos, out NavMeshHit hit, 20f, NavMesh.AllAreas))
        {
            return hit.position;
        }

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
        perkMenuUI.SetActive(false);

        currentStage++;
        enemiesPerStage += 5;
        StartCoroutine(StartStage());
    }
}
