namespace Namotion.Reflection
{
    internal static class StringExtensions
    {
        public static string FirstToken(this string s, char splitChar)
        {
            var idx = s.IndexOf(splitChar);
            return idx != -1 ? s.Substring(0, idx) : s;
        }

        public static string LastToken(this string s, char splitChar)
        {
            var idx = s.LastIndexOf(splitChar);
            return idx != -1 ? s.Substring(idx + 1) : s;
        }
    }
}