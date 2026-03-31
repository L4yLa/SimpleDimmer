using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.GameplaySetup;
using SimpleDimmer.Configuration;
using UnityEngine;
using Zenject;

namespace SimpleDimmer.UI
{
    internal class DimmerViewController : IInitializable, IDisposable, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public void Initialize()
        {
            GameplaySetup.Instance.AddTab("Simple Dimmer", "SimpleDimmer.UI.Views.DimmerView.bsml", this);
        }

        public void Dispose()
        {
            GameplaySetup.Instance.RemoveTab("Simple Dimmer");
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        [UIValue("enabled")]
        public bool Enabled
        {
            get => PluginConfig.Instance.Enabled;
            set
            {
                PluginConfig.Instance.Enabled = value;
                NotifyPropertyChanged();
            }
        }

        [UIValue("brightness")]
        public float Brightness
        {
            get => PluginConfig.Instance.Brightness;
            set
            {
                PluginConfig.Instance.Brightness = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(BrightnessText));
            }
        }

        [UIValue("brightness-text")]
        public string BrightnessText => PluginConfig.Instance.Brightness.ToString("0.00");

        private static readonly List<object> _stepOptions = new List<object> { "0.1", "0.05", "0.01" };

        [UIValue("step-options")]
        public List<object> StepOptions => _stepOptions;

        [UIValue("step-index")]
        public int StepIndex
        {
            get => PluginConfig.Instance.StepIndex;
            set
            {
                PluginConfig.Instance.StepIndex = value;
                NotifyPropertyChanged();
            }
        }

        [UIAction("brightness-inc")]
        private void BrightnessIncrease()
        {
            Brightness = Mathf.Clamp(Brightness + GetStep(), 0f, 1f);
        }

        [UIAction("brightness-dec")]
        private void BrightnessDecrease()
        {
            Brightness = Mathf.Clamp(Brightness - GetStep(), 0f, 1f);
        }

        private float GetStep()
        {
            switch (PluginConfig.Instance.StepIndex)
            {
                case 1: return 0.05f;
                case 2: return 0.01f;
                default: return 0.1f;
            }
        }
    }
}
