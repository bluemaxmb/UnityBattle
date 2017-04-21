using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class BattleParticipant : MonoBehaviour {

	[SerializeField] Button m_targetButton;

	protected string m_name;
	protected int m_currentHP;
	protected int m_maxHP;
	protected int m_currentMP;
	protected int m_maxMP;
	protected int m_hitMultiplier = 1;


	public string participantName { get { return m_name; } }
	public int maxHP { get { return m_maxHP; }  }
	public int currentHP { get { return m_currentHP; } set { m_currentHP = value;} }
	public int maxMP { get { return m_maxMP; } }
	public int currentMP { get { return m_currentMP; } set { m_currentMP = value;} }
	public int hitMultiplier { get { return m_hitMultiplier;} set { m_hitMultiplier = value; }}

	public abstract void Init();
	public abstract int AttackDamage();
	public abstract int Hits();
	public abstract int Accuracy();
	public abstract int CritChance();
	public abstract int Evasion();
	public abstract int Defense();
	public abstract int MagicDefense();
}
