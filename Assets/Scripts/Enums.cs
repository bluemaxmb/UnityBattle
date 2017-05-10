using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//TODO: Convert these to bitmasks
public enum MagicType
{
	Black,
	White
}

public enum Element
{
	None,
	Fire
}

public enum Classes
{
	Fighter,
	RedMage,
	WhiteMage,
	BlackMage
}

public enum SpellEffect
{
	Heal,
	Damage,
}

public enum TargetType
{
	SingleEnemy,
	SingleAlly,
	AllEnemies,
	AllAllies,
	Caster
}