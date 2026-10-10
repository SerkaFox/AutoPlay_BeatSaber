using System;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.FloatingScreen;
using BS_Utils.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AutoContinuousPlay
{
    internal static class TimerOverlay
    {
        private static TimerOverlayBehaviour _behaviour;

        internal static void Enable()
        {
            BSEvents.gameSceneLoaded -= HandleGameSceneLoaded;
            BSEvents.gameSceneLoaded += HandleGameSceneLoaded;
            BSEvents.menuSceneActive -= HandleMenuSceneActive;
            BSEvents.menuSceneActive += HandleMenuSceneActive;

            if (_behaviour == null)
            {
                var gameObject = new GameObject("AutoContinuousPlay.Timer");
                UnityEngine.Object.DontDestroyOnLoad(gameObject);
                _behaviour = gameObject.AddComponent<TimerOverlayBehaviour>();
            }
        }

        internal static void Disable()
        {
            BSEvents.gameSceneLoaded -= HandleGameSceneLoaded;
            BSEvents.menuSceneActive -= HandleMenuSceneActive;
            if (_behaviour != null)
            {
                UnityEngine.Object.Destroy(_behaviour.gameObject);
                _behaviour = null;
            }
        }

        internal static void Refresh()
        {
            _behaviour?.RefreshImmediately();
        }

        private static void HandleGameSceneLoaded()
        {
            SessionController.MarkGameplayReady();
            if (SessionController.IsSessionActive)
                _behaviour?.Show();
        }

        private static void HandleMenuSceneActive()
        {
            SessionController.MarkGameplayNotReady();
            _behaviour?.Hide();
        }
    }

    internal sealed class TimerOverlayBehaviour : MonoBehaviour
    {
        private FloatingScreen _screen;
        private TextMeshProUGUI _text;
        private int _lastDisplayedSecond = -1;

        internal void Show()
        {
            Hide();

            try
            {
                // SimpleClock uses (0, 2.75, 4). This places the countdown neatly below it.
                _screen = FloatingScreen.CreateFloatingScreen(
                    new Vector2(30f, 10f),
                    false,
                    new Vector3(0f, 2.53f, 4f),
                    Quaternion.identity,
                    0f,
                    false);

                _text = BeatSaberUI.CreateText(
                    _screen.gameObject.GetComponent<RectTransform>(),
                    FormatRemaining(),
                    Vector2.zero);
                _text.alignment = TextAlignmentOptions.Center;
                _text.fontSize = 6.2f;
                _text.color = new Color(0.55f, 0.9f, 1f, 1f);
                _lastDisplayedSecond = -1;
                RefreshImmediately();
            }
            catch (Exception ex)
            {
                Plugin.Log.Error("Could not create the session countdown.");
                Plugin.Log.Error(ex);
                Hide();
            }
        }

        internal void Hide()
        {
            if (_screen != null)
                UnityEngine.Object.Destroy(_screen.gameObject);
            _screen = null;
            _text = null;
            _lastDisplayedSecond = -1;
        }

        internal void RefreshImmediately()
        {
            _lastDisplayedSecond = -1;
            UpdateText();
        }

        private void Update()
        {
            SessionController.AdvanceSessionClock(Time.unscaledDeltaTime, Time.timeScale > 0f);
            UpdateText();
        }

        private void OnDestroy()
        {
            Hide();
        }

        private void UpdateText()
        {
            if (_text == null)
                return;

            var remaining = SessionController.RemainingTime;
            var second = (int)Math.Ceiling(remaining.TotalSeconds);
            if (second == _lastDisplayedSecond)
                return;

            _lastDisplayedSecond = second;
            _text.text = FormatRemaining();
            _text.color = second <= 60
                ? new Color(1f, 0.45f, 0.35f, 1f)
                : new Color(0.55f, 0.9f, 1f, 1f);
        }

        private static string FormatRemaining()
        {
            var totalSeconds = Math.Max(0, (int)Math.Ceiling(SessionController.RemainingTime.TotalSeconds));
            var hours = totalSeconds / 3600;
            var minutes = (totalSeconds % 3600) / 60;
            var seconds = totalSeconds % 60;
            return hours > 0
                ? $"{hours}:{minutes:00}:{seconds:00}"
                : $"{minutes:00}:{seconds:00}";
        }
    }
}
