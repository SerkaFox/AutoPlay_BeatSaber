# Compatibility test results

Manual runtime tests were performed on 2026-07-26 using equivalent 0.4.0
compatibility builds, Steam builds managed by BSManager, SteamVR/OpenXR, and a
shared library containing 856 custom-map folders. Version 0.4.2 retains the
tested compatibility fixes and adds an active-play
session clock: pause time and inter-song loading time no longer consume the
configured session. Its source was compiled against all five supported game
installations after the changes.

| Beat Saber | Result | Tested behavior |
| --- | --- | --- |
| 1.39.1 | Pass | Plugin and Solo tab load; a 30-minute Expert session starts and advances through custom maps. The manifest uses SongCore 3.14.15 or newer. |
| 1.40.7 | Pass with input limitation | AutoPlay, countdown, restart, and automatic transitions work. Pause/Skip was not manually verified because the current SteamVR controller binding did not open Beat Saber's pause menu. |
| 1.40.8 | Pass with input limitation | AutoPlay, countdown, restart, and automatic transitions work. Pause/Skip was not manually verified because the current SteamVR controller binding did not open Beat Saber's pause menu. |
| 1.43.0 | Pass | Full interactive test completed successfully, including the in-game controls. |
| 1.44.1 | Pass | Full interactive test completed successfully, including the in-game controls. |
| 1.44.2 | Unsupported | No compatible official BSIPA release was available during testing, so plugin support is not claimed. |

The pause-menu limitation observed on 1.40.x also affected the unmodified game
input path and produced no AutoPlay exception in the log. It is therefore
tracked as a SteamVR/OpenXR controller-binding issue rather than a plugin
failure.

Some legacy custom-map folders fail SongCore metadata parsing on older game
versions. Those map-specific errors are independent of AutoPlay; compatible
maps continue to load and play.

## Build validation

The 0.4.2 source compiled against all supported installations:

- 1.39.1, 1.40.7, and 1.40.8: zero errors and zero warnings.
- 1.43.0 and 1.44.1: zero errors and one BSML deprecation warning for
  `BeatSaberUI.CreateText`, which remains functional in the tested timer
  overlay.

## Version-specific manifests

The root `manifest.json` targets the latest supported version, Beat Saber
1.44.1. The `manifests` directory contains dependency sets used for the older
tested versions. Select one with the `PluginManifest` MSBuild property.

## Release asset names

Every published archive includes both the plugin version and its exact target
game version:

| Target game | Asset filename |
| --- | --- |
| Beat Saber 1.39.1 | `AutoPlay-Beat-Saber-v0.4.2-bs1.39.1.zip` |
| Beat Saber 1.40.7 | `AutoPlay-Beat-Saber-v0.4.2-bs1.40.7.zip` |
| Beat Saber 1.40.8 | `AutoPlay-Beat-Saber-v0.4.2-bs1.40.8.zip` |
| Beat Saber 1.43.0 | `AutoPlay-Beat-Saber-v0.4.2-bs1.43.0.zip` |
| Beat Saber 1.44.1 | `AutoPlay-Beat-Saber-v0.4.2-bs1.44.1.zip` |

Do not infer compatibility from the plugin version alone. The `bsX.Y.Z`
filename suffix is the authoritative game-version target.
