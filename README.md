# DisplayXR Unity Test Project

A minimal Unity test project for the [DisplayXR Unity plugin](https://github.com/DisplayXR/displayxr-unity). Use this project to validate the plugin against new releases, test scene setups, and try out the spatial display rendering on a tracked 3D display.

## Requirements

- **Unity 6000.3 LTS** (Unity 6) or newer
- A spatial display supported by [DisplayXR](https://github.com/DisplayXR/displayxr-runtime), or use the built-in `sim_display` driver for development without hardware
- The DisplayXR runtime installed (via the [installer](https://github.com/DisplayXR/displayxr-shell-releases/releases))

## Opening the Project

1. Clone this repo:
   ```bash
   git clone https://github.com/DisplayXR/displayxr-unity-test.git
   ```
2. Open the project in Unity Hub (`File → Open Project`)
3. Unity will fetch dependencies — this may take a few minutes on first open

### URP setup

The project ships with the Universal Render Pipeline package in its manifest.
On first import, `Assets/Editor/URPSetupBootstrap.cs` automatically creates an
XR-friendly URP pipeline asset (`Assets/Settings/URP-Pipeline.asset` with
`UpscalingFilter=Auto`, MSAA off — both required to keep the OpenXR project
validator happy) and assigns it to Project Settings → Graphics + Quality.

If the cube renders magenta on first open, the wood-crate material is still
referencing the Built-in `Standard` shader. Run the URP converter once to
upgrade materials:

1. `Window → Rendering → Render Pipeline Converter`
2. Choose **Built-in to URP**
3. Tick *Material and Material Reference Upgrade*, then *Initialize Converters*
   and *Convert Assets*

4. Open `Assets/CubeTest.unity` to load the test scene.

## Plugin Reference

The project depends on the DisplayXR Unity plugin via Unity Package Manager. The dependency is declared in `Packages/manifest.json`:

```json
"com.displayxr.unity": "https://github.com/DisplayXR/displayxr-unity.git#upm/v1.2.7"
```

To test against a different plugin version, edit the URL fragment (`#upm/v1.2.7`) to point at the desired tag, then run `Window → Package Manager → Refresh`.

To test against a local development build of the plugin, change the dependency to:
```json
"com.displayxr.unity": "file:/absolute/path/to/displayxr-unity"
```

## Test Scenes

| Scene | Description |
|-------|-------------|
| `Assets/CubeTest.unity` | Rotating cube on a tracked 3D display, plus a runtime-built window-space UI panel with IPD / virtual-display-height sliders and a render-mode cycle button. Verifies the basic rendering pipeline AND the `XrCompositionLayerWindowSpaceEXT` overlay path. |

The window-space UI is constructed at runtime by `Assets/Scripts/DisplayXRTuningUI.cs` (programmatic Canvas + sliders + button — no hand-authored UI prefab). Adjust `panelX/panelY/panelWidth/panelHeight` on the `DisplayXR_TuningUI` GameObject to reposition the panel inside the runtime window.

## Running the Project

1. With a spatial display connected: Press Play in the Unity Editor — the scene will render with stereo 3D and head tracking
2. Without hardware: The DisplayXR runtime's `sim_display` driver activates automatically — use WASD + mouse to simulate eye movement
3. To build a standalone player: `File → Build Settings → Build`

## Reporting Issues

For plugin bugs, file issues on the [DisplayXR Unity plugin repo](https://github.com/DisplayXR/displayxr-unity/issues).
For runtime bugs, file issues on the [DisplayXR Shell releases repo](https://github.com/DisplayXR/displayxr-shell-releases/issues).

## License

ISC. See [LICENSE](LICENSE).
