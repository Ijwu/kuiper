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
    public JsonNode Value { get; set; }

    public DataStorageOperationWithValue(JsonNode value)
    {
        Value = value;
    }
}

public record Replace : DataStorageOperationWithValue
{
    public Replace(JsonNode value) : base(value)
    {
    }
}

public record Default : DataStorageOperation
{

}

public record Add : DataStorageOperationWithValue
{
    public Add(JsonNode value) : base(value)
    {
    }
}

public record Mul : DataStorageOperationWithValue
{
    public Mul(JsonNode value) : base(value)
    {
    }
}

public record Pow : DataStorageOperationWithValue
{
    public Pow(JsonNode value) : base(value)
    {
    }
}

public record Mod : DataStorageOperationWithValue
{
    public Mod(JsonNode value) : base(value)
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
    public Max(JsonNode value) : base(value)
    {
    }
}

public record Min : DataStorageOperationWithValue
{
    public Min(JsonNode value) : base(value)
    {
    }
}

public record And : DataStorageOperationWithValue
{
    public And(JsonNode value) : base(value)
    {
    }
}

public record Or : DataStorageOperationWithValue
{
    public Or(JsonNode value) : base(value)
    {
    }
}

public record Xor : DataStorageOperationWithValue
{
    public Xor(JsonNode value) : base(value)
    {
    }
}

public record LeftShift : DataStorageOperationWithValue
{
    public LeftShift(JsonNode value) : base(value)
    {
    }
}

public record RightShift : DataStorageOperationWithValue
{
    public RightShift(JsonNode value) : base(value)
    {
    }
}

public record Remove : DataStorageOperationWithValue
{
    public Remove(JsonNode value) : base(value)
    {
    }
}

public record Pop : DataStorageOperationWithValue
{
    public Pop(JsonNode value) : base(value)
    {
    }
}

public record Update : DataStorageOperationWithValue
{
    public Update(JsonNode value) : base(value)
    {
    }
}