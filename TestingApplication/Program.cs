using TheAirBlow.DataFormat;

Dictionary<object, object> CreateDictionary()
    => new() {
        ["byte"] = byte.MaxValue,
        ["bool"] = true,
        ["short"] = short.MaxValue,
        ["ushort"] = ushort.MaxValue,
        ["int"] = int.MaxValue,
        ["uint"] = uint.MaxValue,
        ["long"] = long.MaxValue,
        ["ulong"] = ulong.MaxValue,
        ["float"] = float.MaxValue,
        ["string"] = "Hello World!",
        ["stringArray"] = new[] {
            "Example", "Array", "Of", "Strings"
        },
        [byte.MaxValue] = byte.MaxValue,
        [float.MaxValue] = float.MaxValue,
        [new[] { "A" }] = new[] { "B" }
    };

var dictionary = CreateDictionary();
dictionary.Add("dictionary", CreateDictionary());
var original = JasonSerializer.Serialize(dictionary);
Console.WriteLine($"Original object: {original}\n");

using var stream = new FileStream("testing.tdf", FileMode.Create, FileAccess.ReadWrite);
BinarySerializer.Serialize(stream, dictionary, true); stream.Seek(0, SeekOrigin.Begin);
dictionary = (Dictionary<object, object>)BinarySerializer.Deserialize(stream);

var deserialized = JasonSerializer.Serialize(dictionary);
Console.WriteLine($"Deserialized object: {deserialized}\n");

Console.WriteLine(original == deserialized ? "Binary serialization succeeded!" : "Binary serialization failed!");
