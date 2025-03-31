using System.Text.Json.Nodes;

namespace kbo;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "operation")]
[JsonDerivedType(typeof(Replace), "replace")]
[JsonDerivedType(typeof(Default), "default")]
[JsonDerivedType(typeof(Add), "add")]
[JsonDerivedType(typeof(Mul), "mul")]
[JsonDerivedType(typeof(Pow), "pow")]
[JsonDerivedType(typeof(Mod), "mod")]
[JsonDerivedType(typeof(Floor), "floor")]
[JsonDerivedType(typeof(Ceil), "ceil")]
[JsonDerivedType(typeof(Max), "max")]
[JsonDerivedType(typeof(Min), "min")]
[JsonDerivedType(typeof(And), "and")]
[JsonDerivedType(typeof(Or), "or")]
[JsonDerivedType(typeof(Xor), "xor")]
[JsonDerivedType(typeof(LeftShift), "left_shift")]
[JsonDerivedType(typeof(RightShift), "right_shift")]
[JsonDerivedType(typeof(Remove), "remove")]
[JsonDerivedType(typeof(Pop), "pop")]
[JsonDerivedType(typeof(Update), "update")]
public abstract record DataStorageOperation
{

}

public record DataStorageOperationWithValue : DataStorageOperation
{
    [JsonPropertyName("value")]
    public object Value { get; set; }

    public DataStorageOperationWithValue(object value)
    {
        Value = value;
    }
}

public record Replace : DataStorageOperationWithValue
{
    public Replace(object value) : base(value)
    {
    }
}

public record Default : DataStorageOperation
{

}

public record Add : DataStorageOperationWithValue
{
    public Add(object value) : base(value)
    {
    }
}

public record Mul : DataStorageOperationWithValue
{
    public Mul(object value) : base(value)
    {
    }
}

public record Pow : DataStorageOperationWithValue
{
    public Pow(object value) : base(value)
    {
    }
}

public record Mod : DataStorageOperationWithValue
{
    public Mod(object value) : base(value)
    {
    }
}

public record Floor : DataStorageOperation
{

}

public record Ceil : DataStorageOperation
{

}

public record Max : DataStorageOperationWithValue
{
    public Max(object value) : base(value)
    {
    }
}

public record Min : DataStorageOperationWithValue
{
    public Min(object value) : base(value)
    {
    }
}

public record And : DataStorageOperationWithValue
{
    public And(object value) : base(value)
    {
    }
}

public record Or : DataStorageOperationWithValue
{
    public Or(object value) : base(value)
    {
    }
}

public record Xor : DataStorageOperationWithValue
{
    public Xor(object value) : base(value)
    {
    }
}

public record LeftShift : DataStorageOperationWithValue
{
    public LeftShift(object value) : base(value)
    {
    }
}

public record RightShift : DataStorageOperationWithValue
{
    public RightShift(object value) : base(value)
    {
    }
}

public record Remove : DataStorageOperationWithValue
{
    public Remove(object value) : base(value)
    {
    }
}

public record Pop : DataStorageOperationWithValue
{
    public Pop(object value) : base(value)
    {
    }
}

public record Update : DataStorageOperationWithValue
{
    public Update(object value) : base(value)
    {
    }
}