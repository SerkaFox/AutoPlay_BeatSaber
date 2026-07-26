using HarmonyLib;
using IPA;
using IPA.Config;
using IPA.Config.Stores;
using IPA.Logging;

namespace AutoContinuousPlay
{
    [Plugin(RuntimeOptions.DynamicInit)]
    public sealed class Plugin
    {
        internal static Logger Log { get; private set; }
        private Harmony _harmony;

        [Init]
        public Plugin(Logger logger, Config config)
        {
            Log = logger;
            PluginConfig.Instance = config.Generated<PluginConfig>();
        }

        [OnEnable]
        public void OnEnable()
        {
            _harmony = new Harmony("com.serka.autocontinuousplay");
            _harmony.PatchAll(typeof(Plugin).Assembly);
            GameplaySetupUI.Enable();
            TimerOverlay.Enable();
            Log.Info("Auto Continuous Play enabled.");
        }

        [OnDisable]
        public void OnDisable()
        {
            TimerOverlay.Disable();
            GameplaySetupUI.Disable();
            _harmony?.UnpatchSelf();
            SessionController.Reset();
            Log.Info("Auto Continuous Play disabled.");
        }
    }
}
