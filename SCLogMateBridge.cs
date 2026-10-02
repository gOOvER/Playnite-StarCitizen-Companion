using Microsoft.Data.Sqlite;
using Playnite.SDK;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace StarCitizenCompanion
{
    public class FlightDebrief
    {
        public string SessionName { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public TimeSpan Duration => (EndTime ?? DateTime.UtcNow) - (StartTime ?? DateTime.UtcNow);
        public long Profit { get; set; }
        public int MissionsCompleted { get; set; }
        public int Kills { get; set; }
        public int Deaths { get; set; }
        public string LastShip { get; set; }
        public long CurrentBalance { get; set; }
    }

    public static class SCLogMateBridge
    {
        private static readonly ILogger logger = LogManager.GetLogger();

        public static string GetDatabasePath()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var dbPath = Path.Combine(appData, "SCLogMate", "sessions.db");
            return File.Exists(dbPath) ? dbPath : null;
        }

        public static string GetSettingsPath()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var path = Path.Combine(appData, "SCLogMate", "settings.json");
            return File.Exists(path) ? path : null;
        }

        public static string FindSCLogMateExecutable(string customPath = null)
        {
            if (!string.IsNullOrWhiteSpace(customPath) && File.Exists(customPath))
            {
                return customPath;
            }

            var candidates = new[]
            {
                @"X:\Projekte\SCVerse\SCLogMate\publish\SCLogMate.exe",
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Programs\SCLogMate\SCLogMate.exe"),
                @"C:\Program Files\SCLogMate\SCLogMate.exe"
            };

            foreach (var c in candidates)
            {
                if (File.Exists(c)) return c;
            }

            return null;
        }

        public static bool IsSCLogMateRunning()
        {
            return Process.GetProcessesByName("SCLogMate").Length > 0;
        }

        public static bool StartSCLogMate(string customPath = null)
        {
            if (IsSCLogMateRunning()) return true;

            var exe = FindSCLogMateExecutable(customPath);
            if (!string.IsNullOrEmpty(exe) && File.Exists(exe))
            {
                try
                {
                    Process.Start(new ProcessStartInfo(exe) { UseShellExecute = true });
                    return true;
                }
                catch (Exception ex)
                {
                    logger.Warn(ex, "Failed to start SCLogMate from " + exe);
                }
            }
            return false;
        }

        public static long GetCurrentBalance()
        {
            var settingsPath = GetSettingsPath();
            if (settingsPath != null && File.Exists(settingsPath))
            {
                try
                {
                    var text = File.ReadAllText(settingsPath);
                    var match = Regex.Match(text, "\"Balance\"\\s*:\\s*(\\d+)");
                    if (match.Success && long.TryParse(match.Groups[1].Value, out var bal))
                    {
                        return bal;
                    }
                }
                catch (Exception ex)
                {
                    logger.Warn(ex, "Failed to read balance from settings");
                }
            }
            return 0;
        }

        public static FlightDebrief GetLatestDebrief()
        {
            var dbPath = GetDatabasePath();
            if (dbPath == null) return null;

            try
            {
                using (var conn = new SqliteConnection($"Data Source={dbPath};Mode=ReadOnly;"))
                {
                    conn.Open();

                    string sessionName = null;
                    string startStr = null;
                    string endStr = null;

                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "SELECT name, start, end FROM sessions ORDER BY start DESC LIMIT 1;";
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                sessionName = reader.GetString(0);
                                if (!reader.IsDBNull(1)) startStr = reader.GetString(1);
                                if (!reader.IsDBNull(2)) endStr = reader.GetString(2);
                            }
                        }
                    }

                    if (string.IsNullOrEmpty(sessionName)) return null;

                    var debrief = new FlightDebrief
                    {
                        SessionName = sessionName,
                        CurrentBalance = GetCurrentBalance()
                    };

                    if (DateTime.TryParse(startStr, out var st)) debrief.StartTime = st;
                    if (DateTime.TryParse(endStr, out var et)) debrief.EndTime = et;

                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"
                            SELECT 
                                COALESCE(SUM(amount), 0),
                                COUNT(CASE WHEN kind = 'Mission' AND amount > 0 THEN 1 END),
                                COUNT(CASE WHEN kind = 'Kill' THEN 1 END),
                                COUNT(CASE WHEN kind = 'Death' THEN 1 END)
                            FROM events WHERE session = @sess;
                        ";
                        cmd.Parameters.AddWithValue("@sess", sessionName);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                debrief.Profit = reader.GetInt64(0);
                                debrief.MissionsCompleted = reader.GetInt32(1);
                                debrief.Kills = reader.GetInt32(2);
                                debrief.Deaths = reader.GetInt32(3);
                            }
                        }
                    }

                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "SELECT ship FROM events WHERE session = @sess AND ship IS NOT NULL AND ship != '' ORDER BY time DESC LIMIT 1;";
                        cmd.Parameters.AddWithValue("@sess", sessionName);
                        var shipObj = cmd.ExecuteScalar();
                        if (shipObj != null && shipObj != DBNull.Value)
                        {
                            debrief.LastShip = shipObj.ToString();
                        }
                    }

                    return debrief;
                }
            }
            catch (Exception ex)
            {
                logger.Warn(ex, "Failed to read flight debrief from " + dbPath);
                return null;
            }
        }
    }
}
