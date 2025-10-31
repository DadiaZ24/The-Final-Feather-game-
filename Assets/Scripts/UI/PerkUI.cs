using UnityEditor.SceneManagement;
using UnityEngine;

public class PerkUI : MonoBehaviour
{
    // Nao ha muito que saber neste script. Mais tarde adicionamos um sistema de random things a aparecer.
    private StatsSystem playerStats;
    public static PerkUI Instance;
    public SliderBar playerSliderBar;

    void Start()
    {
        // No inicio do script queremos sempre manter o menu fechado.
        playerStats = GameManager.Instance.player.GetComponent<StatsSystem>();
        gameObject.SetActive(false);
    }

    void Awake()
    {
        Instance = this;
    }

    public void ShowMenu()
    {
        // Mostrar menu, pausar o jogo (podemos remover depois e deixar o jogo a correr com as animações, simplesmente bloquear camara)
        gameObject.SetActive(true);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        playerSliderBar.gameObject.SetActive(false);
    }

    public void HideMenu()
    {
        gameObject.SetActive(false);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        playerSliderBar.gameObject.SetActive(true);

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

    public void OnEnergyPerk()
    {
        playerStats.maxEnergy += 20;
        HideMenu();
        GameManager.Instance.OnPerkSelected();
    }
}
