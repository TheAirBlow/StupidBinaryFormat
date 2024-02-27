namespace SBF.Core;

/// <summary>
/// TDF entry type
/// </summary>
public enum EntryType : byte {
    /// <summary>
    /// A byte (8-bit)
    /// </summary>
    Byte = 0,
    
    /// <summary>
    /// A boolean (8-bit)
    /// </summary>
    Bool = 1,
    
    /// <summary>
    /// A signed short (16-bit)
    /// </summary>
    Short = 2,
    
    /// <summary>
    /// An unsigned short (16-bit)
    /// </summary>
    UShort = 3,
    
    /// <summary>
    /// A signed integer (32-bit)
    /// </summary>
    Int = 4,
    
    /// <summary>
    /// An unsigned integer (32-bit)
    /// </summary>
    UInt = 5,
    
    /// <summary>
    /// A signed long (64-bit)
    /// </summary>
    Long = 6,
    
    /// <summary>
    /// An unsigned long (64-bit)
    /// </summary>
    ULong = 7,
    
    /// <summary>
    /// A float (32-bit)
    /// </summary>
    Float = 8,
    
    /// <summary>
    /// A string (dynamic)
    /// </summary>
    String = 9,
        
    /// <summary>
    /// An array (dynamic)
    /// </summary>
    Array = 10,
        
    /// <summary>
    /// A dictionary (dynamic)
    /// </summary>
    Dictionary = 11,
    
    /// <summary>
    /// An undetermined dynamic type.
    /// Only valid for enumerable types
    /// e.g. dictionaries and arrays.
    /// </summary>
    Dynamic = 255
}