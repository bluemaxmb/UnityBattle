using UnityEngine;

public static class SuperLogger
{
	public static void Log (object message)
	{
#if DEBUG_MODE || UNITY_EDITOR
		Debug.Log (message);
		OnScreenLog.Add ("" + message);
#endif
	}

	public static void LogWarning (object message)
	{
#if DEBUG_MODE || UNITY_EDITOR
		Debug.LogWarning (message);
		OnScreenLog.Add ("WARNING: " + message); //TODO: Maybe add a warning mode to this?
#endif
	}

	public static void LogWarning (string v, UnityEngine.Object obj)
	{
#if DEBUG_MODE || UNITY_EDITOR
		Debug.LogWarning (v, obj);
		OnScreenLog.Add ("WARNING: " + v); //TODO: Maybe add a error mode to this?
#endif
	}

	public static void LogError (object message)
	{
#if DEBUG_MODE || UNITY_EDITOR
		Debug.LogError (message);
		OnScreenLog.Add ("ERROR: " + message); //TODO: Maybe add a error mode to this?
#endif
	}

	public static void LogError (string v, UnityEngine.Object obj)
	{
#if DEBUG_MODE || UNITY_EDITOR
		Debug.LogError (v, obj);
		OnScreenLog.Add ("WARNING: " + v); //TODO: Maybe add a error mode to this?
#endif
	}
}