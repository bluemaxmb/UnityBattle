﻿using System;

public enum StatusEffectMask
{
	None = 0,
	Blind = 1 << 1,
	Poison = 1 << 2,
	Asleep = 1 << 3,
	Paralyzed = 1 << 4,
	Dead = 1 << 5,
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