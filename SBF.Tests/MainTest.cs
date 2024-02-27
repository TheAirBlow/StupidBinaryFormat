using System.Text.Json;
using System.Text.Json.Serialization;
using NUnit.Framework;
using SBF.Core;
using SBF.Core.Attributes;

namespace SBF.Tests;

/// <summary>
/// SBF serialization and deserialization tests
/// </summary>
public class Tests {
    private Dictionary<object, object> _testDictionary = null!;

    [SetUp]
    public void Setup() {
        _testDictionary = new Dictionary<object, object> {
            ["byte"] = byte.MaxValue,
            ["bool"] = true,
            ["short"] = short.MaxValue,
            ["ushort"] = ushort.MaxValue,
            ["int"] = int.MaxValue,
            ["uint"] = uint.MaxValue,
            ["long"] = long.MaxValue,
            ["ulong"] = ulong.MaxValue,
            ["float"] = float.MaxValue,
            ["Hello World!"] = "ÜÜÜÜÜÜÜÜÜÜÜÜ",
            ["ÜÜÜÜÜÜÜÜÜÜÜÜ"] = "Hello World!",
            ["stringArray"] = new[] {
                "Example", "Array"
            },
            [byte.MaxValue] = byte.MaxValue,
            [float.MaxValue] = float.MaxValue
        };
        _testDictionary.Add("dictionary", _testDictionary.ToDictionary(pair => pair.Key, pair => pair.Value));
    }

    [Test]
    public void BinarySerializer_NativeOnly_NotCompressed() {
        using var stream = new MemoryStream();
        BinarySerializer.SerializeRaw(stream, _testDictionary);
        stream.Seek(0, SeekOrigin.Begin);
        var deserialized = BinarySerializer.DeserializeRaw(stream);
        Assert.That(_testDictionary, Is.EqualTo(deserialized));
    }
    
    [Test]
    public void BinarySerializer_NativeOnly_GZipCompressed() {
        using var stream = new MemoryStream();
        BinarySerializer.SerializeRaw(stream, _testDictionary, true);
        stream.Seek(0, SeekOrigin.Begin);
        var deserialized = BinarySerializer.DeserializeRaw(stream);
        Assert.That(_testDictionary, Is.EqualTo(deserialized));
    }
    
    public class TestClass {
        public class InnerClass {
            public enum InnerEnum {
                Whatever1 = 0,
                Whatever2 = 69,
                Whatever3 = 420
            }

            public InnerEnum Whatever1 { get; set; }
            public InnerEnum Whatever2 { get; set; }
            public InnerEnum Whatever3 { get; set; }
            public string FunkyString { get; set; }
            public string HelloWorld { get; set; }
            public float FunnyFloat { get; set; }
            public int FunnyNumber { get; set; }
        }

        public class PrivateTest {
            [JsonInclude] [ForceSerialize]
            public DateTime CurrentTime { get; private set; }

            [JsonInclude] [ForceSerialize]
            public TimeSpan TimeSpan { get; private set; }
        }
        
        public string[] StringArray { get; set; }
        public string FunkyString { get; set; }
        public string HelloWorld { get; set; }
        public PrivateTest Private { get; set; }
        public float FunnyFloat { get; set; }
        public InnerClass Inner { get; set; }
        public int FunnyNumber { get; set; }
        public byte FunnyByte { get; set; }

        public static TestClass Initialize() {
            var instance = new TestClass {
                StringArray = new[] { "Example", "Values" },
                FunkyString = "ÜÜÜÜÜÜÜÜÜÜÜÜ",
                HelloWorld = "Hello World!",
                Private = new PrivateTest(),
                FunnyFloat = 69.420f,
                Inner = new InnerClass {
                    Whatever1 = InnerClass.InnerEnum.Whatever1,
                    Whatever2 = InnerClass.InnerEnum.Whatever2,
                    Whatever3 = InnerClass.InnerEnum.Whatever3,
                    FunkyString = "ÜÜÜÜÜÜÜÜÜÜÜÜ",
                    HelloWorld = "Hello World!",
                    FunnyFloat = 69.420f,
                    FunnyNumber = 69420
                },
                FunnyNumber = 69420,
                FunnyByte = 0x69
            };

            var type = typeof(PrivateTest);
            type.GetProperty("CurrentTime")!.SetValue(instance.Private, DateTime.Now);
            type.GetProperty("TimeSpan")!.SetValue(instance.Private, TimeSpan.MaxValue);
            return instance;
        }
    }
    
    private readonly JsonSerializerOptions _options = new() { IncludeFields = true };
    
    [Test]
    public void BinarySerializer_Reflection_NotCompressed() {
        var instance = TestClass.Initialize();
        var before = JsonSerializer.Serialize(instance, _options);
        Console.WriteLine($"Before: {before}");
        using var stream = new MemoryStream();
        BinarySerializer.Serialize(stream, instance);
        stream.Seek(0, SeekOrigin.Begin);
        var deserialized = BinarySerializer.Deserialize<TestClass>(stream);
        var after = JsonSerializer.Serialize(deserialized, _options);
        Console.WriteLine($"After: {before}");
        Assert.That(after, Is.EqualTo(before));
    }
    
    [Test]
    public void BinarySerializer_Reflection_GZipCompressed() {
        var instance = TestClass.Initialize();
        var before = JsonSerializer.Serialize(instance, _options);
        Console.WriteLine($"Before: {before}");
        using var stream = new MemoryStream();
        BinarySerializer.Serialize(stream, instance, true);
        stream.Seek(0, SeekOrigin.Begin);
        var deserialized = BinarySerializer.Deserialize<TestClass>(stream);
        var after = JsonSerializer.Serialize(deserialized, _options);
        Console.WriteLine($"After: {before}");
        Assert.That(after, Is.EqualTo(before));
    }
}