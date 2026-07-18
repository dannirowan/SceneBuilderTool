using UnityEngine;
using UnityEditor;

public class RoomBuilderTool : EditorWindow
{
    // Generation Settings
    private float wallThickness = 0.05f;
    private Material wallMaterial;
    private Material floorMaterial;

    // Feature Placement Settings
    private GameObject targetWall;
    private enum FeatureMode { Door, Window }
    private FeatureMode activeMode = FeatureMode.Door;

    // Door & Doorstep Settings
    private GameObject doorPrefab;
    private float doorWidth = 1.2f;
    private float doorHeight = 2.1f;
    private float doorHorizontalOffset = 0.0f; 
    private bool includeDoorstep = false;
    private float doorstepHeight = 0.1f;
    private GameObject doorstepPrefab;
    private Material doorstepMaterial;

    // Window Settings
    private GameObject windowPrefab;
    private float windowWidth = 1.5f;
    private float windowHeight = 1.2f;
    private float windowHorizontalOffset = 0.0f; 
    private float windowVerticalOffset = 0.0f;   

    [MenuItem("Tools/Advanced Room Builder")]
    public static void ShowWindow()
    {
        GetWindow<RoomBuilderTool>("Room Builder");
    }

    private void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    private void OnGUI()
    {
        // --- SECTION 1: GENERATION ---
        GUILayout.Label("1. Generate Room From Primitive", EditorStyles.boldLabel);
        wallThickness = EditorGUILayout.FloatField("Wall Thickness", wallThickness);
        wallMaterial = (Material)EditorGUILayout.ObjectField("Wall Material", wallMaterial, typeof(Material), false);
        floorMaterial = (Material)EditorGUILayout.ObjectField("Floor/Ceiling Material", floorMaterial, typeof(Material), false);

        if (GUILayout.Button("Convert Selected Cube to Room"))
        {
            ConvertSelected();
        }

        EditorGUILayout.Space(15);
        
        // --- SECTION 2: EDITING ---
        GUILayout.Label("2. Add Doors & Windows", EditorStyles.boldLabel);
        
        GameObject newTarget = (GameObject)EditorGUILayout.ObjectField("Target Wall Piece", targetWall, typeof(GameObject), true);
        if (newTarget != targetWall)
        {
            targetWall = newTarget;
        }
        
        if (targetWall == null)
        {
            EditorGUILayout.HelpBox("Select a generated wall piece to enable interactive scene tools.", MessageType.Info);
            return;
        }

        activeMode = (FeatureMode)GUILayout.Toolbar((int)activeMode, new string[] { "Door Setup Mode", "Window Setup Mode" });

        EditorGUILayout.Space(5);
        
        if (activeMode == FeatureMode.Door)
        {
            GUILayout.BeginVertical("box");
            GUILayout.Label("Door Settings (W = Move, R = Scale in Scene View)", EditorStyles.miniBoldLabel);
            doorPrefab = (GameObject)EditorGUILayout.ObjectField("Door Prefab", doorPrefab, typeof(GameObject), false);
            doorWidth = EditorGUILayout.FloatField("Door Width", doorWidth);
            doorHeight = EditorGUILayout.FloatField("Door Height", doorHeight);
            doorHorizontalOffset = EditorGUILayout.Slider("Position Along Wall", doorHorizontalOffset, -0.45f, 0.45f);
            
            EditorGUILayout.Space(5);
            includeDoorstep = EditorGUILayout.Toggle("Include Doorstep?", includeDoorstep);
            if (includeDoorstep)
            {
                EditorGUI.indentLevel++;
                doorstepHeight = EditorGUILayout.FloatField("Doorstep Height", doorstepHeight);
                doorstepPrefab = (GameObject)EditorGUILayout.ObjectField("Doorstep Prefab (Opt)", doorstepPrefab, typeof(GameObject), false);
                if (doorstepPrefab == null) doorstepMaterial = (Material)EditorGUILayout.ObjectField("Doorstep Material", doorstepMaterial, typeof(Material), false);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.Space(10);

            if (GUILayout.Button("Punch Doorway", GUILayout.Height(30)))
            {
                SpawnDoorInEditor();
            }
            GUILayout.EndVertical();
        }
        else
        {
            GUILayout.BeginVertical("box");
            GUILayout.Label("Window Settings (W = Move, R = Scale in Scene View)", EditorStyles.miniBoldLabel);
            windowPrefab = (GameObject)EditorGUILayout.ObjectField("Window Prefab", windowPrefab, typeof(GameObject), false);
            windowWidth = EditorGUILayout.FloatField("Window Width", windowWidth);
            windowHeight = EditorGUILayout.FloatField("Window Height", windowHeight);
            windowHorizontalOffset = EditorGUILayout.Slider("Horizontal Position", windowHorizontalOffset, -0.45f, 0.45f);
            windowVerticalOffset = EditorGUILayout.Slider("Vertical Position", windowVerticalOffset, -0.45f, 0.45f);
            
            EditorGUILayout.Space(10);

            if (GUILayout.Button("Punch Window", GUILayout.Height(30)))
            {
                SpawnWindowInEditor();
            }
            GUILayout.EndVertical();
        }

        if (GUI.changed) SceneView.RepaintAll();
    }

private void OnSceneGUI(SceneView sceneView)
    {
        if (targetWall == null) return;

        Transform wTransform = targetWall.transform;
        Vector3 origScale = wTransform.localScale;
        Vector3 origLocalPos = wTransform.localPosition;

        // Prevent division-by-zero errors if a wall scale axis is 0
        float scaleX = Mathf.Max(0.001f, origScale.x);
        float scaleY = Mathf.Max(0.001f, origScale.y);
        float scaleZ = Mathf.Max(0.001f, origScale.z);

        // Determine dimensions based on active mode
        float currentWidth, currentHeight;
        Vector3 localCenterPos;

        if (activeMode == FeatureMode.Door)
        {
            currentWidth = doorWidth;
            float activeStepHeight = includeDoorstep ? doorstepHeight : 0f;
            currentHeight = doorHeight + activeStepHeight;

            // Horizontal offset is already a normalized percentage (-0.45 to 0.45)
            float centerOffset = doorHorizontalOffset * scaleX;
            float centerY = (-scaleY / 2f) + (currentHeight / 2f);
            
            // Convert to true local space coordinates relative to wall scale
            localCenterPos = new Vector3(centerOffset / scaleX, centerY / scaleY, 0f);
        }
        else
        {
            currentWidth = windowWidth;
            currentHeight = windowHeight;

            float xOffset = windowHorizontalOffset * scaleX;
            float yOffset = windowVerticalOffset * scaleY;
            
            localCenterPos = new Vector3(xOffset / scaleX, yOffset / scaleY, 0f);
        }

        // Convert the local center position to absolute world space
        Vector3 targetWorldPos = wTransform.TransformPoint(localCenterPos);

        // --- FIX: Normalize the absolute dimensions against the wall's local scale ---
        Vector3 localBoxSize = new Vector3(
            currentWidth / scaleX, 
            currentHeight / scaleY, 
            1.2f // Slightly thicker than the wall layer depth inherently
        );
        // -----------------------------------------------------------------------------

        // Draw the preview box using local-to-world point transformations
        DrawPreviewBox(wTransform, localCenterPos, localBoxSize);

        Handles.Label(targetWorldPos + wTransform.up * (currentHeight * 0.6f), 
            $"{activeMode} Editor Mode\n[W] Move | [R/T] Scale", EditorStyles.boldLabel);

        // Process Interactive Handles based on active Unity Tool
        if (Tools.current == Tool.Scale || Tools.current == Tool.Rect)
        {
            float handleSize = HandleUtility.GetHandleSize(targetWorldPos) * 0.15f;
            
            // --- WIDTH HANDLE (Right Side) ---
            Vector3 rightEdgeWorld = wTransform.TransformPoint(localCenterPos + Vector3.right * (localBoxSize.x * 0.5f));
            EditorGUI.BeginChangeCheck();
            Vector3 newRightEdge = Handles.Slider(rightEdgeWorld, wTransform.right, handleSize, Handles.CubeHandleCap, 0.1f);
            if (EditorGUI.EndChangeCheck())
            {
                float localX = wTransform.InverseTransformPoint(newRightEdge).x;
                // Calculate real world width by multiplying back the local delta by transform scale
                float newWidth = Mathf.Max(0.1f, Mathf.Abs(localX - localCenterPos.x) * 2f * scaleX);
                
                if (activeMode == FeatureMode.Door) doorWidth = newWidth;
                else windowWidth = newWidth;
                
                Repaint();
            }

            // --- HEIGHT HANDLE (Top Side) ---
            Vector3 topEdgeWorld = wTransform.TransformPoint(localCenterPos + Vector3.up * (localBoxSize.y * 0.5f));
            EditorGUI.BeginChangeCheck();
            Vector3 newTopEdge = Handles.Slider(topEdgeWorld, wTransform.up, handleSize, Handles.CubeHandleCap, 0.1f);
            if (EditorGUI.EndChangeCheck())
            {
                float localY = wTransform.InverseTransformPoint(newTopEdge).y;
                // Calculate real world height by multiplying back the local delta by transform scale
                float newHeight = Mathf.Max(0.1f, Mathf.Abs(localY - localCenterPos.y) * 2f * scaleY);
                
                if (activeMode == FeatureMode.Door)
                {
                    float activeStep = includeDoorstep ? doorstepHeight : 0f;
                    doorHeight = Mathf.Max(0.1f, newHeight - activeStep);
                }
                else
                {
                    windowHeight = Mathf.Max(0.1f, newHeight);
                }
                
                Repaint();
            }
        }
        else // Fallback to Move Tool (W)
        {
            EditorGUI.BeginChangeCheck();
            Vector3 newWorldPos = Handles.PositionHandle(targetWorldPos, wTransform.rotation);
            if (EditorGUI.EndChangeCheck())
            {
                Vector3 localDelta = wTransform.InverseTransformPoint(newWorldPos);

                if (activeMode == FeatureMode.Door)
                {
                    doorHorizontalOffset = Mathf.Clamp(localDelta.x, -0.45f, 0.45f);
                }
                else
                {
                    windowHorizontalOffset = Mathf.Clamp(localDelta.x, -0.45f, 0.45f);
                    windowVerticalOffset = Mathf.Clamp(localDelta.y, -0.45f, 0.45f);
                }
                Repaint();
            }
        }
    }

    private void DrawPreviewBox(Transform wallTransform, Vector3 localCenter, Vector3 localSize)
    {
        Vector3 h = localSize * 0.5f;
        Vector3[] localVerts = new Vector3[]
        {
            localCenter + new Vector3(-h.x, -h.y, -h.z), localCenter + new Vector3(h.x, -h.y, -h.z),
            localCenter + new Vector3(h.x, h.y, -h.z),   localCenter + new Vector3(-h.x, h.y, -h.z),
            localCenter + new Vector3(-h.x, -h.y, h.z),  localCenter + new Vector3(h.x, -h.y, h.z),
            localCenter + new Vector3(h.x, h.y, h.z),    localCenter + new Vector3(-h.x, h.y, h.z)
        };

        Vector3[] worldVerts = new Vector3[8];
        for (int i = 0; i < 8; i++)
        {
            worldVerts[i] = wallTransform.TransformPoint(localVerts[i]);
        }

        Color faceColor = new Color(0f, 0.75f, 1f, 0.25f);
        Color wireColor = new Color(0f, 0.85f, 1f, 0.8f);

        Handles.color = faceColor;
        // Draw solid structural faces
        Handles.DrawAAConvexPolygon(worldVerts[0], worldVerts[1], worldVerts[2], worldVerts[3]);
        Handles.DrawAAConvexPolygon(worldVerts[4], worldVerts[5], worldVerts[6], worldVerts[7]);
        Handles.DrawAAConvexPolygon(worldVerts[0], worldVerts[1], worldVerts[5], worldVerts[4]);
        Handles.DrawAAConvexPolygon(worldVerts[2], worldVerts[3], worldVerts[7], worldVerts[6]);
        Handles.DrawAAConvexPolygon(worldVerts[0], worldVerts[3], worldVerts[7], worldVerts[4]);
        Handles.DrawAAConvexPolygon(worldVerts[1], worldVerts[2], worldVerts[6], worldVerts[5]);

        // Draw structural wireframe outlines
        Handles.color = wireColor;
        Handles.DrawLine(worldVerts[0], worldVerts[1]); Handles.DrawLine(worldVerts[1], worldVerts[2]);
        Handles.DrawLine(worldVerts[2], worldVerts[3]); Handles.DrawLine(worldVerts[3], worldVerts[0]);
        Handles.DrawLine(worldVerts[4], worldVerts[5]); Handles.DrawLine(worldVerts[5], worldVerts[6]);
        Handles.DrawLine(worldVerts[6], worldVerts[7]); Handles.DrawLine(worldVerts[7], worldVerts[4]);
        Handles.DrawLine(worldVerts[0], worldVerts[4]); Handles.DrawLine(worldVerts[1], worldVerts[5]);
        Handles.DrawLine(worldVerts[2], worldVerts[6]); Handles.DrawLine(worldVerts[3], worldVerts[7]);
    }
    #region Generation Engine
    private void ConvertSelected()
    {
        GameObject selected = Selection.activeGameObject;
        if (selected == null)
        {
            EditorUtility.DisplayDialog("Error", "Please select a solid layout Cube first.", "OK");
            return;
        }

        Vector3 size = selected.transform.localScale;
        Vector3 center = selected.transform.position;

        if (selected.TryGetComponent<Collider>(out Collider col))
        {
            size = col.bounds.size;
            center = col.bounds.center;
        }

        GameObject roomRoot = new GameObject(selected.name + "_Room");
        roomRoot.transform.position = center;
        Undo.RegisterCreatedObjectUndo(roomRoot, "Generate Procedural Room");

        float hx = size.x / 2f;
        float hy = size.y / 2f;
        float hz = size.z / 2f;
        float t = wallThickness;

        CreateWall("Floor", roomRoot.transform, new Vector3(0, -hy + (t / 2f), 0), new Vector3(size.x, t, size.z), floorMaterial);
        CreateWall("Ceiling", roomRoot.transform, new Vector3(0, hy - (t / 2f), 0), new Vector3(size.x, t, size.z), floorMaterial);
        
        CreateWall("Wall_North", roomRoot.transform, new Vector3(0, 0, hz - (t / 2f)), new Vector3(size.x, size.y - (t * 2f), t), wallMaterial);
        CreateWall("Wall_South", roomRoot.transform, new Vector3(0, 0, -hz + (t / 2f)), new Vector3(size.x, size.y - (t * 2f), t), wallMaterial);
        
        CreateWall("Wall_East", roomRoot.transform, new Vector3(hx - (t / 2f), 0, 0), new Vector3(t, size.y - (t * 2f), size.z - (t * 2f)), wallMaterial);
        CreateWall("Wall_West", roomRoot.transform, new Vector3(-hx + (t / 2f), 0, 0), new Vector3(t, size.y - (t * 2f), size.z - (t * 2f)), wallMaterial);

        Undo.DestroyObjectImmediate(selected);
        Selection.activeGameObject = roomRoot;
    }

    private void CreateWall(string name, Transform parent, Vector3 localPos, Vector3 scale, Material mat)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = name;
        wall.transform.SetParent(parent);
        wall.transform.localPosition = localPos;
        wall.transform.localScale = scale;

        if (name == "Wall_East" || name == "Wall_West")
        {
            wall.transform.localRotation = Quaternion.Euler(0, 90, 0);
            wall.transform.localScale = new Vector3(scale.z, scale.y, scale.x);
        }

        if (mat != null && wall.TryGetComponent<Renderer>(out Renderer rend))
        {
            rend.sharedMaterial = mat;
        }
        Undo.RegisterCreatedObjectUndo(wall, "Create Room Part");
    }
    #endregion

