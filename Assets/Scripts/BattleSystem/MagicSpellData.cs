using UnityEngine;

public class MagicSpellData : ScriptableObject 
{
	public string spellName; //TODO: replace with loc id
	public MagicType magicType;
	public ElementTypeMask element;
	public SpellEffect spellEffect;
	public TargetType targetType;
	public byte effectiveness;
	public byte accuracy;
	public int mpCost;
}
