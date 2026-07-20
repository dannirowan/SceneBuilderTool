using UnityEngine;
using UnityEditor;

public class RoomBuilderEditor : EditorWindow
{
    // Generation Settings
    private float wallThickness = 0.05f;
    private Material wallMaterial;
    private Material floorMaterial;

    // Multi-Floor & Ground Settings
    private bool addGround = true;
    private Material groundMaterial;
    private float groundSize = 50f;

    // Stacking Options
    private enum StackDirection { None, ExtendUp, ExtendDown, ExtendBoth }
    private StackDirection stackDirection = StackDirection.None;
    private int extraFloors = 1;

    // Vertical Circulation Settings
    private bool autoGenerateCirculation = true;
    private GameObject stairwellPrefab;
    private Vector3 stairwellPosition = new Vector3(-3f, 0f, 0f);
    private Vector2 stairwellDimensions = new Vector2(3f, 5f);

    private GameObject liftShaftPrefab;
    private Vector3 liftShaftPosition = new Vector3(3f, 0f, 0f);
    private Vector2 liftShaftDimensions = new Vector2(2f, 2f);

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

    [MenuItem("Tools/Room Builder")]
    public static void ShowWindow()
    {
        GetWindow<RoomBuilderEditor>("Room Builder");
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
        // --- SECTION 1: GENERATION & MULTI-FLOOR ---
        GUILayout.Label("1. Generation & Layout Setup", EditorStyles.boldLabel);
        wallThickness = EditorGUILayout.FloatField("Wall Thickness", wallThickness);
        wallMaterial = (Material)EditorGUILayout.ObjectField("Wall Material", wallMaterial, typeof(Material), false);
        floorMaterial = (Material)EditorGUILayout.ObjectField("Floor/Ceiling Material", floorMaterial, typeof(Material), false);

        EditorGUILayout.Space(5);
        addGround = EditorGUILayout.Toggle("Add Ground Plane", addGround);
        if (addGround)
        {
            EditorGUI.indentLevel++;
            groundSize = EditorGUILayout.FloatField("Ground Size", groundSize);
            groundMaterial = (Material)EditorGUILayout.ObjectField("Ground Material", groundMaterial, typeof(Material), false);
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space(5);
        GUILayout.Label("Floor Stacking", EditorStyles.miniBoldLabel);
        EditorGUILayout.HelpBox("All floors match the selected cube's exact scale (X/Y/Z).", MessageType.None);
        stackDirection = (StackDirection)EditorGUILayout.EnumPopup("Stack Direction", stackDirection);
        if (stackDirection != StackDirection.None)
        {
            EditorGUI.indentLevel++;
            extraFloors = Mathf.Max(1, EditorGUILayout.IntField("Extra Floors Count", extraFloors));
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space(5);
        autoGenerateCirculation = EditorGUILayout.Toggle("Auto Vertical Circulation", autoGenerateCirculation);
        if (autoGenerateCirculation)
        {
            EditorGUI.indentLevel++;
            GUILayout.Label("Stairwell Settings", EditorStyles.miniBoldLabel);
            stairwellPrefab = (GameObject)EditorGUILayout.ObjectField("Stairwell Prefab", stairwellPrefab, typeof(GameObject), false);
            stairwellPosition = EditorGUILayout.Vector3Field("Stairwell Rel Pos", stairwellPosition);
            stairwellDimensions = EditorGUILayout.Vector2Field("Stairwell Size (W, D)", stairwellDimensions);

            GUILayout.Label("Lift Shaft Settings", EditorStyles.miniBoldLabel);
            liftShaftPrefab = (GameObject)EditorGUILayout.ObjectField("Lift Shaft Prefab", liftShaftPrefab, typeof(GameObject), false);
            liftShaftPosition = EditorGUILayout.Vector3Field("Lift Shaft Rel Pos", liftShaftPosition);
            liftShaftDimensions = EditorGUILayout.Vector2Field("Lift Size (W, D)", liftShaftDimensions);
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space(5);
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
        // Visualise Circulation Shafts preview on current selected cube prior to conversion
        if (Selection.activeGameObject != null && targetWall == null && autoGenerateCirculation)
        {
            DrawCirculationVisualisation(Selection.activeGameObject);
        }

        if (targetWall == null) return;

        Transform wTransform = targetWall.transform;
        Vector3 origScale = wTransform.localScale;

        float scaleX = Mathf.Max(0.001f, origScale.x);
        float scaleY = Mathf.Max(0.001f, origScale.y);

        float currentWidth, currentHeight;
        Vector3 localCenterPos;

        if (activeMode == FeatureMode.Door)
        {
            currentWidth = doorWidth;
            float activeStepHeight = includeDoorstep ? doorstepHeight : 0f;
            currentHeight = doorHeight + activeStepHeight;

            float centerOffset = doorHorizontalOffset * scaleX;
            float centerY = (-scaleY / 2f) + (currentHeight / 2f);
            
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

        Vector3 targetWorldPos = wTransform.TransformPoint(localCenterPos);

        Vector3 localBoxSize = new Vector3(
            currentWidth / scaleX, 
            currentHeight / scaleY, 
            1.2f
        );

        DrawPreviewBox(wTransform, localCenterPos, localBoxSize);

        Handles.Label(targetWorldPos + wTransform.up * (currentHeight * 0.6f), 
            $"{activeMode} Editor Mode\n[W] Move | [R/T] Scale", EditorStyles.boldLabel);

        if (Tools.current == Tool.Scale || Tools.current == Tool.Rect)
        {
            float handleSize = HandleUtility.GetHandleSize(targetWorldPos) * 0.15f;
            
            Vector3 rightEdgeWorld = wTransform.TransformPoint(localCenterPos + Vector3.right * (localBoxSize.x * 0.5f));
            EditorGUI.BeginChangeCheck();
            Vector3 newRightEdge = Handles.Slider(rightEdgeWorld, wTransform.right, handleSize, Handles.CubeHandleCap, 0.1f);
            if (EditorGUI.EndChangeCheck())
            {
                float localX = wTransform.InverseTransformPoint(newRightEdge).x;
                float newWidth = Mathf.Max(0.1f, Mathf.Abs(localX - localCenterPos.x) * 2f * scaleX);
                
                if (activeMode == FeatureMode.Door) doorWidth = newWidth;
                else windowWidth = newWidth;
                
                Repaint();
            }

            Vector3 topEdgeWorld = wTransform.TransformPoint(localCenterPos + Vector3.up * (localBoxSize.y * 0.5f));
            EditorGUI.BeginChangeCheck();
            Vector3 newTopEdge = Handles.Slider(topEdgeWorld, wTransform.up, handleSize, Handles.CubeHandleCap, 0.1f);
            if (EditorGUI.EndChangeCheck())
            {
                float localY = wTransform.InverseTransformPoint(newTopEdge).y;
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
        else
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

    private void DrawCirculationVisualisation(GameObject selectedObj)
    {
        Vector3 center = selectedObj.transform.position;
        Vector3 size = selectedObj.transform.localScale;

        if (selectedObj.TryGetComponent<Collider>(out Collider col))
        {
            center = col.bounds.center;
            size = col.bounds.size;
        }

        // Draw Stairwell visualizer
        Vector3 stairCenter = center + stairwellPosition;
        Vector3 stairSize = new Vector3(stairwellDimensions.x, size.y, stairwellDimensions.y);
        Handles.color = new Color(0f, 1f, 0.3f, 0.3f);
        Handles.DrawWireCube(stairCenter, stairSize);
        Handles.Label(stairCenter, "Stairwell Shaft", EditorStyles.boldLabel);

        // Draw Lift visualizer
        Vector3 liftCenter = center + liftShaftPosition;
        Vector3 liftSize = new Vector3(liftShaftDimensions.x, size.y, liftShaftDimensions.y);
        Handles.color = new Color(1f, 0.5f, 0f, 0.3f);
        Handles.DrawWireCube(liftCenter, liftSize);
        Handles.Label(liftCenter, "Lift Shaft", EditorStyles.boldLabel);
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
        Handles.DrawAAConvexPolygon(worldVerts[0], worldVerts[1], worldVerts[2], worldVerts[3]);
        Handles.DrawAAConvexPolygon(worldVerts[4], worldVerts[5], worldVerts[6], worldVerts[7]);
        Handles.DrawAAConvexPolygon(worldVerts[0], worldVerts[1], worldVerts[5], worldVerts[4]);
        Handles.DrawAAConvexPolygon(worldVerts[2], worldVerts[3], worldVerts[7], worldVerts[6]);
        Handles.DrawAAConvexPolygon(worldVerts[0], worldVerts[3], worldVerts[7], worldVerts[4]);
        Handles.DrawAAConvexPolygon(worldVerts[1], worldVerts[2], worldVerts[6], worldVerts[5]);

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

        Vector3 roomSize = size;
        float roomHeight = size.y;
        Vector3 baseRoomCenter = center;

        GameObject buildingRoot = new GameObject(selected.name + "_Building");
        buildingRoot.transform.position = center;
        Undo.RegisterCreatedObjectUndo(buildingRoot, "Generate Procedural Room Structure");

        // Place ground plane beneath the base floor using the original selection bottom
        if (addGround)
        {
            float bottomY = center.y - (size.y / 2f);
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground_Plane";
            ground.transform.SetParent(buildingRoot.transform);
            ground.transform.position = new Vector3(center.x, bottomY, center.z);
            ground.transform.localScale = new Vector3(groundSize / 10f, 1f, groundSize / 10f);
            if (groundMaterial != null && ground.TryGetComponent<Renderer>(out Renderer gRend))
            {
                gRend.sharedMaterial = groundMaterial;
            }
            Undo.RegisterCreatedObjectUndo(ground, "Create Ground Plane");
        }

        // Calculate stacked floor offsets
        int startFloorIndex = 0;
        int endFloorIndex = 0;

        if (stackDirection == StackDirection.ExtendUp) endFloorIndex = extraFloors;
        else if (stackDirection == StackDirection.ExtendDown) startFloorIndex = -extraFloors;
        else if (stackDirection == StackDirection.ExtendBoth)
        {
            startFloorIndex = -extraFloors;
            endFloorIndex = extraFloors;
        }

        for (int f = startFloorIndex; f <= endFloorIndex; f++)
        {
            string floorLabel = $"Floor_{f}";
            Vector3 floorCenter = baseRoomCenter + new Vector3(0f, f * roomHeight, 0f);
            GameObject roomRoot = new GameObject(floorLabel);
            Undo.RegisterCreatedObjectUndo(roomRoot, "Create Floor");
            roomRoot.transform.SetParent(buildingRoot.transform);
            roomRoot.transform.position = floorCenter;

            GenerateSingleRoom(roomRoot.transform, roomSize, floorLabel, baseRoomCenter, startFloorIndex, endFloorIndex, f);
        }

        // Generate total-height circulation shafts
        if (autoGenerateCirculation)
        {
            float totalHeight = (endFloorIndex - startFloorIndex + 1) * roomHeight;
            GenerateCirculationShafts(buildingRoot.transform, totalHeight, baseRoomCenter);
        }

        Undo.DestroyObjectImmediate(selected);

        // Collapse all other building roots so only the new one is visible in the hierarchy
        var hierarchyType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.SceneHierarchyWindow");
        var hierarchyWindow = EditorWindow.GetWindow(hierarchyType);
        var setExpanded = hierarchyType?.GetMethod("SetExpandedRecursive",
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        foreach (GameObject go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            if (go != buildingRoot && go.transform.parent == null && go.name.EndsWith("_Building"))
            {
                setExpanded?.Invoke(hierarchyWindow, new object[] { go.GetInstanceID(), false });
            }
        }

        Selection.activeGameObject = buildingRoot;
    }

    private void GenerateSingleRoom(Transform roomRoot, Vector3 size, string floorLabel = "Floor_0", Vector3 buildingCenter = default, int startFloor = 0, int endFloor = 0, int currentFloor = 0)
    {
        float hx = size.x / 2f;
        float hy = size.y / 2f;
        float hz = size.z / 2f;
        float t = wallThickness;

        // Create floor with punch-throughs for shafts
        CreateFloorWithShaftOpenings(roomRoot, $"{floorLabel}_Floor", new Vector3(0, -hy + (t / 2f), 0), new Vector3(size.x, t, size.z), floorMaterial, hx, hz);
        
        // Create ceiling - punch through if not the top floor of building
        bool isTopFloor = (currentFloor == endFloor);
        if (isTopFloor)
        {
            CreateWall($"{floorLabel}_Ceiling", roomRoot, new Vector3(0, hy - (t / 2f), 0), new Vector3(size.x, t, size.z), floorMaterial);
        }
        else
        {
            CreateFloorWithShaftOpenings(roomRoot, $"{floorLabel}_Ceiling", new Vector3(0, hy - (t / 2f), 0), new Vector3(size.x, t, size.z), floorMaterial, hx, hz);
        }
        
        // Create north/south walls with punch-throughs for circulation shafts
        CreateWallWithShaftOpenings($"{floorLabel}_Wall_North", roomRoot, 
            new Vector3(0, 0, hz - (t / 2f)), new Vector3(size.x, size.y - (t * 2f), t), wallMaterial,
            buildingCenter, currentFloor, startFloor, endFloor, size.y, true);
            
        CreateWallWithShaftOpenings($"{floorLabel}_Wall_South", roomRoot, 
            new Vector3(0, 0, -hz + (t / 2f)), new Vector3(size.x, size.y - (t * 2f), t), wallMaterial,
            buildingCenter, currentFloor, startFloor, endFloor, size.y, true);
        
        // Create east/west walls with punch-throughs
        CreateWallWithShaftOpenings($"{floorLabel}_Wall_East", roomRoot, 
            new Vector3(hx - (t / 2f), 0, 0), new Vector3(t, size.y - (t * 2f), size.z - (t * 2f)), wallMaterial,
            buildingCenter, currentFloor, startFloor, endFloor, size.y, false);
            
        CreateWallWithShaftOpenings($"{floorLabel}_Wall_West", roomRoot, 
            new Vector3(-hx + (t / 2f), 0, 0), new Vector3(t, size.y - (t * 2f), size.z - (t * 2f)), wallMaterial,
            buildingCenter, currentFloor, startFloor, endFloor, size.y, false);
    }

    private void CreateFloorWithShaftOpenings(Transform parent, string name, Vector3 localPos, Vector3 floorScale, Material mat, float roomHalfX, float roomHalfZ)
    {
        if (!autoGenerateCirculation)
        {
            CreateWall(name, parent, localPos, floorScale, mat);
            return;
        }

        // Check if stairwell or lift shafts intersect this floor in XZ plane
        Vector3 stairwellXZ = new Vector3(stairwellPosition.x, 0, stairwellPosition.z);
        Vector3 liftShaftXZ = new Vector3(liftShaftPosition.x, 0, liftShaftPosition.z);
        
        float stairHalfW = stairwellDimensions.x / 2f;
        float stairHalfD = stairwellDimensions.y / 2f;
        float liftHalfW = liftShaftDimensions.x / 2f;
        float liftHalfD = liftShaftDimensions.y / 2f;

        bool stairIntersects = IsShaftIntersectingFloor(stairwellXZ, stairHalfW, stairHalfD, roomHalfX, roomHalfZ);
        bool liftIntersects = IsShaftIntersectingFloor(liftShaftXZ, liftHalfW, liftHalfD, roomHalfX, roomHalfZ);

        if (!stairIntersects && !liftIntersects)
        {
            // No shafts intersect - create full floor
            CreateWall(name, parent, localPos, floorScale, mat);
        }
        else
        {
            // Create floor pieces around shaft openings
            CreateFloorSegmentsAroundShafts(parent, name, localPos, floorScale, mat, 
                stairwellXZ, stairHalfW, stairHalfD, stairIntersects,
                liftShaftXZ, liftHalfW, liftHalfD, liftIntersects,
                roomHalfX, roomHalfZ);
        }
    }

    private bool IsShaftIntersectingFloor(Vector3 shaftCenter, float shaftHalfW, float shaftHalfD, float roomHalfX, float roomHalfZ)
    {
        // Check AABB intersection between shaft footprint and floor footprint
        float shaftMinX = shaftCenter.x - shaftHalfW;
        float shaftMaxX = shaftCenter.x + shaftHalfW;
        float shaftMinZ = shaftCenter.z - shaftHalfD;
        float shaftMaxZ = shaftCenter.z + shaftHalfD;

        float floorMinX = -roomHalfX;
        float floorMaxX = roomHalfX;
        float floorMinZ = -roomHalfZ;
        float floorMaxZ = roomHalfZ;

        return !(shaftMaxX < floorMinX || shaftMinX > floorMaxX || shaftMaxZ < floorMinZ || shaftMinZ > floorMaxZ);
    }

    private void CreateFloorSegmentsAroundShafts(Transform parent, string baseName, Vector3 baseLocalPos, Vector3 baseScale, Material mat,
        Vector3 stairPos, float stairHW, float stairHD, bool stairIntersects,
        Vector3 liftPos, float liftHW, float liftHD, bool liftIntersects,
        float roomHalfX, float roomHalfZ)
    {
        float t = wallThickness;
        float floorThickness = baseScale.y;
        float roomWidth = roomHalfX * 2f;
        float roomDepth = roomHalfZ * 2f;

        // Determine combined opening bounds
        float minX = roomHalfX, maxX = -roomHalfX, minZ = roomHalfZ, maxZ = -roomHalfZ;
        bool hasOpening = false;

        if (stairIntersects)
        {
            minX = Mathf.Min(minX, stairPos.x - stairHW);
            maxX = Mathf.Max(maxX, stairPos.x + stairHW);
            minZ = Mathf.Min(minZ, stairPos.z - stairHD);
            maxZ = Mathf.Max(maxZ, stairPos.z + stairHD);
            hasOpening = true;
        }
        if (liftIntersects)
        {
            minX = Mathf.Min(minX, liftPos.x - liftHW);
            maxX = Mathf.Max(maxX, liftPos.x + liftHW);
            minZ = Mathf.Min(minZ, liftPos.z - liftHD);
            maxZ = Mathf.Max(maxZ, liftPos.z + liftHD);
            hasOpening = true;
        }

        if (!hasOpening) { CreateWall(baseName, parent, baseLocalPos, baseScale, mat); return; }

        // Clamp to room bounds
        minX = Mathf.Max(minX, -roomHalfX);
        maxX = Mathf.Min(maxX, roomHalfX);
        minZ = Mathf.Max(minZ, -roomHalfZ);
        maxZ = Mathf.Min(maxZ, roomHalfZ);

        // Create 4 floor segments around the opening
        float openingWidth = maxX - minX;
        float openingDepth = maxZ - minZ;

        // North segment (above opening)
        if (maxZ < roomHalfZ)
        {
            float segW = roomWidth;
            float segD = roomHalfZ - maxZ;
            Vector3 segPos = new Vector3(0, baseLocalPos.y, maxZ + segD / 2f);
            Vector3 segScale = new Vector3(segW, floorThickness, segD);
            CreateWall($"{baseName}_North", parent, segPos, segScale, mat);
        }

        // South segment (below opening)
        if (minZ > -roomHalfZ)
        {
            float segW = roomWidth;
            float segD = roomHalfZ + minZ;
            Vector3 segPos = new Vector3(0, baseLocalPos.y, minZ - segD / 2f);
            Vector3 segScale = new Vector3(segW, floorThickness, segD);
            CreateWall($"{baseName}_South", parent, segPos, segScale, mat);
        }

        // East segment (right of opening, between north and south)
        if (maxX < roomHalfX)
        {
            float segW = roomHalfX - maxX;
            float segD = openingDepth;
            Vector3 segPos = new Vector3(maxX + segW / 2f, baseLocalPos.y, (minZ + maxZ) / 2f);
            Vector3 segScale = new Vector3(segW, floorThickness, segD);
            CreateWall($"{baseName}_East", parent, segPos, segScale, mat);
        }

        // West segment (left of opening, between north and south)
        if (minX > -roomHalfX)
        {
            float segW = roomHalfX + minX;
            float segD = openingDepth;
            Vector3 segPos = new Vector3(minX - segW / 2f, baseLocalPos.y, (minZ + maxZ) / 2f);
            Vector3 segScale = new Vector3(segW, floorThickness, segD);
            CreateWall($"{baseName}_West", parent, segPos, segScale, mat);
        }
    }

    private void CreateWallWithShaftOpenings(string name, Transform parent, Vector3 localPos, Vector3 scale, Material mat, 
        Vector3 buildingCenter, int currentFloor, int startFloor, int endFloor, float floorHeight, bool isNorthSouth)
    {
        // Only punch through floors if circulation shafts are enabled and we have valid dimensions
        if (!autoGenerateCirculation)
        {
            CreateWall(name, parent, localPos, scale, mat);
            return;
        }

        // For now, create the full wall - floor punch-throughs will be handled by the shaft visualization
        // The hollow shaft frame extends through floors visually
        CreateWall(name, parent, localPos, scale, mat);
    }

    private void GenerateCirculationShafts(Transform parent, float height, Vector3 buildingCenter)
    {
        GameObject circContainer = new GameObject("Vertical_Circulation");
        Undo.RegisterCreatedObjectUndo(circContainer, "Create Vertical Circulation");
        circContainer.transform.SetParent(parent, false);

        // Create stairwell hollow shaft
        if (stairwellPrefab != null)
        {
            GameObject stairs = PrefabUtility.InstantiatePrefab(stairwellPrefab) as GameObject;
            stairs.name = "Stairwell_Shaft";
            stairs.transform.SetParent(circContainer.transform, false);
            stairs.transform.localPosition = stairwellPosition;
            stairs.transform.localScale = new Vector3(stairwellDimensions.x, height, stairwellDimensions.y);
            Undo.RegisterCreatedObjectUndo(stairs, "Create Stairwell");
        }
        else
        {
            CreateHollowShaft("Stairwell_Shaft", circContainer.transform, stairwellPosition, 
                stairwellDimensions.x, stairwellDimensions.y, height, new Color(0f, 1f, 0.3f, 0.3f));
        }

        // Create lift shaft hollow shaft
        if (liftShaftPrefab != null)
        {
            GameObject lift = PrefabUtility.InstantiatePrefab(liftShaftPrefab) as GameObject;
            lift.name = "Lift_Shaft";
            lift.transform.SetParent(circContainer.transform, false);
            lift.transform.localPosition = liftShaftPosition;
            lift.transform.localScale = new Vector3(liftShaftDimensions.x, height, liftShaftDimensions.y);
            Undo.RegisterCreatedObjectUndo(lift, "Create Lift Shaft");
        }
        else
        {
            CreateHollowShaft("Lift_Shaft", circContainer.transform, liftShaftPosition, 
                liftShaftDimensions.x, liftShaftDimensions.y, height, new Color(1f, 0.5f, 0f, 0.3f));
        }

        Undo.RegisterCreatedObjectUndo(circContainer, "Create Vertical Circulation");
    }

    private void CreateHollowShaft(string name, Transform parent, Vector3 localPos, float width, float depth, float height, Color frameColor)
    {
        GameObject shaftRoot = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(shaftRoot, $"Create {name}");
        shaftRoot.transform.SetParent(parent);
        shaftRoot.transform.localPosition = localPos;

        float hw = width / 2f;
        float hd = depth / 2f;
        float t = wallThickness; // Thin frame thickness

        // Create 4 vertical corner pillars
        CreateFramePiece($"{name}_Corner_NW", shaftRoot.transform, new Vector3(-hw, 0, hd), new Vector3(t, height, t), frameColor);
        CreateFramePiece($"{name}_Corner_NE", shaftRoot.transform, new Vector3(hw, 0, hd), new Vector3(t, height, t), frameColor);
        CreateFramePiece($"{name}_Corner_SW", shaftRoot.transform, new Vector3(-hw, 0, -hd), new Vector3(t, height, t), frameColor);
        CreateFramePiece($"{name}_Corner_SE", shaftRoot.transform, new Vector3(hw, 0, -hd), new Vector3(t, height, t), frameColor);

        // Create horizontal frame edges at top and bottom
        CreateFramePiece($"{name}_Edge_Top_N", shaftRoot.transform, new Vector3(0, height/2 - t/2, hd), new Vector3(width, t, t), frameColor);
        CreateFramePiece($"{name}_Edge_Top_S", shaftRoot.transform, new Vector3(0, height/2 - t/2, -hd), new Vector3(width, t, t), frameColor);
        CreateFramePiece($"{name}_Edge_Top_W", shaftRoot.transform, new Vector3(-hw, height/2 - t/2, 0), new Vector3(t, t, depth), frameColor);
        CreateFramePiece($"{name}_Edge_Top_E", shaftRoot.transform, new Vector3(hw, height/2 - t/2, 0), new Vector3(t, t, depth), frameColor);

        CreateFramePiece($"{name}_Edge_Bot_N", shaftRoot.transform, new Vector3(0, -height/2 + t/2, hd), new Vector3(width, t, t), frameColor);
        CreateFramePiece($"{name}_Edge_Bot_S", shaftRoot.transform, new Vector3(0, -height/2 + t/2, -hd), new Vector3(width, t, t), frameColor);
        CreateFramePiece($"{name}_Edge_Bot_W", shaftRoot.transform, new Vector3(-hw, -height/2 + t/2, 0), new Vector3(t, t, depth), frameColor);
        CreateFramePiece($"{name}_Edge_Bot_E", shaftRoot.transform, new Vector3(hw, -height/2 + t/2, 0), new Vector3(t, t, depth), frameColor);
    }

    private void CreateFramePiece(string name, Transform parent, Vector3 localPos, Vector3 scale, Color color)
    {
        GameObject piece = GameObject.CreatePrimitive(PrimitiveType.Cube);
        piece.name = name;
        piece.transform.SetParent(parent);
        piece.transform.localPosition = localPos;
        piece.transform.localScale = scale;

        // Remove collider - frame is visual only
        if (piece.TryGetComponent<Collider>(out Collider col))
        {
            Object.DestroyImmediate(col);
        }

        if (piece.TryGetComponent<Renderer>(out Renderer rend))
        {
            Material frameMat = new Material(Shader.Find("Standard"));
            frameMat.color = color;
            rend.sharedMaterial = frameMat;
        }

        Undo.RegisterCreatedObjectUndo(piece, $"Create frame {name}");
    }

    private void CreateWall(string name, Transform parent, Vector3 localPos, Vector3 scale, Material mat)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = name;
        wall.transform.SetParent(parent);
        wall.transform.localPosition = localPos;
        wall.transform.localScale = scale;

        if (name.EndsWith("Wall_East") || name.EndsWith("Wall_West"))
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