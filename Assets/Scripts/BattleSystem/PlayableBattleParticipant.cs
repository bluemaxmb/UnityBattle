using UnityEngine;

public class PlayableBattleParticipant : BattleParticipant
{
	[SerializeField] private AllyBattleParticipantData m_battleData = null;
	[SerializeField] private WeaponDatabase m_weaponDatabase = null; //TODO: Should they really have the whole database or just a reference to their equipped weapon?
	[SerializeField] private ArmorDatabase m_armorDatabase = null; //TODO: Should they really have the whole database or just a reference to their equipped armor?

	public AllyBattleParticipantData battleData { get { return m_battleData; } }

	private int m_weaponDamage = 0;
	private int m_weaponAccuracy = 0;
	private int m_critChance = 0;
	private int m_armorWeight = 0;
	private int m_totalDefense = 0;

	public override void Init()
	{
		m_name = m_battleData.participantName;
		m_maxHP = m_currentHP = m_battleData.maxHP;
		m_maxMP = m_currentMP = m_battleData.maxMP;
		InitWeapon();
		InitArmor();
	}

	private void InitWeapon()
	{
		WeaponData equippedWeapon = m_weaponDatabase.GetWeaponData(m_battleData.equippedWeaponIndex);

		m_weaponDamage = ((equippedWeapon != null) ? equippedWeapon.damage : 0); //should there be a different formula for unarmed?
		m_weaponAccuracy = ((equippedWeapon != null) ? equippedWeapon.accuracy : 0);
		m_critChance = ((equippedWeapon != null) ? equippedWeapon.critChance : 0);
	}

	private void InitArmor()
	{
		ArmorData equippedArmor = m_armorDatabase.GetArmorData(m_battleData.equippedArmorIndex);
		ArmorData equippedShield = m_armorDatabase.GetArmorData(m_battleData.equippedShieldIndex);
		ArmorData equippedHelmet = m_armorDatabase.GetArmorData(m_battleData.equippedHelmetIndex);
		ArmorData equippedGloves = m_armorDatabase.GetArmorData(m_battleData.equippedGloveIndex);

		m_totalDefense = ((equippedArmor != null) ? equippedArmor.defense : 0) +
			((equippedShield != null) ? equippedShield.defense : 0) +
			((equippedHelmet != null) ? equippedHelmet.defense : 0) +
			((equippedGloves != null) ? equippedGloves.defense : 0);

		m_armorWeight = ((equippedArmor != null) ? equippedArmor.weight : 0) +
			((equippedShield != null) ? equippedShield.weight : 0) +
			((equippedHelmet != null) ? equippedHelmet.weight : 0) +
			((equippedGloves != null) ? equippedGloves.weight : 0);
	}

	public override int AttackDamage()
	{
		int baseAttack = m_weaponDamage + m_battleData.strength / 2;
		int damageRoll = Random.Range(baseAttack, 2 * baseAttack);

		SuperLogger.Log(m_name + " damage roll: " + damageRoll);

		return damageRoll;
	}

	public override int Hits()
	{
		return (1 + (Accuracy() / 32)) * m_hitMultiplier;
	}

	public override int Accuracy()
	{
		return m_weaponAccuracy + m_battleData.accuracy;
	}

	public override int CritChance()
	{
		return m_critChance;
	}

	public override int Evasion()
	{
		return 48 + m_battleData.agility - m_armorWeight;
	}

	public override int Defense()
	{
		return m_armorWeight;
	}

	public override int MagicDefense()
	{
		return 0;
	}
}
