using System;

[Flags]
public enum EnemyTypeMask
{
	None = 0,
	Magical = 1 << 0,
	Dragon = 1 << 1,
	Giant = 1 << 2,
	Undead = 1 << 3,
	Were = 1 << 4,
	Aquatic = 1 << 5,
	Mage = 1 << 6,
	Regenerative = 1 << 7
}

public static class EnemyTypeMaskMaskExtensions
{
	public static bool IsEnemyType(this EnemyTypeMask thisMask, EnemyTypeMask checkAgainst)
	{
		return (thisMask & checkAgainst) == checkAgainst;
	}

	public static bool HasEnemyType(this EnemyTypeMask thisMask)
	{
		return thisMask != 0;
	}

	public static bool ContainsEnemyType(this EnemyTypeMask thisMask, EnemyTypeMask checkAgainst)
	{
		if ((thisMask & checkAgainst) != 0)
		{
			return true;
		}

		return false;
	}

	public static bool HasMultipleEnemyTypes(this EnemyTypeMask thisMask)
	{
		return (thisMask != 0) && !thisMask.IsPowerOfTwo();
	}

	public static bool IsEverything(this EnemyTypeMask thisMask)
	{
		EnemyTypeMask everything = 0;
		foreach (EnemyTypeMask element in Enum.GetValues(typeof(EnemyTypeMask)))
		{
			everything |= element;
		}

		return thisMask == everything;
	}

	public static bool IsPowerOfTwo(this EnemyTypeMask thisMask)
	{
		return (thisMask != 0) && ((thisMask & (thisMask - 1)) == 0);
	}
}