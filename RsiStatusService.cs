using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StarCitizenCompanion
{
    public enum RsiSystemStatus
    {
        Operational,
        Degraded,
        Down,
        Unknown
    }

    public class RsiSystemInfo
    {
        public string Name { get; set; }
        public RsiSystemStatus Status { get; set; }
    }

    public class RsiStatusResult
    {
        public RsiSystemStatus SummaryStatus { get; set; } = RsiSystemStatus.Unknown;
        public string Title { get; set; } = "RSI Status";
        public List<RsiSystemInfo> Systems { get; set; } = new List<RsiSystemInfo>();
        public DateTime LastChecked { get; set; } = DateTime.MinValue;

        public string GetStatusText()
        {
            switch (SummaryStatus)
            {
                case RsiSystemStatus.Operational: return "Operational (Alle Systeme online)";
                case RsiSystemStatus.Degraded: return "Degraded (Eingeschränkte Leistung)";
                case RsiSystemStatus.Down: return "Outage / Wartung";
                default: return "Status unbekannt";
            }
        }
    }

    public static class RsiStatusService
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private static readonly HttpClient httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(8) };
        private const string StatusJsonUrl = "https://status.robertsspaceindustries.com/index.json";

        private static RsiStatusResult cachedStatus;
        private static DateTime lastFetch = DateTime.MinValue;

        public static async Task<RsiStatusResult> GetStatusAsync(bool forceRefresh = false)
        {
            if (!forceRefresh && cachedStatus != null && (DateTime.UtcNow - lastFetch).TotalMinutes < 10)
            {
                return cachedStatus;
            }

            try
            {
                httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Playnite-StarCitizen-Companion/1.0");
                var json = await httpClient.GetStringAsync(StatusJsonUrl);

                var result = new RsiStatusResult
                {
                    LastChecked = DateTime.Now
                };

                var summaryMatch = Regex.Match(json, @"[""']summaryStatus[""']s*:s*[""']([^""']+)[""']");
                if (summaryMatch.Success)
                {
                    result.SummaryStatus = ParseStatus(summaryMatch.Groups[1].Value);
                }

                var systemMatches = Regex.Matches(json, @"{s*[""']name[""']s*:s*[""']([^""']+)[""'].*?[""']status[""']s*:s*[""']([^""']+)[""']", RegexOptions.Singleline);
                foreach (Match m in systemMatches)
                {
                    var name = m.Groups[1].Value;
                    var status = ParseStatus(m.Groups[2].Value);
                    result.Systems.Add(new RsiSystemInfo { Name = name, Status = status });
                }

                cachedStatus = result;
                lastFetch = DateTime.UtcNow;
                return result;
            }
            catch (Exception ex)
            {
                logger.Warn(ex, "Failed to fetch RSI server status from " + StatusJsonUrl);
                return cachedStatus ?? new RsiStatusResult { SummaryStatus = RsiSystemStatus.Unknown };
            }
        }

        private static RsiSystemStatus ParseStatus(string val)
        {
            if (string.IsNullOrEmpty(val)) return RsiSystemStatus.Unknown;
            switch (val.ToLowerInvariant())
            {
                case "operational": return RsiSystemStatus.Operational;
                case "degraded":
                case "disrupted": return RsiSystemStatus.Degraded;
                case "down":
                case "maintenance":
                case "outage": return RsiSystemStatus.Down;
                default: return RsiSystemStatus.Unknown;
            }
        }
    }
}
