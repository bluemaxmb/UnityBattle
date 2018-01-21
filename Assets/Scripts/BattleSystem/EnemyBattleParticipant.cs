using UnityEngine;
using UnityEngine.UI;

public class EnemyBattleParticipant : NonPlayableBattleParticipant {

	public enum EnemySizeClass
	{
		Small,
		Large,
		Boss
	}

	[SerializeField] private EnemySizeClass m_enemySize = EnemySizeClass.Small;
	[SerializeField] private EnemyBattleParticipantData[] m_smallEnemyBattleData = null;
	[SerializeField] private EnemyBattleParticipantData[] m_largeEnemyBattleData = null;
	[SerializeField] private EnemyBattleParticipantData[] m_bossEnemyBattleData = null;
	[SerializeField] private Image m_battleImage = null;

	private EnemyBattleParticipantData m_battleData;

	public override void Init()
	{
		//TODO: This is awful and not the eventual design.
		switch (m_enemySize)
		{
		case EnemySizeClass.Small:
			{
				int index = UnityEngine.Random.Range(0,m_smallEnemyBattleData.Length);
				m_battleData = m_smallEnemyBattleData[index];
			}
			break;
		case EnemySizeClass.Large:
			{
				int index = UnityEngine.Random.Range(0,m_largeEnemyBattleData.Length);
				m_battleData = m_largeEnemyBattleData[index];	
			}
			break;
		case EnemySizeClass.Boss:
			{
				m_battleData = m_bossEnemyBattleData[0];
			}
			break;
			default:
			{
                   SuperLogger.LogError("Undefined");
			}
			break;
		}

		m_name = m_battleData.participantName;
		m_maxHP = m_currentHP = m_battleData.maxHP;
		m_maxMP = m_currentMP = m_battleData.maxMP;
		m_battleImage.sprite = m_battleData.battleSprite;

		m_attackElement = m_battleData.attackElement;
		m_defenseWeakElement = m_battleData.defenseWeakElement;
		m_defenseStrongElement = m_battleData.defenseStrongElement;
}

	public override int AttackDamage ()
	{
		//I'm not sure this is correct but I haven't been able to verify This is the comment for the mechanics guide:
		/* Many sources refer to enemy "Strength" and "Agility." However, 
		 * this terminology can be confusing because enemies do not have "Strength" or 
		 * "Agility" in the same way that PCs do. The stats that enemies possess are 
		 * analogous to the derived stats of "Attack" (or "Damage" in-game) and "Evasion."
		 * This is an important distinction, because if their stats were really "Strength"
		 * and "Agility," that would imply dividing by 2 to get Attack and adding 48 to
		 * get Evasion, which is not the case.
		*/

		int baseAttack = m_battleData.attack;
		int damageRoll = Random.Range(baseAttack, 2 * baseAttack);

		SuperLogger.Log(m_name + " damage roll: " + damageRoll);

		return damageRoll;
	}

	public override int Hits ()
	{
		return m_battleData.numHits;
	}

	public override int Accuracy ()
	{
		return m_battleData.accuracy;
	}

	public override int CritChance ()
	{
		return m_battleData.critChance;
	}

	public override int Evasion()
	{
		return m_battleData.evasion;
	}

	public override int Defense ()
	{
		return m_battleData.defense;
	}

	public override int MagicDefense ()
	{
		return m_battleData.magicDefense;
	}

	public bool IsUndead()
	{
		return EnemyTypeMaskMaskExtensions.IsEnemyType(m_battleData.enemyType, EnemyTypeMask.Undead);
	}
}