    #region Structural Modification Engine
    private void SpawnDoorInEditor()
    {
        Transform wTransform = targetWall.transform;
        Vector3 origScale = wTransform.localScale;
        Vector3 origLocalPos = wTransform.localPosition;
        string origName = targetWall.name;
        Transform parent = wTransform.parent;
        Material wallMat = targetWall.GetComponent<Renderer>()?.sharedMaterial;

        float wallLength = origScale.x;
        float centerOffset = doorHorizontalOffset * wallLength;
        float halfWidth = doorWidth / 2f;

        float activeStepHeight = includeDoorstep ? doorstepHeight : 0f;
        float totalOpeningHeight = doorHeight + activeStepHeight;

        float leftWidth = (wallLength / 2f) + centerOffset - halfWidth;
        float rightWidth = (wallLength / 2f) - centerOffset - halfWidth;
        float topHeight = origScale.y - totalOpeningHeight;

        if (leftWidth > 0.01f)
            CreateSubWall(origName + "_Left", parent, origLocalPos + wTransform.right * leftCenterOffset(wallLength, leftWidth), new Vector3(leftWidth, origScale.y, origScale.z), wTransform.localRotation, wallMat);

        if (rightWidth > 0.01f)
            CreateSubWall(origName + "_Right", parent, origLocalPos + wTransform.right * rightCenterOffset(wallLength, rightWidth), new Vector3(rightWidth, origScale.y, origScale.z), wTransform.localRotation, wallMat);

        if (topHeight > 0.01f)
        {
            float topCenterY = (origScale.y / 2f) - (topHeight / 2f);
            Vector3 topLocalPos = origLocalPos + wTransform.right * centerOffset + wTransform.up * topCenterY;
            CreateSubWall(origName + "_Top", parent, topLocalPos, new Vector3(doorWidth, topHeight, origScale.z), wTransform.localRotation, wallMat);
        }

        if (includeDoorstep && activeStepHeight > 0.01f)
        {
            float stepCenterY = (-origScale.y / 2f) + (activeStepHeight / 2f);
            Vector3 stepLocalPos = origLocalPos + wTransform.right * centerOffset + wTransform.up * stepCenterY;
            Vector3 stepWorldPos = parent != null ? parent.TransformPoint(stepLocalPos) : stepLocalPos;

            GameObject stepInstance;
            if (doorstepPrefab != null)
            {
                stepInstance = (GameObject)PrefabUtility.InstantiatePrefab(doorstepPrefab);
                stepInstance.transform.position = stepWorldPos;
                stepInstance.transform.rotation = wTransform.rotation;
                stepInstance.transform.SetParent(parent);
            }
            else
            {
                stepInstance = GameObject.CreatePrimitive(PrimitiveType.Cube);
                stepInstance.transform.SetParent(parent);
                stepInstance.transform.position = stepWorldPos;
                stepInstance.transform.rotation = wTransform.rotation;
                stepInstance.transform.localScale = new Vector3(doorWidth, activeStepHeight, origScale.z * 1.3f); 
                if (doorstepMaterial != null && stepInstance.TryGetComponent<Renderer>(out Renderer r)) r.sharedMaterial = doorstepMaterial;
            }
            stepInstance.name = origName + "_Doorstep";
            Undo.RegisterCreatedObjectUndo(stepInstance, "Instantiate Doorstep");
        }

        if (doorPrefab != null)
        {
            float doorCenterY = (-origScale.y / 2f) + activeStepHeight + (doorHeight / 2f);
            Vector3 spawnPos = origLocalPos + wTransform.right * centerOffset + wTransform.up * doorCenterY;
            Vector3 worldPos = parent != null ? parent.TransformPoint(spawnPos) : spawnPos;
            
            GameObject doorInstance = (GameObject)PrefabUtility.InstantiatePrefab(doorPrefab);
            doorInstance.transform.position = worldPos;
            doorInstance.transform.rotation = wTransform.rotation;
            doorInstance.transform.SetParent(parent);
            doorInstance.name = origName + "_DoorFrame";
            Undo.RegisterCreatedObjectUndo(doorInstance, "Instantiate Door");
        }

        Undo.DestroyObjectImmediate(targetWall);
        targetWall = null;
    }

