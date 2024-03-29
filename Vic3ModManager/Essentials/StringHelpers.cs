﻿using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;

namespace Vic3ModManager.Essentials
{
    internal static class StringHelpers
    {
        private static readonly Dictionary<char, string> CyrillicToLatinMap = new()
        {
            // Uppercase letters
            {'А', "A"}, {'Б', "B"}, {'В', "V"}, {'Г', "G"}, {'Д', "D"},
            {'Е', "E"}, {'Ё', "Yo"}, {'Ж', "Zh"}, {'З', "Z"}, {'И', "I"},
            {'Й', "Y"}, {'К', "K"}, {'Л', "L"}, {'М', "M"}, {'Н', "N"},
            {'О', "O"}, {'П', "P"}, {'Р', "R"}, {'С', "S"}, {'Т', "T"},
            {'У', "U"}, {'Ф', "F"}, {'Х', "Kh"}, {'Ц', "Ts"}, {'Ч', "Ch"},
            {'Ш', "Sh"}, {'Щ', "Shch"}, {'Ъ', "Ie"}, {'Ы', "Y"}, {'Ь', ""},
            {'Э', "E"}, {'Ю', "Yu"}, {'Я', "Ya"},

            // Lowercase letters
            {'а', "a"}, {'б', "b"}, {'в', "v"}, {'г', "g"}, {'д', "d"},
            {'е', "e"}, {'ё', "yo"}, {'ж', "zh"}, {'з', "z"}, {'и', "i"},
            {'й', "y"}, {'к', "k"}, {'л', "l"}, {'м', "m"}, {'н', "n"},
            {'о', "o"}, {'п', "p"}, {'р', "r"}, {'с', "s"}, {'т', "t"},
            {'у', "u"}, {'ф', "f"}, {'х', "kh"}, {'ц', "ts"}, {'ч', "ch"},
            {'ш', "sh"}, {'щ', "shch"}, {'ъ', "ie"}, {'ы', "y"}, {'ь', ""},
            {'э', "e"}, {'ю', "yu"}, {'я', "ya"}
        };

        public static string FormatString(string input)
        {
            input = TransliterateCyrillicToLatin(input);
            input = ReplaceSpaces(input);

            // remove all non letters, numbers and underscores
            input = new string(input.Where(c => char.IsLetterOrDigit(c) || c == '_').ToArray());

            return input;
        }

        public static string ReplaceSpaces(string input)
        {
            string output = "";
            char prevChar = ' ';

            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] == '-')
                {
                    continue;
                }

                if (input[i] == ' ')
                {
                    if (prevChar == '_')
                        continue; // preventing multiple underscores in a row

                    output += "_";
                    prevChar = '_';
                    continue;
                }

                output += input[i];
                prevChar = input[i];
            }

            return output;
        }

        public static string TransliterateCyrillicToLatin(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            string output = input.Select(c => CyrillicToLatinMap.ContainsKey(c) ? CyrillicToLatinMap[c] : c.ToString())
                                 .Aggregate((current, next) => current + next);
            return output;
        }

        public static string ConvertCamelCaseToSpaces(string input)
        {
            return Regex.Replace(input, "(\\B[A-Z])", " $1");
        }
    }
}