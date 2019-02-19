using System;

[Flags]
public enum StatusEffectMask
{
	None = 0,
	Dead = 1 << 0,
	Petrified = 1 << 1,
	Poisoned = 1 << 2,
	Blind = 1 << 3,
	Paralyzed = 1 << 4,
	Asleep = 1 << 5,
	Silenced = 1 << 6,
	Confused = 1 << 7
}

public static class StatusEffectMaskMaskExtensions
{
	public static bool IsStatus(this StatusEffectMask thisMask, StatusEffectMask checkAgainst)
	{
		return (thisMask & checkAgainst) == checkAgainst;
	}

	public static bool HasStatus(this StatusEffectMask thisMask)
	{
		return thisMask != 0;
	}

	public static bool ContainsStatus(this StatusEffectMask thisMask, StatusEffectMask checkAgainst)
	{
		if ((thisMask & checkAgainst) != 0)
		{
			return true;
		}

		return false;
	}

	public static bool HasMultipleStatuss(this StatusEffectMask thisMask)
	{
		return (thisMask != 0) && !thisMask.IsPowerOfTwo();
	}

	public static bool IsEverything(this StatusEffectMask thisMask)
	{
		StatusEffectMask everything = 0;
		foreach (StatusEffectMask element in Enum.GetValues(typeof(StatusEffectMask)))
		{
			everything |= element;
		}

		return thisMask == everything;
	}

	public static bool IsPowerOfTwo(this StatusEffectMask thisMask)
	{
		return (thisMask != 0) && ((thisMask & (thisMask - 1)) == 0);
	}
}