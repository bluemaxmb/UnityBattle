﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicSpellData : ScriptableObject 
{
	public string spellName; //TODO: replace with loc id
	public MagicType magicType;
	public Element element;
	public SpellEffect spellEffect;
	public TargetType targetType;
	public byte effectiveness;
	public byte accuracy;
	public int mpCost;
}
