using System;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyStats", menuName = "Enemies/Enemy Stats")]
public class EnemyStats : ScriptableObject
{
    public string enemyName = "Default Enemy";
    public float baseHealth = 100f;
    public float baseDamage = 6f;
    public float baseArmor = 0f;
    public float baseAgility = 1f;
    public float attackRange = 1f;
    public float attackCooldown = 1f;
    [Tooltip("Não esquecer de utilizar números decimais de 0-1 (%)")]
    public float armorPen = 0f;
    public float healthMultiplierPerStage = 1.15f;
    public float damageMultiplierPerStage = 1.1f;
    public float armorFlatIncreasePerStage = 0.5f;
    public float agilityMultiplierPerStage = 1.1f;
    public int spawnStage = 1;
    public long scorePerKill = 20;
    public AudioClip attackSound;
	public AudioClip deathSound;

    public float GetHealth(int currentStage) => (float)(baseHealth * healthMultiplierPerStage); // Math.Pow(healthMultiplierPerStage, currentStage - spawnStage));
    public float GetDamage(int currentStage) => (float)(baseDamage * damageMultiplierPerStage); // Mathf.Pow(damageMultiplierPerStage, currentStage - spawnStage));
    public float GetArmor(int currentStage) => (float)(baseArmor + armorFlatIncreasePerStage); //* (currentStage - spawnStage));
    public float GetAgility(int currentStage) => (float)(baseAgility * agilityMultiplierPerStage); //* Mathf.Pow(agilityMultiplierPerStage, currentStage - spawnStage));
}
