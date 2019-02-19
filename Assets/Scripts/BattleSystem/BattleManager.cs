using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleManager : MonoBehaviour
{
	[SerializeField] Button btnUndo = null;
	[SerializeField] Button btnFight = null;
	[SerializeField] Button btnMagic = null;
	[SerializeField] Button btnRun = null;
	[SerializeField] Button btnItem = null;

	[SerializeField] Text[] txtPCNames = null;
	[SerializeField] Text[] txtPCHPs = null;
	[SerializeField] Text[] txtPCMPs = null;

	[SerializeField] Transform[] monsterFormation1Placements = null;
	[SerializeField] Transform[] monsterFormation2Placements = null;
	[SerializeField] Transform[] monsterFormation3Placements = null;
	[SerializeField] Transform[] bossPlacement = null;
	[SerializeField] Transform[] heroPlacements = null;
	[SerializeField] GridLayoutGroup[] heroMagicPanels = null;

	[SerializeField] GameObject[] monsterPrefabs = null;
	[SerializeField] GameObject[] heroPrefabs = null;
	[SerializeField] GameObject spellButtonPrefab = null;

	[SerializeField] MagicDatabase magicDatabase = null;

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
		Run,
		StatusRecover
	}

	private struct CombatAction
	{
		public BattleParticipant sourceParticipant;
		public List<BattleParticipant> targetParticipant;
		public CombatActionType actionType;
		public MagicSpellData magicSpellData;
	}

	private List<BattleParticipant> playerPartyList = new List<BattleParticipant>();
	private List<BattleParticipant> enemyPartyList = new List<BattleParticipant>();
	private Queue<CombatAction> combatActionQueue = new Queue<CombatAction>();
	private BattleState currentBattleState = BattleState.startCombat;
	private TargetState currentTargetState = TargetState.None;
	private MagicSpellData spellBeingTargeted;
	private const int kBaseChanceToHitMelee = 168;
	private const int kBaseChanceToHitMagic = 148;
	private const int kBaseChanceToInflictStatus = 100;
	private int currentHero;
	private int aliveHeros;
	private int aliveMonsters;

	#region Initialization
	void Start()
	{

		btnUndo.gameObject.SetActive(false);

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
		btnUndo.onClick.AddListener(OnUndoButtonClicked);
		btnFight.onClick.AddListener(OnFightButtonClicked);
		btnMagic.onClick.AddListener(OnMagicButtonClicked);
		btnRun.onClick.AddListener(OnRunButtonClicked);
		btnItem.onClick.AddListener(OnItemButtonClicked);
	}

	private void UnBind()
	{
		btnUndo.onClick.RemoveListener(OnUndoButtonClicked);
		btnFight.onClick.RemoveListener(OnFightButtonClicked);
		btnMagic.onClick.RemoveListener(OnMagicButtonClicked);
		btnRun.onClick.RemoveListener(OnRunButtonClicked);
		btnItem.onClick.RemoveListener(OnItemButtonClicked);
	}

	private void InitializePlayerParty()
	{
		for (int i = 0; i < 4; ++i)
		{
			GameObject tempObject = Instantiate(heroPrefabs[i]);

			PlayableBattleParticipant newParticipant = tempObject.GetComponentInChildren<PlayableBattleParticipant>();
			newParticipant.Init();
			playerPartyList.Add(newParticipant);

			if (newParticipant.battleData.spellIndexArray.Length > 0)
			{
				for (int j = 0; j < newParticipant.battleData.spellIndexArray.Length; ++j)
				{
					GameObject spellObject = Instantiate(spellButtonPrefab);
					spellObject.transform.SetParent(heroMagicPanels[i].transform, false);

					MagicSpellButton magicSpellButton = spellObject.GetComponentInChildren<MagicSpellButton>();
					magicSpellButton.onClick.AddListener(() => OnMagicSpellButtonClicked(magicSpellButton));
					magicSpellButton.SetMagicSpellData(magicDatabase.GetMagicSpellData(newParticipant.battleData.spellIndexArray[j]));
				}
			}

			heroMagicPanels[i].gameObject.SetActive(false);

			AllyTargetButton targetButton = tempObject.GetComponentInChildren<AllyTargetButton>();
			targetButton.onClick.AddListener(() => OnTargetClicked(targetButton));
			targetButton.battleParticipant = newParticipant;

			txtPCNames[i].text = newParticipant.participantName;
			txtPCHPs[i].text = newParticipant.currentHP.ToString() + "/" + newParticipant.maxHP.ToString();
			txtPCMPs[i].text = newParticipant.currentMP.ToString() + "/" + newParticipant.maxMP.ToString();
			tempObject.transform.SetParent(heroPlacements[i], false);

			aliveHeros++;
		}
	}

	private void InitializeMonsterParty()
	{
		int monsterGroup = 0;// Random.Range(0,3);

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
			GameObject tempObject = Instantiate(monsterPrefabs[0]);

			EnemyBattleParticipant newParticipant = tempObject.GetComponentInChildren<EnemyBattleParticipant>();
			newParticipant.Init();
			enemyPartyList.Add(newParticipant);

			EnemyTargetButton targetButton = tempObject.GetComponentInChildren<EnemyTargetButton>();
			targetButton.onClick.AddListener(() => OnTargetClicked(targetButton));
			targetButton.battleParticipant = newParticipant;

			tempObject.transform.SetParent(monsterFormation1Placements[i], false);

			aliveMonsters++;
		}
	}

	private void InitializeMonsterGroupOne()
	{
		int numMonsters = Random.Range(1,5);

		for (int i = 0; i < numMonsters; ++i)
		{
			GameObject tempObject = Instantiate(monsterPrefabs[1]);

			EnemyBattleParticipant newParticipant = tempObject.GetComponentInChildren<EnemyBattleParticipant>();
			newParticipant.Init();
			enemyPartyList.Add(newParticipant);

			EnemyTargetButton targetButton = tempObject.GetComponentInChildren<EnemyTargetButton>();
			targetButton.onClick.AddListener(() => OnTargetClicked(targetButton));
			targetButton.battleParticipant = newParticipant;

			tempObject.transform.SetParent(monsterFormation2Placements[i], false);

			aliveMonsters++;
		}

		if (aliveMonsters < 1)
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
			GameObject tempObject = Instantiate(monsterPrefabs[1]);

			EnemyBattleParticipant newParticipant = tempObject.GetComponentInChildren<EnemyBattleParticipant>();
			newParticipant.Init();
			enemyPartyList.Add(newParticipant);

			EnemyTargetButton targetButton = tempObject.GetComponentInChildren<EnemyTargetButton>();
			targetButton.onClick.AddListener(() => OnTargetClicked(targetButton));
			targetButton.battleParticipant = newParticipant;

			tempObject.transform.SetParent(monsterFormation3Placements[i], false);

			aliveMonsters++;
		}

		for (int j = 0; j < numSmallMonsters; ++j)
		{
			GameObject tempObject = Instantiate(monsterPrefabs[0]);

			EnemyBattleParticipant newParticipant = tempObject.GetComponentInChildren<EnemyBattleParticipant>();
			newParticipant.Init();
			enemyPartyList.Add(newParticipant);

			EnemyTargetButton targetButton = tempObject.GetComponentInChildren<EnemyTargetButton>();
			targetButton.onClick.AddListener(() => OnTargetClicked(targetButton));
			targetButton.battleParticipant = newParticipant;

			tempObject.transform.SetParent(monsterFormation3Placements[j + 2], false);

			aliveMonsters++;
		}

		if (aliveMonsters < 1)
		{
			InitializeMonsterGroupTwo();
		}
	}

	#endregion

	// Update is called once per frame
	void Update()
	{
		switch (currentBattleState)
		{
			case BattleState.startCombat:
				{
					currentBattleState = BattleState.playerInput;
				}
				break;
			case BattleState.playerInput:
				{
					if (currentHero == 4)
					{
						btnUndo.gameObject.SetActive(false);
						currentBattleState = BattleState.monsterDecision;
					}
					else if (playerPartyList[currentHero].currentHP < 1 || playerPartyList[currentHero].IsIncapacitated())
					{
						SuperLogger.Log("Skipping incapacitated hero.");
						currentHero++;
					}
					else if (playerPartyList[currentHero].IsIncapacitated())
					{
						CombatAction action = new CombatAction();
						action.sourceParticipant = playerPartyList[currentHero];
						SuperLogger.Log(action.sourceParticipant.participantName + " is debilitated, will attempt to recover at end of round.");
						action.actionType = CombatActionType.StatusRecover;
						combatActionQueue.Enqueue(action);
						currentHero++;
					}
				}
				break;
			case BattleState.monsterDecision:
				{
					SuperLogger.Log("The Monster is making his mind up!");
					MonsterDecision();
					currentBattleState = BattleState.resolveRound;
				}
				break;
			case BattleState.resolveRound:
				{
					SuperLogger.Log("Let's see how it all plays out...");
					ResolveRound();

					if (aliveHeros < 1)
					{
						SuperLogger.Log("You Lose!");
						currentBattleState = BattleState.endCombat;
					}
					else if (aliveMonsters < 1)
					{
						SuperLogger.Log("You Win!");
						currentBattleState = BattleState.endCombat;
					}
					else
					{
						SuperLogger.Log("Another round");
						currentHero = 0;
						currentBattleState = BattleState.playerInput;
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
		if (currentBattleState == BattleState.playerInput)
		{
			if (currentTargetState != TargetState.None)
			{
				switch (currentTargetState)
				{
					case TargetState.Fight:
						{
							currentTargetState = TargetState.None;
						}
						break;
					case TargetState.Magic_Select:
						{
							currentTargetState = TargetState.None;
							heroMagicPanels[currentHero].gameObject.SetActive(false);
						}
						break;
					case TargetState.Magic_Target:
						{
							currentTargetState = TargetState.Magic_Select;
							heroMagicPanels[currentHero].gameObject.SetActive(true);
							spellBeingTargeted = null;
						}
						break;
					case TargetState.Item_Select:
						{
							currentTargetState = TargetState.None;
						}
						break;
					case TargetState.Item_Target:
						{
							currentTargetState = TargetState.Item_Target;
						}
						break;
				}
			}
			else
			{
				if (combatActionQueue.Count > 0)
				{
					combatActionQueue.Dequeue();
					currentHero--;
					spellBeingTargeted = null;
				}
			}
		}
	}

	private void OnFightButtonClicked()
	{
		if (currentBattleState == BattleState.playerInput)
		{
			SuperLogger.Log("Select Target! currentHero " + currentHero);
			currentTargetState = TargetState.Fight;

			if (!btnUndo.gameObject.activeSelf)
			{
				btnUndo.gameObject.SetActive(true);
			}
		}
	}

	private void OnMagicButtonClicked()
	{
		if (currentBattleState == BattleState.playerInput)
		{
			SuperLogger.Log("Select Spell! currentHero " + currentHero);

			PlayableBattleParticipant participant = (PlayableBattleParticipant)playerPartyList[currentHero];

			if (participant.battleData.spellIndexArray.Length > 0)
			{
				currentTargetState = TargetState.Magic_Select;

				if (!heroMagicPanels[currentHero].gameObject.activeSelf)
				{
					heroMagicPanels[currentHero].gameObject.SetActive(true);
				}

				if (!btnUndo.gameObject.activeSelf)
				{
					btnUndo.gameObject.SetActive(true);
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
		SuperLogger.Log("button " + button + " target state" + currentTargetState + " battle state " + currentBattleState);

		switch (currentTargetState)
		{
			case TargetState.Fight:
				{
					CombatAction action = new CombatAction();
					action.sourceParticipant = playerPartyList[currentHero];

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

					combatActionQueue.Enqueue(action);
					currentHero++;

					SuperLogger.Log(action.sourceParticipant.participantName + " is targeting  " + action.targetParticipant[0].participantName);
				}
				break;
			case TargetState.Magic_Target:
				{
					CombatAction action = new CombatAction();
					action.sourceParticipant = playerPartyList[currentHero];
					action.magicSpellData = spellBeingTargeted;
					action.actionType = CombatActionType.Magic;

					if (spellBeingTargeted.targetType == TargetType.SingleEnemy)
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
					else if (spellBeingTargeted.targetType == TargetType.SingleAlly)
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

					combatActionQueue.Enqueue(action);
					currentHero++;
					spellBeingTargeted = null;

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

		currentTargetState = TargetState.None;
	}

	private void OnMagicSpellButtonClicked(Button button)
	{
		if (currentTargetState == TargetState.Magic_Select)
		{
			MagicSpellData magicSpellData = (button as MagicSpellButton).magicSpellData;

			if (playerPartyList[currentHero].currentMP >= magicSpellData.mpCost)
			{
				spellBeingTargeted = magicSpellData;

				switch (magicSpellData.targetType)
				{
					case TargetType.AllAllies:
					case TargetType.AllEnemies:
					case TargetType.Caster:
						{
							CombatAction action = new CombatAction();
							action.sourceParticipant = playerPartyList[currentHero];
							action.magicSpellData = spellBeingTargeted;
							action.actionType = CombatActionType.Magic;

							//Add targets
							if (magicSpellData.targetType == TargetType.AllAllies)
							{
								action.targetParticipant = playerPartyList;
							}
							else if (magicSpellData.targetType == TargetType.AllEnemies)
							{
								action.targetParticipant = enemyPartyList;
							}
							else
							{
								action.targetParticipant = new List<BattleParticipant>();
								action.targetParticipant.Add(playerPartyList[currentHero]);
							}

							combatActionQueue.Enqueue(action);
							heroMagicPanels[currentHero].gameObject.SetActive(false);
							currentHero++;
							spellBeingTargeted = null;
						}
						break;
					case TargetType.SingleAlly:
					case TargetType.SingleEnemy:
					default:
						{
							SuperLogger.Log("Select Target! currentHero " + currentHero);
							currentTargetState = TargetState.Magic_Target;
							heroMagicPanels[currentHero].gameObject.SetActive(false);
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
		for (int i = 0; i < enemyPartyList.Count; ++i)
		{
			CombatAction action = new CombatAction ();
			action.sourceParticipant = enemyPartyList[i];

			if (action.sourceParticipant.currentHP <= 0 || action.sourceParticipant.IsIncapacitated())
			{
				SuperLogger.Log(action.sourceParticipant.participantName + " is dead or incapacitated.");
				continue;
			}

			if (action.sourceParticipant.IsDebilitated())
			{
				SuperLogger.Log(action.sourceParticipant.participantName + " is debilitated.");
				action.actionType = CombatActionType.StatusRecover;
				combatActionQueue.Enqueue(action);
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

				if (playerPartyList[index].currentHP > 0)
				{
					action.targetParticipant = new List<BattleParticipant>();
					action.targetParticipant.Add( playerPartyList[index]);
					foundTarget = true;
					action.actionType = CombatActionType.Fight;

					SuperLogger.Log(action.sourceParticipant.participantName + " is targeting  " + action.targetParticipant[0].participantName);

					combatActionQueue.Enqueue(action);
				}
			}
		}
	}

	private void ResolveRound()
	{
		while (combatActionQueue.Count != 0)
		{
			CombatAction action = combatActionQueue.Dequeue ();

			if (action.sourceParticipant.currentHP > 0 && !action.sourceParticipant.IsIncapacitated() && !action.sourceParticipant.IsDebilitated())				
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
				if (action.sourceParticipant.AttemptToRecoverFromDebilitation())
				{
					SuperLogger.Log(action.sourceParticipant.participantName + " recovered!");
				}
				else
				{
					SuperLogger.Log(action.sourceParticipant.participantName + " is unable to act, may be dead, incapacitated or otherwise debilitated.");
				}
			}

			ResolvePoisonDamage();
			UpdateHitPointText();
			UpdateMagicPointText();

			if (aliveHeros < 1 || aliveMonsters < 1)
			{
				break;
			}
		}
	}

	private void ResolvePoisonDamage()
	{
		for (int i=0; i < playerPartyList.Count; ++i)
		{
			if (playerPartyList[i].activeStatusEffect.ContainsStatus(StatusEffectMask.Poisoned))
			{
				playerPartyList[i].currentHP -= 2;
			}
		}

		for (int j = 0; j < playerPartyList.Count; ++j)
		{
			if (enemyPartyList[j].activeStatusEffect.ContainsStatus(StatusEffectMask.Poisoned))
			{
				enemyPartyList[j].currentHP -= 2;
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
			int attackerBlindPenalty =  sourceParticipant.activeStatusEffect.ContainsStatus(StatusEffectMask.Blind) ? 40 : 0;
			int targetBlindBonus = targetParticipant.activeStatusEffect.ContainsStatus(StatusEffectMask.Blind) ? 40 : 0;
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
		bool isDebilitated = targetParticipant.IsDebilitated();

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

			if (AttemptStatusAffliction(sourceParticipant, targetParticipant))
			{
				if (!targetParticipant.activeStatusEffect.ContainsStatus(sourceParticipant.statusEffectToInflict))
				{
					targetParticipant.activeStatusEffect |= sourceParticipant.statusEffectToInflict;
					SuperLogger.Log(sourceParticipant.participantName +  " inflicts " + targetParticipant.participantName + " with status " + sourceParticipant.statusEffectToInflict);
				}
			}
		}

		SuperLogger.Log(sourceParticipant.participantName + " hits " + targetParticipant.participantName + " " + sourceParticipant.Hits() + " times for " + totalDamage + " damage.");

		targetParticipant.currentHP -= totalDamage;		

		if (targetParticipant.currentHP <= 0)
		{
			SuperLogger.Log(targetParticipant.participantName + " has died!");
			targetParticipant.currentHP = 0;
			if (targetParticipant is PlayableBattleParticipant)
			{
				aliveHeros--;
			}
			else if (targetParticipant is EnemyBattleParticipant)
			{
				aliveMonsters--;
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
					if (!targetParticipant.activeStatusEffect.ContainsStatus(statusToInflict))
					{
						targetParticipant.activeStatusEffect |= statusToInflict;
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
					if (targetParticipant.activeStatusEffect.ContainsStatus(statusToUndo))
					{
						targetParticipant.activeStatusEffect &= ~statusToUndo;
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
				aliveHeros--;
			}
			else if (targetParticipant is EnemyBattleParticipant)
			{
				aliveMonsters--;
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
			txtPCHPs[i].text = playerPartyList[i].currentHP.ToString() + "/" + playerPartyList[i].maxHP.ToString();
		}
	}

	private void UpdateMagicPointText()
	{
		for (int i = 0; i < 4; ++i)
		{
			txtPCMPs[i].text = playerPartyList[i].currentMP.ToString() + "/" + playerPartyList[i].maxMP.ToString();
		}
	}

	private bool AttemptStatusAffliction(BattleParticipant sourceParticipant, BattleParticipant targetParticipant)
	{
		int BaseChance = targetParticipant.defenseStrongElement == sourceParticipant.attackElement ? 0 : kBaseChanceToInflictStatus;
		int HitRoll = Random.Range(0,200);

		SuperLogger.Log(sourceParticipant.name + " status effect hit roll: " + HitRoll);

		return HitRoll <= BaseChance ? true : false;
	}
	#endregion
}
