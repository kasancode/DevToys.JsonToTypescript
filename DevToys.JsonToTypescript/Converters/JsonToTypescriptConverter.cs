using DevToys.Api;
using System.Text;
using System.Text.Json;

namespace DevToys.JsonToTypescript.Converters;

public enum TypescriptDataType
{
    Interface,
    Type,
}


public class JsonToTypescriptConverter(TypescriptDataType outputType = TypescriptDataType.Interface, bool addExport = true)
{
    private readonly string _indent = "    ";
    private readonly string _rootNameBase = "JsonRootElement";

    internal void InitFlag()
    {
    }

    public string ConvertFromSchema(string json)
    {
        this.InitFlag();

        using var document = JsonDocument.Parse(json, new JsonDocumentOptions
        {
            AllowTrailingCommas = true,
            CommentHandling = JsonCommentHandling.Skip
        });

        var classCodes = new List<(string ClassName, string Code, int Hash)>();

        // check root class name
        var rootName = this._rootNameBase;

        var index = 1;
        while (json.Contains(rootName))
        {
            index++;
            rootName = $"{this._rootNameBase}{index}";
        }

        // list up objects in root element
        var components = document.RootElement.GetProperty("components").GetProperty("schemas");
        foreach (var property in components.EnumerateObject())
        {
            if (property.Value.ValueKind == JsonValueKind.Object)
            {
                // create class definition for each object
                this.CreateClassDefinitionSchema(property.Value, property.Name.ToPascalCase(), classCodes);
            }
        }

        return this.convertToTypescriptCode(classCodes);

    }
    internal (string SlassName, int ClassHash) CreateClassDefinitionSchema(JsonElement element, string name, List<(string ClassName, string Code, int Hath)> outputList)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            throw new InvalidOperationException("Root element must be an object.");
        }

        var type = element.GetProperty("type").GetString();
        var hash = 0;
        var headerBuilder = new StringBuilder();
        var bodyBuilder = new StringBuilder();

        if (type != "object")
        {
            throw new InvalidOperationException($"Element type must be 'object', but found '{type}'.");
        }

        foreach (var property in element.GetProperty("properties").EnumerateObject())
        {
            var propertyTypeHash = 0;
            var propertyName = property.Name.ToPascalCase();
            var propertyTypeName = property.Value.GetProperty("type").GetString();

            switch (propertyTypeName)
            {
                case "object":
                    (propertyName, propertyTypeHash) = this.CreateClassDefinitionSchema(property.Value, property.Name.ToPascalCase(), outputList);
                    break;
                case "array":
                    if (property.Value.TryGetProperty("items", out var itemsElement))
                    {
                        string itemType;
                        int itemHash;

                        if (itemsElement.GetProperty("type").GetString() == "object")
                        {
                            (itemType, itemHash) = this.CreateClassDefinitionSchema(itemsElement, property.Name.ToPascalCase() + "Item", outputList);
                        }
                        else
                        {
                            itemType = this.ToTypescriptType(itemsElement.GetProperty("type").GetString());
                            itemHash = itemType.GetHashCode();
                        }

                        propertyTypeHash = HashCode.Combine(JsonValueKind.Array, itemHash);
                    }
                    else
                    {
                        propertyTypeName = "list";
                        propertyTypeHash = JsonValueKind.Array.GetHashCode();
                    }

                    break;
                default:
                    propertyTypeName = this.ToTypescriptType(property.Value.GetProperty("type").GetString());
                    propertyTypeHash = propertyTypeName.GetHashCode();
                    break;
            }

            hash = this.SimpleConbineHash(hash, HashCode.Combine(propertyName, propertyTypeHash));
            this.addToBody(bodyBuilder, property, propertyTypeName);
        }

        return this.createCodeIfFirst(name, outputList, headerBuilder, bodyBuilder, hash);
    }

    public string Convert(string json)
    {
        this.InitFlag();

        using var document = JsonDocument.Parse(json, new JsonDocumentOptions
        {
            AllowTrailingCommas = true,
            CommentHandling = JsonCommentHandling.Skip
        });

        var classCodes = new List<(string ClassName, string Code, int Hash)>();

        // check root class name
        var rootName = this._rootNameBase;

        var index = 1;
        while (json.Contains(rootName))
        {
            index++;
            rootName = $"{this._rootNameBase}{index}";
        }

        // convert json to Typescript
        this.CreateClassDefinition(document.RootElement, rootName, classCodes);

        return this.convertToTypescriptCode(classCodes);
    }

    private string convertToTypescriptCode(List<(string ClassName, string Code, int Hash)> classCodes)
    {
        // check importing packages
        var importBuilder = new StringBuilder();

        var typingClasses = new List<string>();
        var importLines = new List<string>();

        if (typingClasses.Count > 0)
        {
            importLines.Add($"from typing import {string.Join(", ", typingClasses)}");
        }

        if (importLines.Count > 0)
        {
            // for 2 empty line
            importLines.AddRange(["", ""]);
        }

        importLines.AddRange(classCodes.Select(item => item.Code));

        return string.Join(Environment.NewLine, importLines);
    }

    internal string ToTypescriptType(JsonValueKind kind)
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

    internal string ToTypescriptType(string schemaType)
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

    internal int SimpleConbineHash(params int[] hashes)
    {
        // To ignore property order, simply sum hash codes
        var baseHash = 0;
        foreach (var hash in hashes)
        {
            baseHash = unchecked(baseHash ^ hash);
        }

        return baseHash;
    }

    internal (string Name, int Hash) CreateClassDefinition(JsonElement element, string elementName, List<(string ClassName, string Code, int Hash)> outputList)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            throw new InvalidOperationException();
        }

        var headerBuilder = new StringBuilder();
        var bodyBuilder = new StringBuilder();

        // for check already defined
        var hash = 0;

        foreach (var property in element.EnumerateObject())
        {
            int propertyTypeHash;
            string propertyTypeName;

            switch (property.Value.ValueKind)
            {
                case JsonValueKind.Object:
                    (propertyTypeName, propertyTypeHash) = this.CreateClassDefinition(property.Value, property.Name.ToPascalCase(), outputList);
                    break;

                case JsonValueKind.Array:
                    if (property.Value.GetArrayLength() == 0)
                    {
                        propertyTypeHash = JsonValueKind.Array.GetHashCode();
                        propertyTypeName = "any[]";
                    }
                    else
                    {
                        var listItemElement = property.Value.EnumerateArray().First();
                        string itemType;
                        int itemHash;

                        if (listItemElement.ValueKind == JsonValueKind.Object)
                        {
                            (itemType, itemHash) = this.CreateClassDefinition(listItemElement, property.Name.ToPascalCase() + "Item", outputList);
                        }
                        else
                        {
                            itemType = this.ToTypescriptType(listItemElement.ValueKind);
                            itemHash = listItemElement.ValueKind.GetHashCode();

                        }

                        propertyTypeName = $"{itemType}[]";
                        propertyTypeHash = HashCode.Combine(JsonValueKind.Array, itemHash);
                    }

                    break;
                default:
                    propertyTypeHash = property.Value.ValueKind.GetHashCode();
                    propertyTypeName = this.ToTypescriptType(property.Value.ValueKind);
                    break;

            }

            hash = this.SimpleConbineHash(hash, HashCode.Combine(property.Name, propertyTypeHash));
            this.addToBody(bodyBuilder, property, propertyTypeName);
        }

        return this.createCodeIfFirst(elementName, outputList, headerBuilder, bodyBuilder, hash);

    }

    private (string Name, int Hash) createCodeIfFirst(string elementName, List<(string ClassName, string Code, int Hash)> outputList, StringBuilder headerBuilder, StringBuilder bodyBuilder, int hash)
    {
        // search same definition
        var definedItem = outputList.FirstOrDefault(item => item.Hash == hash);

        if (definedItem == default)
        {
            // not found definition
            var classNameBase = elementName.ToPascalCase();
            var className = classNameBase;

            // check class name
            var index = 1;
            while (outputList.Any(item => item.ClassName == className))
            {
                index++;
                className = $"{classNameBase}{index}";
            }

            var export = addExport ? "export " : "";

            switch (outputType)
            {
                case TypescriptDataType.Interface:
                    headerBuilder.AppendLine($"{export}interface {className}{{");
                    bodyBuilder.AppendLine("}");
                    break;
                case TypescriptDataType.Type:
                    headerBuilder.AppendLine($"{export}type {className} = {{");
                    bodyBuilder.AppendLine("};");
                    break;
            }

            headerBuilder.Append(bodyBuilder);

            outputList.Add((className, headerBuilder.ToString(), hash));

            return (className, hash);
        }

        // retrun found item;
        return (definedItem.ClassName, definedItem.Hash);
    }

    private void addToBody(StringBuilder bodyBuilder, JsonProperty property, string propertyTypeName)
    {
        bodyBuilder.AppendLine($"{this._indent}{property.Name}: {propertyTypeName};");
    }
}

