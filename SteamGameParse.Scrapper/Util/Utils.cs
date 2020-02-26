using System;
using System.Net;
using System.Text.RegularExpressions;

namespace SteamGameParse.Scrapper.Util
{
    internal static class Utils
    {
        private static readonly Lazy<Regex> HtmlStripRegex =
            new Lazy<Regex>(() => new Regex(@"<[^>]*>", RegexOptions.Compiled | RegexOptions.IgnoreCase));

        private static readonly Lazy<Regex> HtmlNewLineRegex = new Lazy<Regex>(() =>
            new Regex(@"</?[brph0-9]{1,2}[^a-z0-9].*?>", RegexOptions.Compiled | RegexOptions.IgnoreCase));

        private static readonly Lazy<Regex> HtmlNewLineCompressionRegex =
            new Lazy<Regex>(() => new Regex("[\r\n]+", RegexOptions.Compiled));

        public static string StripHtml(dynamic content, bool removeRepeatingLines = true)
        {
            if (content == null)
                return null;
            var contentStr = content.ToString();
            if (string.IsNullOrEmpty(contentStr))
                return contentStr;
            contentStr = WebUtility.HtmlDecode(contentStr);
            contentStr = HtmlNewLineRegex.Value.Replace(contentStr, Environment.NewLine);
            contentStr = HtmlStripRegex.Value.Replace(contentStr, string.Empty);
            if (removeRepeatingLines)
                contentStr = HtmlNewLineCompressionRegex.Value.Replace(contentStr, Environment.NewLine);
            return contentStr.Trim();
            
        }

        public static string RemoveSubString(string str, string subStr)
        {
            var n = str.IndexOf(subStr, StringComparison.Ordinal);
            return str.Remove(n, subStr.Length);;
        }
    }
}