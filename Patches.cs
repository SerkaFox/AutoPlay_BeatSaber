using System;
using System.Linq;
using HarmonyLib;
using HMUI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AutoContinuousPlay
{
    [HarmonyPatch(typeof(SinglePlayerLevelSelectionFlowCoordinator), nameof(SinglePlayerLevelSelectionFlowCoordinator.StartLevel))]
    internal static class StartLevelPatch
    {
        private static void Prefix(SinglePlayerLevelSelectionFlowCoordinator __instance)
        {
            SessionController.MarkLevelStarted(__instance);
        }
    }

    [HarmonyPatch(typeof(SoloFreePlayFlowCoordinator), "ProcessLevelCompletionResultsAfterLevelDidFinish")]
    internal static class LevelFinishedPatch
    {
        private static void Prefix(LevelCompletionResults levelCompletionResults)
        {
            SessionController.PrepareLevelFinished(levelCompletionResults);
        }

        private static void Postfix(SoloFreePlayFlowCoordinator __instance, LevelCompletionResults levelCompletionResults)
        {
            SessionController.HandleLevelFinished(__instance, levelCompletionResults);
        }
    }

    [HarmonyPatch(typeof(FlowCoordinator), "PresentViewController")]
    internal static class SuppressResultsViewPatch
    {
        private static bool Prefix(ViewController viewController)
        {
            return !(viewController is ResultsViewController) ||
                   !SessionController.ShouldSuppressResultsView;
        }
    }

    [HarmonyPatch(typeof(PauseMenuManager), "Start")]
    internal static class PauseMenuSkipButtonPatch
    {
        private const string SkipButtonName = "AutoContinuousPlay.SkipButton";

        private static void Postfix(PauseMenuManager __instance)
        {
            if (!PluginConfig.Instance.Enabled || !SessionController.IsSessionActive)
                return;

            try
            {
                var restartButton = AccessTools.Field(typeof(PauseMenuManager), "_restartButton")
                    .GetValue(__instance) as Button;
                var backButton = AccessTools.Field(typeof(PauseMenuManager), "_backButton")
                    .GetValue(__instance) as Button;
                if (restartButton == null || backButton == null)
                    throw new InvalidOperationException("Pause menu buttons were not found.");

                if (restartButton.transform.parent.Find(SkipButtonName) != null)
                    return;

                var skipButton = UnityEngine.Object.Instantiate(restartButton, restartButton.transform.parent);
                skipButton.name = SkipButtonName;
                skipButton.gameObject.SetActive(true);
                skipButton.interactable = true;
                skipButton.onClick.RemoveAllListeners();
                skipButton.onClick.AddListener(() =>
                {
                    skipButton.interactable = false;
                    SessionController.RequestSkip();
                });

                // The cloned Restart button also contains a Polyglot component
                // which rewrites its caption after Start. Disable and remove it
                // before assigning our permanent label.
                foreach (var component in skipButton.GetComponentsInChildren<Component>(true)
                             .Where(component => component.GetType().Name.StartsWith("LocalizedText")))
                {
                    if (component is Behaviour behaviour)
                        behaviour.enabled = false;
                    UnityEngine.Object.Destroy(component);
                }

                foreach (var text in skipButton.GetComponentsInChildren<TextMeshProUGUI>(true))
                    text.text = GameplaySetupUI.SkipLabel;

                var restartRect = restartButton.GetComponent<RectTransform>();
                var skipRect = skipButton.GetComponent<RectTransform>();
                var layoutElement = skipButton.GetComponent<LayoutElement>() ??
                                    skipButton.gameObject.AddComponent<LayoutElement>();

                // The 1.40.8 pause buttons are arranged horizontally by a layout
                // group. Keep the cloned button out of that group and place it in
                // a second row, centered below Restart.
                layoutElement.ignoreLayout = true;
                skipRect.anchorMin = restartRect.anchorMin;
                skipRect.anchorMax = restartRect.anchorMax;
                skipRect.pivot = restartRect.pivot;
                skipRect.sizeDelta = restartRect.sizeDelta;
                var verticalOffset = Math.Max(7f, restartRect.rect.height + 1.5f);
                skipRect.anchoredPosition = restartRect.anchoredPosition +
                                            new Vector2(0f, -verticalOffset);
                skipRect.SetAsLastSibling();

                Plugin.Log.Info(
                    $"Added the Skip button below the pause row at {skipRect.anchoredPosition}.");
            }
            catch (Exception ex)
            {
                Plugin.Log.Error("Could not add the Skip button to the pause menu.");
                Plugin.Log.Error(ex);
            }
        }
    }
}
