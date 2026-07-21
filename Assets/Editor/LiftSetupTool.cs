using UnityEngine;
using UnityEditor;

public class LiftSetupTool : EditorWindow
{
    [MenuItem("Tools/Setup Lift Components")]
    public static void ShowWindow()
    {
        GetWindow<LiftSetupTool>("Lift Setup Tool");
    }

    private GameObject liftRootObject;

    private void OnGUI()
    {
        GUILayout.Label("Automatic Lift Component Installer", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        liftRootObject = (GameObject)EditorGUILayout.ObjectField(
            "Lift Root / Container", 
            liftRootObject, 
            typeof(GameObject), 
            true
        );

        EditorGUILayout.Space();

        if (GUILayout.Button("Configure Lift & Buttons Now", GUILayout.Height(35)))
        {
            if (liftRootObject == null)
            {
                EditorUtility.DisplayDialog("Error", "Please assign the Lift Root GameObject first!", "OK");
                return;
            }

            SetupLiftSystem(liftRootObject);
        }
    }

    private static void SetupLiftSystem(GameObject root)
    {
        // 1. Ensure LiftController is attached to the root object
        LiftController controller = root.GetComponent<LiftController>();
        if (controller == null)
        {
            controller = Undo.AddComponent<LiftController>(root);
            Debug.Log($"<color=green>[LiftTool]</color> Added LiftController to '{root.name}'.");
        }

        // 2. Find all child objects containing "Button" in their name
        Transform[] allTransforms = root.GetComponentsInChildren<Transform>(true);
        int buttonsConfigured = 0;

        foreach (Transform t in allTransforms)
        {
            string objName = t.gameObject.name;

            // Target objects like Button_Floor_0, Button_Floor_1, etc.
            if (objName.StartsWith("Button_Floor_") || objName.ToLower().Contains("button"))
            {
                // Ensure a Collider exists so OnMouseDown / Raycasts work
                if (t.GetComponent<Collider>() == null)
                {
                    Undo.AddComponent<BoxCollider>(t.gameObject);
                    Debug.Log($"<color=yellow>[LiftTool]</color> Added BoxCollider to '{objName}'.");
                }

                // Add or get LiftButton
                LiftButton btn = t.GetComponent<LiftButton>();
                if (btn == null)
                {
                    btn = Undo.AddComponent<LiftButton>(t.gameObject);
                }

                // Link the controller reference
                Undo.RecordObject(btn, "Configure Lift Button");
                btn.controller = controller;

                // Parse the floor number from the name if possible (e.g. "Button_Floor_2" -> index 2)
                int floorIndex = ExtractFloorIndexFromName(objName);
                if (floorIndex != -1)
                {
                    btn.targetFloorIndex = floorIndex;
                }

                buttonsConfigured++;
            }
        }

        // 3. Cleanup missing script components on the root and children if any remain
        GameObjectUtility.RemoveMonoBehavioursWithMissingScript(root);
        foreach (Transform t in allTransforms)
        {
            GameObjectUtility.RemoveMonoBehavioursWithMissingScript(t.gameObject);
        }

        Debug.Log($"<color=green>[LiftTool]</color> Successfully configured Lift System! Managed {buttonsConfigured} buttons.");
        EditorUtility.DisplayDialog("Success", $"Configured {buttonsConfigured} floor buttons and linked them to {root.name}.", "OK");
    }

    private static int ExtractFloorIndexFromName(string name)
    {
        // Extracts trailing digits from names like Button_Floor_0, Button_1, etc.
        string[] parts = name.Split('_');
        foreach (string part in parts)
        {
            if (int.TryParse(part, out int index))
            {
                return index;
            }
        }
        return -1;
    }
}