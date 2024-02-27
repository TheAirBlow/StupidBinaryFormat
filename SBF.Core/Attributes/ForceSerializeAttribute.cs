using System;

namespace SBF.Core.Attributes;

/// <summary>
/// Forces the serializer to serialize this private field or property
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class ForceSerializeAttribute : Attribute { }