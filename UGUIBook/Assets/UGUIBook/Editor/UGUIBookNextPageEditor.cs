using UnityEditor.UI;
using UnityEditor;

[CustomEditor(typeof(UGUIBookNextPage))]
public class UGUIBookNextPageEditor : RawImageEditor {

	private SerializedProperty propertyDamping;

	protected override void OnEnable ()
	{
		base.OnEnable ();
		propertyDamping = serializedObject.FindProperty ("damping");
	}

	public override void OnInspectorGUI ()
	{
		base.OnInspectorGUI ();
		EditorGUILayout.PropertyField (propertyDamping);
		serializedObject.ApplyModifiedProperties ();
	}
}
