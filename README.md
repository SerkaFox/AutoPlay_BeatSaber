# AutoPlay Beat Saber

AutoPlay Beat Saber is a BSIPA plugin for Beat Saber 1.40.8 that keeps a Solo
session moving without requiring the player to select every song manually.

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
- In-game interface available in English, Russian, and Spanish.

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

## Installation

Extract the release archive into the Beat Saber installation directory. The DLL
must end up at `Plugins/AutoContinuousPlay.dll`.

## Building

```powershell
dotnet build -c Release -p:BeatSaberDir="D:\Path\To\Beat Saber"
```

## License

MIT © 2026 SerkaFox
