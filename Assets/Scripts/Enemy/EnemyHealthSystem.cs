using System;
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
    public event Action OnDeath;
    private Scoring scoringSystem;

    void Start()
    {
        scoringSystem = Scoring.Instance;
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

        if (enemyHealthSlider != null)
            enemyHealthSlider.value = currentHealth;

        if (currentHealth < 0)
        {
            currentHealth = 0;
            Die();
        }
    }

    public void Die()
    {
        OnDeath?.Invoke();
        Destroy(gameObject);
        // Scoring singleton might not be initialized in some execution orders (editor tests, scenes without scoring)
        // Guard call to avoid NullReferenceException
        scoringSystem.AddScore(20);
    }
}
