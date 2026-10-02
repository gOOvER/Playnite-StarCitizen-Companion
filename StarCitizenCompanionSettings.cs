using Playnite.SDK;
using Playnite.SDK.Data;
using System.Collections.Generic;

namespace StarCitizenCompanion
{
    public class StarCitizenCompanionSettings : ObservableObject
    {
        private bool autoStartSCLogMate = true;
        private bool showFlightDebrief = true;
        private bool showRsiStatusTopPanel = true;
        private string scLogMatePath = @"X:\Projekte\SCVerse\SCLogMate\publish\SCLogMate.exe";

        public bool AutoStartSCLogMate
        {
            get => autoStartSCLogMate;
            set => SetValue(ref autoStartSCLogMate, value);
        }

        public bool ShowFlightDebrief
        {
            get => showFlightDebrief;
            set => SetValue(ref showFlightDebrief, value);
        }

        public bool ShowRsiStatusTopPanel
        {
            get => showRsiStatusTopPanel;
            set => SetValue(ref showRsiStatusTopPanel, value);
        }

        public string SCLogMatePath
        {
            get => scLogMatePath;
            set => SetValue(ref scLogMatePath, value);
        }
    }

    public class StarCitizenCompanionSettingsViewModel : ObservableObject, ISettings
    {
        private readonly StarCitizenCompanion plugin;
        private StarCitizenCompanionSettings editingClone { get; set; }

        private StarCitizenCompanionSettings settings;
        public StarCitizenCompanionSettings Settings
        {
            get => settings;
            set
            {
                settings = value;
                OnPropertyChanged();
            }
        }

        public StarCitizenCompanionSettingsViewModel(StarCitizenCompanion plugin)
        {
            this.plugin = plugin;
            var savedSettings = plugin.LoadPluginSettings<StarCitizenCompanionSettings>();
            if (savedSettings != null)
            {
                Settings = savedSettings;
            }
            else
            {
                Settings = new StarCitizenCompanionSettings();
            }
        }

        public void BeginEdit()
        {
            editingClone = Serialization.GetClone(Settings);
        }

        public void CancelEdit()
        {
            Settings = editingClone;
        }

        public void EndEdit()
        {
            plugin.SavePluginSettings(Settings);
        }

        public bool VerifySettings(out List<string> errors)
        {
            errors = new List<string>();
            return true;
        }
    }
}
