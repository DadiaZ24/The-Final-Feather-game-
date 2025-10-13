using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(StatsSystem))]
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class Player : MonoBehaviour
{

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
    private float crouchSpeed = 1f;

    private float verticalVelocity;
	private bool isGrounded;

	//dash
	private float dashDistance = 4f;
	private float dashDuration = 0.2f;
	private float dashTimer;
	private bool isDashing;
	private Vector3 dashDirection;
	private float dashStep;
	private float dashDistanceMoved;
	public StatsSystem statSystem;
	


    //Animations

    private float horizontalAnimVelocity = 0.0f;
    private float verticalAnimVelocity = 0.0f;
    private Animator animator;

    // attack settings
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackRadius = 0.6f;
    [SerializeField] private float attackDamage = 20f;
    [SerializeField] private LayerMask enemyLayer; // set to Enemy layer in Inspector

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
        HandleLook();
        HandleMovement();
        HandleJump();
		HandleCrouch();
		HandleDash();
        ApplyGravity();
        HandleAbilities();
        HandleAttack();
        HandleCameraToggle();
        HandleAnimation();
        // Example ability usage
        if (Input.GetKeyDown(KeyCode.Space)) // Jump/Skill
        {
            if (statSystem.UseEnergy(20)) // cost = 20
            {
                Debug.Log("Ability activated!");
                // Trigger your ability logic here
            }
            statSystem.TakeDamage(10f);
        }

        if (Input.GetKeyDown(KeyCode.F)) // Another ability
        {
            if (statSystem.UseEnergy(50)) // cost = 50
            {
                Debug.Log("Big ability activated!");
            }
        }
    }
    void HandleLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

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
        float moveX = Input.GetAxis("Horizontal"); // A/D
        float moveZ = Input.GetAxis("Vertical");   // W/S

        Vector3 inputDir = new Vector3(moveX, 0f, moveZ).normalized;

        // Velocidade base
        float currentSpeed = walkSpeed;
        if (Input.GetKey(KeyCode.LeftShift)) currentSpeed = runSpeed;
        else if (Input.GetKey(KeyCode.LeftControl)) currentSpeed = crouchSpeed;

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
        Vector3 moveDir = (camForward * inputDir.z + camRight * inputDir.x).normalized;

        // O player SEMPRE olha pra frente da câmera (horizontalmente)
        if (!isFirstPerson)
        {
            Quaternion targetRotation = Quaternion.LookRotation(camForward);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }

        // Movimento (frente/trás/lados relativos à câmera)
        if (inputDir.magnitude >= 0.1f)
        {
            characterController.Move(moveDir * currentSpeed * Time.deltaTime);
        }
    }

    void ApplyGravity()
    {
        isGrounded = characterController.isGrounded;

        if (isGrounded && verticalVelocity < 0)
            verticalVelocity = -2f;
        else
            verticalVelocity += Physics.gravity.y * Time.deltaTime;

        characterController.Move(new Vector3(0, verticalVelocity, 0) * Time.deltaTime);
    }

    void HandleJump()
    {
        //space key = jump
        bool isGrounded = CheckGrounded(); //so i can jump idle

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            Debug.Log("Jumping!!!");
            verticalVelocity = Mathf.Sqrt(2f * -Physics.gravity.y);
        }
    }

    bool CheckGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, characterController.height / 2 + 0.1f);
    }

    void HandleCrouch()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            characterController.height = 1f; //crouch height
            Debug.Log("DOWN!!!");
        }
        else if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            characterController.height = 2f; //stand height
            Debug.Log("UP!!!");
        }
    }

    void HandleDash()
	{
		if (isDashing)
		{
			float dashStep = (dashDistance / dashDuration) * Time.deltaTime;
			Vector3 move = dashDirection * dashStep;
			characterController.Move(move);
			dashDistanceMoved += move.magnitude;

			dashTimer += Time.deltaTime;

			//se chegar a distancia e tempo = acabou o dashing então fazemos reset
			if (dashTimer >= dashDuration || dashDistanceMoved >= dashDistance)
			{
				animator.SetBool("Dashing", false);
				isDashing = false;
				dashTimer = 0f;
				dashDistanceMoved = 0f;
			}
			return;
		}
		
		if (Input.GetKeyDown(KeyCode.Q) && !isDashing && statSystem.GetCurrentEnergy() >= 25)
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
        if (Input.GetKeyDown(KeyCode.R))
            Debug.Log("Ability RRR!!!");
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

}
