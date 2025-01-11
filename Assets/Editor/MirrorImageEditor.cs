using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEditor.UI;

[CustomEditor(typeof(MirrorImage))]
public class MirrorImageEditor:ImageEditor
{
    SerializedProperty m_imageType;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUILayout.PropertyField(m_imageType);
        serializedObject.ApplyModifiedProperties();
    }
   

    protected override void OnEnable()
    {
        base.OnEnable();
        m_imageType = serializedObject.FindProperty("m_imageResourceType");
    }
}

