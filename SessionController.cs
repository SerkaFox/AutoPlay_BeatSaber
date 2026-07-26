using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using SongCore;
using UnityEngine;

namespace AutoContinuousPlay
{
    internal static class SessionController
    {
        private static readonly System.Random Random = new System.Random();
        private static readonly Queue<string> RecentLevelIds = new Queue<string>();
        private static DateTime? _sessionStartedAt;
        private static bool _transitionPending;
        private static bool _suppressResultsView;
        private static bool _skipRequested;

        internal static bool IsSessionActive => _sessionStartedAt.HasValue;
        internal static bool ShouldSuppressResultsView => _suppressResultsView;

        internal static TimeSpan RemainingTime
        {
            get
            {
                if (!_sessionStartedAt.HasValue)
                    return TimeSpan.Zero;

                var remaining = TimeSpan.FromMinutes(PluginConfig.Instance.SessionMinutes) -
                                (DateTime.UtcNow - _sessionStartedAt.Value);
                return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
            }
        }

        internal static void MarkLevelStarted(SinglePlayerLevelSelectionFlowCoordinator flow)
        {
            if (!(flow is SoloFreePlayFlowCoordinator) || !PluginConfig.Instance.Enabled)
                return;

            if (!_sessionStartedAt.HasValue)
            {
                _sessionStartedAt = DateTime.UtcNow;
                Plugin.Log.Info($"Continuous session started ({PluginConfig.Instance.SessionMinutes} minutes, {PluginConfig.Instance.ParsedDifficulty}).");
            }

            try
            {
                var selectedLevel = AccessTools.Property(
                    typeof(LevelSelectionFlowCoordinator),
                    "selectedBeatmapLevel")?.GetValue(flow) as BeatmapLevel;
                if (selectedLevel != null &&
                    (RecentLevelIds.Count == 0 || RecentLevelIds.Last() != selectedLevel.levelID))
                {
                    Remember(selectedLevel.levelID);
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.Warn($"Could not remember the current map: {ex.Message}");
            }
        }

        internal static void PrepareLevelFinished(LevelCompletionResults results)
        {
            _suppressResultsView =
                PluginConfig.Instance.Enabled &&
                _sessionStartedAt.HasValue &&
                ((results.levelEndAction == LevelCompletionResults.LevelEndAction.Quit && _skipRequested) ||
                 (results.levelEndAction == LevelCompletionResults.LevelEndAction.None &&
                  (results.levelEndStateType == LevelCompletionResults.LevelEndStateType.Cleared ||
                  (results.levelEndStateType == LevelCompletionResults.LevelEndStateType.Failed &&
                  PluginConfig.Instance.RestartAfterFailure))));
        }

        internal static void RequestSkip()
        {
            if (!PluginConfig.Instance.Enabled || !_sessionStartedAt.HasValue || _transitionPending)
                return;

            try
            {
                var pauseController = Resources.FindObjectsOfTypeAll<PauseController>()
                    .FirstOrDefault(controller => controller.isActiveAndEnabled);
                if (pauseController == null)
                    throw new InvalidOperationException("Active PauseController was not found.");

                var returnController = AccessTools.Field(typeof(PauseController), "_returnToMenuController")
                    .GetValue(pauseController) as IReturnToMenuController;
                if (returnController == null)
                    throw new InvalidOperationException("ReturnToMenuController was not found.");

                _skipRequested = true;
                Plugin.Log.Info("Player requested to skip the current map.");
                returnController.ReturnToMenu();
            }
            catch (Exception ex)
            {
                _skipRequested = false;
                Plugin.Log.Error("Could not skip the current map.");
                Plugin.Log.Error(ex);
            }
        }

        internal static void HandleLevelFinished(
            SoloFreePlayFlowCoordinator flow,
            LevelCompletionResults results)
        {
            _suppressResultsView = false;

            if (!PluginConfig.Instance.Enabled || _transitionPending || !_sessionStartedAt.HasValue)
                return;

            if (results.levelEndAction == LevelCompletionResults.LevelEndAction.Quit)
            {
                if (_skipRequested)
                {
                    _skipRequested = false;
                    if (RemainingTime <= TimeSpan.Zero)
                    {
                        Stop("configured session time elapsed");
                        return;
                    }

                    _transitionPending = true;
                    flow.StartCoroutine(PlayNextAfterDelay(flow));
                    return;
                }

                Stop("player returned to the menu");
                return;
            }

            if (results.levelEndAction == LevelCompletionResults.LevelEndAction.Restart)
                return;

            if ((DateTime.UtcNow - _sessionStartedAt.Value).TotalMinutes >= PluginConfig.Instance.SessionMinutes)
            {
                Stop("configured session time elapsed");
                return;
            }

            if (results.levelEndStateType == LevelCompletionResults.LevelEndStateType.Failed)
            {
                if (!PluginConfig.Instance.RestartAfterFailure)
                {
                    Stop("level failed");
                    return;
                }

                _transitionPending = true;
                flow.StartCoroutine(RestartAfterDelay(flow));
                return;
            }

            if (results.levelEndStateType != LevelCompletionResults.LevelEndStateType.Cleared)
                return;

            _transitionPending = true;
            flow.StartCoroutine(PlayNextAfterDelay(flow));
        }

        private static IEnumerator RestartAfterDelay(SoloFreePlayFlowCoordinator flow)
        {
            yield return new WaitForSecondsRealtime(Math.Max(0f, PluginConfig.Instance.DelayBetweenSongsSeconds));

            try
            {
                Plugin.Log.Info("Restarting failed map.");
                flow.StartLevel(null, false);
            }
            catch (Exception ex)
            {
                Plugin.Log.Error("Could not restart the failed map automatically.");
                Plugin.Log.Error(ex);
                Reset();
            }

            _transitionPending = false;
        }

        private static IEnumerator PlayNextAfterDelay(SoloFreePlayFlowCoordinator flow)
        {
            yield return new WaitForSecondsRealtime(Math.Max(0f, PluginConfig.Instance.DelayBetweenSongsSeconds));

            (BeatmapLevel level, BeatmapCharacteristicSO characteristic, BeatmapDifficulty difficulty)? next = null;
            try
            {
                next = PickNext();
                if (!next.HasValue)
                {
                    Stop($"no local Standard maps found for {PluginConfig.Instance.ParsedDifficulty}");
                    yield break;
                }

            }
            catch (Exception ex)
            {
                Plugin.Log.Error("Could not prepare the next map automatically.");
                Plugin.Log.Error(ex);
                Reset();
                yield break;
            }

            yield return null;

            try
            {
                SelectBeatmap(next.Value.level, next.Value.characteristic, next.Value.difficulty);
            }
            catch (Exception ex)
            {
                Plugin.Log.Error("Could not select the next map automatically.");
                Plugin.Log.Error(ex);
                Reset();
                yield break;
            }

            yield return null;
            yield return null;

            try
            {
                Remember(next.Value.level.levelID);
                Plugin.Log.Info($"Starting next map: {next.Value.level.songName} [{next.Value.difficulty}]");
                flow.StartLevel(null, false);
            }
            catch (Exception ex)
            {
                Plugin.Log.Error("Could not start the next map automatically.");
                Plugin.Log.Error(ex);
                Reset();
            }

            _transitionPending = false;
        }

        private static (BeatmapLevel level, BeatmapCharacteristicSO characteristic, BeatmapDifficulty difficulty)? PickNext()
        {
            var config = PluginConfig.Instance;
            var candidates = new List<(BeatmapLevel, BeatmapCharacteristicSO, BeatmapDifficulty)>();

            foreach (var level in Loader.CustomLevels.Values.Distinct())
            {
                if (level.songDuration < config.MinimumSongLengthSeconds ||
                    level.songDuration > config.MaximumSongLengthSeconds)
                    continue;

                if (config.AvoidImmediateRepeats && RecentLevelIds.Contains(level.levelID))
                    continue;

                foreach (var characteristic in level.GetCharacteristics())
                {
                    if (!string.Equals(characteristic.serializedName, "Standard", StringComparison.OrdinalIgnoreCase))
                        continue;

                    if (level.GetDifficulties(characteristic).Contains(config.ParsedDifficulty))
                        candidates.Add((level, characteristic, config.ParsedDifficulty));
                }
            }

            if (candidates.Count == 0 && RecentLevelIds.Count > 0)
            {
                RecentLevelIds.Clear();
                return PickNext();
            }

            return candidates.Count == 0 ? null : candidates[Random.Next(candidates.Count)];
        }

        private static void SelectBeatmap(
            BeatmapLevel level,
            BeatmapCharacteristicSO characteristic,
            BeatmapDifficulty difficulty)
        {
            var table = Resources.FindObjectsOfTypeAll<LevelCollectionTableView>().First();
            table.SelectLevel(level);

            var detailController = Resources.FindObjectsOfTypeAll<StandardLevelDetailViewController>().First();
            var detailView = AccessTools.Field(typeof(StandardLevelDetailViewController), "_standardLevelDetailView")
                .GetValue(detailController);

            var characteristicController = AccessTools.Field(detailView.GetType(),
                "_beatmapCharacteristicSegmentedControlController").GetValue(detailView);
            var characteristics = (List<BeatmapCharacteristicSO>)AccessTools.Field(
                characteristicController.GetType(), "_currentlyAvailableBeatmapCharacteristics").GetValue(characteristicController);
            var characteristicIndex = characteristics.IndexOf(characteristic);
            AccessTools.Method(characteristicController.GetType(),
                "HandleBeatmapCharacteristicSegmentedControlDidSelectCell")
                .Invoke(characteristicController, new object[]
                {
                    AccessTools.Field(characteristicController.GetType(), "_segmentedControl").GetValue(characteristicController),
                    characteristicIndex
                });

            var difficultyController = AccessTools.Field(detailView.GetType(),
                "_beatmapDifficultySegmentedControlController").GetValue(detailView);
            var difficulties = (List<BeatmapDifficulty>)AccessTools.Field(
                difficultyController.GetType(), "_difficulties").GetValue(difficultyController);
            var difficultyIndex = difficulties.IndexOf(difficulty);
            AccessTools.Method(difficultyController.GetType(),
                "HandleDifficultySegmentedControlDidSelectCell")
                .Invoke(difficultyController, new object[]
                {
                    AccessTools.Field(difficultyController.GetType(), "_difficultySegmentedControl").GetValue(difficultyController),
                    difficultyIndex
                });
        }

        private static void Remember(string levelId)
        {
            RecentLevelIds.Enqueue(levelId);
            while (RecentLevelIds.Count > 10)
                RecentLevelIds.Dequeue();
        }

        private static void Stop(string reason)
        {
            Plugin.Log.Info($"Continuous session stopped: {reason}.");
            Reset();
        }

        internal static void Reset()
        {
            _sessionStartedAt = null;
            _transitionPending = false;
            _suppressResultsView = false;
            _skipRequested = false;
            RecentLevelIds.Clear();
            TimerOverlay.Refresh();
        }
    }
}
