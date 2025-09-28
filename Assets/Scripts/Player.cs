using UnityEngine;

[RequireComponent(typeof(StatsSystem))]
public class Player : MonoBehaviour
{
    private StatsSystem statSystem;

    [Header("Look Settings")]
    public float mouseSensitivity = 100f;
    public Camera firstPersonCamera;
    public Camera thirdPersonCamera;

    private float verticalRotation = 0f;
    private float horizontalRotation = 0f;
    private bool isFirstPerson = true;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        SetCamera(true);


        statSystem = GetComponent<StatsSystem>();

        if (statSystem == null)
            Debug.LogError("StatsSystem not found on Player!");
    }

    void Update()
    {
        HandleLook();
        HandleCameraToggle();
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

}
