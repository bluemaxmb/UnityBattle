using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Base class for in battle NPCs, friendly or otherwise
public class NonPlayableBattleParticipant : BattleParticipant {
	public override void Init ()
	{
		throw new System.NotImplementedException ();
	}

	public override int AttackDamage ()
	{
		throw new System.NotImplementedException ();
	}

	public override int Hits ()
	{
		throw new System.NotImplementedException ();
	}

	public override int Accuracy ()
	{
		throw new System.NotImplementedException ();
	}

	public override int Evasion()
	{
		throw new System.NotImplementedException ();
	}

	public override int Defense ()
	{
		throw new System.NotImplementedException ();
	}

	public override int CritChance ()
	{
		throw new System.NotImplementedException ();
	}

	public override int MagicDefense ()
	{
		throw new System.NotImplementedException ();
	}
}
