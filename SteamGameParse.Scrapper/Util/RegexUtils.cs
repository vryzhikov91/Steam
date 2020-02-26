using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SteamGameParse.Scrapper.Util
{
    public static class RegexUtils
    {
        private static Dictionary<string, Regex> _regexCache = new Dictionary<string, Regex>();


        public static string GetGroup(this string regexStr, string matchStr, int groupId = 1)
        {
            if (string.IsNullOrEmpty(matchStr))
                return matchStr;
            var regex = GetRegex(regexStr);
            var m = regex.Match(matchStr);
            if (m.Success && m.Groups.Count > groupId)
            {
                return m.Groups[groupId]?.Value;
            }

            return null;
        }

        public static IEnumerable<string> GetGroupMany(this string regexStr, string matchStr, int groupId = 1)
        {
            if (string.IsNullOrEmpty(matchStr)) yield break;
            var regex = GetRegex(regexStr);
            var m = regex.Match(matchStr);
            while (m.Success)
            {
                yield return m.Groups[groupId]?.Value;
                m = m.NextMatch();
            }
        }


        public static Regex GetRegex(string regexStr)
        {
            if (_regexCache.TryGetValue(regexStr, out var regex)) return regex;
            regex = new Regex(regexStr, RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Multiline);
            _regexCache.Add(regexStr, regex);
            return regex;
        }
    }
}