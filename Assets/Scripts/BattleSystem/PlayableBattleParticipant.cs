using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayableBattleParticipant : BattleParticipant 
{
	[SerializeField] AllyBattleParticipantData m_battleData;

	public AllyBattleParticipantData battleData { get { return m_battleData; }}

	public override void Init()
	{
		m_name = m_battleData.participantName;
		m_maxHP = m_currentHP = m_battleData.maxHP;
		m_maxMP = m_currentMP = m_battleData.maxMP;
	}

	public override int AttackDamage ()
	{
		int baseAttack = /*TODO: Add weapon value* + */ m_battleData.strength / 2;
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
		return /*Weapon accuracy + */ m_battleData.accuracy;
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
