using IPA.Config.Stores.Attributes;

namespace AutoContinuousPlay
{
    public class PluginConfig
    {
        public static PluginConfig Instance { get; set; }

        public virtual bool Enabled { get; set; } = true;
        public virtual string Language { get; set; } = "Русский";
        public virtual int SessionMinutes { get; set; } = 30;
        public virtual float DelayBetweenSongsSeconds { get; set; } = 0f;
        public virtual string Difficulty { get; set; } = "Expert";
        public virtual float MinimumSongLengthSeconds { get; set; } = 60f;
        public virtual float MaximumSongLengthSeconds { get; set; } = 360f;
        public virtual bool RestartAfterFailure { get; set; } = false;
        public virtual bool AvoidImmediateRepeats { get; set; } = true;

        [Ignore]
        public BeatmapDifficulty ParsedDifficulty
        {
            get
            {
                BeatmapDifficulty value;
                return System.Enum.TryParse(Difficulty, true, out value)
                    ? value
                    : BeatmapDifficulty.Expert;
            }
        }
    }
}
