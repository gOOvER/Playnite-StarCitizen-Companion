using Playnite.SDK;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace StarCitizenCompanion
{
    public static class GameMaintenance
    {
        private static readonly ILogger logger = LogManager.GetLogger();

        public static int ClearShaderCache()
        {
            int deletedCount = 0;
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var scLocal = Path.Combine(localAppData, "Star Citizen");

            if (!Directory.Exists(scLocal)) return 0;

            try
            {
                foreach (var dir in Directory.GetDirectories(scLocal))
                {
                    try
                    {
                        Directory.Delete(dir, true);
                        deletedCount++;
                    }
                    catch { }
                }

                foreach (var file in Directory.GetFiles(scLocal))
                {
                    try
                    {
                        File.Delete(file);
                        deletedCount++;
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                logger.Warn(ex, "Error while cleaning Star Citizen shader cache");
            }

            return deletedCount;
        }

        public static bool OpenScreenshotsFolder(string gameInstallDir)
        {
            var candidates = new[]
            {
                Path.Combine(gameInstallDir ?? "", "ScreenShots"),
                Path.Combine(gameInstallDir ?? "", "screenshots"),
                @"J:\StarCitizen\LIVE\ScreenShots",
                @"J:\StarCitizen\LIVE\screenshots"
            };

            foreach (var path in candidates)
            {
                if (Directory.Exists(path))
                {
                    Process.Start("explorer.exe", "\"" + path + "\"");
                    return true;
                }
            }
            return false;
        }

        public static bool OpenLogBackupsFolder(string gameInstallDir)
        {
            var candidates = new[]
            {
                Path.Combine(gameInstallDir ?? "", "logbackups"),
                @"J:\StarCitizen\LIVE\logbackups"
            };

            foreach (var path in candidates)
            {
                if (Directory.Exists(path))
                {
                    Process.Start("explorer.exe", "\"" + path + "\"");
                    return true;
                }
            }
            return false;
        }

        public static bool OpenGameLog(string gameInstallDir)
        {
            var candidates = new[]
            {
                Path.Combine(gameInstallDir ?? "", "Game.log"),
                @"J:\StarCitizen\LIVE\Game.log"
            };

            foreach (var file in candidates)
            {
                if (File.Exists(file))
                {
                    Process.Start("notepad.exe", "\"" + file + "\"");
                    return true;
                }
            }
            return false;
        }

        public static bool OpenUserCfg(string gameInstallDir)
        {
            var candidates = new[]
            {
                Path.Combine(gameInstallDir ?? "", "user.cfg"),
                @"J:\StarCitizen\LIVE\user.cfg"
            };

            foreach (var file in candidates)
            {
                if (File.Exists(file))
                {
                    Process.Start("notepad.exe", "\"" + file + "\"");
                    return true;
                }
            }
            return false;
        }

        public static bool BackupActionMaps(string gameInstallDir)
        {
            var candidates = new[]
            {
                Path.Combine(gameInstallDir ?? "", "user", "client", "0", "Controls", "Mappings", "actionmaps.xml"),
                @"J:\StarCitizen\LIVE\user\client\0\Controls\Mappings\actionmaps.xml"
            };

            foreach (var file in candidates)
            {
                if (File.Exists(file))
                {
                    var backupDir = Path.Combine(Path.GetDirectoryName(file), "Backups");
                    Directory.CreateDirectory(backupDir);
                    var dest = Path.Combine(backupDir, string.Format("actionmaps_{0:yyyyMMdd_HHmmss}.xml", DateTime.Now));
                    File.Copy(file, dest, true);
                    return true;
                }
            }
            return false;
        }

        public static void SetDisplayInfo(string gameInstallDir, int level)
        {
            var candidates = new[]
            {
                Path.Combine(gameInstallDir ?? "", "user.cfg"),
                @"J:\StarCitizen\LIVE\user.cfg"
            };

            foreach (var cfgPath in candidates)
            {
                var dir = Path.GetDirectoryName(cfgPath);
                if (!Directory.Exists(dir)) continue;

                var displayInfoLine = string.Format("r_displayinfo = {0}", level);

                if (File.Exists(cfgPath))
                {
                    var text = File.ReadAllText(cfgPath);
                    var regex = new Regex(@"^\s*r_displayinfo\s*=.*$", RegexOptions.Multiline | RegexOptions.IgnoreCase);
                    string updated;
                    if (regex.IsMatch(text))
                    {
                        updated = regex.Replace(text, displayInfoLine);
                    }
                    else
                    {
                        updated = text.TrimEnd() + "\r\n" + displayInfoLine + "\r\n";
                    }
                    File.WriteAllText(cfgPath, updated);
                }
                else
                {
                    File.WriteAllText(cfgPath, displayInfoLine + "\r\n");
                }
                break;
            }
        }
    }
}
