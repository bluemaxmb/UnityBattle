using UnityEngine;

public class EnemyBattleParticipantData : BattleParticipantData 
{
	public Sprite battleSprite;
	public byte attack;
	public byte accuracy;
	public byte numHits;
	public byte critChance;
	public byte defense;
	public byte evasion;
	public byte magicDefense;
	public byte morale;
	public int goldValue;
	public int expValue;

	[BitMask(typeof(StatusEffectMask))] public StatusEffectMask StatusAttack;
	[BitMask(typeof(ElementTypeMask))] public ElementTypeMask StatusAttackElement;
	[BitMask(typeof(ElementTypeMask))] public ElementTypeMask defenseWeakElement;
	[BitMask(typeof(ElementTypeMask))] public ElementTypeMask defenseStrongElement;
	[BitMask(typeof(EnemyTypeMask))] public EnemyTypeMask enemyType;
}
