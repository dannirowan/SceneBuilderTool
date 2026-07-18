# SceneBuilderTool
Turn cubes into rooms with a Unity editor script.

### 1. Project Setup

1. In your Unity Project window, right-click the `Assets` folder and select **Create > Folder**.
2. Name this new folder exactly **`Editor`**.
3. Drop this C# script directly into that `Editor` folder so Unity can run its custom inspector drawing logic.

### 2. Create the Room

1. Right-click in the Hierarchy and select **3D Object > Cube**.
2. Scale the cube to match the overall dimensions you want for your room.
3. Click **Convert to Room** in the inspector. The script will swap the solid cube out for individual, hollowed wall segments.

### 3. Set Up the Openings

1. Select the wall segment you want to modify.
2. Choose either **Door** or **Window** mode from the inspector panel.
3. A blue box preview will appear on the wall to show where the opening will go.

### 4. Adjust and Cut

* **To Move:** Press **`W`** and drag the center handle to slide the preview along the wall face.
* **To Resize:** Press **`R`** or **`T`** to bring up edge handles. Pull the side or top boxes to change the width and height instantly.
* **To Cut:** Click **Apply Cut** in the inspector to punch the hole out of the wall geometry permanently.
