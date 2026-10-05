using System;
using System.IO;
using System.Text;
using Playnite.SDK;

namespace StarCitizenCompanion
{
    public static class FlightLogStorage
    {
        private static readonly ILogger logger = LogManager.GetLogger();

        public static string GetFlightLogPath(string pluginUserDataPath)
        {
            var dir = !string.IsNullOrEmpty(pluginUserDataPath)
                ? pluginUserDataPath
                : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Playnite", "StarCitizenCompanion");

            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            return Path.Combine(dir, "Flugberichte.md");
        }

        public static void AppendDebrief(string pluginUserDataPath, FlightDebrief debrief, string gameName)
        {
            if (debrief == null) return;

            try
            {
                var filePath = GetFlightLogPath(pluginUserDataPath);
                var durationStr = string.Format("{0:D2}h {1:D2}m", (int)debrief.Duration.TotalHours, debrief.Duration.Minutes);
                var profitStr = debrief.Profit >= 0 ? $"+{debrief.Profit:N0}" : $"{debrief.Profit:N0}";
                var shipStr = !string.IsNullOrEmpty(debrief.LastShip) ? debrief.LastShip : "Unbekanntes Schiff";
                var timeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm");

                var isNewFile = !File.Exists(filePath);
                var sb = new StringBuilder();

                if (isNewFile)
                {
                    sb.AppendLine("# 🚀 Star Citizen Flugbuch (Playnite Companion)");
                    sb.AppendLine("*Automatisch erfasste Flugberichte nach Spielende über SCLogMate*");
                    sb.AppendLine();
                }

                sb.AppendLine($"## 🛸 Flugbericht — {timeStamp} ({gameName ?? "Star Citizen"})");
                sb.AppendLine($"- **Session**: `{debrief.SessionName}`");
                sb.AppendLine($"- **Schiff**: {shipStr}");
                sb.AppendLine($"- **Flugzeit**: {durationStr}");
                sb.AppendLine($"- **Bilanz**: **{profitStr} aUEC**");
                sb.AppendLine($"- **Missionen abgeschlossen**: {debrief.MissionsCompleted}");
                sb.AppendLine($"- **Kills / Tode**: ⚔️ {debrief.Kills} | 💀 {debrief.Deaths}");
                sb.AppendLine($"- **Kontostand**: {debrief.CurrentBalance:N0} aUEC");
                sb.AppendLine();
                sb.AppendLine("---");
                sb.AppendLine();

                File.AppendAllText(filePath, sb.ToString(), Encoding.UTF8);
                logger.Info($"Appended flight debrief to {filePath}");
            }
            catch (Exception ex)
            {
                logger.Warn(ex, "Failed to append flight debrief to log file");
            }
        }
    }
}
