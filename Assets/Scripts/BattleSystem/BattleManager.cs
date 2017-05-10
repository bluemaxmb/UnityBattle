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
	[SerializeField] GridLayoutGroup[] m_heroMagicPanels;

	[SerializeField] GameObject[] m_monsterPrefabs;
	[SerializeField] GameObject[] m_heroPrefabs;
	[SerializeField] GameObject m_spellButtonPrefab;

	[SerializeField] MagicDatabase m_magicDatabase;

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
		Magic_Select,
		Magic_Target,
		Item_Select,
		Item_Target
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
		public MagicSpellData magicSpellData;
	}

	private List<PlayableBattleParticipant> m_playerPartyList = new List<PlayableBattleParticipant>();
	private List<EnemyBattleParticipant> m_enemyPartyList = new List<EnemyBattleParticipant>();
	private Queue<CombatAction> m_combatActionQueue = new Queue<CombatAction>();
	private BattleState m_currentBattleState = BattleState.startCombat;
	private TargetState m_currentTargetState = TargetState.None;
	private MagicSpellData m_spellBeingTargeted;
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

		SuperLogger.Log("Battle Start");
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

			if (newParticipant.battleData.spellIndexArray.Length > 0)
			{
				for (int j=0;j< newParticipant.battleData.spellIndexArray.Length;++j)
				{
					GameObject spellObject = Instantiate(m_spellButtonPrefab);
					spellObject.transform.SetParent(m_heroMagicPanels[i].transform, false);

					MagicSpellButton magicSpellButton = spellObject.GetComponentInChildren<MagicSpellButton>();
					magicSpellButton.onClick.AddListener(() => OnMagicSpellButtonClicked(magicSpellButton));
					magicSpellButton.SetMagicSpellData(m_magicDatabase.GetMagicSpellData(newParticipant.battleData.spellIndexArray[j]));
				}							
			}

			m_heroMagicPanels[i].gameObject.SetActive(false);

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

		SuperLogger.Log("monsterGroup: " + monsterGroup);

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
				SuperLogger.Log("Unexpected result: " + monsterGroup);
			}
			break;
		}			
	}

	//TODO: This is god awful We have three functions doing so much of the same shit and there is a better way to do this.
	private void InitializeMonsterGroupZero()
	{
		int numMonsters = UnityEngine.Random.Range(1,9);

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
		int numMonsters = UnityEngine.Random.Range(1,4);

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

		if (m_aliveMonsters < 1)
		{
			InitializeMonsterGroupOne();
		}
	}

	private void InitializeMonsterGroupTwo()
	{
		int numLargeMonsters = UnityEngine.Random.Range(1,3);
		int numSmallMonsters = UnityEngine.Random.Range(1,5);

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

		if (m_aliveMonsters < 1)
		{
			InitializeMonsterGroupTwo();
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
				SuperLogger.Log("The Monster is making his mind up!");
				MonsterDecision();
				m_currentBattleState = BattleState.resolveRound;
			}
			break;
		case BattleState.resolveRound: 
			{
				SuperLogger.Log("Let's see how it all plays out...");
				ResolveRound();

				if (m_aliveHeros < 1)
				{
					SuperLogger.Log("You Lose!");
					m_currentBattleState = BattleState.endCombat;
				}
				else if (m_aliveMonsters < 1)
				{
					SuperLogger.Log("You Win!");
					m_currentBattleState = BattleState.endCombat;
				}
				else
				{
					SuperLogger.Log("Another round");
					m_currentHero = 0;
					m_currentBattleState = BattleState.playerInput;
				}
			}
			break;
		case BattleState.endCombat: 
			{
				//SuperLogger.Log("Victory\n");
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
				switch (m_currentTargetState)
				{
				case TargetState.Fight:
					{
						m_currentTargetState = TargetState.None;
					}
					break;
				case TargetState.Magic_Select:
					{
						m_currentTargetState = TargetState.None;
						m_heroMagicPanels[m_currentHero].gameObject.SetActive(false);
					}
					break;
				case TargetState.Magic_Target:
					{
						m_currentTargetState = TargetState.Magic_Select;
						m_heroMagicPanels[m_currentHero].gameObject.SetActive(true);
						m_spellBeingTargeted = null;
					}
					break;
				case TargetState.Item_Select:
					{
						m_currentTargetState = TargetState.None;
					}
					break;
				case TargetState.Item_Target:
					{
						m_currentTargetState = TargetState.Item_Select;
					}
					break;
				}
			}
			else
			{
				if (m_combatActionQueue.Count > 0)
				{
					m_combatActionQueue.Dequeue();
					m_currentHero--;
					m_spellBeingTargeted = null;
				}
			}
		}	
	}

	private void OnFightButtonClicked()
	{
		if (m_currentBattleState == BattleState.playerInput)
		{
			SuperLogger.Log("Select Target! m_currentHero " + m_currentHero);
			m_currentTargetState = TargetState.Fight;

			if (!m_btnUndo.gameObject.activeSelf)
			{
				m_btnUndo.gameObject.SetActive(true);
			}
		}
	}

	private void OnMagicButtonClicked()
	{
		if (m_currentBattleState == BattleState.playerInput)
		{
			SuperLogger.Log("Select Spell! m_currentHero " + m_currentHero);

			if (m_playerPartyList[m_currentHero].battleData.spellIndexArray.Length > 0)
			{
				m_currentTargetState = TargetState.Magic_Select;

				if  (!m_heroMagicPanels[m_currentHero].gameObject.activeSelf)
				{
					m_heroMagicPanels[m_currentHero].gameObject.SetActive(true);
				}

				if (!m_btnUndo.gameObject.activeSelf)
				{
					m_btnUndo.gameObject.SetActive(true);
				}
			}
			else
			{
				//TODO: Needs in game UI
				SuperLogger.LogWarning("No spells!");
			}
		}
	}

	private void OnRunButtonClicked()
	{

	}

	private void OnItemButtonClicked()
	{

	}

	private void OnTargetClicked(Button button)
	{
		SuperLogger.Log("button " + button + " target state" + m_currentTargetState + " battle state " + m_currentBattleState);

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

				SuperLogger.Log(action.sourceParticipant.participantName + " is targeting  " + action.targetParticipant.participantName);
			}
			break;
		case TargetState.Magic_Target:
			{
				CombatAction action = new CombatAction();
				action.sourceParticipant = m_playerPartyList[m_currentHero];

				//TODO: Pull spell targeting type here from m_spellBeingTargeted

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

				action.magicSpellData = m_spellBeingTargeted;
				action.actionType = CombatActionType.Magic;

				m_combatActionQueue.Enqueue(action);
				m_currentHero++;
				m_spellBeingTargeted = null;

				SuperLogger.Log(action.sourceParticipant.participantName + " is targeting  " + action.targetParticipant.participantName + " with spell " + action.magicSpellData.spellName);
			}
			break;
		case TargetState.Item_Target:
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

	private void OnMagicSpellButtonClicked(Button button)
	{
		if (m_currentTargetState == TargetState.Magic_Select)
		{
			MagicSpellData magicSpellData = (button as MagicSpellButton).magicSpellData;

			if (m_playerPartyList[m_currentHero].currentMP >= magicSpellData.mpCost)
			{
				SuperLogger.Log("Select Target! m_currentHero " + m_currentHero);
				m_currentTargetState = TargetState.Magic_Target;
				m_spellBeingTargeted = magicSpellData;

				m_heroMagicPanels[m_currentHero].gameObject.SetActive(false);
			}
			else
			{
				//TODO: Needs in game UI
				SuperLogger.LogWarning("Insufficient MP!");
			}
		}
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

					SuperLogger.Log(action.sourceParticipant.participantName + " is targeting  " + action.targetParticipant.participantName);

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
						DoMeleeHitRoll(action.sourceParticipant, action.targetParticipant);
					}
					break;
				case CombatActionType.Magic:
					{
						DetermineSpellEffect(action.sourceParticipant, action.targetParticipant, action.magicSpellData);
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
			UpdateMagicPointText();

			if (m_aliveHeros < 1 || m_aliveMonsters < 1) 
			{
				break;
			}
		}
	}

	private void DoMeleeHitRoll(BattleParticipant sourceParticipant, BattleParticipant targetParticipant)
	{
		if (targetParticipant.currentHP < 0)
		{
			SuperLogger.Log (sourceParticipant.participantName + " is ineffective!");
		}
		else
		{
			int chanceToHit = (m_baseChanceToHit + sourceParticipant.Accuracy ()) - targetParticipant.Evasion ();
			int hitRoll = UnityEngine.Random.Range (0, 200);

			/*
								If that
								number is less than or equal to the Chance to Hit, the Hit connects. 0 is an
								automatic hit, and 200 is an automatic miss. */
			if (hitRoll == 200)
			{
				//Miss
				SuperLogger.Log (sourceParticipant.participantName + " misses " + targetParticipant.participantName);
			} 
			else if (hitRoll == 0) 
			{
				//Auto Critical
				DoMeleeDamage (sourceParticipant, targetParticipant, true);

			} 
			else if (hitRoll <= chanceToHit) 
			{
				DoMeleeDamage (sourceParticipant, targetParticipant, hitRoll <= sourceParticipant.CritChance ());
			}
			else 
			{
				//Also miss
				SuperLogger.Log (sourceParticipant.participantName + " misses " + targetParticipant.participantName);
			} 
		}
	}

	private void DoMeleeDamage(BattleParticipant sourceParticipant, BattleParticipant targetParticipant, bool isCritical)
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
				SuperLogger.Log("Absorbed!");
				damageThisHit = 0;
			}

			totalDamage += damageThisHit;
		}

		SuperLogger.Log (sourceParticipant.participantName + " hits " + targetParticipant.participantName + " " + sourceParticipant.Hits () + " times for " + totalDamage + " damage.");

		targetParticipant.currentHP -= totalDamage;

		if (targetParticipant.currentHP <= 0)
		{
			SuperLogger.Log(targetParticipant.participantName + " has died!");
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

	private void DoMagicHitRoll(BattleParticipant sourceParticipant, BattleParticipant targetParticipant, MagicSpellData magicSpellData)
	{
		if (targetParticipant.currentHP < 0)
		{
			SuperLogger.Log (sourceParticipant.participantName + " is ineffective!");
		}
		else
		{
			/* TODO:
			 	1) Base Chance to Hit = 148
				--If the target is Resistant to the spell's element, set BC to 0
				--If the target is Weak to the spell's element, add +40 to BC

				NOTE: If a target is both Resistant and Weak, the base chance is set to 0, but
				40 is still added, resulting in BC = 40.
			*/
			int chanceToHit = (m_baseChanceToHit + magicSpellData.accuracy) - targetParticipant.MagicDefense();
			int hitRoll = UnityEngine.Random.Range (0, 200);

			/*
								If that
								number is less than or equal to the Chance to Hit, the Hit connects. 0 is an
								automatic hit, and 200 is an automatic miss. */
			if (hitRoll == 200)
			{
				//Resisted
				SuperLogger.Log (targetParticipant.participantName + " resisted " + sourceParticipant.participantName + "'s spell");
				DoMagicEffect(sourceParticipant, targetParticipant, magicSpellData, true);
			} 
			else if (hitRoll == 0) 
			{
				//Auto Unresisted
				DoMagicEffect(sourceParticipant, targetParticipant, magicSpellData, false);

			} 
			else if (hitRoll <= chanceToHit) 
			{
				DoMagicEffect(sourceParticipant, targetParticipant, magicSpellData, false);
			}
			else 
			{
				//Also resisted
				SuperLogger.Log (targetParticipant.participantName + " resisted " + sourceParticipant.participantName + "'s spell");
				DoMagicEffect(sourceParticipant, targetParticipant, magicSpellData, true);
			} 
		}
	}

	private void DoMagicEffect (BattleParticipant sourceParticipant, BattleParticipant targetParticipant, MagicSpellData magicSpellData, bool wasResisted)
	{
		switch (magicSpellData.spellEffect) 
		{
		case SpellEffect.Damage:
			{
				// TODO:
				// --If target is resistant to spell element, divide effectivity by 2
				// --If the target is weak to spell element, multiply effectivity by 1.5

				int spellDamage = 0;
				if (wasResisted)
				{
					spellDamage = UnityEngine.Random.Range (magicSpellData.effectiveness, 2 * magicSpellData.effectiveness);
				}
				else
				{
					spellDamage = 2 * (UnityEngine.Random.Range (magicSpellData.effectiveness, 2 * magicSpellData.effectiveness));
				}

				SuperLogger.Log (sourceParticipant.participantName + " casts " +  magicSpellData.spellName + " on " + targetParticipant.participantName + " for " + spellDamage + " damage.");

				targetParticipant.currentHP -= spellDamage;

				if (targetParticipant.currentHP <= 0)
				{
					SuperLogger.Log(targetParticipant.participantName + " has died!");
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
			break;
		case SpellEffect.Heal:
		default:
			{
				SuperLogger.Log ("Should never happen.");
			}
			break;
		}
	}

	private void DetermineSpellEffect (BattleParticipant sourceParticipant, BattleParticipant targetParticipant, MagicSpellData magicSpellData)
	{
		if (magicSpellData.spellEffect == SpellEffect.Heal)
		{
			if (targetParticipant.currentHP > 0) 
			{
				int healAmount = UnityEngine.Random.Range (magicSpellData.effectiveness, 2 * magicSpellData.effectiveness);
				targetParticipant.currentHP += healAmount;

				if (targetParticipant.currentHP > targetParticipant.maxHP) 
				{
					targetParticipant.currentHP = targetParticipant.maxHP;
				}

				SuperLogger.Log(sourceParticipant.participantName + " heals " + targetParticipant.participantName + " for " + healAmount);
			} 
			else 
			{
				SuperLogger.Log (sourceParticipant.participantName + "'s spell is ineffective!");
			}
		}
		else
		{
			DoMagicHitRoll (sourceParticipant, targetParticipant, magicSpellData);
		}

		sourceParticipant.currentMP -= magicSpellData.mpCost;
	}

	private void UpdateHitPointText()
	{
		for (int i=0;i<4;++i)
		{
			m_txtPCHPs[i].text = m_playerPartyList[i].currentHP.ToString() + "/" + m_playerPartyList[i].maxHP.ToString();
		}
	}

	private void UpdateMagicPointText()
	{
		for (int i=0;i<4;++i)
		{
			m_txtPCMPs[i].text = m_playerPartyList[i].currentMP.ToString() + "/" + m_playerPartyList[i].maxMP.ToString();
		}
	}
	#endregion
}
