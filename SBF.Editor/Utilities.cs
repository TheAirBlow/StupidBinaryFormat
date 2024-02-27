using System.Security.Cryptography;
using SBF.Core;

namespace SBF.Editor;

/// <summary>
/// Random utilities
/// </summary>
public static class Utilities {
    /// <summary>
    /// Parses a string into required type
    /// </summary>
    /// <param name="str">String</param>
    /// <param name="type">Entry Type</param>
    /// <returns>Parsed Object</returns>
    public static object ParseString(string str, EntryType type) {
        switch (type) {
            case EntryType.Byte:
                return byte.Parse(str);
            case EntryType.Bool:
                return bool.Parse(str);
            case EntryType.Short:
                return short.Parse(str);
            case EntryType.UShort:
                return ushort.Parse(str);
            case EntryType.Int:
                return int.Parse(str);
            case EntryType.UInt:
                return uint.Parse(str);
            case EntryType.Long:
                return long.Parse(str);
            case EntryType.ULong:
                return ulong.Parse(str);;
            case EntryType.Float:
                return float.Parse(str);
            case EntryType.String:
                return str;
        }

        return null!;
    }

    /// <summary>
    /// Parses an array string
    /// </summary>
    /// <param name="str">Array String</param>
    /// <param name="type">Array Type</param>
    /// <returns>Array Elements</returns>
    public static List<object> ParseArrayString(string str, EntryType type) {
        if (!str.StartsWith("[ ") || !str.EndsWith(" ]")) throw new ArgumentException(
            "Array contents value must begin with [ and end with ]", nameof(str));
        var contents = str[2..^2];
        if (type == EntryType.String) {
            var list = new List<object>();
            var quoteState = 0; // 0 - none, 1 - first quote, 2 - second quote
            var buf = ""; var lastChar = ' ';
            foreach (var ch in contents) {
                if (ch == ',' && quoteState == 2) {
                    list.Add(buf); buf = "";
                    quoteState = 0; continue;
                }

                if (ch == '\"') {
                    switch (quoteState) {
                        case 0:
                            quoteState = 1;
                            continue;
                        case 1 when lastChar == '\\':
                            buf = buf[..^1];
                            buf += ch;
                            continue;
                        case 1:
                            quoteState = 2;
                            break;
                    }
                }

                if (quoteState == 1) buf += ch;
                lastChar = ch;
            }

            if (quoteState == 2)
                list.Add(buf);
            return list;
        }

        return contents.Split(",").Select(x => ParseString(x.TrimStart(), type)).ToList();
    }
    
    /// <summary>
    /// Get default object for entry type
    /// </summary>
    /// <param name="type">Entry Type</param>
    /// <param name="value">Optional Value Type</param>
    /// <param name="key">Optional Key Type</param>
    /// <returns>Default object value</returns>
    public static object GetDefault(EntryType type, EntryType? value = null, EntryType? key = null) {
        switch (type) {
            case EntryType.Byte:
                return RandomNumberGenerator.GetBytes(1)[0];
            case EntryType.Bool:
                return RandomNumberGenerator.GetInt32(100) > 50;
            case EntryType.Short:
                return (short)RandomNumberGenerator.GetInt32(short.MaxValue);
            case EntryType.UShort:
                return (ushort)RandomNumberGenerator.GetInt32(ushort.MaxValue);
            case EntryType.Int:
                return RandomNumberGenerator.GetInt32(int.MaxValue);
            case EntryType.UInt:
                return (uint)RandomNumberGenerator.GetInt32(int.MaxValue);
            case EntryType.Long:
                return (long)RandomNumberGenerator.GetInt32(int.MaxValue);
            case EntryType.ULong:
                return (ulong)RandomNumberGenerator.GetInt32(int.MaxValue);
            case EntryType.Float:
                return (float)RandomNumberGenerator.GetInt32(int.MaxValue) / 10000;
            case EntryType.String:
                return Guid.NewGuid().ToString();
            case EntryType.Array:
                value ??= EntryType.String;
                var arrType = TypeHandler.Get(value.Value).MakeArrayType();
                return Activator.CreateInstance(arrType, args: 0)!;
            case EntryType.Dictionary:
                key ??= EntryType.String; value ??= EntryType.Dynamic;
                var dict = typeof(Dictionary<,>).MakeGenericType(
                    TypeHandler.Get(key.Value), TypeHandler.Get(value.Value));
                return Activator.CreateInstance(dict)!;
        }

        return null!;
    }
}