using UnityEngine;

public class WeaponDatabase : ScriptableObject
{
	[SerializeField] private WeaponData[] m_weaponData = null;

	public WeaponData GetWeaponData(int index)
	{
		if (index < 0 || index > m_weaponData.Length)
		{
			return null;
		}

		return m_weaponData[index];
	}
}
