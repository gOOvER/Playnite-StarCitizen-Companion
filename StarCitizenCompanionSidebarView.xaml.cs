using Playnite.SDK;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StarCitizenCompanion
{
    public partial class StarCitizenCompanionSidebarView : UserControl
    {
        private readonly IPlayniteAPI _api;
        private readonly StarCitizenCompanionSettingsViewModel _settings;
        private readonly string _defaultLiveDir = @"J:\StarCitizen\LIVE";

        public StarCitizenCompanionSidebarView(IPlayniteAPI api, StarCitizenCompanionSettingsViewModel settings)
        {
            InitializeComponent();
            _api = api;
            _settings = settings;

            Loaded += OnLoaded;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            await RefreshAllAsync();
        }

        private async Task RefreshAllAsync()
        {
            await LoadRsiStatusAsync();
            LoadSCLogMateInfo();
            LoadFlightDebriefInfo();
        }

        private void LoadFlightDebriefInfo()
        {
            try
            {
                var debrief = SCLogMateBridge.GetLatestDebrief();
                if (debrief != null)
                {
                    var durationStr = string.Format("{0:D2}h {1:D2}m", (int)debrief.Duration.TotalHours, debrief.Duration.Minutes);
                    var profitStr = debrief.Profit >= 0 ? $"+{debrief.Profit:N0}" : $"{debrief.Profit:N0}";
                    var shipStr = !string.IsNullOrEmpty(debrief.LastShip) ? debrief.LastShip : "Unbekanntes Schiff";
                    var timeStr = debrief.EndTime?.ToLocalTime().ToString("dd.MM.yyyy HH:mm") ?? debrief.StartTime?.ToLocalTime().ToString("dd.MM.yyyy HH:mm") ?? "Kürzlich";

                    TxtDebriefSession.Text = $"Session: {debrief.SessionName} ({timeStr})";
                    TxtDebriefShip.Text = $"🚀 Schiff: {shipStr}";
                    TxtDebriefDuration.Text = $"⏱️ Flugzeit: {durationStr}";
                    TxtDebriefMissions.Text = $"🎯 Missionen: {debrief.MissionsCompleted}";
                    TxtDebriefProfit.Text = $"💰 Bilanz: {profitStr} aUEC";
                    TxtDebriefProfit.Foreground = debrief.Profit >= 0
                        ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#81c784"))
                        : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#e57373"));
                    TxtDebriefCombat.Text = $"⚔️ Kills: {debrief.Kills} | 💀 Tode: {debrief.Deaths}";
                    TxtDebriefBalance.Text = $"🏦 Kontostand: {debrief.CurrentBalance:N0} aUEC";
                }
                else
                {
                    TxtDebriefSession.Text = "Noch kein Flugbericht in SCLogMate erfasst.";
                }
            }
            catch
            {
                TxtDebriefSession.Text = "Fehler beim Laden des Flugberichts.";
            }
        }

        private void BtnOpenFlightLog_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var logPath = FlightLogStorage.GetFlightLogPath(_settings.Plugin.GetPluginUserDataPath());
                if (!File.Exists(logPath))
                {
                    var debrief = SCLogMateBridge.GetLatestDebrief();
                    if (debrief != null)
                    {
                        FlightLogStorage.AppendDebrief(_settings.Plugin.GetPluginUserDataPath(), debrief, "Star Citizen");
                    }
                    else
                    {
                        File.WriteAllText(logPath, "# 🚀 Star Citizen Flugbuch (Playnite Companion)\n\n*Noch keine Flugberichte erfasst.*\n", System.Text.Encoding.UTF8);
                    }
                }
                Process.Start(new ProcessStartInfo(logPath) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                _api.Dialogs.ShowErrorMessage("Fehler beim Öffnen des Flugbuchs: " + ex.Message, "Flugbuch Fehler");
            }
        }

        private async Task LoadRsiStatusAsync()
        {
            TxtRsiStatus.Text = "Prüfe RSI Serverstatus...";
            TxtRsiDetails.Text = "";

            try
            {
                var status = await RsiStatusService.GetStatusAsync(true);
                TxtRsiStatus.Text = status.GetStatusText();
                TxtRsiStatus.Foreground = (status.SummaryStatus == RsiSystemStatus.Operational)
                    ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#81c784"))
                    : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#e57373"));

                var details = "";
                foreach (var s in status.Systems)
                {
                    details += string.Format("• {0}: {1}\n", s.Name, s.Status);
                }
                TxtRsiDetails.Text = details.TrimEnd();
            }
            catch (Exception ex)
            {
                TxtRsiStatus.Text = "Fehler beim Laden des Serverstatus.";
                TxtRsiStatus.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#e57373"));
                TxtRsiDetails.Text = ex.Message;
            }
        }

        private void LoadSCLogMateInfo()
        {
            var isRunning = SCLogMateBridge.IsSCLogMateRunning();
            TxtLogMateStatus.Text = isRunning ? "Status: Aktiv (läuft im Hintergrund)" : "Status: Nicht gestartet";
            TxtLogMateStatus.Foreground = isRunning 
                ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#81c784"))
                : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#b0bec5"));

            var balance = SCLogMateBridge.GetCurrentBalance();
            if (balance > 0)
            {
                TxtWalletBalance.Text = string.Format("Aktueller Kontostand: {0:N0} aUEC", balance);
            }
            else
            {
                TxtWalletBalance.Text = "Noch kein Kontostand erfasst (wird nach erster Session aktualisiert)";
            }
        }

        private async void BtnRefreshStatus_Click(object sender, RoutedEventArgs e)
        {
            await RefreshAllAsync();
        }

        private void BtnOpenRsiStatus_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://status.robertsspaceindustries.com/") { UseShellExecute = true });
        }

        private void BtnStartSCLogMate_Click(object sender, RoutedEventArgs e)
        {
            SCLogMateBridge.StartSCLogMate(_settings.Settings.SCLogMatePath);
            LoadSCLogMateInfo();
        }

        private void BtnOpenSessionsFolder_Click(object sender, RoutedEventArgs e)
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var folder = Path.Combine(appData, "SCLogMate");
            if (Directory.Exists(folder))
            {
                Process.Start("explorer.exe", "\"" + folder + "\"");
            }
        }

        private void BtnClearShader_Click(object sender, RoutedEventArgs e)
        {
            var count = GameMaintenance.ClearShaderCache();
            _api.Dialogs.ShowMessage(
                string.Format("Shader-Cache wurde bereinigt ({0} Elemente gelöscht).", count),
                "Star Citizen Wartung",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
        }

        private void BtnBackupKeybinds_Click(object sender, RoutedEventArgs e)
        {
            var ok = GameMaintenance.BackupActionMaps(_defaultLiveDir);
            if (ok)
            {
                _api.Dialogs.ShowMessage("actionmaps.xml wurde gesichert!", "Star Citizen Backup", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                _api.Dialogs.ShowMessage("actionmaps.xml nicht gefunden.", "Star Citizen Backup", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnOpenScreenshots_Click(object sender, RoutedEventArgs e)
        {
            GameMaintenance.OpenScreenshotsFolder(_defaultLiveDir);
        }

        private void BtnOpenLogBackups_Click(object sender, RoutedEventArgs e)
        {
            GameMaintenance.OpenLogBackupsFolder(_defaultLiveDir);
        }

        private void BtnOpenGameLog_Click(object sender, RoutedEventArgs e)
        {
            GameMaintenance.OpenGameLog(_defaultLiveDir);
        }

        private void BtnOpenSettings_Click(object sender, RoutedEventArgs e)
        {
            _api.MainView.OpenPluginSettings(_settings.Plugin.Id);
        }

        private void BtnOpenErkul_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://www.erkul.games/live/calculator") { UseShellExecute = true });
        }

        private void BtnOpenSCTrade_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://sc-trade.tools/") { UseShellExecute = true });
        }

        private void BtnOpenCommLink_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://robertsspaceindustries.com/comm-link") { UseShellExecute = true });
        }
    }
}
