using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour
{
	[Header("View Settings")]
	public float mouseSensitivity = 2f;
	public float horizontalRotation = 0f;
	public float verticalRotation = 0f;

	[Header("Movement Settings")]
	private CharacterController characterController;

	private float walkSpeed = 2f;
	private float runSpeed = 5f;
	private float crouchSpeed = 1f;

	private float verticalVelocity;
	private bool isGrounded;


	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
	{
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;

		characterController = GetComponent<CharacterController>();
	}

	// Update is called once per frame
	void Update()
	{
		HandleView();
		HandleMovement();
		HandleJump();
		HandleCrouch();
		ApplyGravity();
		HandleAbilities();
		HandleAttack();
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
		//getting input from player
		float moveX = Input.GetAxis("Horizontal"); //AD keys
		float moveZ = Input.GetAxis("Vertical"); //WS keys

		float currentSpeed = walkSpeed; //start with walking speed

		if (Input.GetKey(KeyCode.LeftShift)) //shift = running
			currentSpeed = runSpeed;
		else if (Input.GetKey(KeyCode.LeftControl)) //ctrl = crounching
			currentSpeed = crouchSpeed;

		//direction to move in based on player input
		//vector3 = 3D vector (x,y,z)
		Vector3 move = transform.right * moveX + transform.forward * moveZ;

		//move player = move * speed * time between frames
		characterController.Move(move * currentSpeed * Time.deltaTime);
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

	// void HandleDash()
	// {
	// 	// double key movement forward and sides
	// }


	void HandleAttack()
	{
		if (Input.GetMouseButtonDown(0))
			Debug.Log("Basic attacking!!!");

	}

	void HandleAbilities()
	{
		if (Input.GetKeyDown(KeyCode.R))
			Debug.Log("Ability RRR!!!");
	}
}
