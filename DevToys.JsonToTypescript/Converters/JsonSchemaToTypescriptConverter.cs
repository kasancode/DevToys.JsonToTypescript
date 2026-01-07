using System.Text;
using System.Text.Json;

namespace DevToys.JsonToTypescript.Converters;

public record class ClassItem(
    string ClassName,
    string Code,
    int Hash,
    List<string> PathList
    );

public class JsonSchemaToTypescriptConverter(TypescriptDataType outputType = TypescriptDataType.Interface, bool addExport = true)
{
    private readonly string _indent = "    ";
    private readonly string _rootNameBase = "JsonRootElement";
    private Uri? baseId = null;

    internal void InitFlag()
    {
    }

    public static bool IsJsonSchema(string json)
    {
        try
        {
            using var document = JsonDocument.Parse(json, new JsonDocumentOptions
            {
                AllowTrailingCommas = true,
                CommentHandling = JsonCommentHandling.Skip
            });
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
            {
                return false;
            }

            if (root.TryGetProperty("$schema", out var schemaElement))
            {
                var schemaValue = schemaElement.GetString();
                if (!string.IsNullOrEmpty(schemaValue) && schemaValue.Contains("json-schema.org"))
                {
                    return true;
                }
            }

            if (root.TryGetProperty("type", out var typeElement) && root.TryGetProperty("properties", out _))
            {
                var typeValue = typeElement.GetString();
                if (typeValue == "object")
                {
                    return true;
                }
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    internal (string ClassName, int ClassHash) CreateClassDefinitionSchema(JsonElement element, string name, string? path, List<ClassItem> outputList)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            throw new InvalidOperationException("Root element must be an object.");
        }

        var type = element.GetProperty("type").GetString();


        if (type != "object")
        {
            throw new InvalidOperationException($"Element type must be 'object', but found '{type}'.");
        }

        var hash = 0;
        string? anchor = null;
        string? id = null;

        if (element.TryGetProperty("$anchor", out var anchorElement))
        {
            anchor = $"#{anchorElement.GetString()}";
        }

        if (element.TryGetProperty("$id", out var idElement))
        {
            id = idElement.GetString();
        }

        var headerBuilder = new StringBuilder();
        var bodyBuilder = new StringBuilder();

        foreach (var property in element.GetProperty("properties").EnumerateObject())
        {
            if (property.Value.ValueKind != JsonValueKind.Object)
                continue;

            string? propertyTypeName;
            var propertyTypeHash = 0;
            var propertyName = property.Name.ToPascalCase();
            if (property.Value.TryGetProperty("$ref", out var refElement) && !string.IsNullOrEmpty(refElement.GetString()))
            {
                var defItem = outputList.FirstOrDefault(item => item.PathList.Contains(refElement.GetString()!)) ?? throw new InvalidOperationException($"Referenced definition '{refElement.GetString()}' not found.");

                propertyTypeName = defItem.ClassName;
                propertyTypeHash = defItem.Hash;
            }
            else
            {
                propertyTypeName = property.Value.GetProperty("type").GetString();
                switch (propertyTypeName)
                {
                    case "object":
                        if (!string.IsNullOrEmpty(path))
                        {
                            path = $"{path}/{property.Name}";
                        }
                        (propertyTypeName, propertyTypeHash) = this.CreateClassDefinitionSchema(property.Value, property.Name.ToPascalCase(), path, outputList);
                        break;
                    case "array":
                        if (property.Value.TryGetProperty("items", out var itemsElement))
                        {
                            string itemType;
                            int itemHash;

                            if (itemsElement.GetProperty("type").GetString() == "object")
                            {
                                (itemType, itemHash) = this.CreateClassDefinitionSchema(itemsElement, property.Name.ToPascalCase() + "Item", null, outputList);
                            }
                            else
                            {
                                itemType = ConvertUtils.ToTypescriptType(itemsElement.GetProperty("type").GetString());
                                itemHash = itemType.GetHashCode();
                            }

                            propertyTypeHash = HashCode.Combine(JsonValueKind.Array, itemHash);
                            propertyTypeName = $"{itemType}[]";
                        }
                        else
                        {
                            propertyTypeName = "any[]";
                            propertyTypeHash = JsonValueKind.Array.GetHashCode();
                        }

                        break;
                    default:
                        propertyTypeName = ConvertUtils.ToTypescriptType(property.Value.GetProperty("type").GetString());
                        propertyTypeHash = propertyTypeName.GetHashCode();
                        break;
                }
            }
            hash = ConvertUtils.SimpleCombineHash(hash, HashCode.Combine(propertyName, propertyTypeHash));
            bodyBuilder.AppendLine($"{this._indent}{property.Name}: {propertyTypeName};");
        }

        return this.CreateCodeIfFirst(name, outputList, headerBuilder, bodyBuilder, hash, path, id, anchor);
    }

    private (string Name, int Hash) CreateCodeIfFirst(string elementName, List<ClassItem> outputList, StringBuilder headerBuilder, StringBuilder bodyBuilder, int hash, string? path, string? id, string? anchor)
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

            definedItem = new(className, headerBuilder.ToString(), hash, path is null ? [] : [path]);

            outputList.Add(definedItem);
        }
        else if (!string.IsNullOrEmpty(path) && !definedItem.PathList.Contains(path))
        {
            // add new path to existing definition
            definedItem.PathList.Add(path);
        }

        if (!string.IsNullOrEmpty(id) && this.baseId is not null)
        {
            var combinedId = new Uri(this.baseId, id);
            definedItem.PathList.Add(combinedId.ToString());
        }

        if (!string.IsNullOrEmpty(anchor))
        {
            definedItem.PathList.Add(anchor);
        }

        // return found item;
        return (definedItem.ClassName, definedItem.Hash);
    }

    public string Convert(string json)
    {
        this.InitFlag();

        using var document = JsonDocument.Parse(json, new JsonDocumentOptions
        {
            AllowTrailingCommas = true,
            CommentHandling = JsonCommentHandling.Skip
        });

        var classCodes = new List<ClassItem>();

        // check root class name
        var rootName = this._rootNameBase;

        var index = 1;
        while (json.Contains(rootName))
        {
            index++;
            rootName = $"{this._rootNameBase}{index}";
        }

        // check base id
        if (document.RootElement.TryGetProperty("$id", out var idProperty))
        {
            var id = idProperty.GetString();
            if (!string.IsNullOrEmpty(id))
            {
                if (Uri.TryCreate(id, UriKind.Absolute, out var uri))
                {
                    this.baseId = uri;
                }
            }
        }

        if (document.RootElement.TryGetProperty("$defs", out var refElement))
        {
            foreach (var property in refElement.EnumerateObject())
            {
                this.CreateClassDefinitionSchema(property.Value, property.Name.ToPascalCase(), $"#/$defs/{property.Name}", classCodes);
            }
        }

        this.CreateClassDefinitionSchema(document.RootElement, rootName, null, classCodes);

        return string.Join(Environment.NewLine, classCodes.Select(item => item.Code));
    }
}
