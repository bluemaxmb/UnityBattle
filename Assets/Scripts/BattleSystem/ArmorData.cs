﻿using UnityEngine;

public class ArmorData : ScriptableObject
{
	public ElementTypeMask elementResist;
	public ArmorType armorType;
	public string armorName; //TODO: replace with loc id
	public byte defense;
	public byte weight;
	public int magicSpellIndex;
}