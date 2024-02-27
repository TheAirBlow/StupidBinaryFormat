using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace SBF.Core;

/// <summary>
/// SBF binary extensions
/// </summary>
public static class Extensions {
    /// <summary>
    /// Writes a binary entry
    /// </summary>
    /// <param name="writer">Binary Writer</param>
    /// <param name="value">Entry Value</param>
    /// <param name="type">Entry Type</param>
    public static void WriteEntry(this BinaryWriter writer, object value, EntryType type = EntryType.Dynamic) {
        if (type == EntryType.Dynamic) {
            type = TypeHandler.Get(value.GetType());
            writer.Write((byte)type);
        }
        
        switch (type) {
            case EntryType.Byte:
                writer.Write((byte)value);
                return;
            case EntryType.Bool:
                writer.Write((bool)value);
                return;
            case EntryType.Short:
                writer.Write((short)value);
                return;
            case EntryType.UShort:
                writer.Write((ushort)value);
                return;
            case EntryType.Int:
                writer.Write((int)value);
                return;
            case EntryType.UInt:
                writer.Write((uint)value);
                return;
            case EntryType.Long:
                writer.Write((long)value);
                return;
            case EntryType.ULong:
                writer.Write((ulong)value);
                return;
            case EntryType.Float:
                writer.Write((float)value);
                return;
            case EntryType.String:
                writer.Write((string)value);
                return;
            case EntryType.Array: {
                var array = (Array)value; 
                var arrayType = value.GetType().GetElementType();
                var entryType = TypeHandler.Get(arrayType!);
                writer.Write(array.Length);
                writer.Write((byte)entryType);
                foreach (var obj in array) 
                    WriteEntry(writer, obj, entryType);
                return;
            }
            case EntryType.Dictionary: {
                var dict = (IDictionary)value;
                var dictTypes = dict.GetType().GetGenericArguments();
                var keyType = TypeHandler.Get(dictTypes[0]);
                var valueType = TypeHandler.Get(dictTypes[1]);
                writer.Write(dict.Count);
                writer.Write((byte)keyType);
                writer.Write((byte)valueType);
                foreach (DictionaryEntry entry in dict) {
                    WriteEntry(writer, entry.Key, keyType);
                    WriteEntry(writer, entry.Value, valueType);
                }
                return;
            }
            case EntryType.Dynamic:
                throw new InvalidDataException("Entries of type \"Dynamic\" are illegal");
        }
    }
    
    /// <summary>
    /// Reads a binary entry
    /// </summary>
    /// <param name="reader">Binary Reader</param>
    /// <param name="type">Element type</param>
    /// <returns>Read object</returns>
    public static object ReadEntry(this BinaryReader reader, EntryType type = EntryType.Dynamic) {
        if (type == EntryType.Dynamic)
            type = (EntryType)reader.ReadByte();
        switch (type) {
            case EntryType.Byte:
                return reader.ReadByte();
            case EntryType.Bool:
                return reader.ReadByte() == 1;
            case EntryType.Short:
                return reader.ReadInt16();
            case EntryType.UShort:
                return reader.ReadUInt16();
            case EntryType.Int:
                return reader.ReadInt32();
            case EntryType.UInt:
                return reader.ReadUInt32();
            case EntryType.Long:
                return reader.ReadInt64();
            case EntryType.ULong:
                return reader.ReadUInt64();
            case EntryType.Float:
                return reader.ReadSingle();
            case EntryType.String:
                return reader.ReadString();
            case EntryType.Array: {
                var count = reader.ReadInt32();
                var entryType = (EntryType)reader.ReadByte();
                var arrayType = TypeHandler.Get(entryType).MakeArrayType();
                var array = (Array)Activator.CreateInstance(
                    arrayType, args: count)!;
                for (var i = 0; i < count; i++)
                    array.SetValue(ReadEntry(reader, entryType), i);
                return array;
            }
            case EntryType.Dictionary: {
                var count = reader.ReadInt32();
                var keyType = (EntryType)reader.ReadByte();
                var valueType = (EntryType)reader.ReadByte();
                var dictType = typeof(Dictionary<,>).MakeGenericType(
                    TypeHandler.Get(keyType), TypeHandler.Get(valueType));
                var dict = (IDictionary)Activator.CreateInstance(dictType);
                for (var i = 0; i < count; i++) {
                    var key = ReadEntry(reader, keyType);
                    var value = ReadEntry(reader, valueType);
                    dict.Add(key, value);
                }

                return dict;
            }
            case EntryType.Dynamic:
                throw new InvalidDataException("Entries of type \"Dynamic\" are illegal");
        }

        return null!;
    }

    /// <summary>
    /// Does member have attribute
    /// </summary>
    /// <param name="info">Member Info</param>
    /// <typeparam name="T">Attribute Type</typeparam>
    /// <returns>True or false</returns>
    public static bool HasAttribute<T>(this MemberInfo info) where T : Attribute
        => info.GetCustomAttributes(typeof(T), false).Length > 0;

    /// <summary>
    /// Try get attribute
    /// </summary>
    /// <param name="info">Member Info</param>
    /// <param name="attribute">Attribute</param>
    /// <typeparam name="T">Attribute Type</typeparam>
    /// <returns>True on success</returns>
    public static bool TryGetAttribute<T>(this MemberInfo info, out T attribute) where T : Attribute {
        var attrs = info.GetCustomAttributes(typeof(T), false);
        if (attrs.Length == 0) {
            attribute = null!;
            return false;
        }
        
        attribute = (T)attrs[0];
        return true;
    }
}