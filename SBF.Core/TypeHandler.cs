using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SBF.Core;

/// <summary>
/// Handles internal type mapping
/// </summary>
public static class TypeHandler {
    /// <summary>
    /// Entry type to C# type mapping
    /// </summary>
    private static readonly Dictionary<Type, EntryType> _types = new() {
        [typeof(byte)] = EntryType.Byte,
        [typeof(bool)] = EntryType.Bool,
        [typeof(short)] = EntryType.Short,
        [typeof(ushort)] = EntryType.UShort,
        [typeof(int)] = EntryType.Int,
        [typeof(uint)] = EntryType.UInt,
        [typeof(long)] = EntryType.Long,
        [typeof(ulong)] = EntryType.ULong,
        [typeof(float)] = EntryType.Float,
        [typeof(string)] = EntryType.String,
        [typeof(object)] = EntryType.Dynamic,
        [typeof(Array)] = EntryType.Array,
        [typeof(IDictionary)] = EntryType.Dictionary
    };

    /// <summary>
    /// C# type to entry type mapping
    /// </summary>
    private static readonly Dictionary<EntryType, Type> _reverse = 
        _types.ToDictionary(pair => pair.Value, pair => pair.Key);

    /// <summary>
    /// Returns Entry Type for specified type
    /// </summary>
    /// <param name="type">Type</param>
    /// <returns>Entry Type</returns>
    public static EntryType Get(Type type) {
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            return EntryType.Dictionary;
        if (type.IsArray) return EntryType.Array;
        if (!_types.TryGetValue(type, out var entryType))
            throw new ArgumentOutOfRangeException(nameof(type),
                $"Type {type.FullName} is not supported");
        return entryType;
    }

    /// <summary>
    /// Is type supported natively
    /// </summary>
    /// <param name="type">Type</param>
    /// <returns>True if supported</returns>
    public static bool IsNative(Type type) {
        try {
            Get(type);
            return true;
        } catch {
            return false;
        }
    }

    /// <summary>
    /// Returns C# type for specified Entry Type
    /// </summary>
    /// <param name="type">Entry Type</param>
    /// <returns>C# type</returns>
    public static Type Get(EntryType type) {
        try {
            return type switch {
                EntryType.Array => typeof(Array),
                EntryType.Dictionary => typeof(IDictionary),
                _ => _reverse[type]
            };
        } catch {
            throw new ArgumentOutOfRangeException(nameof(type),
                $"Invalid entry type 0x{(int)type:X2}");
        }
    }
}