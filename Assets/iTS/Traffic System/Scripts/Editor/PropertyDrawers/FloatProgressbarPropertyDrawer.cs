
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof (ProgressbarAttribute), true)]
public class FloatProgressBarPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType != SerializedPropertyType.Float)
        {
            EditorGUI.HelpBox(position, "Thi Attribute Progressbar only works with floats!", MessageType.Error);
            return;
        }
        
        EditorGUI.ProgressBar(position, property.floatValue, label.text);
    }
}
