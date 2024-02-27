using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using SBF.Core.Attributes;

namespace SBF.Core;

/// <summary>
/// SBF core binary serialization and deserialization
/// </summary>
public static class BinarySerializer {
    /// <summary>
    /// File magic value
    /// </summary>
    private static readonly byte[] _magic = Encoding.ASCII.GetBytes("SBF");
    
    /// <summary>
    /// Version index (changes every update)
    /// </summary>
    public const ushort Version = 0;

    /// <summary>
    /// Deserializes a SBF formatted file to a natively supported type
    /// </summary>
    /// <param name="stream">Stream</param>
    /// <returns>Root Element</returns>
    public static object DeserializeRaw(Stream stream) {
        var reader = new BinaryReader(stream, Encoding.UTF8, true);
        if (!reader.ReadBytes(_magic.Length).SequenceEqual(_magic)) 
            throw new InvalidDataException("Invalid file magic, expected SBF");
        var version = reader.ReadUInt16();
        if (version != Version) throw new InvalidDataException(
            $"Binary serializer version {Version} does not match file's version, which is {version}");
        var isCompressed = reader.ReadByte() == 1;
        if (isCompressed) {
            stream = new GZipStream(stream, CompressionMode.Decompress, true);
            reader = new BinaryReader(stream, Encoding.UTF8);
        }
        
        var obj = reader.ReadEntry();
        reader.Dispose(); return obj;
    }

    /// <summary>
    /// Serializes a natively supported object into a SBF formatted file
    /// </summary>
    /// <param name="stream">Stream</param>
    /// <param name="obj">Object to serialize</param>
    /// <param name="compress">GZip compression</param>
    /// <returns>Root Element</returns>
    public static void SerializeRaw(Stream stream, object obj, bool compress = false) {
        var writer = new BinaryWriter(stream, Encoding.UTF8, true);
        writer.Write(_magic); writer.Write(Version); 
        writer.Write((byte)(compress ? 1 : 0));
        if (compress) {
            stream = new GZipStream(stream, CompressionMode.Compress, true);
            writer = new BinaryWriter(stream, Encoding.UTF8);
        }
        
        writer.WriteEntry(obj);
        writer.Dispose();
    }
    
    /// <summary>
    /// Deserializes a SBF file to a natively supported type,
    /// to a class or to any JSON serializable type
    /// </summary>
    /// <param name="stream">Stream</param>
    /// <returns>Root Element</returns>
    public static T Deserialize<T>(Stream stream)
        => (T)MapToCLR(typeof(T), DeserializeRaw(stream));

    /// <summary>
    /// Serializes a natively supported object, a class or any
    /// JSON serializable type into a SBF formatted file
    /// </summary>
    /// <param name="stream">Stream</param>
    /// <param name="obj">Object to serialize</param>
    /// <param name="compress">GZip compression</param>
    /// <returns>Root Element</returns>
    public static void Serialize(Stream stream, object obj, bool compress = false)
        => SerializeRaw(stream, MapToNative(obj), compress);

    /// <summary>
    /// Maps CLR object to a natively supported type.
    /// Adds support for classes and JSON-serializable types.
    /// </summary>
    /// <param name="obj">Object to map</param>
    /// <returns>Instance of mapped object</returns>
    private static object MapToNative(object obj) {
        var type = obj.GetType();
        if (TypeHandler.IsNative(type)) return obj;
        
        // Map class to dictionary
        if (type.IsClass) {
            var dict = new Dictionary<string, object>();
            foreach (var info in type.GetProperties()) {
                if (!info.CanRead || info.HasAttribute<DoNotSerializeAttribute>()) continue;
                dict[info.Name] = MapToNative(info.GetValue(obj));
            }

            foreach (var info in type.GetFields()) {
                if (info.HasAttribute<DoNotSerializeAttribute>()) continue;
                dict[info.Name] = MapToNative(info.GetValue(obj));
            }

            return dict;
        }
        
        // Serialize to a JSON
        try {
            return JsonSerializer.Serialize(obj);
        } catch (Exception e) {
            throw new InvalidDataException(
                $"Unable to map {type} to a natively supported type", e);
        }
    }

    /// <summary>
    /// Maps natively supported types to specified CLR type.
    /// Adds support for classes and JSON serializable types.
    /// </summary>
    /// <param name="type">Type to map to</param>
    /// <param name="obj">Object to map</param>
    /// <returns>Instance of mapped object</returns>
    private static object MapToCLR(Type type, object obj) {
        var objType = obj.GetType();
        
        // Mapping to an object or object is already the requested type
        if (type == typeof(object) || type == objType) return obj;

        // Map dictionary to class
        if (type.IsClass && objType == typeof(Dictionary<string, object>)) {
            var dict = (IDictionary)obj;
            var instance = Activator.CreateInstance(type);
            foreach (var info in type.GetProperties(
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)) {
                if (!info.CanWrite) continue;
                if (info.HasAttribute<DoNotSerializeAttribute>()) continue;
                if (info.GetSetMethod(true).IsPrivate
                    && !info.HasAttribute<ForceSerializeAttribute>()) continue;
                if (dict.Contains(info.Name)) {
                    info.SetValue(instance, MapToCLR(
                        info.PropertyType, dict[info.Name]));
                    continue;
                }

                if (info.TryGetAttribute<FormerlySerializedAsAttribute>(out var attr))
                    foreach (var name in attr.Names)
                        if (dict.Contains(name)) {
                            info.SetValue(instance, MapToCLR(
                                info.PropertyType, dict[name]));
                            break;
                        }
            }

            foreach (var info in type.GetFields(
                         BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)) {
                if (info.HasAttribute<DoNotSerializeAttribute>()) continue;
                if (info.IsPrivate && !info.HasAttribute<ForceSerializeAttribute>()) continue;
                if (dict.Contains(info.Name)) {
                    info.SetValue(instance, MapToCLR(
                        info.FieldType, dict[info.Name]));
                    continue;
                }

                if (info.TryGetAttribute<FormerlySerializedAsAttribute>(out var attr))
                    foreach (var name in attr.Names)
                        if (dict.Contains(name)) {
                            info.SetValue(instance, MapToCLR(
                                info.FieldType, dict[name]));
                            break;
                        }
            }

            return instance;
        }
        
        // JSON serialized object
        if (objType == typeof(string) && type != typeof(string))
            return JsonSerializer.Deserialize((string)obj, type)!;
        
        // Throw an exception otherwise
        throw new InvalidDataException(
            $"Unable to map {objType} to {type}");
    }
}