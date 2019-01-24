using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace Gravitybox.CommonUtilities
{
    public static class StringUtilities
    {
        /// <summary>
        /// This will remove the target file if it exists and move the soruce file to that name
        /// </summary>
        public static bool MoveFile(string source, string target)
        {
            var index = 0;
            do
            {
                try
                {
                    //Delete old file
                    if (File.Exists(target))
                    {
                        File.Delete(target);
                        System.Threading.Thread.Sleep(1000);
                    }
                    File.Move(source, target);
                    return true;
                }
                catch (Exception ex)
                {
                    index++;
                    if (index > 3)
                        throw;
                    else
                        System.Threading.Thread.Sleep(1000);
                }
            } while (true);
        }

        /// <summary>
        /// Given a input and output file, this will compress the file
        /// </summary>
        public static void CompressFile(string inputFile, string zipFile)
        {
            var shortFile = (new FileInfo(inputFile)).Name;
            using (var zip = ZipFile.Open(zipFile, System.IO.Compression.ZipArchiveMode.Create))
                zip.CreateEntryFromFile(inputFile, shortFile);
        }

        public static bool DeleteFileWithRetry(string fileName, int retryCount, bool throwError = true)
        {
            if (!File.Exists(fileName)) return false;
            if (retryCount < 0) retryCount = 0;

            var success = false;
            var tryCount = 0;
            do
            {
                try
                {
                    File.Delete(fileName);
                    success = true;
                }
                catch
                {
                    if (tryCount < retryCount) System.Threading.Thread.Sleep(1000);
                    tryCount++;
                }
            } while (!success && (tryCount <= retryCount));

            if (throwError && !success)
                throw new Exception($"File '{fileName}' could not be deleted");

            return success;
        }

        /// <summary>
        /// Remove HTML from string with Regex.
        /// </summary>
        public static string StripHtmlTags(string source)
        {
            if (string.IsNullOrEmpty(source)) return source;

            //For line breaks try to insert a space so it looks nicer
            source = System.Text.RegularExpressions.Regex.Replace(source, "<br.*?/>", " ");

            source = System.Text.RegularExpressions.Regex.Replace(source, "<.*?>", string.Empty);
            source = source.Replace("&nbsp;", " ");
            return source;
        }

        /// <summary>
        /// This will remove all but a small set of HTML tags
        /// </summary>
        public static string CleanseHtml(this string text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            try
            {
                //Allowed tags: UL,OL,LI,B,STRONG

                //Replace line breaks
                text = text.Replace("<br>", "hsyq76s736fy463524cpe6d", StringComparison.InvariantCultureIgnoreCase);
                text = text.Replace("<br >", "hsyq76s736fy463524cpe6d", StringComparison.InvariantCultureIgnoreCase);
                text = text.Replace("<br/>", "hsyq76s736fy463524cpe6d", StringComparison.InvariantCultureIgnoreCase);
                text = text.Replace("<br />", "hsyq76s736fy463524cpe6d", StringComparison.InvariantCultureIgnoreCase);

                //build replacement list
                var replacements = new Dictionary<string, string>();
                replacements.Add("<b>", "19aud8736fy463524cpe6d");
                replacements.Add("</b>", "19auds76d342jhd4cpe6d");

                replacements.Add("<ul>", "19azz0d8815sd24cpe6d");
                replacements.Add("</ul>", "19ckifuyf62jd3524cpe6d");

                replacements.Add("<ol>", "19mmzocud927dgh4cpe6d");
                replacements.Add("</ol>", "1900a7sjhjw7266dtu44cpe6d");

                replacements.Add("<li>", "19auccjduejji6812kde6d");
                replacements.Add("</li>", "19aud8c8jv73hd73j56pe6d");

                replacements.Add("<strong>", "19auccsdsd2ddfff3jji6812kde6d");
                replacements.Add("</strong>", "19chsyd7726fffj56pe6d");

                replacements.Add("<img ", "sjs981ikd2ud263");
                replacements.Add(">", "71uuusyyq89xw");

                var oldText = text;
                try
                {
                    //Try to fix images
                    var imgLoops = 0;
                    while (imgLoops < 5 && text.Contains("<img ", StringComparison.InvariantCultureIgnoreCase))
                    {
                        var ii = text.IndexOf("<img ", StringComparison.InvariantCultureIgnoreCase);
                        var jj = text.IndexOf(">", ii + 1);
                        if (ii != -1 && jj != -1)
                        {
                            //Onlu support absolute path images
                            var middle = text.Substring(ii + 5, jj - ii - 5);
                            if (middle.Contains("src=\"http", StringComparison.InvariantCultureIgnoreCase) ||
                                middle.Contains("src=\'http", StringComparison.InvariantCultureIgnoreCase))
                            {
                                text = text.Substring(0, ii) +
                                    "sjs981ikd2ud263" +
                                    middle +
                                    "71uuusyyq89xw" +
                                    text.Substring(jj + 1, text.Length - jj - 1);
                            }
                        }
                        imgLoops++;
                    }
                }
                catch (Exception ex)
                {
                    //If error just skip all work here and revert to previous text
                    text = oldText;
                }

                //Kill all <p> tags and replace </p> with line break
                text = text.Replace("<p>", string.Empty, StringComparison.InvariantCultureIgnoreCase);
                text = text.Replace("</p>", "\r\n", StringComparison.InvariantCultureIgnoreCase);

                //Perform replacements
                foreach (var key in replacements.Keys)
                {
                    text = text.Replace(key, replacements[key], StringComparison.InvariantCultureIgnoreCase);
                }

                //Now look and see if there are any unmatched tags. If so remove the whole lot
                text = FixUnmatchedTag(text, replacements["<b>"], replacements["</b>"]);
                text = FixUnmatchedTag(text, replacements["<ul>"], replacements["</ul>"]);
                text = FixUnmatchedTag(text, replacements["<ol>"], replacements["</ol>"]);
                text = FixUnmatchedTag(text, replacements["<li>"], replacements["</li>"]);
                text = FixUnmatchedTag(text, replacements["<strong>"], replacements["</strong>"]);

                //Strip HTML
                text = StripHtmlTags(text);

                //Now replace the allowed tags
                foreach (var key in replacements.Keys)
                {
                    text = text.Replace(replacements[key], key, StringComparison.InvariantCultureIgnoreCase);
                }

                //Change <b> to <strong>
                text = text.Replace("<b>", "<strong>", StringComparison.InvariantCultureIgnoreCase);
                text = text.Replace("</b>", "</strong>", StringComparison.InvariantCultureIgnoreCase);

                //Convert all <br> to <br />
                text = text.Replace("hsyq76s736fy463524cpe6d", "<br />", StringComparison.InvariantCultureIgnoreCase);

                //Remove hard breaks inside of lists
                try
                {
                    var document = new XmlDocument();
                    document.LoadXml($"<a>{text}</a>");

                    var ulList = document.SelectNodes(".//ul").ToList();
                    ulList.AddRange(document.SelectNodes(".//ol").ToList());
                    foreach (XmlNode n in ulList)
                    {
                        var delNodes = new List<XmlNode>();
                        foreach (XmlNode c in n.ChildNodes)
                            if (c.Name == "br") delNodes.Add(c);
                        foreach (XmlNode c in delNodes)
                            n.RemoveChild(c);
                    }
                    text = document.DocumentElement.InnerXml;
                }
                catch { }

                return text.Trim();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// If token1 and token2 do not have same number of occurrances then remove both from string
        /// </summary>
        private static string FixUnmatchedTag(string text, string token1, string token2)
        {
            var c1 = System.Text.RegularExpressions.Regex.Matches(text, token1).Count;
            var c2 = System.Text.RegularExpressions.Regex.Matches(text, token2).Count;
            if (c1 != c2)
            {
                text = text.Replace(token1, string.Empty);
                text = text.Replace(token2, string.Empty);
            }
            return text;
        }

        /// <summary>
        /// Converts LR and CR to HTML breaks
        /// </summary>
        public static string ConvertLineBreaksToHTML(this string s, int maxLength = 0, bool trailer = false)
        {
            try
            {
                if (s == null) s = string.Empty;

                //If there is a max length then truncate the string
                if (maxLength > 0 && s.Length > maxLength)
                {
                    s = s.Substring(0, maxLength) + (trailer ? "..." : string.Empty);
                }

                s = s.Replace("\r\n", "\n");
                s = s.Replace("\r", "\n");
                //Now loop and only replace line breaks that are NOT after a ">" so we do not add them to HTML tags
                s = s.Replace("li>\n", "s7dywd652sys52usdf");
                s = s.Replace("\n", "<br />");
                s = s.Replace("s7dywd652sys52usdf", "li>\n");

                return s;
            }
            catch (Exception ex)
            {
                return s;
            }
        }

        public static string HtmlTrim(this string str)
        {
            if (string.IsNullOrEmpty(str)) return str;
            var orig = str;
            do
            {
                orig = str;
                str = str.TrimStart("<br>").TrimStart("<br/>").TrimStart("<br />");
                str = str.TrimEnd("<br>").TrimEnd("<br/>").TrimEnd("<br />");
            } while (orig != str);
            return str;
        }


        /// <summary>
        /// Converts HTML breaks to spaces
        /// </summary>
        public static string HTMLRemoveLineBreaks(this string s)
        {
            if (s == null) s = string.Empty;
            s = s.Replace("<br />", " ");
            return s;
        }

        public static string Replace(this string originalString, string oldValue, string newValue, StringComparison comparisonType)
        {
            try
            {
                int startIndex = 0;
                while (true)
                {
                    startIndex = originalString.IndexOf(oldValue, startIndex, comparisonType);
                    if (startIndex == -1)
                        break;

                    originalString = originalString.Substring(0, startIndex) + newValue + originalString.Substring(startIndex + oldValue.Length);

                    startIndex += newValue.Length;
                }

                return originalString;

            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public static DateTime MaxDate(DateTime a, DateTime b)
        {
            return a > b ? a : b;
        }

        public static DateTime? MaxDate(DateTime? a, DateTime? b)
        {
            if (a == null && b == null) return null;
            if (a != null && b == null) return a;
            if (a == null && b != null) return b;
            return MaxDate(a.Value, b.Value);
        }

        private static Random _rnd = new Random();
        public static string RandomString(int length)
        {
            if (length < 1) length = 1;
            var sb = new StringBuilder();
            for (var ii = 0; ii < length; ii++)
            {
                sb.Append((char)_rnd.Next(65, 91));
            }
            return sb.ToString();
        }

        public static bool IsValidUrl(string url)
        {
            Uri myUri;
            return Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out myUri);
        }

        public static string SerializeSoap<T>(T value)
        {
            if (value == null)
            {
                return null;
            }

            var serializer = new System.Xml.Serialization.XmlSerializer(typeof(T));
            var settings = new System.Xml.XmlWriterSettings();
            settings.Encoding = new UnicodeEncoding(false, false); // no BOM in a .NET string
            settings.Indent = false;
            settings.OmitXmlDeclaration = false;

            using (var textWriter = new StringWriter())
            {
                using (var xmlWriter = System.Xml.XmlWriter.Create(textWriter, settings))
                {
                    serializer.Serialize(xmlWriter, value);
                }
                return textWriter.ToString();
            }
        }

        public static string GetFileExtension(string fileName)
        {
            try
            {
                var fi = new FileInfo(fileName);
                return fi.Extension;
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }

        public static string CalculateMD5(string input)
        {
            if (string.IsNullOrEmpty(input)) return null;
            // step 1, calculate MD5 hash from input
            var md5 = System.Security.Cryptography.MD5.Create();
            var inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            var hash = md5.ComputeHash(inputBytes);

            // step 2, convert byte array to hex string
            var sb = new StringBuilder();
            for (var i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString();
        }

        public static string ToTitleCase(this string str)
        {
            try
            {
                var textInfo = new CultureInfo("en-US", false).TextInfo;
                return textInfo.ToTitleCase(str);
            }
            catch (Exception ex)
            {
                return str;
            }
        }

        public static string MergeRssFiles(IEnumerable<string> fileNames)
        {
            try
            {
                var master = new XmlDocument();
                master.LoadXml("<rss version='2.0'><channel></channel></rss>");
                var parentNode = master.DocumentElement.SelectSingleNode("channel");

                foreach (var f in fileNames)
                {
                    if (File.Exists(f))
                    {
                        var document = new XmlDocument();
                        document.Load(f);
                        foreach (XmlNode node in document.DocumentElement.SelectNodes("channel/item"))
                        {
                            XmlNode copiedNode = master.ImportNode(node, true);
                            parentNode.AppendChild(copiedNode);
                        }
                    }
                }

                var tempfile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                master.Save(tempfile);
                return tempfile;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public static string GetEmbeddedResource(string resource)
        {
            try
            {
                if (string.IsNullOrEmpty(resource)) return null;
                if (!resource.StartsWith(".")) resource = "." + resource;
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                using (var stream = assembly.GetManifestResourceStream(assembly.GetName().Name + resource))
                {
                    if (stream == null) return null;
                    using (var reader = new System.IO.StreamReader(stream))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static List<string> ToEmailList(string text)
        {
            if (string.IsNullOrEmpty(text))
                return new List<string>();
            return ((text + string.Empty).Split(new char[] { ';', ' ', '\r', '\n', ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList());
        }

        public static List<string> GetEmbeddedUrls(string txt)
        {
            var retval = new List<string>();
            var regx = new Regex(@"(http|ftp|https):\/\/([\w\-_]+(?:(?:\.[\w\-_]+)+))([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])?", RegexOptions.IgnoreCase);
            var mactches = regx.Matches(txt);
            foreach (Match match in mactches)
            {
                retval.Add(match.Value);
            }
            return retval;
        }

        /// <summary>
        /// Clean some unloadable elements
        /// </summary>
        public static void CleanXmlFile(string fileName)
        {
            var text = File.ReadAllText(fileName);
            var text2 = text.Replace("&ndash;", "&#8211;");
            text2 = text2.Replace("&nbsp;", "&#160;");
            text2 = text2.Replace("\u082e", "&nbsp;");
            text2 = text2.Replace("\u0003", "&nbsp;"); //ETX char
            text2 = text2.Replace(((char)3).ToString(), "&nbsp;");

            var level = 0;
            while (level < 5)
            {
                System.Threading.Thread.Sleep(500);
                try
                {
                    if (text != text2)
                    {
                        File.WriteAllText(fileName, text2);
                    }
                    return;
                }
                catch { level++; }
            }
        }

        public static string FormatPhone(string phone)
        {
            try
            {
                if (string.IsNullOrEmpty(phone))
                    return phone;

                var retval = Regex.Replace(phone, "[^0-9]", "");
                if (retval.Length == 11)
                    return string.Format("{0:#-###-###-####}", double.Parse(retval));
                if (retval.Length == 10)
                    return string.Format("{0:###-###-####}", double.Parse(retval));
                else if (retval.Length == 7)
                    return string.Format("{0:###-####}", double.Parse(retval));
                else
                    return phone;
            }
            catch (Exception ex)
            {
                return phone;
            }
        }

        public static string GetRelativeUrl(string str)
        {
            try
            {
                if (string.IsNullOrEmpty(str)) return null;
                if (str.StartsWith("//")) return null;
                var url = new Uri(str, UriKind.RelativeOrAbsolute);
                if (!IsLocalUrl(str)) return null;
                if (!url.IsAbsoluteUri && str.StartsWith("/")) return str;
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private static bool IsLocalUrl(string url)
        {
            if (string.IsNullOrEmpty(url)) return false;

            //https://docs.microsoft.com/en-us/aspnet/mvc/overview/security/preventing-open-redirection-attacks
            return ((url[0] == '/' && (url.Length == 1 ||
                    (url[1] != '/' && url[1] != '\\'))) ||   // "/" or "/foo" but not "//" or "/\"
                    (url.Length > 1 &&
                     url[0] == '~' && url[1] == '/'));   // "~/" or "~/foo"
        }

        public static string EncryptUrl(string url)
        {
            try
            {
                return SecurityUtilities.Encrypt(url);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static string DecryptUrl(string url)
        {
            try
            {
                return SecurityUtilities.Decrypt(url);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static List<XmlNode> ToList(this XmlNodeList list)
        {
            var retval = new List<XmlNode>();
            if (list != null)
            {
                foreach (XmlNode n in list)
                {
                    retval.Add(n);
                }
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

        public static List<string> TrimStart(this List<string> list, string[] str, bool ignoreCase = false)
        {
            try
            {
                if (list == null) return list;
                if (str == null || str.Length == 0) return list;

                var retval = new List<string>();
                var stillFirst = true;
                foreach (var s in list)
                {
                    if (stillFirst)
                    {
                        if (ignoreCase && !str.Any(x => s.Match(x)))
                        {
                            retval.Add(s);
                            stillFirst = false;
                        }
                        else if (!ignoreCase && !str.Any(x => s == x))
                        {
                            retval.Add(s);
                            stillFirst = false;
                        }
                    }
                    else
                    {
                        retval.Add(s);
                    }
                }
                return retval;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public static List<string> TrimEnd(this List<string> list, string[] str, bool ignoreCase = false)
        {
            try
            {
                if (list == null) return list;
                if (str == null || str.Length == 0) return list;
                list.Reverse();
                list = list.TrimStart(str, ignoreCase);
                list.Reverse();
                return list;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            return source.IndexOf(toCheck, comp) >= 0;
        }

        public static void TrimIt(this System.Web.UI.WebControls.TextBox ctrl)
        {
            if (ctrl == null) return;
            ctrl.Text = ctrl.Text.Trim();
        }

        public static bool? ToBool(this string v)
        {
            if (string.IsNullOrEmpty(v)) return null;
            if (v.Match("true") || v == "1" || v.Match("t") || v.Match("y") || v.Match("yes")) return true;
            if (v.Match("false") || v == "0" || v.Match("f") || v.Match("n") || v.Match("no")) return false;
            return null;
        }

        public static int ToInt32(this string v)
        {
            if (v == null) return 0;
            int.TryParse(v, out int r);
            return r;
        }

        public static long ToInt64(this string v)
        {
            if (v == null) return 0;
            long.TryParse(v, out long r);
            return r;
        }

        public static Guid ToGuid(this string v)
        {
            if (v == null) return Guid.Empty;
            if (v.Length == 32)
            {
                //There are no slashes so add them
                // xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx
                v = v.Substring(0, 8) + "-" +
                    v.Substring(8, 4) + "-" +
                    v.Substring(12, 4) + "-" +
                    v.Substring(16, 4) + "-" +
                    v.Substring(20, 12);
            }

            Guid.TryParse(v, out Guid r);
            return r;
        }

        /// <summary />
        public static double ToDouble(this string v)
        {
            double.TryParse(v, out double parsed);
            return parsed;
        }

        public static bool Match(this string s, string str)
        {
            if (s == null && str == null) return true;
            if (s != null && str == null) return false;
            if (s == null && str != null) return false;
            return string.Equals(s, str, StringComparison.InvariantCultureIgnoreCase);
        }

        public static byte[] ToByteArray(this string str)
        {
            if (str == null) return null;
            return Encoding.Default.GetBytes(str);
        }

        public static string TrimTo(this string str, int maxLength, bool useEllipses = false)
        {
            if (string.IsNullOrEmpty(str)) return string.Empty;
            if (maxLength < 0) maxLength = 0;
            if (str.Length > maxLength) return str.Substring(0, maxLength) + (useEllipses ? "..." : "");
            return str;
        }

        public static string ToDateString(this DateTime date)
        {
            return date.ToString("MMM dd, yyyy");
        }

        public static string ToDateTimeString(this DateTime date)
        {
            return date.ToString("MMM dd, yyyy HH:mm:ss");
        }

        public static bool IsValidUrlName(string name)
        {
            if (String.IsNullOrEmpty(name))
                return false;

            var isValid = true;
            foreach (var ch in name)
            {
                isValid = (ch >= 'A' && ch <= 'Z') || (ch >= 'a' && ch <= 'z') || (ch >= '0' && ch <= '9');
                if (!isValid) break;
            }
            return isValid;
        }

        public static byte[] ZipBytes(this byte[] byteArray)
        {
            try
            {
                //Prepare for compress
                using (var ms = new System.IO.MemoryStream())
                {
                    using (var sw = new System.IO.Compression.GZipStream(ms, System.IO.Compression.CompressionMode.Compress))
                    {
                        //Compress
                        sw.Write(byteArray, 0, byteArray.Length);
                    }
                    //Transform byte[] zip data to string
                    return ms.ToArray();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static string Unzip(this byte[] byteArray)
        {
            //If null stream return null string
            if (byteArray == null)
                return null;

            //If NOT compressed then return string, no de-compression
            if (byteArray.Length > 3 && (byteArray[0] == 31 && byteArray[1] == 139 && byteArray[2] == 8))
            {
                //Compressed
            }
            else
            {
                var xml = System.Text.Encoding.Unicode.GetString(byteArray);
                if (xml == "")
                {
                    // Treat this just like a null - deserializer can't handle ""
                    return null;
                }
                else
                {
                    // Check for byte order mark
                    if (xml.StartsWith("<") || xml[0] == 0xfeff)
                    {
                        xml = System.Text.RegularExpressions.Regex.Replace(xml, @"[^\u0000-\u007F]", string.Empty);
                        return xml;
                    }
                    else
                    {
                        return System.Text.Encoding.UTF8.GetString(byteArray);
                    }
                }
            }
            return System.Text.Encoding.UTF8.GetString(byteArray.UnzipBytes());
        }

        public static byte[] UnzipBytes(this byte[] byteArray)
        {
            //If null stream return null string
            if (byteArray == null)
                return null;

            try
            {
                using (var memoryStream = new MemoryStream(byteArray))
                {
                    using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
                    {
                        using (var writerStream = new MemoryStream())
                        {
                            gZipStream.CopyTo(writerStream);
                            gZipStream.Dispose();
                            memoryStream.Dispose();
                            return writerStream.ToArray();
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary />
        public static byte[] ZipString(this string value)
        {
            if (string.IsNullOrEmpty(value)) return null;
            var byteArray = System.Text.Encoding.UTF8.GetBytes(value);
            return byteArray.ZipBytes();
        }

        /// <summary>
        /// Determines if the specified email address is valid
        /// </summary>
        public static bool IsValidEmailAddress(string email)
        {
            //string MatchEmailPattern = @"[A-Za-z0-9_%+-]+@[A-Za-z0-9-]+\.[A-Za-z]{2,10}";
            var MatchEmailPattern = @"[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?";

            var validEnding = new string[] { ".com", ".edu", ".org", ".int", ".gov", ".mil", ".net", ".biz", ".info" };

            if (email == null) return false;
            if (email.Contains(" ")) return false;
            if (email.Count(x => x == '@') != 1) return false;
            var b = Regex.IsMatch(email, MatchEmailPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);

            //If a country postfix like ".ca" then just ignore
            var checkEnding = email.Split(new char[] { '.' }).Last().Length >= 3;
            if (checkEnding)
                return b && validEnding.Any(x => email.EndsWith(x, StringComparison.InvariantCultureIgnoreCase));
            else
                return b;
        }

    }
}