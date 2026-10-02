using Playnite.SDK;
using System.Windows;
using System.Windows.Controls;

namespace StarCitizenCompanion
{
    public partial class StarCitizenCompanionSettingsView : UserControl
    {
        public StarCitizenCompanionSettingsView()
        {
            InitializeComponent();
        }

        private void BrowseSCLogMate_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is StarCitizenCompanionSettingsViewModel vm)
            {
                var res = API.Instance.Dialogs.SelectFile("SCLogMate.exe|SCLogMate.exe|Alle Dateien (*.*)|*.*");
                if (!string.IsNullOrEmpty(res))
                {
                    vm.Settings.SCLogMatePath = res;
                }
            }
        }
    }
}
