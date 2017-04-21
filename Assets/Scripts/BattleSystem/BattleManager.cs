using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleManager : MonoBehaviour 
{
	[SerializeField] Button m_btnUndo;
	[SerializeField] Button m_btnFight;
	[SerializeField] Button m_btnMagic;
	[SerializeField] Button m_btnRun;
	[SerializeField] Button m_btnItem;

	[SerializeField] Text[] m_txtPCNames;
	[SerializeField] Text[] m_txtPCHPs;
	[SerializeField] Text[] m_txtPCMPs;

	[SerializeField] Transform[] m_monsterFormation1Placements;
	[SerializeField] Transform[] m_monsterFormation2Placements;
	[SerializeField] Transform[] m_monsterFormation3Placements;
	[SerializeField] Transform[] m_bossPlacement;
	[SerializeField] Transform[] m_heroPlacements;

	[SerializeField] GameObject[] m_monsterPrefabs;
	[SerializeField] GameObject[] m_heroPrefabs;

	private enum BattleState 
	{
		startCombat,
		playerInput,
		monsterDecision,
		resolveRound,
		endCombat
	}		

	private enum TargetState
	{
		None,
		Fight,
		Magic,
		Item
	}

	private enum CombatActionType
	{
		Fight,
		Magic,
		Item,
		Run
	}

	private struct CombatAction
	{
		public BattleParticipant sourceParticipant;
		public BattleParticipant targetParticipant;
		public CombatActionType actionType;
	}

	private List<PlayableBattleParticipant> m_playerPartyList = new List<PlayableBattleParticipant>();
	private List<EnemyBattleParticipant> m_enemyPartyList = new List<EnemyBattleParticipant>();
	private Queue<CombatAction> m_combatActionQueue = new Queue<CombatAction>();
	private BattleState m_currentBattleState = BattleState.startCombat;
	private TargetState m_currentTargetState = TargetState.None;
	private const int m_baseChanceToHit = 168;
	private int m_currentHero;
	private int m_aliveHeros;
	private int m_aliveMonsters;

	#region Initialization
	void Start () 
	{

		m_btnUndo.gameObject.SetActive(false);

		Bind();
		InitializePlayerParty();
		InitializeMonsterParty();

		Debug.Log("Battle Start");
	}

	private void OnDestroy()
	{
		UnBind();
	}

	private void Bind()
	{
		m_btnUndo.onClick.AddListener(OnUndoButtonClicked);
		m_btnFight.onClick.AddListener(OnFightButtonClicked);
		m_btnMagic.onClick.AddListener(OnMagicButtonClicked);
		m_btnRun.onClick.AddListener(OnRunButtonClicked);
		m_btnItem.onClick.AddListener(OnItemButtonClicked);
	}

	private void UnBind()
	{
		m_btnUndo.onClick.RemoveListener(OnUndoButtonClicked);
		m_btnFight.onClick.RemoveListener(OnFightButtonClicked);
		m_btnMagic.onClick.RemoveListener(OnMagicButtonClicked);
		m_btnRun.onClick.RemoveListener(OnRunButtonClicked);
		m_btnItem.onClick.RemoveListener(OnItemButtonClicked);
	}

	private void InitializePlayerParty()
	{
		for (int i=0;i<4;++i)
		{
			GameObject tempObject = Instantiate(m_heroPrefabs[i]);

			PlayableBattleParticipant newParticipant = tempObject.GetComponentInChildren<PlayableBattleParticipant>();
			newParticipant.Init();
			m_playerPartyList.Add(newParticipant);

			AllyTargetButton targetButton = tempObject.GetComponentInChildren<AllyTargetButton>();
			targetButton.onClick.AddListener(() => OnTargetClicked(targetButton));
			targetButton.battleParticipant = newParticipant;

			m_txtPCNames[i].text = newParticipant.participantName;
			m_txtPCHPs[i].text = newParticipant.currentHP.ToString() + "/" + newParticipant.maxHP.ToString();
			m_txtPCMPs[i].text = newParticipant.currentMP.ToString() + "/" + newParticipant.maxMP.ToString();
			tempObject.transform.SetParent(m_heroPlacements[i], false);

			m_aliveHeros++;
		}
	}

	private void InitializeMonsterParty()
	{
		int monsterGroup = UnityEngine.Random.Range(0,2);

		switch (monsterGroup)
		{
			case 0:
			{
				InitializeMonsterGroupZero();
			}
			break;
			case 1:
			{
				InitializeMonsterGroupOne();
			}
			break;
			case 2:
			{
				InitializeMonsterGroupTwo();
			}
			break;
			default:
			{
				Debug.Log("Unexpected result: " + monsterGroup);
			}
			break;
		}			
	}

	//TODO: This is god awful We have three functions doing so much of the same shit and there is a better way to do this.
	private void InitializeMonsterGroupZero()
	{
		int numMonsters = UnityEngine.Random.Range(0,9);

		for (int i=0;i<numMonsters;++i)
		{
			GameObject tempObject = Instantiate(m_monsterPrefabs[0]);

			EnemyBattleParticipant newParticipant = tempObject.GetComponentInChildren<EnemyBattleParticipant>();
			newParticipant.Init();
			m_enemyPartyList.Add(newParticipant);

			EnemyTargetButton targetButton = tempObject.GetComponentInChildren<EnemyTargetButton>();
			targetButton.onClick.AddListener(() => OnTargetClicked(targetButton));
			targetButton.battleParticipant = newParticipant;

			tempObject.transform.SetParent(m_monsterFormation1Placements[i], false);

			m_aliveMonsters++;
		}
	}

	private void InitializeMonsterGroupOne()
	{
		int numMonsters = UnityEngine.Random.Range(0,4);

		for (int i=0;i<numMonsters;++i)
		{
			GameObject tempObject = Instantiate(m_monsterPrefabs[1]);

			EnemyBattleParticipant newParticipant = tempObject.GetComponentInChildren<EnemyBattleParticipant>();
			newParticipant.Init();
			m_enemyPartyList.Add(newParticipant);

			EnemyTargetButton targetButton = tempObject.GetComponentInChildren<EnemyTargetButton>();
			targetButton.onClick.AddListener(() => OnTargetClicked(targetButton));
			targetButton.battleParticipant = newParticipant;

			tempObject.transform.SetParent(m_monsterFormation2Placements[i], false);

			m_aliveMonsters++;
		}
	}

	private void InitializeMonsterGroupTwo()
	{
		int numLargeMonsters = UnityEngine.Random.Range(0,3);
		int numSmallMonsters = UnityEngine.Random.Range(0,5);

		for (int i=0;i<numLargeMonsters;++i)
		{
			GameObject tempObject = Instantiate(m_monsterPrefabs[1]);

			EnemyBattleParticipant newParticipant = tempObject.GetComponentInChildren<EnemyBattleParticipant>();
			newParticipant.Init();
			m_enemyPartyList.Add(newParticipant);

			EnemyTargetButton targetButton = tempObject.GetComponentInChildren<EnemyTargetButton>();
			targetButton.onClick.AddListener(() => OnTargetClicked(targetButton));
			targetButton.battleParticipant = newParticipant;

			tempObject.transform.SetParent(m_monsterFormation3Placements[i], false);

			m_aliveMonsters++;
		}

		for (int i=0;i<numSmallMonsters;++i)
		{
			GameObject tempObject = Instantiate(m_monsterPrefabs[0]);

			EnemyBattleParticipant newParticipant = tempObject.GetComponentInChildren<EnemyBattleParticipant>();
			newParticipant.Init();
			m_enemyPartyList.Add(newParticipant);

			EnemyTargetButton targetButton = tempObject.GetComponentInChildren<EnemyTargetButton>();
			targetButton.onClick.AddListener(() => OnTargetClicked(targetButton));
			targetButton.battleParticipant = newParticipant;

			tempObject.transform.SetParent(m_monsterFormation3Placements[i+2], false);

			m_aliveMonsters++;
		}
	}

	#endregion

	// Update is called once per frame
	void Update () 
	{
		switch (m_currentBattleState)
		{
		case  BattleState.startCombat: 
			{
				m_currentBattleState = BattleState.playerInput;
			}
			break;
		case BattleState.playerInput: 
			{
				if (m_currentHero == 4)
				{
					m_btnUndo.gameObject.SetActive(false);
					m_currentBattleState = BattleState.monsterDecision;
				}
				else if (m_playerPartyList[m_currentHero].currentHP < 1)
				{
					m_currentHero++;
				}
			}
			break;
		case BattleState.monsterDecision: 
			{
				Debug.Log("The Monster is making his mind up!");
				MonsterDecision();
				m_currentBattleState = BattleState.resolveRound;
			}
			break;
		case BattleState.resolveRound: 
			{
				Debug.Log("Let's see how it all plays out...");
				ResolveRound();

				if (m_aliveHeros < 1)
				{
					Debug.Log("You Lose!");
					m_currentBattleState = BattleState.endCombat;
				}
				else if (m_aliveMonsters < 1)
				{
					Debug.Log("You Win!");
					m_currentBattleState = BattleState.endCombat;
				}
				else
				{
					Debug.Log("Another round");
					m_currentHero = 0;
					m_currentBattleState = BattleState.playerInput;
				}
			}
			break;
		case BattleState.endCombat: 
			{
				//Debug.Log("Victory\n");
			}
			break;
		}	
	}

	#region ButtonHandlers
	private void OnUndoButtonClicked()
	{
		if (m_currentBattleState == BattleState.playerInput)
		{
			if (m_currentTargetState != TargetState.None)
			{
				m_currentTargetState = TargetState.None;
			}
			else
			{
				if (m_combatActionQueue.Count > 0)
				{
					m_combatActionQueue.Dequeue();
					m_currentHero--;
				}
			}
		}	
	}

	private void OnFightButtonClicked()
	{
		if (m_currentBattleState == BattleState.playerInput)
		{
			Debug.Log("Select Target! m_currentHero " + m_currentHero);
			m_currentTargetState = TargetState.Fight;

			if (!m_btnUndo.gameObject.activeSelf)
			{
				m_btnUndo.gameObject.SetActive(true);
			}
		}
	}

	private void OnMagicButtonClicked()
	{

	}

	private void OnRunButtonClicked()
	{

	}

	private void OnItemButtonClicked()
	{

	}

	private void OnTargetClicked(Button button)
	{

		switch (m_currentTargetState)	
		{
			case TargetState.Fight:
			{
				CombatAction action = new CombatAction();
				action.sourceParticipant = m_playerPartyList[m_currentHero];

				if (button is EnemyTargetButton)
				{
					action.targetParticipant = (button as EnemyTargetButton).battleParticipant;
				}
				else if (button is AllyTargetButton)
				{
					action.targetParticipant = (button as AllyTargetButton).battleParticipant;
				}
				else
				{
					action.targetParticipant = null;
				}
				action.actionType = CombatActionType.Fight;

				m_combatActionQueue.Enqueue(action);
				m_currentHero++;

				Debug.Log(action.sourceParticipant.participantName + " is targeting  " + action.targetParticipant.participantName);
			}
			break;
			case TargetState.Magic:
			{

			}
			break;
			case TargetState.Item:
			{

			}
			break;
			case TargetState.None:
			default:
			{
				
			}
			break;
		}

		m_currentTargetState = TargetState.None;
	}
	#endregion

	#region Battle Logic
	private void MonsterDecision ()
	{
		for (int i = 0; i < m_enemyPartyList.Count; ++i) 
		{
			CombatAction action = new CombatAction ();
			action.sourceParticipant = m_enemyPartyList [i];

			bool foundTarget = false;

			while (!foundTarget)
			{							
				int index = -1;
				int choiceRoll = UnityEngine.Random.Range (0, 100);

				if (choiceRoll >= 50)
				{
					index = 0;
				}
				else if (choiceRoll >= 25)
				{
					index = 1;
				}
				else if (choiceRoll >= 13) 
				{
					index = 2;
				}
				else
				{
					index = 3;
				}

				if (m_playerPartyList [index].currentHP > 0) 
				{
					action.targetParticipant = m_playerPartyList [index];
					foundTarget = true;
					action.actionType = CombatActionType.Fight;

					Debug.Log(action.sourceParticipant.participantName + " is targeting  " + action.targetParticipant.participantName);

					m_combatActionQueue.Enqueue (action);
				}
			}
		}
	}

	private void ResolveRound ()
	{
		while (m_combatActionQueue.Count != 0) 
		{
			CombatAction action = m_combatActionQueue.Dequeue ();

			//TODO: Should check other debilitating statuses
			if (action.sourceParticipant.currentHP > 0)
			{
				switch (action.actionType)
				{
				case CombatActionType.Fight:
					{						
						if (action.targetParticipant.currentHP < 0)
						{
							Debug.Log (action.sourceParticipant.participantName + " is ineffective!");
						}
						else
						{
							int chanceToHit = (m_baseChanceToHit + action.sourceParticipant.Accuracy ()) - action.targetParticipant.Evasion ();
							int hitRoll = UnityEngine.Random.Range (0, 200);

							/*
								If that
								number is less than or equal to the Chance to Hit, the Hit connects. 0 is an
								automatic hit, and 200 is an automatic miss. */
							if (hitRoll == 200)
							{
								//Miss
								Debug.Log (action.sourceParticipant.participantName + " misses " + action.targetParticipant.participantName);
							} 
							else if (hitRoll == 0) 
							{
								//Auto Critical
								DoDamage (action.sourceParticipant, action.targetParticipant, true);

							} 
							else if (hitRoll <= chanceToHit) 
							{
								DoDamage (action.sourceParticipant, action.targetParticipant, hitRoll <= action.sourceParticipant.CritChance ());
							}
							else 
							{
								//Also miss
								Debug.Log (action.sourceParticipant.participantName + " misses " + action.targetParticipant.participantName);
							} 
						}
					}
					break;
				case CombatActionType.Magic:
					{

					}
					break;
				case CombatActionType.Item:
					{

					}
					break;
				default:
					{

					}
					break;
				}
			}

			UpdateHitPointText();

			if (m_aliveHeros < 1 || m_aliveMonsters < 1) 
			{
				break;
			}
		}
	}

	private void DoDamage(BattleParticipant sourceParticipant, BattleParticipant targetParticipant, bool isCritical)
	{
		int totalDamage = 0;

		//Do Damage
		for (int i = 0; i < sourceParticipant.Hits (); ++i) 
		{
			int attackDamage = sourceParticipant.AttackDamage ();
			int damageThisHit = (attackDamage - targetParticipant.Defense ());

			if (isCritical)
			{
				damageThisHit += attackDamage;
			}


			if (damageThisHit < 0)
			{
				Debug.Log("Absorbed!");
				damageThisHit = 0;
			}

			totalDamage += damageThisHit;
		}

		Debug.Log (sourceParticipant.participantName + " hits " + targetParticipant.participantName + " " + sourceParticipant.Hits () + " times for " + totalDamage + " damage.");

		targetParticipant.currentHP -= totalDamage;

		if (targetParticipant.currentHP <= 0)
		{
			Debug.Log(targetParticipant.participantName + " has died!");
			targetParticipant.currentHP = 0;
			if (targetParticipant is PlayableBattleParticipant) 
			{
				m_aliveHeros--;
			} 
			else if (targetParticipant is EnemyBattleParticipant) 
			{
				m_aliveMonsters--;
				targetParticipant.gameObject.SetActive(false);
			}
		}
	}

	private void UpdateHitPointText()
	{
		for (int i=0;i<4;++i)
		{
			m_txtPCHPs[i].text = m_playerPartyList[i].currentHP.ToString() + "/" + m_playerPartyList[i].maxHP.ToString();
		}
	}
	#endregion
}
