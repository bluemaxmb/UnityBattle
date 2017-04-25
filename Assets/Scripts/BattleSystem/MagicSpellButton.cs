using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class MagicSpellButton : Button 
{
	public MagicSpellData magicSpellData { get; set; }

	public void SetMagicSpellData(MagicSpellData magicSpellData)
	{
		this.magicSpellData = magicSpellData;

		GetComponentInChildren<Text>().text = magicSpellData.spellName;
	}
}
