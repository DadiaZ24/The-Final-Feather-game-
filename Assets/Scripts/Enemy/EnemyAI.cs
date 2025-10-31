using UnityEngine;
using UnityEngine.AI;

/* Explaining Logic:
    Neste script, vamos criar a AI que nos vai seguir durante as hordes.
    Lógica:
        1. Criar um navMesh para que o inimigo saiba por onde pode andar (com navMesh, deteta obstáculos e passa ao lado deles)
            a. Criar navMesh surface no chão e o agent no Prefab do inimigo
        2. Criar a lógica de perseguição
            a. Verificar se não estamos em distância de ataque
            b. Perseguir o player
        3. Criar lógica de ataque
            a. Se estou em attack range, ataco. NOTA: Só chamar o TakeDamage(attackDamage) quando a animação nos atinge (evento de animação)
            b. Se o player sair do range antes de a animação acertar, não leva dano (voltar a verificar a distancia entre player e inimigo)
            c. Se o player sai do range, esperar até ao final da animação para voltar a dar chase (EndAttack).*/

// Caso o GameObject onde dou attach deste script não tenha estes componentes, são criados automaticamente
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class EnemyAI : MonoBehaviour
{
    [Header("References")]
    public GameObject player;
    private NavMeshAgent agent;
    private Animator animator;

    [Header("Stats")]
    public float attackRange = 2f;
    public float attackCooldown = 1f;
    public float attackDamage = 20f;

    private float lastAttackTime = 0f;
    private bool isAttacking = false;

    void Start()
    {
        // Atribuir valor ao agent e animator
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        // Algo de bom é que podemos dar log de erros!!!! E de warnings!!!! Incríiiiiiiiiivel!
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player");
    }

    void Update()
    {
        // Enquanto ele ataca, não dá chase, right? Se não tiver um player, também não vai dar chase a nada.
        if (isAttacking || player == null)
            return;

        ChasePlayer();
    }

    void ChasePlayer()
    {
        // Calcula a distancia entre player e inimigo
        float distance = Vector3.Distance(transform.position, player.transform.position);

        if (distance > attackRange)
        {
            // Se a distancia é maior do que o attackRange, obviamente que vamos dar chase
            agent.isStopped = false;
            // Dá chase a posição do player
            agent.SetDestination(player.transform.position);

            if (animator != null)
            {
                animator.SetBool("isChasing", true);
                animator.SetBool("isAttacking", false);
            }
        }
        else
        {
            // Para o chase e ataca
            agent.isStopped = true;
            if (animator != null)
                animator.SetBool("isChasing", false);

            TryAttack();
        }
    }

    void TryAttack()
    {
        // Vamos criar um pequeno cooldown para ele não estar sempre a atacar?
        if (Time.time - lastAttackTime < attackCooldown)
            return;

        lastAttackTime = Time.time;
        isAttacking = true;

        if (animator != null)
            animator.SetBool("isAttacking", true);
    }

    // É chamado pelo Animation Event quando atinge o exato frame que escolhemos para isso.
    public void DealDamage()
    {
        if (!player) return;

        // Vamos buscar o stats system para chamarmos o método TakeDamage
        var playerStats = player.GetComponent<StatsSystem>();
        if (!playerStats) return;

        // Voltar a calcular distancia para verificar se levamos dano
        float distance = Vector3.Distance(transform.position, player.transform.position);
        if (distance <= attackRange)
            playerStats.TakeDamage(attackDamage);
    }

    // É chamado pelo Animation Event quando atinge o exato frame que escolhemos para isso.
    public void EndAttack()
    {
        isAttacking = false;
        agent.isStopped = false;

        if (animator != null)
            animator.SetBool("isAttacking", false);

        // Volta a dar chase ao player
        if (player)
            agent.SetDestination(player.transform.position);
    }

    // subscribe/unsubscribe
    void OnEnable() => Player.OnPlayerDied += StopAttacking;
    void OnDisable() => Player.OnPlayerDied -= StopAttacking;

    void StopAttacking()
    {
        if (agent != null)
        {
            agent.isStopped = true;
            agent.ResetPath();
            agent.velocity = Vector3.zero;
        }

        if (animator != null)
        {
            animator.SetBool("isChasing", false);
            animator.SetBool("isAttacking", false);
        }

        enabled = false;
    }
}
