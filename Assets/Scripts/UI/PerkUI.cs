using UnityEditor.SceneManagement;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class PerkUI : MonoBehaviour
{
	// Nao ha muito que saber neste script. Mais tarde adicionamos um sistema de random things a aparecer.
	public static PerkUI Instance;
	public SliderBar playerSliderBar;
	public PerkCardElements[] perks;
	public GameObject Options1;
	public GameObject Options2;
	public GameObject Options3;

	public int id1;
	public int id2;
	public int id3;
	int armor = 0;
	int armorpen = 0;

	int screamflag = 0;
	int smashflag = 0;

	int potioncount = 0;
    // persistent pool of available perk ids; removing an id here prevents it
    // from appearing again until the pool is refilled.
    private List<int> availablePerkIds;



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

		// Initialize or refill availablePerkIds when needed. If there are fewer
		// than 3 available, refill so the menu can always show three options.
		if (availablePerkIds == null || availablePerkIds.Count < 3)
		{
			availablePerkIds = new List<int>();
			for (int i = 0; i < perks.Length; i++) availablePerkIds.Add(i);
		}
		// Work from a temporary pool so we don't remove items until player picks one.
		var pool = new List<int>(availablePerkIds);
		if (smashflag == 0)
		{
			pool.Remove(11);
			pool.Remove(12);
		}
		if (screamflag == 0)
		{
			pool.Remove(8);
			pool.Remove(9);
		}
		if (armor == 1)
			pool.Remove(4);
		if (armorpen == 2)
			pool.Remove(5);
		if (screamflag == 1)
			pool.Remove(7);
		if (smashflag == 1)
			pool.Remove(10);
		if (potioncount == 5)
			pool.Remove(3);
		var shuffled = pool.OrderBy(x => Random.value).Take(3).ToList();
		// pool = pool.Where(i => i >= 0 && i < perks.Length).ToList();
		// var shuffled = pool.OrderBy(x => Random.value).ToList();
		id1 = shuffled[0];
		id2 = shuffled[1];
		id3 = shuffled[2];
		// id1 = pool[Random.Range(0, pool.Count)];
		// do { id2 = pool[Random.Range(0, pool.Count)]; } while (id2 == id1);
		// do { id3 = pool[Random.Range(0, pool.Count)]; } while (id3 == id1 || id3 == id2);
		ApplyPerkToOption(Options1, perks[id1]);
		ApplyPerkToOption(Options2, perks[id2]);
		ApplyPerkToOption(Options3, perks[id3]);
	}

	public void SetPerkOption(int i)
	{
		int index;
		if (i >= 1 && i <= 3) index = i - 1;
		else if (i >= 0 && i <= 2) index = i;
		else
		{
			Debug.LogWarning("SetPerkOption: received invalid button parameter: " + i);
			return;
		}
		int[] ids = new int[] { id1, id2, id3 };
		int id = ids[index];
		// Remove the selected id from the persistent available list so it won't
		// be offered again in future menus (until the pool is refilled).
		if (availablePerkIds != null)
		{
			availablePerkIds.Remove(id);
		}
		switch (id)
		{
			case 0: OnHealthPerk(); break;
			case 1: OnEnergyPerk(); break;
			case 2: OnDamagePerk(); break;
			case 3: OnPotionPerk(); break;
			case 4: OnArmorPerk(); break;
			case 5: OnArmorPenPerk(); break;	
			case 6: OnEnergyRegenPerk(); break;
			case 7: OnScreamPerk(); break;
			case 8: OnScreamCooldownPerk(); break;
			case 9: OnScreamDamagePerk(); break;
			case 10: OnSmashPerk(); break;
			case 11: OnSmashDamagePerk(); break;
			case 12: OnSmashCooldownPerk(); break;
			default: Debug.LogWarning("SetPerkOption: unknown perk id " + id); break;
		}
	}

	private void ApplyPerkToOption(GameObject option, PerkCardElements perk)
	{
		if (option == null || perk == null)
		{
			Debug.LogWarning($"ApplyPerkToOption: missing option or perk. option={(option==null)}, perk={(perk==null)}");
			return;
		}
		//Debug.Log($"ApplyPerkToOption: option={option.name}, perk.displayName={perk.displayName}");

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
		potioncount++;
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
		armor++;
		Player.Instance.armor += 10f;
		HideMenu();
		GameManager.Instance.OnPerkSelected();
	}

	public void OnArmorPenPerk()
	{
		armorpen++;
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

	public void OnScreamPerk()
	{
		screamflag = 1;
		Player.Instance.ScreamScript.SetBlocked(false);
		HideMenu();
		GameManager.Instance.OnPerkSelected();
	}

	public void OnScreamDamagePerk()
	{
		Player.Instance.ScreamScript.ScreamDamage *= 1.2f;
		HideMenu();
		GameManager.Instance.OnPerkSelected();
	}

	public void OnScreamCooldownPerk()
	{
		Player.Instance.ScreamScript.Cooldown -= 1f;
		HideMenu();
		GameManager.Instance.OnPerkSelected();
	}

	public void OnSmashPerk()
	{
		smashflag = 1;
		Player.Instance.SparrowQuake.SetBlocked(false);
		HideMenu();
		GameManager.Instance.OnPerkSelected();
	}

	public void OnSmashDamagePerk()
	{
		Player.Instance.SparrowQuake.SmashDamage *= 1.2f;
		HideMenu();
		GameManager.Instance.OnPerkSelected();
	}

	public void OnSmashCooldownPerk()
	{
		Player.Instance.SparrowQuake.Cooldown -= 0.5f;
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
