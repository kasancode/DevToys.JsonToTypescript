using System.Text.Json;

namespace DevToys.JsonToTypescript.Converters;

internal static class ConvertUtils
{
    internal static string ToTypescriptType(JsonValueKind kind)
    {
        var typeName = kind switch
        {
            JsonValueKind.String => "string",
            JsonValueKind.Array => "any[]",
            JsonValueKind.Number => "number",
            JsonValueKind.True => "boolean",
            JsonValueKind.False => "boolean",
            _ => "any"
        };

        return typeName;
    }

    internal static string ToTypescriptType(string? schemaType)
    {
        var typeName = schemaType switch
        {
            "string" => "string",
            "array" => "any[]",
            "integer" => "number",
            "number" => "number",
            "boolean" => "boolean",
            _ => "any"
        };

        return typeName;
    }

    internal static int SimpleCombineHash(params int[] hashes)
    {
        // To ignore property order, simply sum hash codes
        var baseHash = 0;
        foreach (var hash in hashes)
        {
            baseHash = unchecked(baseHash ^ hash);
        }

        return baseHash;
    }
}

