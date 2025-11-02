using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using UnityEngine.UI;
using System;

public interface IDamageable
{
    void TakeDamage(float amount);
}

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
[RequireComponent(typeof(Rigidbody))]
public class EnemyAI : MonoBehaviour, IDamageable
{
    [Header("References")]
    public GameObject enemy;
    private NavMeshAgent agent;
    public Slider enemyHealthSlider;
    private Animator animator;
    private GameObject player;

    [Header("Stats")]
    public int maxHealth = 100;
    public float attackDamage = 6f;
    public float armor = 5f;
    public float agility = 1f;
    public float attackRange = 2f;
    public float attackCooldown = 1f;
    //privates
    private float currentHealth;
    private float lastAttackTime = 0f;
    private bool isAttacking = false;

    public event Action OnDeath;
    private Scoring scoringSystem;

    [Header("Knockback Settings")]
    public float knockbackDuration = 0.18f;
    public float knockbackVelocityFactor = 5f;
    [Tooltip("If true, always use NavMesh-based knockback (ignores Rigidbody physics).")]
    public bool forceNavMeshKnockback = true;
    private Rigidbody rb;
    private Coroutine knockbackCoroutine;
    void Start()
    {
        scoringSystem = Scoring.Instance;

        if (!enemyHealthSlider)
        {
            Debug.LogError("No enemyHealthSlider!");
            return;
        }
        currentHealth = maxHealth;
        if (enemyHealthSlider != null)
        {
            enemyHealthSlider.maxValue = maxHealth;
            enemyHealthSlider.value = currentHealth;
        }

        // Atribuir valor ao agent e animator
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        // Prefer the Rigidbody on the same GameObject (common for enemy prefabs).
        rb = GetComponent<Rigidbody>();

        // Algo de bom é que podemos dar log de erros!!!! E de warnings!!!! Incríiiiiiiiiivel!
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            if (!player)
            {
                Debug.LogError("Can not find the Player!");
            }
        }
    }

    void Update()
    {
        // Enquanto ele ataca, não dá chase, right? Se não tiver um player, também não vai dar chase a nada.
        if (isAttacking || player == null)
            return;

        ChasePlayer();
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;

        if (enemyHealthSlider != null)
            enemyHealthSlider.value = currentHealth;

        if (currentHealth < 0)
        {
            currentHealth = 0;
            Die();
        }
    }

    public void Die()
    {
        OnDeath?.Invoke();
        Destroy(enemy);
        // Scoring singleton might not be initialized in some execution orders (editor tests, scenes without scoring)
        // Guard call to avoid NullReferenceException
        scoringSystem.AddScore(GameManager.Instance.defaultEnemy);
    }

    void ChasePlayer()
    {
        // Calcula a distancia entre player e inimigo
        float distance = Vector3.Distance(transform.position, player.transform.position);

        if (distance > attackRange)
        {
            // Se a distancia é maior do que o attackRange, obviamente que vamos dar chase
            if (agent != null && agent.isOnNavMesh)
            {
                agent.isStopped = false;
                // Dá chase a posição do player
                agent.SetDestination(player.transform.position);
            }

            if (animator != null)
            {
                animator.SetBool("isChasing", true);
                animator.SetBool("isAttacking", false);
            }
        }
        else
        {
            // Para o chase e ataca
            if (agent != null && agent.isOnNavMesh)
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

        // Voltar a calcular distancia para verificar se levamos dano
        float distance = Vector3.Distance(transform.position, player.transform.position);
        if (distance <= attackRange)
            Player.Instance.TakeDamage(attackDamage);
    }

    // É chamado pelo Animation Event quando atinge o exato frame que escolhemos para isso.
    public void EndAttack()
    {
        isAttacking = false;
        if (agent != null && agent.isOnNavMesh)
            agent.isStopped = false;

        if (animator != null)
            animator.SetBool("isAttacking", false);

        // Volta a dar chase ao player
        if (player && agent != null && agent.isOnNavMesh)
            agent.SetDestination(player.transform.position);
        else if (player && agent != null && !agent.isOnNavMesh)
        {
            // Try to place the agent onto the NavMesh near current position, then set destination
            agent.enabled = true;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(transform.position, out hit, 1.5f, NavMesh.AllAreas))
            {
                agent.Warp(hit.position);
                agent.isStopped = false;
                agent.SetDestination(player.transform.position);
            }
            else
            {
                // Couldn't find a NavMesh nearby; disable agent to avoid errors
                agent.enabled = false;
            }
        }
    }

    // subscribe/unsubscribe
    void OnEnable() => Player.OnPlayerDied += StopAttacking;
    void OnDisable() => Player.OnPlayerDied -= StopAttacking;

    void StopAttacking()
    {
        if (agent != null)
        {
            if (agent.isOnNavMesh)
            {
                agent.isStopped = true;
                agent.ResetPath();
                agent.velocity = Vector3.zero;
            }
        }

        if (animator != null)
        {
            animator.SetBool("isChasing", false);
            animator.SetBool("isAttacking", false);
        }

        enabled = false;
    }

    public void ApplyKnockback(Vector3 direction, float force)
    {
        if (direction.sqrMagnitude == 0f)
            direction = (transform.position - (player ? player.transform.position : transform.position)).normalized;

        direction.y = 0f;
        direction.Normalize();

        // If we already have a knockback running, stop it
        if (knockbackCoroutine != null)
        {
            StopCoroutine(knockbackCoroutine);
            knockbackCoroutine = null;
        }

        // Choose knockback method: force NavMesh if requested, otherwise prefer physics when a non-kinematic Rigidbody exists.
        if (forceNavMeshKnockback)
        {
            knockbackCoroutine = StartCoroutine(DoNavMeshKnockback(direction, force));
        }
        else if (rb != null && !rb.isKinematic)
        {
            knockbackCoroutine = StartCoroutine(DoPhysicsKnockback(direction, force));
        }
        else
        {
            knockbackCoroutine = StartCoroutine(DoNavMeshKnockback(direction, force));
        }
    }

    private IEnumerator DoPhysicsKnockback(Vector3 direction, float force)
    {
        isAttacking = false;

        // Stop AI movement
        if (agent != null)
        {
            if (agent.isOnNavMesh)
            {
                agent.isStopped = true;
                agent.ResetPath();
            }
            // disable agent to allow physics-driven motion; we'll later try to re-enable safely
            agent.enabled = false;
        }

        if (animator != null)
        {
            animator.SetBool("isChasing", false);
            animator.SetBool("isAttacking", false);
        }

        // Apply impulse
        // Reset velocity and apply impulse using Unity's correct properties.
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.AddForce((direction + Vector3.up * 0.1f).normalized * force, ForceMode.Impulse);
        }

        // Wait for knockback duration
        yield return new WaitForSeconds(knockbackDuration);

        // Stop physics drift
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // Re-enable NavMeshAgent safely
        if (agent != null)
        {
            Vector3 pos = transform.position;
            Quaternion rot = transform.rotation;

            agent.enabled = true;
            // Try to place agent on NavMesh near current position
            NavMeshHit hit;
            if (NavMesh.SamplePosition(pos, out hit, 1.5f, NavMesh.AllAreas))
            {
                agent.Warp(hit.position);
                agent.isStopped = false;
                yield return null; // wait one frame
                if (player != null)
                    agent.SetDestination(player.transform.position);
            }
            else
            {
                // couldn't find NavMesh; disable agent to avoid further errors
                agent.enabled = false;
            }
        }

        knockbackCoroutine = null;
    }

    private IEnumerator DoNavMeshKnockback(Vector3 direction, float force)
    {
        isAttacking = false;

        // Stop NavMeshAgent and prepare for manual transform-based movement
        if (agent != null)
        {
            if (agent.isOnNavMesh)
            {
                agent.isStopped = true;
                agent.ResetPath();
                agent.velocity = Vector3.zero;
            }
            agent.enabled = false;
        }

        if (animator != null)
        {
            animator.SetBool("isChasing", false);
            animator.SetBool("isAttacking", false);
        }

        // If we have a non-kinematic Rigidbody, temporarily make it kinematic so transform moves won't be fought by physics.
        bool hadRigidbody = rb != null;
        bool prevKinematic = true;
        Vector3 prevVelocity = Vector3.zero;
        Vector3 prevAngular = Vector3.zero;

        if (hadRigidbody)
        {
            prevKinematic = rb.isKinematic;
            prevVelocity = rb.linearVelocity;
            prevAngular = rb.angularVelocity;
            if (!prevKinematic)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true;
            }
        }

        float elapsed = 0f;
        Vector3 velocity = direction * (force * knockbackVelocityFactor * 0.01f);

        while (elapsed < knockbackDuration)
        {
            float t = elapsed / knockbackDuration;
            transform.position += Vector3.Lerp(velocity, Vector3.zero, t) * Time.deltaTime;
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Restore Rigidbody state
        if (hadRigidbody)
        {
            rb.isKinematic = prevKinematic;
            // Only restore velocities if the body was non-kinematic originally. Setting velocity on
            // a kinematic body is not supported and will throw (observed in Unity console).
            if (!prevKinematic)
            {
                rb.linearVelocity = prevVelocity;
                rb.angularVelocity = prevAngular;
            }
        }

        // Re-enable NavMeshAgent safely
        if (agent != null)
        {
            Vector3 pos = transform.position;
            Quaternion rot = transform.rotation;

            agent.enabled = true;
            agent.Warp(pos);
            agent.isStopped = false;

            yield return null;
            if (player != null)
                agent.SetDestination(player.transform.position);
        }

        knockbackCoroutine = null;
    }
}