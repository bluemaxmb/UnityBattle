using UnityEngine;

public class ArmorDatabase : ScriptableObject
{
	[SerializeField] private ArmorData[] m_armorData = null;

	public ArmorData GetArmorData(int index)
	{
		if (index < 0 || index > m_armorData.Length)
		{
			return null;
		}

		return m_armorData[index];
	}
}
