using UnityEngine;
using UnityEngine.UI;
using static SliderBar;

public class StatsSystem : MonoBehaviour
{
    [Header("Energy Settings")]
    public int maxEnergy = 100;
    private float currentEnergy;
    public float energyRegenRate = 5f;

    [Header("Health Settings")]
    public int maxHealth = 100;
    private float currentHealth;
    public float healthRegenRate = 0.2f;

    [Header("Stats Settings")]
    public float basicAttackDamage = 1f;
    public float armor = 0f;
    public float agility = 0f;

    private SliderBar playerSliderBar;
    void Start()
    {
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
        RegenerateEnergy();
        RegenerateHealth();

        UpdateBar();
    }

	// ENERGY METHODS
	private void RegenerateEnergy()
	{
		if (currentEnergy < maxEnergy)
		{
			currentEnergy += energyRegenRate * Time.deltaTime;
			if (currentEnergy > maxEnergy)
				currentEnergy = maxEnergy;
		}
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

    // HEALTH METHODS
    private void RegenerateHealth()
    {
        if (currentHealth < maxHealth)
        {
            currentHealth += healthRegenRate * Time.deltaTime;
            if (currentHealth > maxHealth)
                currentHealth = maxHealth;
        }
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        if (currentHealth < 0)
            currentHealth = 0;
    }

    // UPDATE BAR METHODS

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
}
