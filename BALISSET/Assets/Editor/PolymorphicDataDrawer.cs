using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

[CustomPropertyDrawer(typeof(PolymorphicData), true)]
public class PolymorphicDataDrawer : PropertyDrawer
{
    private Dictionary<Type, string[]> cachedTypeNames = new Dictionary<Type, string[]>();
    private Dictionary<Type, Dictionary<string, Type>> cachedTypeDicts = new Dictionary<Type, Dictionary<string, Type>>();

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        Type fieldType = fieldInfo.FieldType;
        if (fieldType.IsArray || fieldType.IsGenericType)
        {
            fieldType = fieldType.GetElementType() ?? fieldType.GetGenericArguments()[0];
        }

        if (!cachedTypeNames.ContainsKey(fieldType))
        {
            // Find all derived types of the base class
            List<Type> subTypes = (List<Type>)typeof(PolymorphicData)
                .GetMethod("GetAllDerivedTypes")?
                .MakeGenericMethod(fieldType)
                .Invoke(null, null);

            cachedTypeDicts[fieldType] = subTypes.ToDictionary(t => t.Name, t => t);
            cachedTypeNames[fieldType] = cachedTypeDicts[fieldType].Keys.ToArray();
        }

        string[] typeNames = cachedTypeNames[fieldType];
        Dictionary<string, Type> typeDict = cachedTypeDicts[fieldType];

        int selectedIndex = Array.IndexOf(typeNames, property.managedReferenceValue?.GetType().Name ?? "");
        int newIndex = EditorGUI.Popup(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight),
                                       label.text, selectedIndex, typeNames);

        if (newIndex != selectedIndex)
        {
            // Assign a new instance when a different type is selected
            Type selectedType = typeDict[typeNames[newIndex]];
            property.managedReferenceValue = Activator.CreateInstance(selectedType);
            property.serializedObject.ApplyModifiedProperties();
        }

        // Draw the child properties for the polymorphic object
        if (property.managedReferenceValue != null)
        {
            // Copy the property to iterate its children without affecting the original
            SerializedProperty iterator = property.Copy();
            int startingDepth = iterator.depth; // capture the current depth
            float yOffset = position.y + EditorGUIUtility.singleLineHeight + 2;

            // Move to the first child property
            while (iterator.NextVisible(true))
            {
                // Stop if we've moved outside of the current property's children
                if (iterator.depth <= startingDepth)
                    break;

                Rect propertyRect = new Rect(position.x, yOffset, position.width, EditorGUI.GetPropertyHeight(iterator, true));
                EditorGUI.PropertyField(propertyRect, iterator, true);
                yOffset += EditorGUI.GetPropertyHeight(iterator, true) + 2;
            }
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float height = EditorGUIUtility.singleLineHeight;
        if (property.managedReferenceValue != null)
        {
            SerializedProperty iterator = property.Copy();
            int startingDepth = iterator.depth;
            while (iterator.NextVisible(true))
            {
                if (iterator.depth <= startingDepth)
                    break;
                height += EditorGUI.GetPropertyHeight(iterator, true) + 2;
            }
        }
        return height;
    }
}
