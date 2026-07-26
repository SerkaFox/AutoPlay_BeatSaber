# AutoPlay Beat Saber

AutoPlay Beat Saber is a BSIPA plugin that keeps a Solo session moving without
requiring the player to select every song manually.

The 0.4.1 source builds against Beat Saber 1.39.1, 1.40.7, 1.40.8, 1.43.0,
and 1.44.1. Equivalent compatibility builds were manually exercised in VR.
See [COMPATIBILITY.md](COMPATIBILITY.md) for the detailed results and known
limitations.

## Features

- Plays random local custom maps back-to-back.
- Filters maps by difficulty and song length.
- Stops after a configurable session duration.
- Supports seamless transitions by setting the delay to zero.
- Optionally restarts the current map after a failure.
- Avoids the ten most recently played or skipped maps.
- Adds a session countdown below SimpleClock.
- Adds a Skip button to the in-game pause menu.
- Keeps the normal Menu button available to end the session.
- Provides an in-game interface in English, Russian, and Spanish.

## Usage

1. Open Solo mode.
2. Select the `Auto Play` gameplay setup tab.
3. Choose the language and session settings.
4. Start any song manually. AutoPlay handles the following songs.

The pause-menu button is translated as:

- English: `SKIP`
- Russian: `ПРОПУСТИТЬ`
- Spanish: `SALTAR`

## Configuration

The plugin generates `UserData/AutoContinuousPlay.json`. Every option is also
available from the `Auto Play` tab inside Solo mode:

- `Enabled`
- `Language`
- `SessionMinutes`
- `DelayBetweenSongsSeconds`
- `Difficulty`
- `MinimumSongLengthSeconds`
- `MaximumSongLengthSeconds`
- `RestartAfterFailure`
- `AvoidImmediateRepeats`

## Dependencies

- BSIPA
- SongCore
- BS Utils
- BeatSaberMarkupLanguage

Dependency minimums differ between Beat Saber versions. Use the matching
manifest from the `manifests` directory when building an older version.

## Installation

Extract the release archive into the Beat Saber installation directory. The DLL
must end up at `Plugins/AutoContinuousPlay.dll`.

## Building

The default manifest targets Beat Saber 1.44.1:

```powershell
dotnet build -c Release -p:BeatSaberDir="D:\Path\To\Beat Saber"
```

To build another tested version, select its manifest:

```powershell
dotnet build -c Release `
  -p:BeatSaberDir="D:\Path\To\Beat Saber 1.39.1" `
  -p:PluginManifest="manifests\1.39.1.json"
```

## License

MIT © 2026 SerkaFox
