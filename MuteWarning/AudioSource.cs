using System;

namespace MuteWarning
{
    public class AudioSource
    {
        public AudioSource(string sourceName, bool isMuted)
        {
            SourceName = sourceName ?? throw new ArgumentNullException(nameof(sourceName));
            IsMuted = isMuted;
        }

        public string SourceName { get; set; }
        public bool IsMuted { get; set; }
    }
}