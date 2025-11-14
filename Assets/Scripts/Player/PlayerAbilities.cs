using UnityEngine;

[CreateAssetMenu(fileName = "PlayerAbilities", menuName = "Player/Abilities")]
public class PlayerAbilities : ScriptableObject
{
    public float knockbackRadius = 5f;
    public int knockbackForceMultiplier = 4;
    public float knockbackDelay = 0.1f;
    [SerializeField] public bool isBlocked = false;
    public float Cooldown = 60f;
    public int potions = 0;
	public float ScreamDamage = 0f;
	public float SmashDamage = 0f;

    public int energyConsumption = 0;
    [SerializeField] public char keyToUse;
    private float lastCall;

    void OnEnable()
    {
		potions = 1;
        lastCall = -Cooldown - 1;
        Player playerInstance = FindObjectOfType<Player>();
        if (playerInstance != null)
        {
			ScreamDamage = 50f;
            SmashDamage = 20f;
        }
    }

    public void Use()
    {
        lastCall = Time.time;
    }

    public void SetBlocked(bool status)
    {
        isBlocked = status;
    }

    public bool IsBlocked()
    {
        return isBlocked;
    }

    public bool IsReady()
    {
        if (isBlocked)
            return false;
        return Time.time > (lastCall + Cooldown);
    }

    public float GetCooldownPercentage()
    {
        return Mathf.Clamp01((Time.time - lastCall) / Cooldown);
    }

    public int GetRemainingTime()
    {
        return Mathf.CeilToInt(Mathf.Max(0f, (lastCall + Cooldown) - Time.time));
    }
}
