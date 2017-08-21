using UnityEngine;

public class MagicDatabase : ScriptableObject 
{
	[SerializeField] private MagicSpellData[] m_magicData = null;

	public MagicSpellData GetMagicSpellData(int index)
	{
		if (index < 0 || index > m_magicData.Length)
		{
			return null;
		}

		return m_magicData[index];
	}
}
