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
	public ElementTypeMask attackElement;
	public ElementTypeMask defenseWeakElement;
	public ElementTypeMask defenseStrongElement;
}
