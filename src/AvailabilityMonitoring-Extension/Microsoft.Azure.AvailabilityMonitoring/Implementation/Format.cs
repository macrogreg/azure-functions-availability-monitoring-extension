﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Microsoft.Azure.AvailabilityMonitoring
{
    internal static class Format
    {
        private const string NullWord = "null";

        public static string Guid(Guid functionInstanceId)
        {
            return functionInstanceId.ToString("D");
        }

        public static string SpanOperationName(string testDisplayName, string locationDisplayName)
        {
            return String.Format("AvailabilityTest={{TestDisplayName=\"{0}\", LocationDisplayName=\"{1}\"}}", SpellIfNull(testDisplayName), SpellIfNull(locationDisplayName));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string SpanId(Activity activity)
        {
            return SpellIfNull(activity?.SpanId.ToHexString());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string SpellIfNull(string str)
        {
            return str ?? NullWord;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object SpellIfNull(object val)
        {
            return val ?? NullWord;
        }

        public static IEnumerable<string> AsTextLines<TKey, TValue>(IEnumerable<KeyValuePair<TKey, TValue>> table)
        {
            string QuoteIfString<T>(T val)
            {
                if (val == null)
                {
                    return NullWord;
                }

                string str = (val is string) ? '"' + val.ToString() + '"' : val.ToString();
                return str;
            }

            if (table == null)
            {
                yield return NullWord;
                yield break;
            }

            foreach(KeyValuePair<TKey, TValue> row in table)
            {
                string rowStr = $"[{QuoteIfString(row.Key)}] = {QuoteIfString(row.Value)}";
                yield return rowStr;
            }
        }

        public static string LocationNameAsId(string locationDisplayName)
        {
            string locationId = locationDisplayName?.Trim()?.ToLowerInvariant()?.Replace(' ', '-');
            return locationId;
        }

        public static string LimitLength(object value, int maxLength, bool trim)
        {
            string valueStr = value?.ToString();
            return LimitLength(valueStr, maxLength, trim);
        }

        public static string LimitLength(string value, int maxLength, bool trim)
        {
            if (maxLength < 0)
            {
                throw new ArgumentException($"{nameof(maxLength)} may not be smaller than zero, but it was {maxLength}.");
            }

            const string FillStr = "...";
            int fillStrLen = FillStr.Length;

            value = SpellIfNull(value);
            value = trim ? value.Trim() : value;
            int valueLen = value.Length;

            if (valueLen <= maxLength)
            {
                return value;
            }

            if (maxLength < fillStrLen + 2)
            {
                string superShortResult = value.Substring(0, maxLength);
                return superShortResult;
            }

            int postLen = (maxLength - fillStrLen) / 2;
            int preLen = maxLength - fillStrLen - postLen;

            string postStr = value.Substring(valueLen - postLen, postLen);
            string preStr = value.Substring(0, preLen);

            var shortResult = new StringBuilder(preStr, maxLength);
            shortResult.Append(FillStr);
            shortResult.Append(postStr);

            return shortResult.ToString();
        }
    }
}