using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PerkUI : MonoBehaviour
{
	// Nao ha muito que saber neste script. Mais tarde adicionamos um sistema de random things a aparecer.
	public static PerkUI Instance;
	public SliderBar playerSliderBar;
	public PerkCardElements[] perks;
	public GameObject Options1;
	public GameObject Options2;
	public GameObject Options3;


	void Start()
	{
		// No inicio do script queremos sempre manter o menu fechado.
		gameObject.SetActive(false);
	}

	void Awake()
	{
		Instance = this;
	}

	public void ShowMenu()
	{
		SetPerks();
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

	public void SetPerks()
	{

		if (perks == null || perks.Length == 0)
		{
			Debug.LogWarning("PerkUI.SetPerks: no perks assigned in inspector.");
			return;
		}

		int id = Random.Range(0, 7);
		// currently using first perk; change to random if desired
		//TODO: APPLY RANDOM BETWEEN EACH APPLY PERK TO OPTIONS
		// Apply to UI slots (Options1/2/3 should exist in the scene)
		ApplyPerkToOption(Options1, perks[id]);
		id = Random.Range(0, 7);
		ApplyPerkToOption(Options2, perks[id]);
		id = Random.Range(0, 7);
		ApplyPerkToOption(Options3, perks[id]);
	}

	private void ApplyPerkToOption(GameObject option, PerkCardElements perk)
	{
		if (option == null || perk == null)
		{
			Debug.LogWarning($"ApplyPerkToOption: missing option or perk. option={(option==null)}, perk={(perk==null)}");
			return;
		}
		Debug.Log($"ApplyPerkToOption: option={option.name}, perk.displayName={perk.displayName}");

		// ICON
		Image iconImage = null;
		var iconTransform = option.transform.Find("Icon");
		if (iconTransform != null) iconImage = iconTransform.GetComponent<Image>();
		if (iconImage == null) iconImage = option.GetComponentInChildren<Image>();
		if (iconImage != null && perk.icon != null)
		{
			if (perk.icon.sprite != null) iconImage.sprite = perk.icon.sprite;
			iconImage.color = perk.icon.color;
		}

		// TITLE
		TextMeshProUGUI titleText = null;
		var titleTransform = option.transform.Find("Title");
		if (titleTransform != null) titleText = titleTransform.GetComponent<TextMeshProUGUI>();
		if (titleText == null) titleText = option.GetComponentInChildren<TextMeshProUGUI>();
		if (titleText != null && perk.title != null)
		{
			titleText.text = perk.title.text;
			titleText.color = perk.title.color;
		}

		// DESCRIPTION
		TextMeshProUGUI descText = null;
		var descTransform = option.transform.Find("Description");
		if (descTransform != null) descText = descTransform.GetComponent<TextMeshProUGUI>();
		if (descText == null)
		{
			var tmps = option.GetComponentsInChildren<TextMeshProUGUI>();
			if (tmps.Length == 1) descText = tmps[0];
			else if (tmps.Length >= 2)
			{
				descText = tmps[0] == titleText ? tmps[1] : tmps[0];
			}
		}
		if (descText != null && perk.description != null)
		{
			descText.text = perk.description.text;
			descText.color = perk.description.color;
		}
	}

	public void OnHealthPerk()
	{
		Player.Instance.maxHealth += 20;
		Player.Instance.TakeDamage(0);
		HideMenu();
		GameManager.Instance.OnPerkSelected();
	}

	public void OnPotionPerk()
	{
		Player.Instance.HealthPotions.potions += 1;
		HideMenu();
		GameManager.Instance.OnPerkSelected();
	}

	public void OnDamagePerk()
	{
		Player.Instance.attackDamage += 2f;
		HideMenu();
		GameManager.Instance.OnPerkSelected();
	}
	public void OnArmorPerk()
	{
		Player.Instance.armor += 10f;
		HideMenu();
		GameManager.Instance.OnPerkSelected();
	}

	public void OnArmorPenPerk()
	{
		Player.Instance.armorPen += 0.2f;
		HideMenu();
		GameManager.Instance.OnPerkSelected();
	}

	public void OnEnergyPerk()
	{
		Player.Instance.maxEnergy += 20;
		HideMenu();
		GameManager.Instance.OnPerkSelected();
	}

	public void OnEnergyRegenPerk()
	{
		Player.Instance.energyRegenRate += 1f;
		HideMenu();
		GameManager.Instance.OnPerkSelected();
	}

}

[System.Serializable]
public class PerkCardElements
{
	public string displayName;
	public Image icon;
	public TextMeshProUGUI title;
	public TextMeshProUGUI description;

	public PerkCardElements(Image icon, TextMeshProUGUI title, TextMeshProUGUI description)
	{
		this.icon = icon;
		this.title = title;
		this.description = description;
	}
}
