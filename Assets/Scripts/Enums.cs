//TODO: Convert these to bitmasks
public enum MagicType
{
	Black,
	White
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
	Damage,
	UndeadDamage,
	StatusAilment,
	HitDown,
	MoraleDown,
	Heal,
	RestoreStatus,
	DefenseUp,
	ResistElement,
	AttackUp,
	HitMultiplierUp,
	AttackAndAccuracyUp,
	EvasionDown,
	FullHeal,
	EvasionUp,
	RemoveResistance,
	ThreeHundredHPStatus,
}

public enum SpecialEffect
{
	None,
	Dragons,
	Giants,
	Undead,
	Were,
	Water,
	Magic
}

public enum TargetType
{
	SingleEnemy,
	SingleAlly,
	AllEnemies,
	AllAllies,
	Caster
}

public enum WeaponType
{
	None,
	Knife,
	Sword,
	Axe,
	Hammer,
	Staff,
	Rod,
	Nunchuck
}

public enum ArmorType
{
	None,
	Head,
	Body,
	Shield,
	Arms,
}