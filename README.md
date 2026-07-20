# SceneBuilderTool
Turn cubes into multi-floor buildings with a Unity editor script. Generate procedural room structures with doors, windows, and vertical circulation shafts.

## Features

- **Multi-Floor Generation**: Stack multiple floors upward, downward, or both
- **Vertical Circulation**: Auto-generate stairwell and lift shafts that punch through all floors
- **Door & Window Openings**: Interactively place and resize doors and windows on walls
- **Ground Plane**: Optional ground plane beneath the building
- **Hierarchy Organization**: Clean, named hierarchy with floor-based organization
- **Undo Support**: Full undo integration for all generation operations

## Installation

1. In your Unity Project window, right-click the `Assets` folder and select **Create > Folder**.
2. Name this new folder exactly **`Editor`**.
3. Copy `RoomBuilderTool.cs` into the `Editor` folder.

## Usage

### 1. Generate a Multi-Floor Building

1. Create a **3D Cube** in your scene (this acts as the base room layout template).
2. Scale the cube to your desired room dimensions.
3. Open **Tools > Room Builder** to open the editor window.
4. Configure generation settings:
   - **Wall Thickness**: Controls thickness of wall/floor geometry
   - **Wall/Floor Material**: Assign materials to walls and floors
   - **Add Ground Plane**: Toggle to create a ground plane
   - **Stack Direction**: Choose how to stack floors:
     - `None`: Single floor only
     - `ExtendUp`: Generate extra floors above
     - `ExtendDown`: Generate extra floors below
     - `ExtendBoth`: Generate extra floors both directions
   - **Extra Floors Count**: Number of additional floors to generate
5. Enable **Auto Vertical Circulation** to generate shafts:
   - **Stairwell Settings**: Position and dimensions of stairwell shaft
   - **Lift Shaft Settings**: Position and dimensions of lift shaft
6. Select your cube in the scene and click **Convert Selected Cube to Room**.

The script will:
- Replace the cube with hollow wall segments
- Generate multiple floor levels stacked at proper heights
- Create hollow shaft frames for stairs and lifts
- Automatically punch holes in floors/ceilings where shafts intersect
- Collapse other building hierarchies for cleaner organization

### 2. Add Doors & Windows

1. In the Room Builder window, select a **Target Wall Piece** from your generated building.
2. Switch between **Door Setup Mode** or **Window Setup Mode**.
3. Configure the opening dimensions:
   - **Width/Height**: Size of the opening
   - **Position Controls**: Adjust horizontal and vertical placement
4. Use scene view handles for real-time editing:
   - Press **`W`** to move the opening along the wall
   - Press **`R`** or **`T`** to scale the opening
5. Click **Punch Doorway** or **Punch Window** to cut the opening into the wall.

### 3. Vertical Circulation Shafts

Shafts are automatically generated when **Auto Vertical Circulation** is enabled:
- Shafts span the full height of the building
- Hollow frame structure is visible in the scene
- Automatically punch through all intermediate floors and ceilings
- Top ceiling of building remains solid
- Shaft dimensions and positions are configurable

## Menu Items

- **Tools/Room Builder**: Main editor window for building generation

## Script Files

- **RoomBuilderEditor**: Main building generation and door/window placement tool
- **RoomBuilder**: Legacy runtime class (no active menu item)
