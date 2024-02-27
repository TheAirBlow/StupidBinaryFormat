using System;

namespace SBF.Core.Attributes;

/// <summary>
/// Forces the serializer (and deserializer) to skip this field or property
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class DoNotSerializeAttribute : Attribute { }