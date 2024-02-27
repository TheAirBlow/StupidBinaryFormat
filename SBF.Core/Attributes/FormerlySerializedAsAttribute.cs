using System;

namespace SBF.Core.Attributes;

/// <summary>
/// Hint on field's or property's old name for deserialization purposes
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class FormerlySerializedAsAttribute : Attribute {
    /// <summary>
    /// List of former names
    /// </summary>
    public string[] Names { get; }

    /// <summary>
    /// List of former names
    /// </summary>
    /// <param name="names">Names</param>
    public FormerlySerializedAsAttribute(params string[] names)
        => Names = names;
}