# ReStory Better Workbench

A quality-of-life mod for the ReStory workbench. It organizes the parts on the table, highlights them,
and makes the notepad parts list easier to read.

## Features

- Press F to pack every loose part into rows against one side of the bench.
- Press G to highlight the parts on the bench, colored by their condition.
- The notepad's parts list groups what still needs doing.
- Click a section header in the notepad to collapse that section's list.
- The game clock stays on screen while you work at the bench.
- Hover a package to see the services you ordered with it, plus which day it is on for email orders.

## Requirements

- MelonLoader, latest, or
- BepInEx 5.4.23.5.

## Installation (MelonLoader)

1. Download `MelonLoader.Installer.exe` from the
   [MelonLoader releases page](https://github.com/LavaGang/MelonLoader/releases/latest) and run it. Click
   **Select** and point it at your `Restory.exe`, leave the latest version selected, then hit **Install**.
   By default the game is at
   `(YourDrive):\Program Files (x86)\Steam\steamapps\common\Restory\Restory.exe`.

2. Launch the game once, then quit. MelonLoader creates its `Mods` and `UserData` folders on first run.

3. Extract this mod's archive and move `ReStoryBetterWorkbench.dll` into `Mods\`, so it ends up at:

   ```
   ...\Restory\Mods\ReStoryBetterWorkbench.dll
   ```

## Installation (BepInEx)

1. Download `BepInEx_win_x64_5.4.23.5.zip` from the
   [BepInEx releases page](https://github.com/BepInEx/BepInEx/releases) and extract it into your ReStory
   folder, the one containing `Restory.exe`. Any newer 5.4.x release works too. Just take the file with
   `win_x64` in its name.

   Check it extracted correctly: `BepInEx\core\BepInEx.dll` should now exist. By default that is
   `(YourDrive):\Program Files (x86)\Steam\steamapps\common\Restory\BepInEx\core`.

2. Launch the game once from Steam, then quit. BepInEx creates its `plugins` and `config` folders on first
   run.

3. Extract this mod's archive and move the `ReStoryBetterWorkbench` folder into `BepInEx\plugins\`, so the
   DLL ends up at:

   ```
   ...\Restory\BepInEx\plugins\ReStoryBetterWorkbench\ReStoryBetterWorkbench.dll
   ```

That's all. To check the mod loaded, start the game and open `...\BepInEx\LogOutput.log`. It should
contain:

   ```
   [Info   :ReStory Better Workbench] Loaded. Self-check passed.
   ```

## Controls

| Key           | Action                                                          |
|---------------|-----------------------------------------------------------------|
| **F**         | Packs the loose parts on the bench.                             |
| **Shift + F** | Packs the loose parts against the opposite side.                |
| **G**         | Toggles the outline highlight on every part lying on the bench. |

## Configuration

BepInEx writes `...\BepInEx\config\com.archives.restorybetterworkbench.cfg` the first time you run the
game with the mod installed. **Edit it while the game is closed.** The mod reads it once at startup.

On MelonLoader the same settings live in `...\UserData\MelonPreferences.cfg`, shared with your other mods,
under `[ReStoryBetterWorkbenchGeneral]`, `[ReStoryBetterWorkbenchHotkeys]` and
`[ReStoryBetterWorkbenchLayout]` instead of the three sections below.

### `[General]`

| Setting       | Default | Description                                            |
|---------------|---------|--------------------------------------------------------|
| `UpdateCheck` | `true`  | Checks once at startup whether a newer release exists. |

The check reads the latest release tag from `api.github.com`. Set it to `false` and the mod never touches
the network.

### `[Hotkeys]`

Both take any Unity KeyCode name, e.g. `F`, `G`, `R`, `Tab`, `F6`.

| Setting         | Default | Description                                                                       |
|-----------------|---------|-----------------------------------------------------------------------------------|
| `OrganizeKey`   | `F`     | Packs the loose parts on the bench. Hold Shift to pack against the opposite side. |
| `HighlightsKey` | `G`     | Toggles the outline highlight on every part lying on the bench.                   |

### `[Layout]`

| Setting                  | Default | Range           | Description                                                                                                      |
|--------------------------|---------|-----------------|------------------------------------------------------------------------------------------------------------------|
| `PackAgainstSide`        | `Left`  | `Left`, `Right` | Bench side the parts are packed against.                                                                         |
| `CellGap`                | `0.025` | 0 to 0.1        | Gap between packed parts, in meters.                                                                             |
| `SafetyMargin`           | `0.005` | 0 to 0.05       | Extra collision padding safety around each part spot, in meters.                                                 |
| `ShelfSlack`             | `1`     | 1 to 2.5        | How loosely the packed block spreads over the bench.                                                             |
| `ControlsDisplayOffset`  | `0.07`  | 0 to 0.4        | How far the controls UI display position is adjusted when switching sides.                                       |
| `ClockBackgroundOpacity` | `0.75`  | 0 to 1          | How solid the workbench clock background panel is. 0 hides it, 1 makes it fully opaque.                          |
| `TopMarginLeft`          | `0.05`  | 0 to 0.4        | Margin kept clear along the top edge of the bench. Applies when packing against the Left side.                   |
| `TopMarginRight`         | `0.10`  | 0 to 0.4        | Margin kept clear along the top edge of the bench. Applies when packing against the Right side.                  |
| `SideMarginLeft`         | `0.05`  | 0 to 0.4        | Margin kept clear along both the left and right edges of the bench. Applies when packing against the Left side.  |
| `SideMarginRight`        | `0.00`  | 0 to 0.4        | Margin kept clear along both the left and right edges of the bench. Applies when packing against the Right side. |

Every description above is the same line the mod writes into your config file, so the two never disagree.

## Building from source

Requires the .NET SDK. The project references the game's own assemblies from `Restory_Data\Managed`, and
the BepInEx build also references `BepInEx.dll` and `0Harmony.dll` from the game's `BepInEx\core`. If your
game is not at `C:\Program Files (x86)\Steam\steamapps\common\Restory`, create
`ReStoryBetterWorkbench.local.props` next to the csproj (git ignores it):

```xml
<Project>
  <PropertyGroup>
    <GameDir>D:\Program Files (x86)\Steam\steamapps\common\Restory</GameDir>
  </PropertyGroup>
</Project>
```

```
dotnet build                            # Debug   - verbose per-action logging
dotnet build -c Release                 # Release - development logging compiled out
dotnet build -c Release -p:Loader=Melon # the MelonLoader build of the same source
```

The BepInEx build lands in `BepInEx\plugins\ReStoryBetterWorkbench\`, the MelonLoader one in `Mods\`. The
BepInEx build needs BepInEx installed in the game. The MelonLoader build gets MelonLoader and Harmony from
NuGet, so it needs neither loader installed.

Every compiled file lives in `src\`. The SDK's default glob finds them, so adding a file needs no change
to `ReStoryBetterWorkbench.csproj`. The key frame art stays in `Assets\`, where the csproj embeds it by
name.

Loader-specific code lives in `src\Log.cs` and the two `src\BetterWorkbenchPlugin.*.cs` partials, picked
by the `MELONLOADER` symbol. **Build both before releasing.** Only the switched-on one gets compiled.
