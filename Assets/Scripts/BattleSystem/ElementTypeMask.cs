using System;

public enum ElementTypeMask
{
	None = 0,
	Fire = 1 << 0,
	Ice = 1 << 1,
	Lightning = 1 << 2,
	Earth = 1 << 3,
}

public static class ElementTypeMaskExtensions
{
	public static bool IsElement(this ElementTypeMask thisMask, ElementTypeMask element)
	{
		return (thisMask & element) == element;
	}

	public static bool HasElement(this ElementTypeMask thisMask)
	{
		return thisMask != 0;
	}

	public static bool ContainsElement(this ElementTypeMask thisMask, ElementTypeMask checkAgainst)
	{
		if ((thisMask & checkAgainst) != 0)
		{
			return true;
		}

		return false;
	}

	public static bool HasMultipleElements(this ElementTypeMask thisMask)
	{
		return (thisMask != 0) && !thisMask.IsPowerOfTwo();
	}

	public static bool IsEverything(this ElementTypeMask thisMask)
	{
		ElementTypeMask everything = 0;
		foreach (ElementTypeMask element in Enum.GetValues(typeof(ElementTypeMask)))
		{
			everything |= element;
		}

		return thisMask == everything;
	}

	public static bool IsPowerOfTwo(this ElementTypeMask thisMask)
	{
		return (thisMask != 0) && ((thisMask & (thisMask - 1)) == 0);
	}
}