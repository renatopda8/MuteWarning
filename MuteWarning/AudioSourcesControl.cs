using System;
using System.Collections.Generic;
using System.Linq;

namespace MuteWarning
{
    public class AudioSourcesControl
    {
        public Action<bool> OnSourceUpdated { get; }
        private List<AudioSource> AudioSources { get; }

        public AudioSourcesControl(Action<bool> onSourceUpdated)
        {
            AudioSources = new List<AudioSource>();
            OnSourceUpdated = onSourceUpdated;
        }

        public void UpdateSource(string sourceName, bool isMuted)
        {
            var source = AudioSources.SingleOrDefault(s => s.SourceName == sourceName);
            if (source == null)
            {
                source = new AudioSource(sourceName, isMuted);
                AudioSources.Add(source);
            }
            else
            {
                source.IsMuted = isMuted;
            }

            SourceUpdated();
        }

        public void SetSources(params AudioSource[] audioSources)
        {
            AudioSources.Clear();
            foreach (var source in audioSources)
            {
                if (!AudioSources.Any(s => s.SourceName == source.SourceName))
                {
                    AudioSources.Add(source);
                }
            }

            SourceUpdated();
        }

        public void ClearSources()
        {
            //Nenhuma audio source para incluir, apenas limpa e dispara o update
            SetSources();
        }

        private void SourceUpdated()
        {
            OnSourceUpdated?.Invoke(IsAnySourceMuted);
        }

        private bool IsAnySourceMuted { get => AudioSources.Any(s => s.IsMuted); }
    }
}