using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(BitMaskAttribute))]
public class EnumBitMaskPropertyDrawer : PropertyDrawer
{
	public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
	{
		var typeAttr = attribute as BitMaskAttribute;
		// Add the actual int value behind the field name
		label.text = label.text + "(" + prop.intValue + ")";
		prop.intValue = EditorExtension.DrawBitMaskField(position, prop.intValue, typeAttr.propType, label);
	}
}