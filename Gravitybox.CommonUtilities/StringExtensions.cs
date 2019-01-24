using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Xml;

namespace Gravitybox.CommonUtilities
{
    public static class StringExtensions
    {
        public const string YMDFormat = "yyyy-MM-dd";
        public const string YMDHMSFormat = "yyyy-MM-dd HH:mm:ss";

        /// <summary>
        /// Add commas to a number
        /// </summary>
        public static string FormatWholeNumber(this int number)
        {
            return number.ToString("N0");
        }

        public static void AppendCData(this XmlTextWriter writer, string tag, string data)
        {
            writer.WriteStartElement(tag);
            writer.WriteCData(data);
            writer.WriteEndElement();
        }

        public static string ToJSON(this object obj)
        {
            var serializer = new JavaScriptSerializer();
            return serializer.Serialize(obj);
        }

        public static T FromJson<T>(this string str)
        {
            var serializer = new JavaScriptSerializer();
            return serializer.Deserialize<T>(str);
        }

        public static bool IsNumber(this string v)
        {
            decimal d;
            return (decimal.TryParse(v, out d));
        }

        public static bool IsDate(this string v)
        {
            DateTime d;
            return (DateTime.TryParse(v, out d));
        }

        public static bool IsBool(this string v)
        {
            bool b;
            return (bool.TryParse(v.ToLower(), out b));
        }

        public static List<string> BreakLines(this string text)
        {
            if (string.IsNullOrEmpty(text)) return new List<string>();
            return text.Replace("\x01", string.Empty).Split(new[] { "\r\n" }, StringSplitOptions.None).ToList();
        }

        public static string ToStringBlock(this IEnumerable<string> strArray)
        {
            var builder = new StringBuilder();
            strArray.ToList().ForEach(x => builder.AppendLine(x));
            return builder.ToString();
        }

        /// <summary>
        /// Performs a trim on all lines in list
        /// </summary>
        public static List<string> TrimAll(this IEnumerable<string> strArray)
        {
            var retval = new List<string>();
            foreach (var s in strArray)
                retval.Add(s.Trim().Trim(new char[] { '\t', ' ' }).Trim());
            return retval;
        }

        public static bool IsSet(this string str)
        {
            return !string.IsNullOrEmpty(str);
        }

        public static List<int> IndexWith(this IEnumerable<string> strArray, IEnumerable<string> check, StringComparison compare)
        {
            var retval = new List<int>();
            foreach (var str in check)
            {
                var index = 0;
                foreach (var s in strArray)
                {
                    if (s.Contains(str, compare))
                        retval.Add(index);
                    index++;
                }
            }
            return retval.Distinct().ToList();
        }

        /// <summary>
        /// Given a list of string find the indexes that contain the parameter
        /// </summary>
        public static List<int> IndexWith(this IEnumerable<string> strArray, string str, StringComparison compare)
        {
            var retval = new List<int>();
            var index = 0;
            foreach (var s in strArray)
            {
                if (s.Contains(str, compare))
                    retval.Add(index);
                index++;
            }
            return retval;
        }

        //Trim empty lines from top/bottom of string list
        public static List<string> TrimLines(this List<string> strArray)
        {
            var retval = strArray.ToList();

            for (var ii = 0; ii < retval.Count; ii++)
            {
                var s = retval[0];
                if (s != string.Empty)
                {
                    break;
                }
                else if (s == string.Empty)
                {
                    retval.RemoveAt(0);
                }
            }

            for (var ii = retval.Count - 1; ii >= 0; ii--)
            {
                var s = retval[retval.Count - 1];
                if (s != string.Empty)
                {
                    break;
                }
                else if (s == string.Empty)
                {
                    retval.RemoveAt(retval.Count - 1);
                }
            }

            return retval.ToList();
        }

        private static string TrimStr(this string str, string trimStr,
         bool trimEnd = true, bool repeatTrim = true,
         StringComparison comparisonType = StringComparison.OrdinalIgnoreCase)
        {
            int strLen;
            do
            {
                if (!(str ?? "").EndsWith(trimStr)) return str;
                strLen = str.Length;
                {
                    if (trimEnd)
                    {
                        var pos = str.LastIndexOf(trimStr, comparisonType);
                        if ((!(pos >= 0)) || (!(str.Length - trimStr.Length == pos))) break;
                        str = str.Substring(0, pos);
                    }
                    else
                    {
                        var pos = str.IndexOf(trimStr, comparisonType);
                        if (!(pos == 0)) break;
                        str = str.Substring(trimStr.Length, str.Length - trimStr.Length);
                    }
                }
            } while (repeatTrim && strLen > str.Length);
            return str;
        }

