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
        private int _brightness;
        private bool _dimmingLights;
        private bool _dimmingWalls;

        public void Initialize()
        {
            _enabled = PluginConfig.Instance.Enabled;
            _brightness = PluginConfig.Instance.PerceivedBrightness;
            _dimmingLights = PluginConfig.Instance.DimmingLights;
            _dimmingWalls = PluginConfig.Instance.DimmingWalls;
            DimmerState.Enabled = _enabled;
            DimmerState.SetPerceivedBrightness(_brightness);
            DimmerState.DimmingLights = _dimmingLights;
            DimmerState.DimmingWalls = _dimmingWalls;
            GameplaySetup.Instance.AddTab("Simple Dimmer", "SimpleDimmer.UI.Views.DimmerView.bsml", this);
        }

        public void Dispose()
        {
            PluginConfig.Instance.Enabled = _enabled;
            PluginConfig.Instance.PerceivedBrightness = _brightness;
            PluginConfig.Instance.DimmingLights = _dimmingLights;
            PluginConfig.Instance.DimmingWalls = _dimmingWalls;
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
                DimmerState.Enabled = value;
                NotifyPropertyChanged();
            }
        }

        [UIValue("brightness")]
        public int Brightness
        {
            get => _brightness;
            set
            {
                _brightness = value;
                DimmerState.SetPerceivedBrightness(value);
                NotifyPropertyChanged();
            }
        }

        [UIValue("dimming-lights")]
        public bool DimmingLights
        {
            get => _dimmingLights;
            set
            {
                _dimmingLights = value;
                DimmerState.DimmingLights = value;
                NotifyPropertyChanged();
            }
        }

        [UIValue("dimming-walls")]
        public bool DimmingWalls
        {
            get => _dimmingWalls;
            set
            {
                _dimmingWalls = value;
                DimmerState.DimmingWalls = value;
                NotifyPropertyChanged();
            }
        }

        [UIAction("format-percent")]
        public string FormatPercent(int value) => $"{value}%";
    }
}
