using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.GameplaySetup;
using SimpleDimmer.Configuration;
using Zenject;

namespace SimpleDimmer.UI
{
    internal class DimmerViewController : IInitializable, IDisposable, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private bool _enabled;
        private float _brightness;
        private int _stepIndex;

        public void Initialize()
        {
            _enabled = PluginConfig.Instance.Enabled;
            _brightness = PluginConfig.Instance.Brightness;
            _stepIndex = PluginConfig.Instance.StepIndex;
            PluginConfig.Instance.RuntimeEnabled = _enabled;
            PluginConfig.Instance.RuntimeBrightness = _brightness;
            PluginConfig.Instance.RuntimeStepIndex = _stepIndex;
            GameplaySetup.Instance.AddTab("Simple Dimmer", "SimpleDimmer.UI.Views.DimmerView.bsml", this);
        }

        public void Dispose()
        {
            PluginConfig.Instance.Enabled = _enabled;
            PluginConfig.Instance.Brightness = _brightness;
            PluginConfig.Instance.StepIndex = _stepIndex;
            GameplaySetup.Instance.RemoveTab("Simple Dimmer");
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        [UIValue("enabled")]
        public bool Enabled
        {
            get => _enabled;
            set
            {
                _enabled = value;
                PluginConfig.Instance.RuntimeEnabled = value;
                NotifyPropertyChanged();
            }
        }

        [UIValue("brightness")]
        public float Brightness
        {
            get => _brightness;
            set
            {
                _brightness = value;
                PluginConfig.Instance.RuntimeBrightness = value;
                NotifyPropertyChanged();
            }
        }

        private static readonly List<object> _stepOptions = new List<object> { "0.1", "0.05", "0.01" };

        [UIValue("step-options")]
        public List<object> StepOptions => _stepOptions;

        [UIValue("step-index")]
        public int StepIndex
        {
            get => _stepIndex;
            set
            {
                _stepIndex = value;
                PluginConfig.Instance.RuntimeStepIndex = value;
                NotifyPropertyChanged();
            }
        }
    }
}