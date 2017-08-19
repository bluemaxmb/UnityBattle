using UnityEngine;

public class MagicDatabase : ScriptableObject 
{
	[SerializeField] private MagicSpellData[] m_magicData;

	public MagicSpellData GetMagicSpellData(int index)
	{
		return m_magicData[index];
	}
}