        public static string TrimEnd(this string str, string trimStr,
                bool repeatTrim = true,
                StringComparison comparisonType = StringComparison.OrdinalIgnoreCase)
                => TrimStr(str, trimStr, true, repeatTrim, comparisonType);

        public static string TrimStart(this string str, string trimStr,
                bool repeatTrim = true,
                StringComparison comparisonType = StringComparison.OrdinalIgnoreCase)
                => TrimStr(str, trimStr, false, repeatTrim, comparisonType);

        public static string Trim(this string str, string trimStr, bool repeatTrim = true,
            StringComparison comparisonType = StringComparison.OrdinalIgnoreCase)
            => str.TrimStart(trimStr, repeatTrim, comparisonType)
                  .TrimEnd(trimStr, repeatTrim, comparisonType);

        public static string ToYMD(this DateTime date)
        {
            return date.ToString(YMDFormat);
        }

        public static string ToYMDHMS(this DateTime date)
        {
            return date.ToString(YMDHMSFormat);
        }

        public static List<string> BreakWords(this string str, bool requireAlpha = false, bool includeHyphen = false)
        {
            if (string.IsNullOrEmpty(str)) return null;
            char[] breakChars = null;
            if (includeHyphen)
                breakChars = str.ToList().Where(x => !char.IsLetter(x) && !char.IsNumber(x)).Distinct().ToArray();
            else
                breakChars = str.ToList().Where(x => !char.IsLetter(x) && !char.IsNumber(x) && x != '-').Distinct().ToArray();

            List<string> retval = null;
            if (requireAlpha)
                retval = str.Split(breakChars, StringSplitOptions.RemoveEmptyEntries).Where(x => x.HasAlpha()).ToList();
            else
                retval = str.Split(breakChars, StringSplitOptions.RemoveEmptyEntries).ToList();

            //If a word starts with "-" then strip it
            for (var ii = 0; ii < retval.Count; ii++)
            {
                if (retval[ii].StartsWith("-"))
                    retval[ii] = retval[ii].Substring(1, retval[ii].Length - 1);
            }
            return retval;
        }

        /// <summary>
        /// Returns true if there are any alpha chars
        /// </summary>
        public static bool HasAlpha(this string str)
        {
            if (string.IsNullOrEmpty(str)) return false;
            return StripToAlpha(str).Length > 0;
        }

        public static string StripToAlpha(this string str)
        {
            if (string.IsNullOrEmpty(str)) return str;
            return new string(str.ToCharArray().Where(x => char.IsLetter(x)).ToArray());
        }

        public static bool IsWord(this string str)
        {
            if (string.IsNullOrEmpty(str)) return false;
            return !str.Contains(" ");
        }

        /// <summary>
        /// Determines if any strings in the array match the source string
        /// </summary>
        public static bool ContainsAny(this string value, params string[] parameters)
        {
            if (parameters == null || !parameters.Any()) return false;
            return parameters.Any(p => value.Contains(p, StringComparison.InvariantCultureIgnoreCase));
        }

        public static bool MatchAny(this string s, params string[] parameters)
        {
            if (parameters == null) return false;
            if (parameters.Length == 0) return false;
            if (s == null) return false;
            foreach (var t in parameters)
            {
                if (s.Match(t)) return true;
            }
            return false;
        }

        /// <summary>
        /// Concatenates the members of a list into a string with a comma as a separator
        /// </summary>
        internal static string ToCommaList<T>(this IEnumerable<T> list)
        {
            return list.ToStringList(",");
        }

        /// <summary>
        /// Concatenates the members of a list into a string with a separator
        /// </summary>
        public static string ToStringList<T>(this IEnumerable<T> list, string separator)
        {
            if (list == null || !list.Any()) return string.Empty;
            return string.Join(separator, list);
        }

        /// <summary />
        internal static long? ToInt64(this string v)
        {
            if (string.IsNullOrEmpty(v)) return null;
            if (long.TryParse(v, out long parsed))
                return parsed;
            return null;
        }

        public static string ToLinedString(this List<string> list)
        {
            try
            {
                if (list == null) return string.Empty;
                var sb = new StringBuilder();
                list.ForEach(x => sb.AppendLine(x));
                return sb.ToString();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// Strip to only numeric chars
        /// </summary>
        public static string StripToNumeric(this string str)
        {
            if (string.IsNullOrEmpty(str)) return str;
            return new string(str.ToCharArray().Where(x => char.IsNumber(x)).ToArray());
        }

        /// <summary>
        /// Remove all numeric chars
        /// </summary>
        public static string StripAllNumeric(this string str)
        {
            if (string.IsNullOrEmpty(str)) return str;
            return new string(str.ToCharArray().Where(x => !char.IsNumber(x)).ToArray());
        }

    }
}