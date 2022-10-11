using System;
using System.Collections.Generic;
using System.Linq;

namespace MuteWarning
{
    public class AudioInputControl
    {
        public Action<bool> OnInputUpdated { get; }
        private List<AudioInput> AudioInputs { get; }

        public AudioInputControl(Action<bool> onInputUpdated)
        {
            AudioInputs = new List<AudioInput>();
            OnInputUpdated = onInputUpdated;
        }

        public void UpdateInput(string inputName, bool isMuted)
        {
            var input = AudioInputs.SingleOrDefault(s => s.InputName == inputName);
            if (input == null)
            {
                input = new AudioInput(inputName, isMuted);
                AudioInputs.Add(input);
            }
            else
            {
                input.IsMuted = isMuted;
            }

            InputUpdated();
        }

        public void SetInputs(params AudioInput[] audioInputs)
        {
            AudioInputs.Clear();
            foreach (var input in audioInputs)
            {
                if (!AudioInputs.Any(s => s.InputName == input.InputName))
                {
                    AudioInputs.Add(input);
                }
            }

            InputUpdated();
        }

        public void ClearInputs()
        {
            //Nenhum audio input para incluir, apenas limpa e dispara o update
            SetInputs();
        }

        private void InputUpdated()
        {
            OnInputUpdated?.Invoke(IsAnyInputMuted);
        }

        private bool IsAnyInputMuted { get => AudioInputs.Any(s => s.IsMuted); }
    }
}