using UnityEngine;

public class PlayableBattleParticipant : BattleParticipant 
{
	[SerializeField] private AllyBattleParticipantData m_battleData = null;
	[SerializeField] private WeaponDatabase m_weaponDatabase = null; //TODO: Should they really have the whole database or just a reference to their equipped weapon?

	public AllyBattleParticipantData battleData { get { return m_battleData; }}

	private int weaponDamage = 0;
	private int weaponAccuracy = 0;

	public override void Init()
	{
		m_name = m_battleData.participantName;
		m_maxHP = m_currentHP = m_battleData.maxHP;
		m_maxMP = m_currentMP = m_battleData.maxMP;
		WeaponData equippedWeapon = m_weaponDatabase.GetWeaponData(m_battleData.equippedWeaponIndex);

		if (equippedWeapon != null)
		{
			weaponDamage = equippedWeapon.damage;
		}
		else
		{
			//TODO: special formula for unarmed?
		}

		if (equippedWeapon != null)
		{
			weaponAccuracy = equippedWeapon.accuracy;
		}
		else
		{
			//TODO: special formula for unarmed?
		}
	}

	public override int AttackDamage ()
	{	
		int baseAttack = weaponDamage + m_battleData.strength / 2;
		int damageRoll = Random.Range(baseAttack, 2 * baseAttack);

		SuperLogger.Log(m_name + " damage roll: " + damageRoll);

		return damageRoll;
	}

	public override int Hits ()
	{
		return (1 + (Accuracy()/32)) * m_hitMultiplier;
	}

	public override int Accuracy ()
	{
		return weaponAccuracy + m_battleData.accuracy;
	}

	public override int CritChance ()
	{
		return 0;
	}

	public override int Evasion ()
	{
		return 48 + m_battleData.agility ;// - armorWeight;
	}

	public override int Defense ()
	{
		//TODO: Sum of defense of armor
		return 0;
	}

	public override int MagicDefense ()
	{
		return 0;
	}
}
