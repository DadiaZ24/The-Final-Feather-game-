using Unity.VisualScripting;
using UnityEngine;
using System.Collections;
using System;


[RequireComponent(typeof(StatsSystem))]
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class Player : MonoBehaviour
{
    private static readonly WaitForSeconds _waitForSeconds0_6 = new(0.6f);
    private static readonly WaitForSeconds _waitForSeconds0_2 = new(0.2f);

    private static readonly WaitForSeconds _waitForSeconds2 = new(2f);
    [Header("Sound Settings")]
    public AudioSource SparrowQuakeSoundSource;
    private static readonly WaitForSeconds _waitForSeconds0_5 = new(0.2f);
    [Header("VFX Settings")]
    public float attackVfxHeigh = 0.4f;

    [Header("VFX")]
    public GameObject Smash1;
    public GameObject Smash2;
    public GameObject Attack;
    public GameObject Scream;
    public GameObject Heal;

    [Header("View Settings")]
    public Camera firstPersonCamera;
    public Camera thirdPersonCamera;
    public float mouseSensitivity = 2f;
    public float horizontalRotation = 0f;
    public float verticalRotation = 0f;

    private bool isFirstPerson = true;

    [Header("Movement Settings")]
    private CharacterController characterController;

    private float walkSpeed = 2f;
    private float runSpeed = 5f;
    private float acceleration = 3.0f;

    private float verticalVelocity;
    private bool isGrounded;
    private bool isAlmostGrounded;
    [SerializeField] private float almostGroundedDistance = 0.25f;
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
    private StatsSystem statSystem;



    //Animations
    private float horizontalAnimVelocity = 0.0f;
    private float verticalAnimVelocity = 0.0f;
    private Animator animator;

    [Header("Attack Settings")]
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackRadius = 0.6f;
    [SerializeField] private float attackDamage = 20f;
    [SerializeField] private LayerMask enemyLayer;
    public static Player Instance;

    private bool isDead = false;
    public bool IsDead => isDead;

    public static Action OnPlayerDied;

    private bool savedScore;

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

        SetCamera(true);

        statSystem = GetComponent<StatsSystem>();

        if (statSystem == null)
            Debug.LogError("StatsSystem not found on Player!");
    }

    void Update()
    {
        animator.speed = 1f;

        if (isDead)
        {
            ApplyGravity();
            Vector3 finalMoveDead = (moveVelocity + Vector3.up * verticalVelocity) * Time.deltaTime;
            if (characterController != null)
                characterController.Move(finalMoveDead);
            return;
        }
        HandleLook();
        HandleMovement();
        HandleJump();
        HandleDash();
        ApplyGravity();
        HandleAbilities();
        HandleAttack();
        HandleCameraToggle();
        HandleAnimation();
        OnDeath();
        SmashDownWait();

        // apply all movement (horizontal + vertical) once per frame
        Vector3 finalMove = (moveVelocity + Vector3.up * verticalVelocity) * Time.deltaTime;
        if (characterController != null)
            characterController.Move(finalMove);
    }
    void HandleLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f);
        horizontalRotation += mouseX;

        if (isFirstPerson)
        {
            // FPS style: body rotates on Y, camera rotates up/down
            firstPersonCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
            transform.rotation = Quaternion.Euler(0f, horizontalRotation, 0f);
        }
    }

    void HandleCameraToggle()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            isFirstPerson = !isFirstPerson;
            SetCamera(isFirstPerson);
        }
    }

    void SetCamera(bool firstPerson)
    {
        firstPersonCamera.gameObject.SetActive(firstPerson);
        thirdPersonCamera.gameObject.SetActive(!firstPerson);
    }

    void HandleView()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        horizontalRotation += mouseX;
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f);

        transform.localRotation = Quaternion.Euler(verticalRotation, horizontalRotation, 0f);
    }

    void HandleMovement()
    {
        // Use GetAxisRaw to avoid Unity's input smoothing which can cause a stop delay
        float moveX = Input.GetAxisRaw("Horizontal"); // A/D
        float moveZ = Input.GetAxisRaw("Vertical");  // W/S

        Vector3 inputDir = new Vector3(moveX, 0f, moveZ).normalized;

        // Velocidade base
        float currentSpeed = walkSpeed;
        if (Input.GetKey(KeyCode.LeftShift)) currentSpeed = runSpeed;

        // Pega a câmera ativa
        Transform camTransform = isFirstPerson ? firstPersonCamera.transform : thirdPersonCamera.transform;

        // Direções da câmera (apenas no plano XZ)
        Vector3 camForward = camTransform.forward;
        camForward.y = 0f;
        camForward.Normalize();

        Vector3 camRight = camTransform.right;
        camRight.y = 0f;
        camRight.Normalize();

        // Direção de movimento relativa à câmera
        Vector3 moveDir = (camForward * inputDir.z + camRight * inputDir.x);
        if (moveDir.sqrMagnitude > 1f) moveDir.Normalize();

        // O player SEMPRE olha pra frente da câmera (horizontalmente)
        if (!isFirstPerson && moveDir.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(camForward);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }

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

        if (Input.GetKeyDown(KeyCode.V) && !isDashing && statSystem.GetCurrentEnergy() >= 25)
        {
            animator.SetBool("Dashing", true);
            dashDirection = transform.forward;
            statSystem.UseEnergy(25);
            isDashing = true;
            dashTimer = 0f;
            dashDistanceMoved = 0f;
        }
    }

    void HandleAttack()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // trigger animation
            if (animator != null)
                animator.SetBool("Punching", true);

            // origin & direction
            Vector3 origin = isFirstPerson ? firstPersonCamera.transform.position : transform.position + Vector3.up * 1.0f;
            Vector3 dir = isFirstPerson ? firstPersonCamera.transform.forward : transform.forward;

            // com isto, permite-nos atingir vários inimigos duma vez só. Queremos manter???
            Vector3 center = origin + dir * (attackRange * 0.5f);
            Collider[] hits = new Collider[10];
            // Aqui criamos uma esfera em que os inimigos dentro dessa esfera vão levar dano.
            int hitCount = Physics.OverlapSphereNonAlloc(center, attackRadius, hits, enemyLayer, QueryTriggerInteraction.Ignore);
            Vector3 vfxPos = new Vector3(transform.position.x, transform.position.y + attackVfxHeigh, transform.position.z);
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
                // Implementação de Interfaces vai-nos permitir verificar se existe algum componente que seja damageable.
                // Isto é, eu posso ter um inimigo que perde vida, e então vai estar esse método em EnemyHealthSystem.
                // No entanto, eu posso querer dar dano a uma porta e ter esse método no DoorHealthSystem.
                // Se procurar pela Interface, vou buscar qualquer um dos sistemas
                // Exemplo: em vez de ter var dmg = hit.GetComponentInParent<EnemyHealthSystem>(); hit.GetComponentInParent<DoorHealthSystem>();
                // Só tenho hit.GetComponentInParent<IDamageable>();
                var dmg = hit.GetComponentInParent<IDamageable>();
                dmg?.TakeDamage(attackDamage);
            }

            // Se só quisermos dar dano a um inimigo, temos esta forma
            /*if (Physics.SphereCast(origin, attackRadius, dir, out RaycastHit hit, attackRange, enemyLayer, QueryTriggerInteraction.Ignore))
            {
                var dmg = hit.collider.GetComponentsInParent<MonoBehaviour>().OfType<IDamageable>().FirstOrDefault();
                if (dmg != null) dmg.TakeDamage(attackDamage);
            }*/
        }
    }

    void HandleAbilities()
    {
        if (Input.GetKeyDown(KeyCode.Q) && statSystem.GetCurrentEnergy() >= 50)
        {
            statSystem.UseEnergy(50);
            verticalVelocity = Mathf.Sqrt(2f * jumpHeight * -Physics.gravity.y);
            animator.SetBool("SmashUp", true);
            StartCoroutine(SmashMake());
        }
        if (Input.GetKeyDown(KeyCode.E) && statSystem.GetCurrentEnergy() >= 50)
        {
            statSystem.UseEnergy(50);
            animator.SetBool("Scream", true);
            StartCoroutine(ScreamMake());
        }
    }

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

    void HandleAnimation()
    {
        if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.7f && animator.GetCurrentAnimatorStateInfo(0).IsName("Punching"))
            animator.SetBool("Punching", false);
        if (Input.GetKey(KeyCode.W))
        {
            if (Input.GetKey(KeyCode.LeftShift) && verticalAnimVelocity < 2)
                verticalAnimVelocity += Time.deltaTime * acceleration;
            else if (!Input.GetKey(KeyCode.LeftShift) && verticalAnimVelocity < 1)
                verticalAnimVelocity += Time.deltaTime * acceleration;
            if (!Input.GetKey(KeyCode.LeftShift) && verticalAnimVelocity >= 1)
                verticalAnimVelocity -= Time.deltaTime * acceleration;

            animator.SetFloat("Vertical", verticalAnimVelocity);
        }
        else if (!Input.GetKey(KeyCode.W) && verticalAnimVelocity > 0)
        {
            verticalAnimVelocity -= Time.deltaTime * acceleration;
            animator.SetFloat("Vertical", verticalAnimVelocity);

        }
        if (Input.GetKey(KeyCode.D))
        {
            if (Input.GetKey(KeyCode.LeftShift) && horizontalAnimVelocity < 2)
                horizontalAnimVelocity += Time.deltaTime * acceleration;
            else if (!Input.GetKey(KeyCode.LeftShift) && horizontalAnimVelocity < 1)
                horizontalAnimVelocity += Time.deltaTime * acceleration;

            animator.SetFloat("Horizontal", horizontalAnimVelocity);
        }
        else if (!Input.GetKey(KeyCode.D) && horizontalAnimVelocity > 0)
        {
            horizontalAnimVelocity -= Time.deltaTime * acceleration;
            animator.SetFloat("Horizontal", horizontalAnimVelocity);

        }
        if (Input.GetKey(KeyCode.S))
        {
            if (Input.GetKey(KeyCode.LeftShift) && verticalAnimVelocity > -2)
                verticalAnimVelocity -= Time.deltaTime * acceleration;
            else if (!Input.GetKey(KeyCode.LeftShift) && verticalAnimVelocity > -1)
                verticalAnimVelocity -= Time.deltaTime * acceleration;

            animator.SetFloat("Vertical", verticalAnimVelocity);
        }
        else if (!Input.GetKey(KeyCode.S) && verticalAnimVelocity < 0)
        {
            verticalAnimVelocity += Time.deltaTime * acceleration;
            animator.SetFloat("Vertical", verticalAnimVelocity);

        }
        if (Input.GetKey(KeyCode.A))
        {
            if (Input.GetKey(KeyCode.LeftShift) && horizontalAnimVelocity > -2)
                horizontalAnimVelocity -= Time.deltaTime * acceleration;
            else if (!Input.GetKey(KeyCode.LeftShift) && horizontalAnimVelocity > -1)
                horizontalAnimVelocity -= Time.deltaTime * acceleration;

            animator.SetFloat("Horizontal", horizontalAnimVelocity);
        }
        else if (!Input.GetKey(KeyCode.A) && horizontalAnimVelocity < 0)
        {
            horizontalAnimVelocity += Time.deltaTime * acceleration;
            animator.SetFloat("Horizontal", horizontalAnimVelocity);
        }
    }

    void OnDeath()
    {
        if (statSystem.GetCurrentHealth() <= 0f && !isDead)
        {
            isDead = true;
            // notify subscribers (enemies etc.)
            OnPlayerDied?.Invoke();
            StartCoroutine(Death());
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

    public void OnSitEnd()
    {
        if (Smash1 != null && Smash2 != null)
        {
            Vector3 spawnPos = transform.position + Vector3.down * 0.4f;
            Instantiate(Smash1, transform.position, transform.rotation);
            Instantiate(Smash2, transform.position, transform.rotation);
            AudioManager.Instance.PlaySFX(SparrowQuakeSoundSource.clip);
        }
    }

    public void SmashDownWait()
    {
        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);

        if (info.IsName("SmashDown"))
        {
            Debug.Log(isAlmostGrounded);
            if (!isAlmostGrounded)
                animator.speed = 0.1f;

            if (isAlmostGrounded)
                animator.speed = 5f;
        }
    }

    public void OnScream()
    {
        Quaternion spawnRotation = transform.rotation * Quaternion.Euler(90, 180, 0);
        Vector3 spawnPos = new(transform.position.x, transform.position.y + 0.6f, transform.position.z);
        Instantiate(Scream, spawnPos, spawnRotation);
    }

}