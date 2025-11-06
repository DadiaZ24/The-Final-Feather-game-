using Microsoft.Unity.VisualStudio.Editor;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum Abilities
{
    SparrowQuake,
    Scream,
    HealthPotion,
}

public class AbilityUi : MonoBehaviour
{
    public Abilities ability;
    private UnityEngine.UI.Image loading;
    private TextMeshProUGUI letter;
    private TextMeshProUGUI cooldown;

    void Awake()
    {
        loading = transform.Find("Loading").GetComponent<UnityEngine.UI.Image>();
        letter = transform.Find("Letter").GetComponent<TextMeshProUGUI>();
        cooldown = ability == Abilities.HealthPotion
                                    ? transform.Find("Cooldown").GetComponent<TextMeshProUGUI>()
                                    : transform.Find("Loading").GetComponentInChildren<TextMeshProUGUI>();
    }

    void Start()
    {
        UpdateUI();
    }

    void Update()
    {
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (ability == Abilities.SparrowQuake)
        {
            if (Player.Instance.SparrowQuake.IsReady())
                UpdateReady(Player.Instance.SparrowQuake);
            else
                UpdateNotReady(Player.Instance.SparrowQuake);

        }
        else if (ability == Abilities.Scream)
        {
            if (Player.Instance.ScreamScript.IsReady())
                UpdateReady(Player.Instance.ScreamScript);
            else
                UpdateNotReady(Player.Instance.ScreamScript);
        }
        else if (ability == Abilities.HealthPotion)
        {
            if (Player.Instance.HealthPotions.IsReady())
                UpdateReady(Player.Instance.HealthPotions, true);
            else
                UpdateNotReady(Player.Instance.HealthPotions, true);
        }

    }

    private void UpdateReady(PlayerAbilities value, bool isHealthPotion = false)
    {
        if (value.IsBlocked())
        {
            loading.fillAmount = 1;
            letter.text = "";
            cooldown.text = "";
            return;
        }
        if (isHealthPotion)
        {
            loading.fillAmount = value.potions > 0 ? 0 : 1;
            letter.text = value.keyToUse.ToString();
            cooldown.text = value.potions > 1 ? value.potions.ToString() : "";
        }
        else
        {
            loading.fillAmount = 0;
            letter.text = value.keyToUse.ToString();
            cooldown.text = "";
        }
    }

    private void UpdateNotReady(PlayerAbilities value, bool isHealthPotion = false)
    {
        if (isHealthPotion || value.IsBlocked())
        {
            loading.fillAmount = 1;
            letter.text = "";
            cooldown.text = "";
        }
        else
        {
            loading.fillAmount = 1 - value.GetCooldownPercentage();
            letter.text = "";
            cooldown.text = value.GetRemainingTime().ToString();
        }
    }


}
