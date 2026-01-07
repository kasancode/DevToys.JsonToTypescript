using System.Text;
using System.Text.Json;

namespace DevToys.JsonToTypescript.Converters;

public class JsonToTypescriptConverter(TypescriptDataType outputType = TypescriptDataType.Interface, bool addExport = true)
{
    private readonly string _indent = "    ";
    private readonly string _rootNameBase = "JsonRootElement";

    internal void InitFlag()
    {
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

        return ConvertUtils.ConvertToTypescriptCode(classCodes);
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
                            itemType = ConvertUtils.ToTypescriptType(listItemElement.ValueKind);
                            itemHash = listItemElement.ValueKind.GetHashCode();

                        }

                        propertyTypeName = $"{itemType}[]";
                        propertyTypeHash = HashCode.Combine(JsonValueKind.Array, itemHash);
                    }

                    break;
                default:
                    propertyTypeHash = property.Value.ValueKind.GetHashCode();
                    propertyTypeName = ConvertUtils.ToTypescriptType(property.Value.ValueKind);
                    break;

            }

            hash = ConvertUtils.SimpleCombineHash(hash, HashCode.Combine(property.Name, propertyTypeHash));
            bodyBuilder.AppendLine($"{this._indent}{property.Name}: {propertyTypeName};");
        }

        return this.CreateCodeIfFirst(elementName, outputList, headerBuilder, bodyBuilder, hash);

    }

    private (string Name, int Hash) CreateCodeIfFirst(string elementName, List<(string ClassName, string Code, int Hash)> outputList, StringBuilder headerBuilder, StringBuilder bodyBuilder, int hash)
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

        // return found item;
        return (definedItem.ClassName, definedItem.Hash);
    }
}

