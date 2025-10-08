using System.Threading;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas))]
public class SliderBar : MonoBehaviour
{
    public enum SliderBarEnum
    {
        Health,
        Energy,
    }
    private Slider energyBarSlider;
    private Slider healthBarSlider;
    public Transform target;
    public Vector3 offset = new(0, 1.2f, 0);
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
        if (energyBarSlider == null && healthBarSlider == null)
        {
            var sliders = GetComponentsInChildren<Slider>();
            foreach (var s in sliders)
            {
                var name = s.gameObject.name.ToLower();
                if (energyBarSlider == null && name.Contains("energy")) energyBarSlider = s;
                if (healthBarSlider == null && name.Contains("health")) healthBarSlider = s;
            }
        }
    }

    void LateUpdate()
    {
        FollowAsset();
    }

    public void SetValue(SliderBarEnum type, float current, int max)
    {
        if (type == SliderBarEnum.Energy && energyBarSlider != null)
        {
            energyBarSlider.maxValue = max;
            energyBarSlider.value = current;
        }
        else if (type == SliderBarEnum.Health && healthBarSlider != null)
        {
            healthBarSlider.maxValue = max;
            healthBarSlider.value = current;
        }

        /*else if (type == SliderBarEnum.EnemyHealth && enemyHealthBarSlider != null)
        {
            enemyHealthBarSlider.maxValue = max;
            enemyHealthBarSlider.value = current;
        }*/
    }

    void FollowAsset()
    {
        var canvas = GetComponent<Canvas>();

        if (canvas == null || target == null) return;
        if (canvas.renderMode != RenderMode.WorldSpace) return;

        transform.position = target.position + offset;

        if (mainCamera != null)
        {
            transform.LookAt(mainCamera.transform.position, Vector3.up);
        }
    }
}
