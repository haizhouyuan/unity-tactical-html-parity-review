#if UNITY_EDITOR
using System;

public static class JsonGateReader
{
    public static bool RootBool(string json, string key)
    {
        return TryReadRootLiteral(json, key, out var value)
            && string.Equals(value, "true", StringComparison.Ordinal);
    }

    private static bool TryReadRootLiteral(string json, string key, out string value)
    {
        value = "";
        if (string.IsNullOrEmpty(json) || string.IsNullOrEmpty(key))
        {
            return false;
        }

        var depth = 0;
        for (var i = 0; i < json.Length; i++)
        {
            var c = json[i];
            if (c == '"')
            {
                var tokenStart = i + 1;
                var tokenEnd = FindStringEnd(json, tokenStart);
                if (tokenEnd < 0)
                {
                    return false;
                }

                if (depth == 1
                    && tokenEnd - tokenStart == key.Length
                    && string.CompareOrdinal(json, tokenStart, key, 0, key.Length) == 0)
                {
                    var valueStart = SkipWhitespace(json, tokenEnd + 1);
                    if (valueStart >= json.Length || json[valueStart] != ':')
                    {
                        i = tokenEnd;
                        continue;
                    }

                    valueStart = SkipWhitespace(json, valueStart + 1);
                    if (StartsWith(json, valueStart, "true"))
                    {
                        value = "true";
                        return true;
                    }

                    if (StartsWith(json, valueStart, "false"))
                    {
                        value = "false";
                        return true;
                    }

                    return false;
                }

                i = tokenEnd;
                continue;
            }

            if (c == '{' || c == '[')
            {
                depth++;
            }
            else if (c == '}' || c == ']')
            {
                depth--;
            }
        }

        return false;
    }

    private static int FindStringEnd(string text, int start)
    {
        var escaped = false;
        for (var i = start; i < text.Length; i++)
        {
            if (escaped)
            {
                escaped = false;
                continue;
            }

            if (text[i] == '\\')
            {
                escaped = true;
                continue;
            }

            if (text[i] == '"')
            {
                return i;
            }
        }

        return -1;
    }

    private static int SkipWhitespace(string text, int start)
    {
        var index = start;
        while (index < text.Length && char.IsWhiteSpace(text[index]))
        {
            index++;
        }

        return index;
    }

    private static bool StartsWith(string text, int start, string value)
    {
        return start >= 0
            && start + value.Length <= text.Length
            && string.CompareOrdinal(text, start, value, 0, value.Length) == 0;
    }
}
#endif
