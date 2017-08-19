using UnityEngine;

public class WeaponDatabase : ScriptableObject
{
	[SerializeField] private WeaponData[] m_weaponData;

	public WeaponData GetWeaponData(int index)
	{
		return m_weaponData[index];
	}
}
