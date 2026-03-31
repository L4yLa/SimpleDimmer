using System;
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

        public void Initialize()
        {
            _enabled = PluginConfig.Instance.Enabled;
            _brightness = PluginConfig.Instance.Brightness;
            PluginConfig.Instance.RuntimeEnabled = _enabled;
            PluginConfig.Instance.RuntimeBrightness = _brightness;
            GameplaySetup.Instance.AddTab("Simple Dimmer", "SimpleDimmer.UI.Views.DimmerView.bsml", this);
        }

        public void Dispose()
        {
            PluginConfig.Instance.Enabled = _enabled;
            PluginConfig.Instance.Brightness = _brightness;
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
    }
}