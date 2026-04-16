using DevToys.JsonToTypescript.Converters;
using DevToys.JsonToTypescript.Models;

namespace Test;

/// <summary>
/// Tests for CreateTypeAliasDefinitionSchema edge cases:
/// - array $defs with object items (should generate a dedicated definition)
/// - array $defs with tuple-style items array (should fall back to any[])
/// - $anchor/$id registration on non-object $defs
/// </summary>
[TestClass]
public class SchemaRefNonObjectDefArrayItemConvertTest
{
    // -----------------------------------------------------------------------
    // array $defs whose items.type == "object"
    // -----------------------------------------------------------------------

    private static string ArrayWithObjectItemsJson => """
        {
          "$schema": "http://json-schema.org/draft-07/schema#",
          "type": "object",
          "properties": {
            "items": {
              "$ref": "#/$defs/items"
            }
          },
          "$defs": {
            "items": {
              "type": "array",
              "items": {
                "type": "object",
                "properties": {
                  "id": { "type": "number" },
                  "name": { "type": "string" }
                }
              }
            }
          }
        }
        """;

    [TestMethod]
    public void ArrayWithObjectItems_ExportedInterface()
    {
        var converter = new JsonSchemaToTypescriptConverter(TypescriptDataType.Interface, addExport: true);
        var result = converter.Convert(ArrayWithObjectItemsJson);

        Assert.IsTrue(result.Contains("export interface ItemsItem{"), $"Expected ItemsItem interface, got:\n{result}");
        Assert.IsTrue(result.Contains("export type Items = ItemsItem[];"), $"Expected Items = ItemsItem[], got:\n{result}");
    }

    [TestMethod]
    public void ArrayWithObjectItems_ExportedType()
    {
        var converter = new JsonSchemaToTypescriptConverter(TypescriptDataType.Type, addExport: true);
        var result = converter.Convert(ArrayWithObjectItemsJson);

        Assert.IsTrue(result.Contains("export type ItemsItem = {"), $"Expected ItemsItem type, got:\n{result}");
        Assert.IsTrue(result.Contains("export type Items = ItemsItem[];"), $"Expected Items = ItemsItem[], got:\n{result}");
    }

    // -----------------------------------------------------------------------
    // array $defs where items is an array (draft-07 tuple validation)
    // -----------------------------------------------------------------------

    private static string TupleItemsJson => """
        {
          "$schema": "http://json-schema.org/draft-07/schema#",
          "type": "object",
          "properties": {
            "pair": {
              "$ref": "#/$defs/pair"
            }
          },
          "$defs": {
            "pair": {
              "type": "array",
              "items": [
                { "type": "string" },
                { "type": "number" }
              ]
            }
          }
        }
        """;

    [TestMethod]
    public void TupleItems_FallsBackToAnyArray()
    {
        var converter = new JsonSchemaToTypescriptConverter(TypescriptDataType.Interface, addExport: true);
        var result = converter.Convert(TupleItemsJson);

        Assert.IsTrue(result.Contains("export type Pair = any[];"), $"Expected Pair = any[], got:\n{result}");
    }

    // -----------------------------------------------------------------------
    // $anchor registration on non-object $defs
    // -----------------------------------------------------------------------

    private static string AnchorOnNonObjectDefJson => """
        {
          "$schema": "http://json-schema.org/draft-07/schema#",
          "$id": "https://example.com/schema",
          "type": "object",
          "properties": {
            "status": {
              "$ref": "#status"
            }
          },
          "$defs": {
            "status": {
              "$anchor": "status",
              "type": "string"
            }
          }
        }
        """;

    [TestMethod]
    public void AnchorOnNonObjectDef_RefResolvesViaAnchor()
    {
        var converter = new JsonSchemaToTypescriptConverter(TypescriptDataType.Interface, addExport: true);
        // Should not throw and should resolve the $ref "#status" correctly
        var result = converter.Convert(AnchorOnNonObjectDefJson);

        Assert.IsTrue(result.Contains("export type Status = string;"), $"Expected Status = string, got:\n{result}");
        Assert.IsTrue(result.Contains("status: Status;"), $"Expected property status: Status, got:\n{result}");
    }

    // -----------------------------------------------------------------------
    // $id registration on non-object $defs
    // -----------------------------------------------------------------------

    private static string IdOnNonObjectDefJson => """
        {
          "$schema": "http://json-schema.org/draft-07/schema#",
          "$id": "https://example.com/schema",
          "type": "object",
          "properties": {
            "code": {
              "$ref": "https://example.com/code"
            }
          },
          "$defs": {
            "code": {
              "$id": "code",
              "type": "number"
            }
          }
        }
        """;

    [TestMethod]
    public void IdOnNonObjectDef_RefResolvesViaAbsoluteId()
    {
        var converter = new JsonSchemaToTypescriptConverter(TypescriptDataType.Interface, addExport: true);
        var result = converter.Convert(IdOnNonObjectDefJson);

        Assert.IsTrue(result.Contains("export type Code = number;"), $"Expected Code = number, got:\n{result}");
        Assert.IsTrue(result.Contains("code: Code;"), $"Expected property code: Code, got:\n{result}");
    }
}
