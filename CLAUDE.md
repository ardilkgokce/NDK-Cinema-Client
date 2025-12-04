# NDK Cinema Client

VR Cinema application for PICO XR devices built with Unity.

## Tech Stack

- **Engine:** Unity 2020.3.48f1 LTS
- **Language:** C#
- **Platform:** PICO VR Headsets (Android), Windows Standalone
- **Key SDKs:** PICO Unity Integration SDK 2.3.0, XR Interaction Toolkit 2.0.4
- **Libraries:** DOTween, TextMeshPro

## Project Structure

```
Assets/
├── Scripts/                    # Core C# scripts
│   ├── PicoCommunicator.cs    # UDP networking & command handling
│   ├── VideoManager.cs        # 360° video playback control
│   └── TicketsManager.cs      # Usage statistics tracking
├── Scenes/
│   └── SampleScene.unity      # Main application scene
├── Materials/                 # Skybox and render materials
├── Textures/                  # RenderTextures for video output
├── Plugins/                   # Android plugins, DOTween
├── Resources/                 # Runtime configs
└── XR/                        # XR loader configurations
```

## Build Commands

Open in Unity 2020.3.48f1 or build via Visual Studio solution:
- Solution: `NDK-Cinema-Client.sln`
- Main project: `Assembly-CSharp.csproj`

## Architecture

### Core Components

1. **PicoCommunicator** - UDP server on port 4242, receives commands from NDK server (192.168.70.150:2424)
2. **VideoManager** - Controls VideoPlayer, maps 12 movies to device storage paths
3. **TicketsManager** - Tracks play statistics via PlayerPrefs

### Network Protocol

- **Receive:** Port 4242
- **Send:** 192.168.70.150:2424
- **Commands:** `Start_{MovieName}_{startTime}_{stopTime}` or `Stop_Movie`

### Supported Movies

Haunted, DinoChase, Motocross, Starlight, PiratesGold, RiverRush, WallOfChina, Wonderland, CocaCola, NightBefore, ToyFactory, SantaFly

Video path format: `file:///storage/emulated/0/Movies/Movie_{MovieName}.mp4`

## Debug Controls

| Key | Action |
|-----|--------|
| J | Toggle video playback (test mode) |
| T | Triple-click for statistics panel |
| P | Reset all statistics |
| Joystick1Button0 | Audio request / stats toggle |

## Code Conventions

- PascalCase for classes and public methods
- camelCase for private fields and locals
- MonoBehaviour component-based architecture
- Debug.Log() for logging

## Important Notes

- Network IPs/ports are hardcoded in PicoCommunicator.cs
- Application runs in background (`runInBackground = true`)
- Single scene architecture (SampleScene.unity)
- RenderTextures: Master (default), Wonderland, CocaCola (special rendering)
