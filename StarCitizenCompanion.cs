using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace StarCitizenCompanion
{
    public class StarCitizenCompanion : GenericPlugin
    {
        private static readonly ILogger logger = LogManager.GetLogger();

        private StarCitizenCompanionSettingsViewModel settings { get; set; }

        public override Guid Id { get; } = Guid.Parse("24a1d01b-6c77-42d6-a16a-428e6216e5be");

        public StarCitizenCompanion(IPlayniteAPI api) : base(api)
        {
            settings = new StarCitizenCompanionSettingsViewModel(this);
            Properties = new GenericPluginProperties
            {
                HasSettings = true
            };
        }

        public string GetPluginFolder()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

        private bool IsStarCitizen(Game game)
        {
            if (game == null) return false;
            return (!string.IsNullOrEmpty(game.GameId) && game.GameId.StartsWith("RSI_SC_", StringComparison.OrdinalIgnoreCase))
                || (!string.IsNullOrEmpty(game.Name) && game.Name.IndexOf("Star Citizen", StringComparison.OrdinalIgnoreCase) >= 0);
        }

        public override void OnGameStarting(OnGameStartingEventArgs args)
        {
            if (!IsStarCitizen(args.Game)) return;

            if (settings.Settings.AutoStartSCLogMate)
            {
                Task.Run(() =>
                {
                    if (!SCLogMateBridge.IsSCLogMateRunning())
                    {
                        var started = SCLogMateBridge.StartSCLogMate(settings.Settings.SCLogMatePath);
                        if (started)
                        {
                            logger.Info("SCLogMate successfully launched for Star Citizen.");
                        }
                    }
                });
            }
        }

        public override void OnGameStopped(OnGameStoppedEventArgs args)
        {
            if (!IsStarCitizen(args.Game)) return;

            if (settings.Settings.ShowFlightDebrief)
            {
                Task.Run(async () =>
                {
                    // Kurz warten, bis SCLogMate die letzten Logs in sessions.db geschrieben hat
                    await Task.Delay(3000);

                    var debrief = SCLogMateBridge.GetLatestDebrief();
                    if (debrief != null)
                    {
                        var durationStr = string.Format("{0:D2}h {1:D2}m", (int)debrief.Duration.TotalHours, debrief.Duration.Minutes);
                        var profitStr = debrief.Profit >= 0 ? $"+{debrief.Profit:N0}" : $"{debrief.Profit:N0}";
                        var shipStr = !string.IsNullOrEmpty(debrief.LastShip) ? debrief.LastShip : "Unbekanntes Schiff";

                        var message = $"🚀 Schiff: {shipStr}\n" +
                                      $"⏱️ Flugzeit: {durationStr}\n" +
                                      $"💰 Bilanz: {profitStr} aUEC\n" +
                                      $"🎯 Missionen: {debrief.MissionsCompleted} | 💀 Tode: {debrief.Deaths}\n" +
                                      $"🏦 Neuer Kontostand: {debrief.CurrentBalance:N0} aUEC";

                        PlayniteApi.Notifications.Add(new NotificationMessage(
                            $"sc_debrief_{DateTime.Now.Ticks}",
                            $"Star Citizen · Flugbericht ({args.Game.Name})\n{message}",
                            NotificationType.Info
                        ));
                    }
                });
            }
        }

        public override IEnumerable<TopPanelItem> GetTopPanelItems()
        {
            if (!settings.Settings.ShowRsiStatusTopPanel) yield break;

            var iconPath = Path.Combine(GetPluginFolder(), "icon.png");

            yield return new TopPanelItem
            {
                Icon = iconPath,
                Title = "RSI Status",
                Visible = true,
                Activated = async () =>
                {
                    var status = await RsiStatusService.GetStatusAsync(true);
                    var details = status.GetStatusText();
                    foreach (var sys in status.Systems)
                    {
                        details += $"\n• {sys.Name}: {sys.Status}";
                    }

                    var clicked = PlayniteApi.Dialogs.ShowMessage(
                        $"RSI Server Status:\n\n{details}\n\nMöchtest du die offizielle Statusseite im Browser öffnen?",
                        "RSI Status",
                        System.Windows.MessageBoxButton.YesNo,
                        System.Windows.MessageBoxImage.Information
                    );

                    if (clicked == System.Windows.MessageBoxResult.Yes)
                    {
                        Process.Start(new ProcessStartInfo("https://status.robertsspaceindustries.com/") { UseShellExecute = true });
                    }
                }
            };
        }

        public override IEnumerable<GameMenuItem> GetGameMenuItems(GetGameMenuItemsArgs args)
        {
            var scGames = new List<Game>();
            foreach (var g in args.Games)
            {
                if (IsStarCitizen(g)) scGames.Add(g);
            }

            if (scGames.Count == 0) yield break;

            var firstGame = scGames[0];

            yield return new GameMenuItem
            {
                MenuSection = "Star Citizen Tools",
                Description = "🧹 Shader-Cache bereinigen",
                Action = (a) =>
                {
                    var count = GameMaintenance.ClearShaderCache();
                    PlayniteApi.Dialogs.ShowMessage(
                        $"Shader-Cache wurde bereinigt ({count} Elemente gelöscht).",
                        "Star Citizen Wartung",
                        System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Information
                    );
                }
            };

            yield return new GameMenuItem
            {
                MenuSection = "Star Citizen Tools",
                Description = "💾 Tastenbelegung (actionmaps.xml) sichern",
                Action = (a) =>
                {
                    var ok = GameMaintenance.BackupActionMaps(firstGame.InstallDirectory);
                    if (ok)
                    {
                        PlayniteApi.Dialogs.ShowMessage(
                            "actionmaps.xml wurde erfolgreich gesichert!",
                            "Star Citizen Backup",
                            System.Windows.MessageBoxButton.OK,
                            System.Windows.MessageBoxImage.Information
                        );
                    }
                    else
                    {
                        PlayniteApi.Dialogs.ShowMessage(
                            "actionmaps.xml konnte nicht gefunden werden.",
                            "Star Citizen Backup",
                            System.Windows.MessageBoxButton.OK,
                            System.Windows.MessageBoxImage.Warning
                        );
                    }
                }
            };

            yield return new GameMenuItem
            {
                MenuSection = "Star Citizen Tools",
                Description = "📸 Screenshots-Ordner öffnen",
                Action = (a) =>
                {
                    GameMaintenance.OpenScreenshotsFolder(firstGame.InstallDirectory);
                }
            };

            yield return new GameMenuItem
            {
                MenuSection = "Star Citizen Tools",
                Description = "🚀 SCLogMate öffnen",
                Action = (a) =>
                {
                    SCLogMateBridge.StartSCLogMate(settings.Settings.SCLogMatePath);
                }
            };

            yield return new GameMenuItem
            {
                MenuSection = "Star Citizen Tools",
                Description = "🌐 Erkul DPS Calculator",
                Action = (a) =>
                {
                    Process.Start(new ProcessStartInfo("https://www.erkul.games/live/calculator") { UseShellExecute = true });
                }
            };
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return settings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new StarCitizenCompanionSettingsView();
        }
    }
}
