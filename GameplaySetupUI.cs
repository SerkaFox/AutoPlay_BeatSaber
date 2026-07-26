using System.Collections.Generic;
using System.ComponentModel;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.GameplaySetup;
using BeatSaberMarkupLanguage.Util;

namespace AutoContinuousPlay
{
    internal sealed class GameplaySetupUI : INotifyPropertyChanged
    {
        private const string TabName = "Auto Play";
        private const string ResourceName = "AutoContinuousPlay.UI.gameplay.bsml";
        private static readonly GameplaySetupUI Instance = new GameplaySetupUI();
        private static bool _registered;

        public event PropertyChangedEventHandler PropertyChanged;

        [UIValue("language")]
        public string Language
        {
            get => PluginConfig.Instance.Language;
            set
            {
                PluginConfig.Instance.Language = value;
                NotifyLabelsChanged();
            }
        }

        [UIValue("language-options")]
        public List<object> LanguageOptions { get; } = new List<object>
        {
            "Русский",
            "English",
            "Español"
        };

        [UIValue("enabled")]
        public bool Enabled
        {
            get => PluginConfig.Instance.Enabled;
            set => PluginConfig.Instance.Enabled = value;
        }

        [UIValue("session-minutes")]
        public float SessionMinutes
        {
            get => PluginConfig.Instance.SessionMinutes;
            set => PluginConfig.Instance.SessionMinutes = (int)value;
        }

        [UIValue("delay-seconds")]
        public float DelaySeconds
        {
            get => PluginConfig.Instance.DelayBetweenSongsSeconds;
            set => PluginConfig.Instance.DelayBetweenSongsSeconds = value;
        }

        [UIValue("difficulty")]
        public string Difficulty
        {
            get => PluginConfig.Instance.Difficulty;
            set => PluginConfig.Instance.Difficulty = value;
        }

        [UIValue("difficulty-options")]
        public List<object> DifficultyOptions { get; } = new List<object>
        {
            "Easy",
            "Normal",
            "Hard",
            "Expert",
            "ExpertPlus"
        };

        [UIValue("minimum-length")]
        public float MinimumLength
        {
            get => PluginConfig.Instance.MinimumSongLengthSeconds;
            set
            {
                PluginConfig.Instance.MinimumSongLengthSeconds = value;
                if (PluginConfig.Instance.MaximumSongLengthSeconds < value)
                    PluginConfig.Instance.MaximumSongLengthSeconds = value;
            }
        }

        [UIValue("maximum-length")]
        public float MaximumLength
        {
            get => PluginConfig.Instance.MaximumSongLengthSeconds;
            set
            {
                PluginConfig.Instance.MaximumSongLengthSeconds = value;
                if (PluginConfig.Instance.MinimumSongLengthSeconds > value)
                    PluginConfig.Instance.MinimumSongLengthSeconds = value;
            }
        }

        [UIValue("restart-after-failure")]
        public bool RestartAfterFailure
        {
            get => PluginConfig.Instance.RestartAfterFailure;
            set => PluginConfig.Instance.RestartAfterFailure = value;
        }

        [UIValue("avoid-repeats")]
        public bool AvoidRepeats
        {
            get => PluginConfig.Instance.AvoidImmediateRepeats;
            set => PluginConfig.Instance.AvoidImmediateRepeats = value;
        }

        [UIValue("label-language")]
        public string LabelLanguage => L("Язык", "Language", "Idioma");

        [UIValue("label-enabled")]
        public string LabelEnabled => L("Непрерывная игра", "Continuous play", "Juego continuo");

        [UIValue("hint-enabled")]
        public string HintEnabled => L(
            "После завершения карты автоматически запускает следующую случайную локальную карту.",
            "Automatically starts another random local map after the current map ends.",
            "Inicia automáticamente otro mapa local aleatorio al terminar el mapa actual.");

        [UIValue("label-session")]
        public string LabelSession => L("Время сессии, мин", "Session time, min", "Tiempo de sesión, min");

        [UIValue("label-delay")]
        public string LabelDelay => L("Пауза между песнями, сек", "Delay between songs, sec", "Pausa entre canciones, s");

        [UIValue("label-difficulty")]
        public string LabelDifficulty => L("Сложность", "Difficulty", "Dificultad");

        [UIValue("label-minimum")]
        public string LabelMinimum => L("Минимальная длина, сек", "Minimum length, sec", "Duración mínima, s");

        [UIValue("label-maximum")]
        public string LabelMaximum => L("Максимальная длина, сек", "Maximum length, sec", "Duración máxima, s");

        [UIValue("label-restart-failure")]
        public string LabelRestartFailure => L(
            "Перезапускать при проигрыше",
            "Restart after failure",
            "Reiniciar al fallar");

        [UIValue("label-avoid-repeats")]
        public string LabelAvoidRepeats => L(
            "Не повторять последние карты",
            "Avoid recent maps",
            "Evitar mapas recientes");

        internal static string SkipLabel => Translate("ПРОПУСТИТЬ", "SKIP", "SALTAR");

        internal static void Enable()
        {
            MainMenuAwaiter.MainMenuInitializing -= Register;
            MainMenuAwaiter.MainMenuInitializing += Register;
        }

        internal static void Disable()
        {
            MainMenuAwaiter.MainMenuInitializing -= Register;
            if (!_registered)
                return;

            ZenjectSingleton<GameplaySetup>.Instance.RemoveTab(TabName);
            _registered = false;
        }

        private static void Register()
        {
            if (_registered)
                ZenjectSingleton<GameplaySetup>.Instance.RemoveTab(TabName);

            ZenjectSingleton<GameplaySetup>.Instance.AddTab(
                TabName,
                ResourceName,
                Instance,
                MenuType.Solo);
            _registered = true;
            Plugin.Log.Info("Registered the Auto Play Solo setup tab.");
        }

        private static string Translate(string russian, string english, string spanish)
        {
            switch (PluginConfig.Instance.Language)
            {
                case "English":
                    return english;
                case "Español":
                    return spanish;
                default:
                    return russian;
            }
        }

        private string L(string russian, string english, string spanish)
        {
            return Translate(russian, english, spanish);
        }

        private void NotifyLabelsChanged()
        {
            NotifyPropertyChanged(nameof(Language));
            NotifyPropertyChanged(nameof(LabelLanguage));
            NotifyPropertyChanged(nameof(LabelEnabled));
            NotifyPropertyChanged(nameof(HintEnabled));
            NotifyPropertyChanged(nameof(LabelSession));
            NotifyPropertyChanged(nameof(LabelDelay));
            NotifyPropertyChanged(nameof(LabelDifficulty));
            NotifyPropertyChanged(nameof(LabelMinimum));
            NotifyPropertyChanged(nameof(LabelMaximum));
            NotifyPropertyChanged(nameof(LabelRestartFailure));
            NotifyPropertyChanged(nameof(LabelAvoidRepeats));
        }

        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
