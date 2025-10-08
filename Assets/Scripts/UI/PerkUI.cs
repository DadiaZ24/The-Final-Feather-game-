using UnityEngine;

public class PerkUI : MonoBehaviour
{
    // Nao ha muito que saber neste script. Mais tarde adicionamos um sistema de random things a aparecer.
    private StatsSystem playerStats;

    void Start()
    {
        // No inicio do script queremos sempre manter o menu fechado.
        playerStats = GameManager.Instance.player.GetComponent<StatsSystem>();
        gameObject.SetActive(false);
    }

    public void ShowMenu()
    {
        // Mostrar menu, pausar o jogo (podemos remover depois e deixar o jogo a correr com as animações, simplesmente bloquear camara)
        gameObject.SetActive(true);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void HideMenu()
    {
        gameObject.SetActive(false);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void OnHealthPerk()
    {
        playerStats.maxHealth += 20;
        playerStats.TakeDamage(0);
        HideMenu();
        GameManager.Instance.OnPerkSelected();
    }

    public void OnDamagePerk()
    {
        playerStats.basicAttackDamage += 2f;
        HideMenu();
        GameManager.Instance.OnPerkSelected();
    }

    public void OnSpeedPerk()
    {
        playerStats.agility += 2f;
        HideMenu();
        GameManager.Instance.OnPerkSelected();
    }
}
