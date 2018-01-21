using UnityEngine;
using UnityEngine.UI;

public abstract class BattleParticipant : MonoBehaviour {

	[SerializeField] Button m_targetButton = null;

	protected string m_name;
	protected int m_currentHP;
	protected int m_maxHP;
	protected int m_currentMP;
	protected int m_maxMP;
	protected int m_hitMultiplier = 1;

	protected StatusEffectMask m_statusEffect;
	protected ElementTypeMask m_attackElement;
	protected ElementTypeMask m_defenseWeakElement;
	protected ElementTypeMask m_defenseStrongElement;

	public string participantName { get { return m_name; } }
	public int maxHP { get { return m_maxHP; }  }
	public int currentHP { get { return m_currentHP; } set { m_currentHP = value;} }
	public int maxMP { get { return m_maxMP; } }
	public int currentMP { get { return m_currentMP; } set { m_currentMP = value;} }
	public int hitMultiplier { get { return m_hitMultiplier;} set { m_hitMultiplier = value; }}
	public StatusEffectMask statusEffect { get { return m_statusEffect; } set { m_statusEffect = value; } }
	public ElementTypeMask attackElement { get { return m_attackElement; } set { m_attackElement = value; } }
	public ElementTypeMask defenseWeakElement { get { return m_defenseWeakElement; } set { m_defenseWeakElement = value; } }
	public ElementTypeMask defenseStrongElement { get { return m_defenseStrongElement; } set { m_defenseStrongElement = value; } }

	public abstract void Init();
	public abstract int AttackDamage();
	public abstract int Hits();
	public abstract int Accuracy();
	public abstract int CritChance();
	public abstract int Evasion();
	public abstract int Defense();
	public abstract int MagicDefense();

	public bool IsIncapacitated()
	{
		//TODO: It is 2am and I'm sure there is a better way to write this.
		if (statusEffect.ContainsStatus(StatusEffectMask.Dead) 
			|| statusEffect.ContainsStatus(StatusEffectMask.Asleep) 
			|| statusEffect.ContainsStatus(StatusEffectMask.Paralyzed)
			|| statusEffect.ContainsStatus(StatusEffectMask.Petrified))
		{
			return true;
		}

		return false;
	}
}
