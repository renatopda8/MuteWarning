using System;

namespace MuteWarning
{
    public class AudioInput
    {
        public AudioInput(string inputName, bool isMuted)
        {
            InputName = inputName ?? throw new ArgumentNullException(nameof(inputName));
            IsMuted = isMuted;
        }

        public string InputName { get; set; }
        public bool IsMuted { get; set; }
    }
}