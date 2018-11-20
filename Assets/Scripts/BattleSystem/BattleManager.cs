﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleManager : MonoBehaviour
{
	[SerializeField] Button m_btnUndo = null;
	[SerializeField] Button m_btnFight = null;
	[SerializeField] Button m_btnMagic = null;
	[SerializeField] Button m_btnRun = null;
	[SerializeField] Button m_btnItem = null;

	[SerializeField] Text[] m_txtPCNames = null;
	[SerializeField] Text[] m_txtPCHPs = null;
	[SerializeField] Text[] m_txtPCMPs = null;

	[SerializeField] Transform[] m_monsterFormation1Placements = null;
	[SerializeField] Transform[] m_monsterFormation2Placements = null;
	[SerializeField] Transform[] m_monsterFormation3Placements = null;
	[SerializeField] Transform[] m_bossPlacement = null;
	[SerializeField] Transform[] m_heroPlacements = null;
	[SerializeField] GridLayoutGroup[] m_heroMagicPanels = null;

	[SerializeField] GameObject[] m_monsterPrefabs = null;
	[SerializeField] GameObject[] m_heroPrefabs = null;
	[SerializeField] GameObject m_spellButtonPrefab = null;

	[SerializeField] MagicDatabase m_magicDatabase = null;

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
		public List<BattleParticipant> targetParticipant;
		public CombatActionType actionType;
		public MagicSpellData magicSpellData;
	}

	private List<BattleParticipant> m_playerPartyList = new List<BattleParticipant>();
	private List<BattleParticipant> m_enemyPartyList = new List<BattleParticipant>();
	private Queue<CombatAction> m_combatActionQueue = new Queue<CombatAction>();
	private BattleState m_currentBattleState = BattleState.startCombat;
	private TargetState m_currentTargetState = TargetState.None;
	private MagicSpellData m_spellBeingTargeted;
	private const int kBaseChanceToHitMelee = 168;
	private const int kBaseChanceToHitMagic = 148;
	private int m_currentHero;
	private int m_aliveHeros;
	private int m_aliveMonsters;

	#region Initialization
	void Start()
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
		for (int i = 0; i < 4; ++i)
		{
			GameObject tempObject = Instantiate(m_heroPrefabs[i]);

			PlayableBattleParticipant newParticipant = tempObject.GetComponentInChildren<PlayableBattleParticipant>();
			newParticipant.Init();
			m_playerPartyList.Add(newParticipant);

			if (newParticipant.battleData.spellIndexArray.Length > 0)
			{
				for (int j = 0; j < newParticipant.battleData.spellIndexArray.Length; ++j)
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
		int monsterGroup = Random.Range(0,3);

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
		int numMonsters = Random.Range(1,10);

		for (int i = 0; i < numMonsters; ++i)
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
		int numMonsters = Random.Range(1,5);

		for (int i = 0; i < numMonsters; ++i)
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
		int numLargeMonsters = Random.Range(1,3);
		int numSmallMonsters = Random.Range(1,7);

		for (int i = 0; i < numLargeMonsters; ++i)
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

		for (int j = 0; j < numSmallMonsters; ++j)
		{
			GameObject tempObject = Instantiate(m_monsterPrefabs[0]);

			EnemyBattleParticipant newParticipant = tempObject.GetComponentInChildren<EnemyBattleParticipant>();
			newParticipant.Init();
			m_enemyPartyList.Add(newParticipant);

			EnemyTargetButton targetButton = tempObject.GetComponentInChildren<EnemyTargetButton>();
			targetButton.onClick.AddListener(() => OnTargetClicked(targetButton));
			targetButton.battleParticipant = newParticipant;

			tempObject.transform.SetParent(m_monsterFormation3Placements[j + 2], false);

			m_aliveMonsters++;
		}

		if (m_aliveMonsters < 1)
		{
			InitializeMonsterGroupTwo();
		}
	}

	#endregion

	// Update is called once per frame
	void Update()
	{
		switch (m_currentBattleState)
		{
			case BattleState.startCombat:
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
					SuperLogger.Log("Battle Over\n");
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

			PlayableBattleParticipant participant = (PlayableBattleParticipant)m_playerPartyList[m_currentHero];

			if (participant.battleData.spellIndexArray.Length > 0)
			{
				m_currentTargetState = TargetState.Magic_Select;

				if (!m_heroMagicPanels[m_currentHero].gameObject.activeSelf)
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
						action.targetParticipant = new List<BattleParticipant>();
						action.targetParticipant.Add((button as EnemyTargetButton).battleParticipant);
					}
					else if (button is AllyTargetButton)
					{
						action.targetParticipant = new List<BattleParticipant>();
						action.targetParticipant.Add((button as AllyTargetButton).battleParticipant);
					}
					else
					{
						action.targetParticipant = null;
					}

					action.actionType = CombatActionType.Fight;

					m_combatActionQueue.Enqueue(action);
					m_currentHero++;

					SuperLogger.Log(action.sourceParticipant.participantName + " is targeting  " + action.targetParticipant[0].participantName);
				}
				break;
			case TargetState.Magic_Target:
				{
					CombatAction action = new CombatAction();
					action.sourceParticipant = m_playerPartyList[m_currentHero];
					action.magicSpellData = m_spellBeingTargeted;
					action.actionType = CombatActionType.Magic;

					if (m_spellBeingTargeted.targetType == TargetType.SingleEnemy)
					{
						if (button is EnemyTargetButton)
						{
							action.targetParticipant = new List<BattleParticipant>();
							action.targetParticipant.Add((button as EnemyTargetButton).battleParticipant);
						}
						else
						{
							return;
						}
					}
					else if (m_spellBeingTargeted.targetType == TargetType.SingleAlly)
					{
						if (button is AllyTargetButton)
						{
							action.targetParticipant = new List<BattleParticipant>();
							action.targetParticipant.Add((button as AllyTargetButton).battleParticipant);
						}
						else
						{
							return;
						}
					}

					m_combatActionQueue.Enqueue(action);
					m_currentHero++;
					m_spellBeingTargeted = null;

					SuperLogger.Log(action.sourceParticipant.participantName + " is targeting  " + action.targetParticipant[0].participantName + " with spell " + action.magicSpellData.spellName);
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
				m_spellBeingTargeted = magicSpellData;

				switch (magicSpellData.targetType)
				{
					case TargetType.AllAllies:
					case TargetType.AllEnemies:
					case TargetType.Caster:
						{
							CombatAction action = new CombatAction();
							action.sourceParticipant = m_playerPartyList[m_currentHero];
							action.magicSpellData = m_spellBeingTargeted;
							action.actionType = CombatActionType.Magic;

							//Add targets
							if (magicSpellData.targetType == TargetType.AllAllies)
							{
								action.targetParticipant = m_playerPartyList;
							}
							else if (magicSpellData.targetType == TargetType.AllEnemies)
							{
								action.targetParticipant = m_enemyPartyList;
							}
							else
							{
								action.targetParticipant = new List<BattleParticipant>();
								action.targetParticipant.Add(m_playerPartyList[m_currentHero]);
							}

							m_combatActionQueue.Enqueue(action);
							m_heroMagicPanels[m_currentHero].gameObject.SetActive(false);
							m_currentHero++;
							m_spellBeingTargeted = null;
						}
						break;
					case TargetType.SingleAlly:
					case TargetType.SingleEnemy:
					default:
						{
							SuperLogger.Log("Select Target! m_currentHero " + m_currentHero);
							m_currentTargetState = TargetState.Magic_Target;
							m_heroMagicPanels[m_currentHero].gameObject.SetActive(false);
						}
						break;
				}

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
	private void MonsterDecision()
	{		
		for (int i = 0; i < m_enemyPartyList.Count; ++i)
		{
			CombatAction action = new CombatAction ();
			action.sourceParticipant = m_enemyPartyList[i];

			if (action.sourceParticipant.currentHP <= 0 || action.sourceParticipant.statusEffect.ContainsStatus(StatusEffectMask.Asleep))
			{
				SuperLogger.Log(action.sourceParticipant.participantName + " is incapacitated.");
				continue;
			}

			bool foundTarget = false;

			while (!foundTarget)
			{
				int index = -1;
				int choiceRoll = Random.Range (0, 101);

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

				if (m_playerPartyList[index].currentHP > 0)
				{
					action.targetParticipant = new List<BattleParticipant>();
					action.targetParticipant.Add( m_playerPartyList[index]);
					foundTarget = true;
					action.actionType = CombatActionType.Fight;

					SuperLogger.Log(action.sourceParticipant.participantName + " is targeting  " + action.targetParticipant[0].participantName);

					m_combatActionQueue.Enqueue(action);
				}
			}
		}
	}

	private void ResolveRound()
	{
		while (m_combatActionQueue.Count != 0)
		{
			CombatAction action = m_combatActionQueue.Dequeue ();

			//TODO: Should check other debilitating statuses
			if (action.sourceParticipant.currentHP > 0 && !action.sourceParticipant.IsIncapacitated())				
			{
				switch (action.actionType)
				{
					case CombatActionType.Fight:
						{
							DoMeleeHitRoll(action.sourceParticipant, action.targetParticipant[0]);
						}
						break;
					case CombatActionType.Magic:
						{
							for (int i = 0; i < action.targetParticipant.Count; ++i)
							{
								DoMagicHitRoll(action.sourceParticipant, action.targetParticipant[i], action.magicSpellData);
							}

							action.sourceParticipant.currentMP -= action.magicSpellData.mpCost;
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
			else
			{
				SuperLogger.Log(action.sourceParticipant.participantName + " is incapacitated");
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
			SuperLogger.Log(sourceParticipant.participantName + " is ineffective!");
		}
		else
		{
			int attackerBlindPenalty =  sourceParticipant.statusEffect.ContainsStatus(StatusEffectMask.Blind) ? 40 : 0;
			int targetBlindBonus = targetParticipant.statusEffect.ContainsStatus(StatusEffectMask.Blind) ? 40 : 0;
			int targetWeakToAttackElementBonus = targetParticipant.defenseWeakElement.ContainsElement(sourceParticipant.attackElement) ? 40 : 0;

			int chanceToHit = (kBaseChanceToHitMelee + sourceParticipant.Accuracy () - attackerBlindPenalty + targetBlindBonus + targetWeakToAttackElementBonus) - targetParticipant.Evasion ();
			int hitRoll = Random.Range (0, 201);

			 // If that number is less than or equal to the Chance to Hit, the Hit connects. 0 is an automatic hit, and 200 is an automatic miss. 

			if (hitRoll == 200)
			{
				//Miss
				SuperLogger.Log(sourceParticipant.participantName + " misses " + targetParticipant.participantName);
			}
			else if (hitRoll == 0)
			{
				//Auto Critical
				DoMeleeDamage(sourceParticipant, targetParticipant, true);

			}
			else if (hitRoll <= chanceToHit)
			{
				DoMeleeDamage(sourceParticipant, targetParticipant, hitRoll <= sourceParticipant.CritChance());
			}
			else
			{
				//Also miss
				SuperLogger.Log(sourceParticipant.participantName + " misses " + targetParticipant.participantName);
			}
		}
	}

	private void DoMeleeDamage(BattleParticipant sourceParticipant, BattleParticipant targetParticipant, bool isCritical)
	{
		int totalDamage = 0;
		int elementalBonusDamage = targetParticipant.defenseWeakElement.ContainsElement(sourceParticipant.attackElement) ? 4 : 0;
		bool isAsleep = targetParticipant.statusEffect.ContainsStatus(StatusEffectMask.Asleep);
		bool isParalyzed = targetParticipant.statusEffect.ContainsStatus(StatusEffectMask.Paralyzed);
		bool isDebilitated = isAsleep || isParalyzed;

		//Do Damage
		for (int i = 0; i < sourceParticipant.Hits(); ++i)
		{
			int attackDamage = sourceParticipant.AttackDamage () + elementalBonusDamage;
			attackDamage = isDebilitated ? (int)((5.0f / 4.0f) * attackDamage) : attackDamage;

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

		SuperLogger.Log(sourceParticipant.participantName + " hits " + targetParticipant.participantName + " " + sourceParticipant.Hits() + " times for " + totalDamage + " damage.");

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
			SuperLogger.Log(sourceParticipant.participantName + " is ineffective!");
		}
		else
		{
			/*
			 	1) Base Chance to Hit = 148
				--If the target is Resistant to the spell's element, set BC to 0
				--If the target is Weak to the spell's element, add +40 to BC

				NOTE: If a target is both Resistant and Weak, the base chance is set to 0, but
				40 is still added, resulting in BC = 40.
			*/

			int elementWeaknessBonus = 0;
			int chanceToHit = 0;

			int hitRoll = 0;

			//Only calculate this stuff for negative spells, ie not buffs or healing.
			if (magicSpellData.spellEffect == SpellEffect.Damage || magicSpellData.spellEffect == SpellEffect.StatusAilment)
			{
				hitRoll = Random.Range(0, 201);			
				elementWeaknessBonus = targetParticipant.defenseWeakElement.ContainsElement(magicSpellData.element) ? 40 : 0;
				chanceToHit = targetParticipant.defenseStrongElement.ContainsElement(magicSpellData.element) ? 0 : (kBaseChanceToHitMagic + magicSpellData.accuracy) - targetParticipant.MagicDefense();
				chanceToHit += elementWeaknessBonus;

				SuperLogger.Log(sourceParticipant.participantName + " rolled a " + hitRoll + " on their spell. Chance to hit is: " + chanceToHit);
			}

			/*
				If that number is less than or equal to the Chance to Hit, the Hit connects. 0 is an automatic hit, and 200 is an automatic miss. 
		
				Exceptions
				*Positive Effects (Effect Routines 07, 08, 09, 0A, OB, OC, OD, OF, 10) automatically hit.
				* If an enemy is weak to XFER's element, set BC to 188 rather than adding +40. This is immaterial however as XFER has no element.
				* 300HP Threshold Spells (STUN, BLND, and XXXX) always hit if the target is not resistant and its current HP is equal to or less than 300, and always miss otherwise.			
			*/
			if (hitRoll == 200)
			{
				//Resisted
				if (magicSpellData.spellEffect == SpellEffect.Damage)
				{
					SuperLogger.Log(targetParticipant.participantName + " resisted " + sourceParticipant.participantName + "'s spell");
					DoMagicEffect(sourceParticipant, targetParticipant, magicSpellData, true);
				}
				else if (magicSpellData.spellEffect == SpellEffect.StatusAilment) // Missed or didn't effect
				{
					SuperLogger.Log(targetParticipant.participantName + " was not inflicted with " + sourceParticipant.participantName + "'s spell");
				}
				else
				{
					// Should anything happen here?
				}
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
				SuperLogger.Log(targetParticipant.participantName + " resisted " + sourceParticipant.participantName + "'s spell");
				DoMagicEffect(sourceParticipant, targetParticipant, magicSpellData, true);
			}
		}
	}

	private void DoMagicEffect(BattleParticipant sourceParticipant, BattleParticipant targetParticipant, MagicSpellData magicSpellData, bool wasResisted)
	{
		switch (magicSpellData.spellEffect)
		{
			case SpellEffect.Damage:
				{
					DoMagicDamage(sourceParticipant, targetParticipant, magicSpellData, wasResisted, false);
				}
				break;
			case SpellEffect.UndeadDamage:
				{
					DoMagicDamage(sourceParticipant, targetParticipant, magicSpellData, wasResisted, true);
				}
				break;
			case SpellEffect.StatusAilment:
				{
					StatusEffectMask statusToInflict = (StatusEffectMask)magicSpellData.effectiveness;
					if (!targetParticipant.statusEffect.ContainsStatus(statusToInflict))
					{
						targetParticipant.statusEffect |= statusToInflict;
						SuperLogger.Log(sourceParticipant.participantName + " casts " + magicSpellData.spellName + " on " + targetParticipant.participantName + " and inflicts status " + (StatusEffectMask)statusToInflict);
					}
				}
				break;
			case SpellEffect.HitDown:
				{
				}
				break;
			case SpellEffect.MoraleDown:
				{
				}
				break;
			case SpellEffect.Heal:
				{
					DoMagicHeal(sourceParticipant, targetParticipant, magicSpellData);
				}
				break;
			case SpellEffect.RestoreStatus:
				{
					StatusEffectMask statusToUndo = (StatusEffectMask)magicSpellData.effectiveness;
					if (targetParticipant.statusEffect.ContainsStatus(statusToUndo))
					{
						targetParticipant.statusEffect &= ~statusToUndo;
						SuperLogger.Log(sourceParticipant.participantName + " casts " + magicSpellData.spellName + " on " + targetParticipant.participantName + " and undoes status " + (StatusEffectMask)statusToUndo);
					}
				}
				break;
			case SpellEffect.DefenseUp:
				{
				}
				break;
			case SpellEffect.ResistElement:
				{
				}
				break;
			case SpellEffect.AttackUp:
				{
				}
				break;
			case SpellEffect.HitMultiplierUp:
				{
				}
				break;
			case SpellEffect.AttackAndAccuracyUp:
				{
				}
				break;
			case SpellEffect.EvasionDown:
				{
				}
				break;
			case SpellEffect.FullHeal:
				{
					targetParticipant.currentHP = targetParticipant.maxHP;
				}
				break;
			case SpellEffect.EvasionUp:
				{
				}
				break;
			case SpellEffect.RemoveResistance:
				{
				}
				break;
			case SpellEffect.ThreeHundredHPStatus:
				{
				}
				break;
			default:
				{
					SuperLogger.Log("Should never happen.");
				}
				break;
		}
	}

	private void DoMagicDamage(BattleParticipant sourceParticipant, BattleParticipant targetParticipant, MagicSpellData magicSpellData, bool wasResisted, bool onlyAffectUndead)
	{
		EnemyBattleParticipant targetAsEnemy = (targetParticipant as EnemyBattleParticipant);
		if (onlyAffectUndead && targetAsEnemy != null && !targetAsEnemy.IsUndead())
		{
			return;
		}

		int effectiveness =  magicSpellData.effectiveness;
		if (targetParticipant.defenseWeakElement.ContainsElement(magicSpellData.element))
		{
			effectiveness *= 2;
		}

		if (targetParticipant.defenseStrongElement.ContainsElement(magicSpellData.element))
		{
			effectiveness /= 2;
		}

		int spellDamage = 0;
		if (wasResisted)
		{
			spellDamage = Random.Range(effectiveness, (2 * effectiveness) + 1);
		}
		else
		{
			spellDamage = 2 * (Random.Range(effectiveness, (2 * effectiveness) + 1));
		}

		SuperLogger.Log(sourceParticipant.participantName + " casts " + magicSpellData.spellName + " on " + targetParticipant.participantName + " for " + spellDamage + " damage.");

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

	private void DoMagicHeal(BattleParticipant sourceParticipant, BattleParticipant targetParticipant, MagicSpellData magicSpellData)
	{
		if (targetParticipant.currentHP > 0)
		{
			int healAmount = Random.Range (magicSpellData.effectiveness, (2 * magicSpellData.effectiveness) + 1);
			targetParticipant.currentHP += healAmount;

			if (targetParticipant.currentHP > targetParticipant.maxHP)
			{
				targetParticipant.currentHP = targetParticipant.maxHP;
			}

			SuperLogger.Log(sourceParticipant.participantName + " heals " + targetParticipant.participantName + " for " + healAmount);
		}
		else
		{
			SuperLogger.Log(sourceParticipant.participantName + "'s spell is ineffective!");
		}
	}

	private void UpdateHitPointText()
	{
		for (int i = 0; i < 4; ++i)
		{
			m_txtPCHPs[i].text = m_playerPartyList[i].currentHP.ToString() + "/" + m_playerPartyList[i].maxHP.ToString();
		}
	}

	private void UpdateMagicPointText()
	{
		for (int i = 0; i < 4; ++i)
		{
			m_txtPCMPs[i].text = m_playerPartyList[i].currentMP.ToString() + "/" + m_playerPartyList[i].maxMP.ToString();
		}
	}
	#endregion
}