    private void SpawnWindowInEditor()
    {
        Transform wTransform = targetWall.transform;
        Vector3 origScale = wTransform.localScale;
        Vector3 origLocalPos = wTransform.localPosition;
        string origName = targetWall.name;
        Transform parent = wTransform.parent;
        Material wallMat = targetWall.GetComponent<Renderer>()?.sharedMaterial;

        float wallLength = origScale.x;
        float wallHeight = origScale.y;
        
        float xOffset = windowHorizontalOffset * wallLength;
        float yOffset = windowVerticalOffset * wallHeight;

        float leftWidth = (wallLength / 2f) + xOffset - (windowWidth / 2f);
        float rightWidth = (wallLength / 2f) - xOffset - (windowWidth / 2f);
        float bottomHeight = (wallHeight / 2f) + yOffset - (windowHeight / 2f);
        float topHeight = (wallHeight / 2f) - yOffset - (windowHeight / 2f);

        if (leftWidth > 0.01f)
            CreateSubWall(origName + "_Win_Left", parent, origLocalPos + wTransform.right * leftCenterOffset(wallLength, leftWidth), new Vector3(leftWidth, wallHeight, origScale.z), wTransform.localRotation, wallMat);

        if (rightWidth > 0.01f)
            CreateSubWall(origName + "_Win_Right", parent, origLocalPos + wTransform.right * rightCenterOffset(wallLength, rightWidth), new Vector3(rightWidth, wallHeight, origScale.z), wTransform.localRotation, wallMat);

        if (bottomHeight > 0.01f)
        {
            float bottomCenterY = -wallHeight / 2f + (bottomHeight / 2f);
            Vector3 bottomPos = origLocalPos + wTransform.right * xOffset + wTransform.up * bottomCenterY;
            CreateSubWall(origName + "_Win_Bottom", parent, bottomPos, new Vector3(windowWidth, bottomHeight, origScale.z), wTransform.localRotation, wallMat);
        }

        if (topHeight > 0.01f)
        {
            float topCenterY = wallHeight / 2f - (topHeight / 2f);
            Vector3 topPos = origLocalPos + wTransform.right * xOffset + wTransform.up * topCenterY;
            CreateSubWall(origName + "_Win_Top", parent, topPos, new Vector3(windowWidth, topHeight, origScale.z), wTransform.localRotation, wallMat);
        }

        if (windowPrefab != null)
        {
            Vector3 spawnPos = origLocalPos + wTransform.right * xOffset + wTransform.up * yOffset;
            Vector3 worldPos = parent != null ? parent.TransformPoint(spawnPos) : spawnPos;

            GameObject winInstance = (GameObject)PrefabUtility.InstantiatePrefab(windowPrefab);
            winInstance.transform.position = worldPos;
            winInstance.transform.rotation = wTransform.rotation;
            winInstance.transform.SetParent(parent);
            winInstance.name = origName + "_WindowFrame";
            Undo.RegisterCreatedObjectUndo(winInstance, "Instantiate Window");
        }

        Undo.DestroyObjectImmediate(targetWall);
        targetWall = null;
    }

    private void CreateSubWall(string name, Transform parent, Vector3 localPos, Vector3 scale, Quaternion localRot, Material mat)
    {
        GameObject subWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        subWall.name = name;
        subWall.transform.SetParent(parent);
        subWall.transform.localPosition = localPos;
        subWall.transform.localRotation = localRot;
        subWall.transform.localScale = scale;

        if (mat != null && subWall.TryGetComponent<Renderer>(out Renderer rend))
        {
            rend.sharedMaterial = mat;
        }
        Undo.RegisterCreatedObjectUndo(subWall, "Split Wall Assembly");
    }

    private float leftCenterOffset(float wallL, float leftW) => -wallL / 2f + (leftW / 2f);
    private float rightCenterOffset(float wallL, float rightW) => wallL / 2f - (rightW / 2f);
    #endregion
}