using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform player;

    [Header("Settings")]
    public float distance = 5f;
    public float height = 2f;
    public float mouseSense = 2.0f;

    private float currentYaw = 0f;
    private float currentPitch = 10f;
    void LateUpdate()
    {
        if (!player)
        {
            Debug.LogError("There is no player attached to the script");
            return;
        }

        float mouseX = Input.GetAxis("Mouse X") * mouseSense;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSense;

        currentYaw += mouseX;
        currentPitch -= mouseY;
        currentPitch = Mathf.Clamp(currentPitch, -20f, 60f);

        Quaternion rotation = Quaternion.Euler(currentPitch, currentYaw, 0f);
        Vector3 offset = rotation * new Vector3(0, height, -distance);

        transform.position = player.position + offset;

        transform.LookAt(player.position + 0.5f * height * Vector3.up);
    }
}
