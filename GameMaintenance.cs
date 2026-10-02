using Playnite.SDK;
using System;
using System.Diagnostics;
using System.IO;

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
                Path.Combine(gameInstallDir ?? "", "screenshots"),
                Path.Combine(gameInstallDir ?? "", "ScreenShots"),
                @"J:\StarCitizen\LIVE\screenshots"
            };

            foreach (var path in candidates)
            {
                if (Directory.Exists(path))
                {
                    Process.Start("explorer.exe", path);
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
                    var dest = Path.Combine(backupDir, $"actionmaps_{DateTime.Now:yyyyMMdd_HHmmss}.xml");
                    File.Copy(file, dest, true);
                    return true;
                }
            }
            return false;
        }
    }
}
