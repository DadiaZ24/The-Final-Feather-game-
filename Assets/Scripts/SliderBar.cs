using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class SliderBar : MonoBehaviour
{
    public enum SliderBarEnum
    {
        Health,
        Energy,
    }
    public static SliderBar Instance;
    public Slider energyBarSlider;
    public Slider healthBarSlider;

    void Awake()
    {
        Instance = this;
    }

    public void SetValue(SliderBarEnum type, float current, int max)
    {
        switch (type)
        {
            case SliderBarEnum.Energy:
                energyBarSlider.maxValue = max;
                energyBarSlider.value = current;
                break;

            case SliderBarEnum.Health:
                healthBarSlider.maxValue = max;
                healthBarSlider.value = current;
                break;
        }
    }
}
