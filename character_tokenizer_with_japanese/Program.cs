using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace character_tokenizer_with_japanese
{
    class Program
    {
        public const char Separator = '\0';
        private static readonly Regex cjkCharRegex = new Regex(@"\p{IsCJKUnifiedIdeographs}", RegexOptions.Compiled);
        public static bool IsChinese(char c)
        {
            return cjkCharRegex.IsMatch(c.ToString());
        }
        private static void AppendSeparator(StringBuilder sb)
        {
            //Emit a separator unless we already have one
            //Also important: if the last character is a high surrogate it means the next character will be the corresponding
            //low surrogate and we shouldn't emit a separator in between. Unicode surrogates are character pairs that together represent
            //a single code point
            if (sb.Length > 0 && sb[sb.Length - 1] != Separator && !Char.IsHighSurrogate(sb[sb.Length - 1]))
                sb.Append(Separator);
        }
        static public string Tokenize(string str, bool preserveWhitespace = false)
        {
            StringBuilder sb = new StringBuilder((int)(str.Length * 1.25f));
            bool inWS = false;

            for (int i = 0; i < str.Length; i++)
            {
                char c = str[i];
                if (Char.IsWhiteSpace(c))
                {
                    if (sb.Length > 0) //ignore leading spaces
                    {
                        if (preserveWhitespace)
                            sb.Append(c);

                        inWS = true;
                    }
                }
                else
                {
                    if (sb.Length > 0)
                    {
                        //Break if we transition from ws to non-ws or on any non-ws and non-letter or digit
                        if (inWS)
                            AppendSeparator(sb);
                        else if (!Char.IsLetterOrDigit(c) || IsChinese(c) ||
                                (Char.IsLetterOrDigit(c) && !Char.IsLetterOrDigit(sb[sb.Length - 1])))
                            AppendSeparator(sb);
                        else if ((i - 1) >= 0 && IsChinese(str[i - 1]))
                            AppendSeparator(sb);
                    }
                    inWS = false;

                    sb.Append(c);
                }
            }

            //by convention we end the string with a separator ('\0')
            AppendSeparator(sb);

            return sb.ToString();
        }
        static void Main(string[] args)
        {
            string fileName = @"C:\Users\t-abradw\Desktop\Japanese_Stargate.txt",
                output = @"C:\Users\t-abradw\Desktop\japanese_character_tokenized.txt";
            List<string> allLinesText = File.ReadAllLines(fileName).ToList();
            for (int  i = 1; i < allLinesText.Count(); i++)
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(output, true))
                {
                    string add = Tokenize(allLinesText[i]);
                    file.WriteLine(add);
                    Console.WriteLine(add);
                }
            }

        }
    }
}