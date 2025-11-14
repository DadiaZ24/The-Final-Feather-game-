using Unity.VisualScripting;
using UnityEngine;
using System.Collections;
using System;
using static SliderBar;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class Player : MonoBehaviour
{
    // ---[WAIT FOR SECONDS VARIABLES]---
    private static readonly WaitForSeconds _waitForSeconds0_6 = new(0.6f);
    private static readonly WaitForSeconds _waitForSeconds0_2 = new(0.2f);
    private static readonly WaitForSeconds _waitForSeconds2 = new(2f);
	private static readonly WaitForSeconds _waitForSeconds0_5 = new(0.2f);
    //-------------------------------------------------

    // ---[STATS SYSTEM]---
    [Header("Base Stats Settings")]
    public int maxEnergy = 100;
    public int maxHealth = 100;
    public float attackDamage = 10f;
    public float armor = 0f;
    [Tooltip("Não esquecer de utilizar números decimais de 0-1 (%)")]
    public float armorPen = 0f;
    public float agility = 1.0f;
    private float currentHealth;
    private float currentEnergy;
    [Header("Habilities")]
    public PlayerAbilities SparrowQuake;
    public PlayerAbilities ScreamScript;
    public PlayerAbilities HealthPotions;

    [Header("Regen Settings")]
    public float energyRegenRate = 5f;
    private SliderBar playerSliderBar;
    //----------------------------------------

    // ---[SOUNDS AND ANIMATIONS]---
    [Header("Sound Settings")]
    public AudioClip SparrowQuakeSoundSource;

    [Header("VFX Settings")]
    public float attackVfxHeigh = 0.4f;

    [Header("VFX")]
    public GameObject Smash1;
    public GameObject Smash2;
    public GameObject Attack;
    public GameObject Scream;
    public GameObject Heal;

    //Animations
    private Animator animator;
    private float movementAnimationSpeed;
    //----------------------------------------

    // ---[CAMERA SETTINGS]---
    [Header("View Settings")]
    public Camera playerCamera;
    public float mouseSensitivity = 2f;
    public float horizontalRotation = 0f;
    public float verticalRotation = 0f;
    //-----------------------------------------

    // ---[MOVEMENT SETTINGS]
    [Header("Movement Settings")]
    [SerializeField] private float almostGroundedDistance = 0.25f;
    private float walkSpeed = 2f;
    private float runSpeed = 5f;
    private float acceleration = 3.0f;
    private CharacterController characterController;
    private float verticalVelocity;
    private bool isGrounded;
    private bool isAlmostGrounded;
    private float jumpHeight = 1.5f;
    private Vector3 moveVelocity = Vector3.zero;

    //dash
    private float dashDistance = 4f;
    private float dashDuration = 0.2f;
    private float dashTimer;
    private bool isDashing;
    private Vector3 dashDirection;
    private float dashStep;
    private float dashDistanceMoved;
    //-----------------------------------------------------------

    // ---[ATTACK ENEMY SETTINGS]---
    [Header("Attack Settings")]
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackRadius = 0.6f;
    [SerializeField] private LayerMask enemyLayer;
    public float attackCooldown = 0.5f;
    public float knockbackForce = 4f;
    private bool canAttack = true;
    private bool isDead = false;
    public bool IsDead => isDead;
    public static Action OnPlayerDied;
    //-----------------------------------------------------
    private bool savedScore;
    public static Player Instance;

    void Awake()
    {
        Instance = this;
        savedScore = false;
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        animator = GetComponent<Animator>();
        characterController = GetComponentInChildren<CharacterController>();

        currentEnergy = maxEnergy;
        currentHealth = maxHealth;
        playerSliderBar = GetComponentInChildren<SliderBar>();
        if (playerSliderBar == null)
        {
            Debug.LogError("playerSliderBar is null");
            return;
        }
        UpdateBar();
    }

    void Update()
    {
        animator.speed = 1f;

        if (DeadControl())
            return;

        HandleLook();
        HandleMovement();
        HandleJump();
        HandleDash();
        ApplyGravity();
        HandleAbilities();
        HandleAttack();
        OnDeath();
        SmashDownWait();
        RegenerateEnergyUpdate();
        UpdatePlayerSpeed();

        // apply all movement (horizontal + vertical) once per frame
        Vector3 finalMove = (moveVelocity + Vector3.up * verticalVelocity) * Time.deltaTime;
        if (characterController != null)
            characterController.Move(finalMove);
    }

    // ------- [STATS SYSTEM METHODS] ------------------------------------------------------

    private void RegenerateEnergyUpdate()
    {
        if (currentEnergy < maxEnergy)
        {
            currentEnergy += energyRegenRate * Time.deltaTime;
            if (currentEnergy > maxEnergy)
                currentEnergy = maxEnergy;
        }
        UpdateBar();
    }

    public float GetCurrentEnergy()
    {
        return currentEnergy;
    }

    public bool UseEnergy(int amount)
    {
        if (currentEnergy >= amount)
        {
            currentEnergy -= amount;
            return true;
        }
        else
        {
            Debug.Log("Not enough energy");
            return false;
        }
    }

    public float GetCurrentHealth()
    {
        return currentHealth;
    }

    public void RegenerateHealth(float ammount)
    {
        if (currentHealth < maxHealth)
        {
            currentHealth += ammount;
            if (currentHealth > maxHealth)
                currentHealth = maxHealth;
        }
        UpdateBar();
    }

    public void TakeDamage(float amount, float enemyArmorPen = 0f)
    {
        int k = GameManager.Instance.k;
        float realArmor = armor * (1 - enemyArmorPen);
        float damageTaken = amount * (k / (realArmor + k));
        Debug.Log($"Enemy has taken {damageTaken} to the player");
        currentHealth -= damageTaken;
        if (currentHealth < 0)
            currentHealth = 0;
    }

    void UpdatePlayerSpeed()
    {
        runSpeed *= agility;
    }

    private void UpdateBar()
    {
        if (playerSliderBar != null)
        {
            playerSliderBar.SetValue(SliderBarEnum.Energy, currentEnergy, maxEnergy);
            playerSliderBar.SetValue(SliderBarEnum.Health, currentHealth, maxHealth);
        }
        else
        {
            Debug.LogWarning("playerSliderBar is null!");
        }
    }

    // -----------------------------------------------------------------------------------

    // ------- [CAMERA AND VIEW SETTINGS] ------------------------------------------------------

    void HandleLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f);
        horizontalRotation += mouseX;
    }

    // ------------------------------------------------------------------------------------------

    // ------- [MOVEMENT AND PHYSICS SETTINGS] ------------------------------------------------------

    void HandleMovement()
    {
        // Use GetAxisRaw to avoid Unity's input smoothing which can cause a stop delay
        float moveX = Input.GetAxisRaw("Horizontal"); // A/D
        float moveZ = Input.GetAxisRaw("Vertical");  // W/S

        Vector3 inputDir = new Vector3(moveX, 0f, moveZ).normalized;

        // Velocidade base
        float currentSpeed = walkSpeed;
        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        if (isRunning) currentSpeed = runSpeed;

        // Direções da câmera (apenas no plano XZ)
        Vector3 camForward = playerCamera.transform.forward;
        camForward.y = 0f;
        camForward.Normalize();

        Vector3 camRight = playerCamera.transform.right;
        camRight.y = 0f;
        camRight.Normalize();

        // Direção de movimento relativa à câmera
        Vector3 moveDir = camForward * inputDir.z + camRight * inputDir.x;
        if (moveDir.sqrMagnitude > 1f) moveDir.Normalize();

        // O player SEMPRE olha pra frente da câmera (horizontalmente)
        Quaternion targetRotation = Quaternion.LookRotation(camForward);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);

        // set horizontal velocity (do NOT call Move here)
        // Immediate stop when input is released: if magnitude is very small, snap to zero
        if (inputDir.magnitude >= 0.01f)
        {
            moveVelocity = moveDir * currentSpeed;
        }
        else
        {
            // ensure no residual small values remain
            moveVelocity = Vector3.zero;
        }

        float blendTreeTarget = 0f;
        if (inputDir.magnitude >= 0.01f)
            blendTreeTarget = isRunning ? 2f : 1f;

        movementAnimationSpeed = Mathf.MoveTowards(movementAnimationSpeed, blendTreeTarget, acceleration * Time.deltaTime);

        if (animator != null)
            animator.SetFloat("State", movementAnimationSpeed);
    }

    void ApplyGravity()
    {
        if (characterController == null) return;

        isGrounded = characterController.isGrounded;

        Vector3 origin = characterController.transform.position + characterController.center;
        float checkDistance = almostGroundedDistance + characterController.skinWidth;
        isAlmostGrounded = isGrounded || Physics.Raycast(origin, Vector3.down, out RaycastHit hit, checkDistance);

        if (isGrounded && verticalVelocity < 0)
            verticalVelocity = -2f;
        else
            verticalVelocity += Physics.gravity.y * Time.deltaTime;
    }

    void HandleJump()
    {
        if (characterController == null) return;

        if (Input.GetKeyDown(KeyCode.Space) && characterController.isGrounded)
        {
            verticalVelocity = Mathf.Sqrt(2f * jumpHeight * -Physics.gravity.y);
        }
    }

    void HandleDash()
    {
        if (isDashing)
        {
            // moveVelocity passa a ser a dashMove quando damos dash
            float dashSpeed = dashDistance / dashDuration;
            Vector3 dashMove = dashDirection * dashSpeed;
            moveVelocity = dashMove;

            float dashStep = dashSpeed * Time.deltaTime;
            dashDistanceMoved += dashStep;
            dashTimer += Time.deltaTime;

            if (dashTimer >= dashDuration || dashDistanceMoved >= dashDistance)
            {
                animator.SetBool("Dashing", false);
                isDashing = false;
                dashTimer = 0f;
                dashDistanceMoved = 0f;
                moveVelocity = Vector3.zero;
            }
            return;
        }

        if (Input.GetKeyDown(KeyCode.V) && !isDashing && GetCurrentEnergy() >= 25)
        {
            animator.SetBool("Dashing", true);
            dashDirection = transform.forward;
            UseEnergy(25);
            isDashing = true;
            dashTimer = 0f;
            dashDistanceMoved = 0f;
        }
    }

    void HandleAttack()
    {
        if (isDead) return;

        if (Input.GetMouseButtonDown(0) && canAttack)
        {
            canAttack = false;
            StartCoroutine(ResetAttackCooldown());
            // trigger animation
            if (animator != null)
                animator.SetBool("Punching", true);

            // origin & direction
            Vector3 origin = transform.position + Vector3.up * 1.0f;
            Vector3 dir = transform.forward;

            // com isto, permite-nos atingir vários inimigos duma vez só. Queremos manter???
            Vector3 center = origin + dir * (attackRange * 0.5f);
            Collider[] hits = new Collider[10];
            // Aqui criamos uma esfera em que os inimigos dentro dessa esfera vão levar dano.
            // Use LayerMask.value to make sure we pass an int layer mask to the physics call.
            int hitCount = Physics.OverlapSphereNonAlloc(center, attackRadius, hits, enemyLayer.value, QueryTriggerInteraction.Ignore);
            Debug.Log($"Attack sphere check: center={center}, radius={attackRadius}, hitCount={hitCount}");
            Vector3 vfxPos = new(transform.position.x, transform.position.y + attackVfxHeigh, transform.position.z);
            // Instantiate the VFX prefab and destroy the spawned instance when its particle system finishes.
            if (Attack != null)
            {
                GameObject attackInst = Instantiate(Attack, vfxPos, transform.rotation);
                // Start a coroutine that will destroy the spawned instance when its particle system finishes.
                StartCoroutine(DestroyWhenParticleFinished(attackInst));
            }
            for (int i = 0; i < hitCount; i++)
            {
                // Vamos buscar o collider atual e o sistema de vida do inimigo.
                var hit = hits[i];
                if (hit == null) continue;
                Debug.Log($"Hit collider: {hit.name}");
                // Implementação de Interfaces vai-nos permitir verificar se existe algum componente que seja damageable.
                // Isto é, eu posso ter um inimigo que perde vida, e então vai estar esse método em EnemyHealthSystem.
                // No entanto, eu posso querer dar dano a uma porta e ter esse método no DoorHealthSystem.
                // Se procurar pela Interface, vou buscar qualquer um dos sistemas
                // Exemplo: em vez de ter var dmg = hit.GetComponentInParent<EnemyHealthSystem>(); hit.GetComponentInParent<DoorHealthSystem>();
                // Só tenho hit.GetComponentInParent<IDamageable>();
                var dmg = hit.GetComponentInParent<IDamageable>();
                if (dmg != null)
                {
                    Debug.Log($"Dealing {attackDamage} damage to {hit.name} via IDamageable");
                    dmg.TakeDamage(attackDamage);
                }
                else
                {
                    // Fallback: try direct EnemyAI component if present (helps when interface isn't attached where expected)
                    var enemy = hit.GetComponentInParent<EnemyAI>();
                    if (enemy != null)
                    {
                        Debug.Log($"Dealing {attackDamage} damage to {hit.name} via EnemyAI.TakeDamage fallback");
                        enemy.TakeDamage(attackDamage);
                    }
                    else
                    {
                        Debug.LogWarning($"No IDamageable or EnemyAI found on hit '{hit.name}'");
                    }
                }

                Vector3 rawDir = hit.transform.position - transform.position;
                rawDir.y = 0f;
                if (rawDir.sqrMagnitude == 0f) rawDir = transform.forward;
                Vector3 knocbackDir = rawDir.normalized;

                var enemyForKnock = hit.GetComponentInParent<EnemyAI>();
                if (enemyForKnock)
                    enemyForKnock.ApplyKnockback(knocbackDir, knockbackForce);
            }

            // Se só quisermos dar dano a um inimigo, temos esta forma
            /*if (Physics.SphereCast(origin, attackRadius, dir, out RaycastHit hit, attackRange, enemyLayer, QueryTriggerInteraction.Ignore))
            {
                var dmg = hit.collider.GetComponentsInParent<MonoBehaviour>().OfType<IDamageable>().FirstOrDefault();
                if (dmg != null) dmg.TakeDamage(attackDamage);
            }*/
        }
        else if (animator.GetBool("Punching") == true)
            animator.SetBool("Punching", false);
    }

    void HandleAbilities()
    {
        if (Input.GetKeyDown(KeyCode.Q) && SparrowQuake.IsReady() && GetCurrentEnergy() >= SparrowQuake.energyConsumption)
        {
            SparrowQuake.Use();
            UseEnergy(SparrowQuake.energyConsumption);
            verticalVelocity = Mathf.Sqrt(2f * jumpHeight * -Physics.gravity.y);
            animator.SetBool("SmashUp", true);
            StartCoroutine(SmashMake());
        }
        if (Input.GetKeyDown(KeyCode.E) && ScreamScript.IsReady() && GetCurrentEnergy() >= ScreamScript.energyConsumption)
        {
            ScreamScript.Use();
            UseEnergy(ScreamScript.energyConsumption);
            animator.SetBool("Scream", true);
            StartCoroutine(ScreamMake());
        }
    }

    // ---------------------------------------------------------------------------------------------

    // ------- [EVENTS] ------------------------------------------------------

    void OnDeath()
    {
        if (GetCurrentHealth() <= 0f && !isDead)
        {
            isDead = true;
            // notify subscribers (enemies etc.)
            OnPlayerDied?.Invoke();
            StartCoroutine(Death());
        }
    }

    public void OnSitEnd()
    {
        if (Smash1 != null && Smash2 != null)
        {
            Vector3 spawnPos = transform.position + Vector3.down * 0.4f;
            // spawn ground-impact VFX at the lowered position
            Instantiate(Smash1, spawnPos, transform.rotation);
			Instantiate(Smash2, spawnPos, transform.rotation);
			//TODO: REATIVAR O SOM
            //AudioManager.Instance.PlaySFX(SparrowQuakeSoundSource);

            // start knockback (existing) and a delayed helper to apply AOE damage
            StartCoroutine(DelayedSmashKnockback(SparrowQuake.knockbackDelay, transform.position, SparrowQuake.knockbackRadius, knockbackForce * SparrowQuake.knockbackForceMultiplier, enemyLayer, QueryTriggerInteraction.Ignore));
            StartCoroutine(DelayedApplySmashAoEDamage(SparrowQuake.knockbackDelay, transform.position));
        }
    }

    public void OnScream()
    {
        Quaternion spawnRotation = transform.rotation * Quaternion.Euler(90, 180, 0);
        Vector3 spawnPos = new(transform.position.x, transform.position.y + 0.6f, transform.position.z);
        Instantiate(Scream, spawnPos, spawnRotation);
		float screamRange = attackRange * 3f;
        float screamAngle = 60f;
		Vector3 forward = transform.forward;

        Collider[] hits = new Collider[20];

    	int hitCount = Physics.OverlapSphereNonAlloc(
       		spawnPos,
        	screamRange,
        	hits,
        	enemyLayer.value,
        	QueryTriggerInteraction.Ignore
    	);
		for (int i = 0; i < hitCount; i++)
    {
        var hit = hits[i];
        if (hit == null) continue;

        Vector3 toTarget = hit.transform.position - spawnPos;
        toTarget.y = 0f;

        float distance = toTarget.magnitude;
        if (distance > screamRange)
            continue;

        float angle = Vector3.Angle(forward, toTarget);
        if (angle > screamAngle * 0.5f)
            continue;

        // Apply damage
        var dmg = hit.GetComponentInParent<IDamageable>();
        if (dmg != null)
        {
            dmg.TakeDamage(Player.Instance.ScreamScript.ScreamDamage);
        }
        else
        {
            var enemy = hit.GetComponentInParent<EnemyAI>();
            enemy?.TakeDamage(Player.Instance.ScreamScript.ScreamDamage);
        }
	    }
    }


    // ------- [COROUTINES] ------------------------------------------------------

    // Destroy a spawned VFX GameObject when its ParticleSystem (if any) has finished playing.
    // Falls back to a short timed destroy if there's no ParticleSystem.
    private IEnumerator DestroyWhenParticleFinished(GameObject obj)
    {
        if (obj == null)
            yield break;

        // Look for a ParticleSystem on the object or its children.
        if (!obj.TryGetComponent<ParticleSystem>(out var ps))
            ps = obj.GetComponentInChildren<ParticleSystem>();

        if (ps == null)
        {
            // No particle system found: fallback to a short delay then destroy.
            yield return _waitForSeconds2;
            Destroy(obj);
            yield break;
        }

        // Wait until the particle system is no longer alive (including children).
        // IsAlive(true) checks the children as well when 'true' is passed.
        while (ps != null && ps.IsAlive(true))
            yield return null;

        // Destroy the spawned instance when done.
        Destroy(obj);
    }
	IEnumerator SmashMake()
    {
        yield return _waitForSeconds0_6; // wait x second
        animator.SetBool("SmashUp", false);
    }
    IEnumerator ScreamMake()
    {
        yield return _waitForSeconds0_6; // wait x second
        animator.SetBool("Scream", false);
    }

    private IEnumerator DelayedSmashKnockback(float delay, Vector3 origin, float radius, float force, LayerMask layer, QueryTriggerInteraction query)
    {
        if (delay > 0f)
            yield return new WaitForSeconds(delay);
        EnemyAI.KnockbackEnemiesInRadius(origin, radius, force, layer, query);
    }

    // Delayed wrapper: after `delay` seconds, apply AOE damage using a helper.
    private IEnumerator DelayedApplySmashAoEDamage(float delay, Vector3 origin)
    {
        if (delay > 0f)
            yield return new WaitForSeconds(delay);
	// TODO MUDAR RANGE
        float radius = attackRange * 10f; // 3x the character's attack range
        float damage = Player.Instance.SparrowQuake.SmashDamage; // use player's attackDamage as the AOE damage
        ApplySmashAoEDamage(origin, radius, damage);
    }

    // Synchronous helper: checks for enemies in radius and applies damage.
    private void ApplySmashAoEDamage(Vector3 origin, float radius, float damage)
    {
        Collider[] hits = new Collider[32];
        int hitCount = Physics.OverlapSphereNonAlloc(origin, radius, hits, enemyLayer.value, QueryTriggerInteraction.Ignore);
		Debug.Log($"ApplySmashAoEDamage: origin={origin}, radius={radius}, hitCount={hitCount}");

        for (int i = 0; i < hitCount; i++)
        {
            var col = hits[i];
            if (col == null) continue;

            Debug.Log($"ApplySmashAoE hit: {col.name}");

            var dmg = col.GetComponentInParent<IDamageable>();
            if (dmg != null)
            {
                dmg.TakeDamage(damage);
                continue;
            }

            var enemy = col.GetComponentInParent<EnemyAI>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
            else
            {
                Debug.LogWarning($"ApplySmashAoEDamage: no damage handler found on {col.name}");
            }
        }
    }

    IEnumerator Death()
    {
        verticalVelocity = Mathf.Sqrt(1.2f * jumpHeight * -Physics.gravity.y);
        animator.SetBool("Death", true);
        AudioManager.Instance.StopMusic();
        AudioManager.Instance.PlaySFX(AudioManager.Instance.gameOverSound);
        yield return _waitForSeconds2;
        AudioManager.Instance.PlaySFX(AudioManager.Instance.gameOverVoice);
        yield return _waitForSeconds2;
        yield return new WaitUntil(() => characterController.isGrounded);
        if (!savedScore)
            FirebaseManager.Instance.SaveScore(MainMenu.PlayerNickname, Scoring.Instance.GetScore());
        GameManager.Instance.FinishGame();
    }

    IEnumerator ResetAttackCooldown()
    {
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    // -----------------------------------------------------------------------------------------------


    public void SmashDownWait()
    {
        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);

        if (info.IsName("SmashDown"))
        {
            if (!isAlmostGrounded)
                animator.speed = 0.1f;

            if (isAlmostGrounded)
                animator.speed = 5f;
        }
    }

    private bool DeadControl()
    {
        if (isDead)
        {
            ApplyGravity();
            Vector3 finalMoveDead = (moveVelocity + Vector3.up * verticalVelocity) * Time.deltaTime;
            if (characterController != null)
                characterController.Move(finalMoveDead);
            return true;
        }
        return false;
    }

}