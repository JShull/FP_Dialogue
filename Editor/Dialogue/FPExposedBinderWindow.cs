namespace FuzzPhyte.Dialogue.Editor
{
    using UnityEditor;
    using UnityEngine;
    public class FPExposedBinderWindow:EditorWindow
    {
        RTExposedBinder binder;
        GameObject sceneObject;
        string bindingId;

        [MenuItem("FuzzPhyte/Exposed Binder")]
        public static void Open() => GetWindow<FPExposedBinderWindow>("Exposed Binder");

        void OnGUI()
        {
            EditorGUILayout.LabelField("Bind a scene object to an ExposedReference id", EditorStyles.boldLabel);
            binder = (RTExposedBinder)EditorGUILayout.ObjectField("Binder (scene)", binder, typeof(RTExposedBinder), true);
            sceneObject = (GameObject)EditorGUILayout.ObjectField("Scene Object", sceneObject, typeof(GameObject), true);
            bindingId = EditorGUILayout.TextField("Binding Id", bindingId);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Generate Id"))
                    bindingId = System.Guid.NewGuid().ToString("N");

                if (GUILayout.Button("Bind"))
                    Bind();

                if (GUILayout.Button("Clear"))
                    Clear();
            }

            EditorGUILayout.HelpBox(
                "Paste the same Binding Id into your node’s ExposedReference.exposedName, " +
                "or store it as a node option and construct the ExposedReference at resolve time.",
                MessageType.Info);
        }

        void Bind()
        {
            if (!binder || !sceneObject || string.IsNullOrEmpty(bindingId))
            {
                Debug.LogWarning("Assign binder, scene object, and a non-empty binding id.");
                return;
            }

            Undo.RecordObject(binder, "Bind ExposedReference");
            binder.SetReferenceValue(new PropertyName(bindingId), sceneObject);
            EditorUtility.SetDirty(binder);
            Debug.Log($"Bound '{sceneObject.name}' to id '{bindingId}'.", binder);
        }

        void Clear()
        {
            if (!binder || string.IsNullOrEmpty(bindingId))
            {
                Debug.LogWarning("Assign binder and a binding id first.");
                return;
            }

            Undo.RecordObject(binder, "Clear ExposedReference");
            binder.ClearReferenceValue(new PropertyName(bindingId));
            EditorUtility.SetDirty(binder);
            Debug.Log($"Cleared binding id '{bindingId}'.", binder);
        }
    }
}
