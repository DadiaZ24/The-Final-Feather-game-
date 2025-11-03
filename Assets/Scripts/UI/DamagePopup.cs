using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DamagePopup : MonoBehaviour
{
    public float lifetime = 1f;
    public float floatSpeed = 1f;
    public Vector3 randomOffset = new(0.5f, 0.5f, 0.5f);

    private TextMeshPro textMesh;
    private Color textColor;
    private Transform mainCamera;

    void Awake()
    {
        textMesh = GetComponent<TextMeshPro>();
        textColor = textMesh.color;
        mainCamera = Camera.main.transform;
    }

    void Update()
    {
        transform.LookAt(transform.position + mainCamera.forward);

        transform.position += floatSpeed * Time.deltaTime * Vector3.up;

        textColor.a -= Time.deltaTime / lifetime;
        textMesh.color = textColor;

        if (textColor.a <= 0f)
            Destroy(gameObject);
    }

    public void Setup(int damage, Color color)
    {
        textMesh.text = damage.ToString();
        textMesh.color = color;
        textColor = color;
    }
}
