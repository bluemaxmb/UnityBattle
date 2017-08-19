using UnityEngine;

public class WeaponData : ScriptableObject
{
	public Sprite weaponSprite;
	public Color weaponColor;
	public SpecialEffect specialEffect;
	public Element element;
	public WeaponType weaponType;
	public string weaponName; //TODO: replace with loc id
	public byte damage;
	public byte accuracy;
	public byte critChance;
	public int magicSpellIndex;

}