using UnityEngine;
using UnityEngine.UI;


public interface IDamageable
{
    void TakeDamage(float amount);
}

public class EnemyHealthSystem : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    private float currentHealth;

    private Slider enemyHealthSlider;

    void Start()
    {
        enemyHealthSlider = GetComponentInChildren<Slider>();
        if (!enemyHealthSlider)
            return;
        currentHealth = maxHealth;
        if (enemyHealthSlider != null)
        {
            enemyHealthSlider.maxValue = maxHealth;
            enemyHealthSlider.value = currentHealth;
        }
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        if (currentHealth < 0) currentHealth = 0;

        if (enemyHealthSlider != null)
            enemyHealthSlider.value = currentHealth;
        Die();
    }

    public void Die()
    {
        if (currentHealth <= 0)
            Destroy(gameObject);
    }
}
