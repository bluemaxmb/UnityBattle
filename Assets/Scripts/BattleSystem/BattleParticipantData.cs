using UnityEngine;

public class BattleParticipantData : ScriptableObject 
{
	public string participantName;
	public int currentHP;
	public int maxHP;
	public int currentMP;
	public int maxMP;
	public int [] spellIndexArray;
}
