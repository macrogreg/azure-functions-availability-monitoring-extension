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

        public static string QuoteOrSpellNull(string str)
        {
            if (str == null)
            {
                return NullWord;
            }

            var builder = new StringBuilder();
            builder.Append('"');
            builder.Append(str);
            builder.Append('"');

            return builder.ToString();
        }

        public static IEnumerable<string> AsTextLines<TKey, TValue>(IEnumerable<KeyValuePair<TKey, TValue>> table)
        {
            string QuoteIfString<T>(T val)
            {
                if (val == null)
                {
                    return NullWord;
                }

                if (val is string valStr)
                {
                    return QuoteOrSpellNull(valStr);
                }

                return val.ToString();
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

        internal static class AvailabilityTest
        {
            public static class HttpHeaderNames
            {
                // The names of the headers do not perfectly describe the intent, but we use them for compatibility reasons with existing headers used by GSM.
                // See here:
                // https://github.com/microsoft/ApplicationInsights-dotnet/blob/d1865fcba9ad9cbb27b623dd8a1bcdc112bf987e/WEB/Src/Web/Web/WebTestTelemetryInitializer.cs#L15

                public const string TestInvocationInstanceDescriptor = "SyntheticTest-RunId";
                public const string TestInfoDescriptor = "SyntheticTest-Location";
            }

            public const string SpanOperationNameObjectName = nameof(AvailabilityTestScope);

            public const string TelemetryOperationSyntheticSourceMoniker = nameof(Microsoft) + "." + nameof(Microsoft.Azure) + "." + nameof(Microsoft.Azure.AvailabilityMonitoring) + "." + nameof(AvailabilityTestScope);

            public static string LocationNameAsId(string locationDisplayName)
            {
                string locationId = locationDisplayName?.Trim()?.ToLowerInvariant()?.Replace(' ', '-');
                return locationId;
            }

            public static string SpanOperationName(string testDisplayName, string locationDisplayName)
            {
                return String.Format("{0}={{TestDisplayName=\"{1}\", LocationDisplayName=\"{2}\"}}",
                                     SpanOperationNameObjectName,
                                     SpellIfNull(testDisplayName),
                                     SpellIfNull(locationDisplayName));
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static string SpanId(Activity activity)
            {
                return SpellIfNull(activity?.SpanId.ToHexString());
            }
        }
    }
}
