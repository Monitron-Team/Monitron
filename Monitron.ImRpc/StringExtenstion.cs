﻿using System;
using System.Collections.Generic;

namespace Monitron.ImRpc
{
    internal static class StringExtenstion
    {
        public static IEnumerable<string> Split(this string str, 
            Func<char, bool> controller)
        {
            int nextPiece = 0;

            for (int c = 0; c < str.Length; c++)
            {
                if (controller(str[c]))
                {
                    yield return str.Substring(nextPiece, c - nextPiece);
                    nextPiece = c + 1;
                }
            }

            yield return str.Substring(nextPiece);
        }

        public static string TrimMatchingQuotes(this string input)
        {
            if ((input.Length >= 2) && (
                (input[0] == '\'') && (input[input.Length - 1] == '\'') ||
                (input[0] == '\"') && (input[input.Length - 1] == '\"')))
            {
                return input.Substring(1, input.Length - 2);
            }

            return input;
        }
    }
}

