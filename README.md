# SceneBuilderTool
Turn cubes into multi-floor buildings with a Unity editor script. Generate procedural room structures with doors, windows, and vertical circulation shafts.

## Features

- **Multi-Floor Generation**: Stack multiple floors upward, downward, or both
- **Vertical Circulation**: Auto-generate stairwell and lift shafts that punch through all floors
- **Door & Window Openings**: Interactively place and resize doors and windows on walls
- **Ground Plane**: Optional ground plane beneath the building
- **Hierarchy Organization**: Clean, named hierarchy with floor-based organization
- **Undo Support**: Full undo integration for all generation operations
- **Player Controller**: First-person player with movement, jumping, jetpack, crouch, and noclip modes
- **Auto-Tagging**: All floors/ceilings tagged as "Ground" for player ground detection

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
- **PlayerController**: First-person player character controller with movement and flight
- **PlayerCamera**: First-person camera with mouse look
- **AddPlayer**: Editor script for quickly adding a player to the scene

## Player Controller

The Room Builder includes a fully functional first-person player controller. Use it to explore your generated building.

### Adding a Player

1. Go to **Tools > Add Player to Scene** to automatically create a player in your scene
2. Press **Play** to start the game
3. The player spawns with a first-person camera and physics-based movement

### Player Controls

- **WASD**: Move forward/backward/strafe left/right (relative to where camera is looking)
- **Mouse**: Look around (move mouse to rotate view)
- **Space**: Jump (when on ground) / Hold Space: Activate jetpack (fly upward while airborne)
- **Left Ctrl**: Crouch (reduces player height and slows movement)
- **P**: Toggle noclip mode (fly through walls freely, no gravity)
- **Escape**: Unlock and show mouse cursor

### Player Features

- **Ground Detection**: Uses raycast to reliably detect when player is on ground floors/ceilings
- **Jumping**: Jump when grounded, with clamped fall speed to prevent excessive velocity
- **Jetpack Flight**: Hold spacebar while airborne to fly upward, allows exploration of higher areas
- **Crouch**: Reduce profile and movement speed for navigating tight spaces
- **Noclip Mode**: Free flight through all geometry, useful for exploring the entire building structure
