using DevToys.JsonToTypescript.Converters;
using DevToys.JsonToTypescript.Models;

namespace Test;

[TestClass]
public class SchemaRefNonObjectDefConvertTest
{
    internal void ConvertTestCore(TypescriptDataType dataType, bool addExport, string expected)
    {
        var json = """
                {
                  "$schema": "http://json-schema.org/draft-07/schema#",
                  "title": "Generated schema for Root",
                  "type": "object",
                  "properties": {
                    "userId": {
                      "$ref": "#/$defs/userId"
                    },
                    "score": {
                      "$ref": "#/$defs/score"
                    },
                    "tags": {
                      "$ref": "#/$defs/tags"
                    },
                    "address": {
                      "$ref": "#/$defs/address"
                    }
                  },
                  "required": ["userId", "score", "tags", "address"],
                  "$defs": {
                    "userId": {
                      "type": "string"
                    },
                    "score": {
                      "type": "number"
                    },
                    "tags": {
                      "type": "array",
                      "items": {
                        "type": "string"
                      }
                    },
                    "address": {
                      "type": "object",
                      "properties": {
                        "street": {
                          "type": "string"
                        },
                        "zipcode": {
                          "type": "string"
                        }
                      },
                      "required": ["street", "zipcode"]
                    }
                  }
                }
                """;
        var converter = new JsonSchemaToTypescriptConverter(dataType, addExport);
        var converted = converter.Convert(json);

        Assert.AreEqual(expected, converted);
    }

    [TestMethod]
    public void ConvertTestExportedInterface()
    {
        this.ConvertTestCore(
            TypescriptDataType.Interface,
            addExport: true,
            """
            export type UserId = string;

            export type Score = number;

            export type Tags = string[];

            export interface Address{
                street: string;
                zipcode: string;
            }

            export interface JsonRootElement{
                userId: UserId;
                score: Score;
                tags: Tags;
                address: Address;
            }

            """);
    }

    [TestMethod]
    public void ConvertTestUnExportedInterface()
    {
        this.ConvertTestCore(
            TypescriptDataType.Interface,
            addExport: false,
            """
            type UserId = string;

            type Score = number;

            type Tags = string[];

            interface Address{
                street: string;
                zipcode: string;
            }

            interface JsonRootElement{
                userId: UserId;
                score: Score;
                tags: Tags;
                address: Address;
            }

            """);
    }

    [TestMethod]
    public void ConvertTestExportedType()
    {
        this.ConvertTestCore(
            TypescriptDataType.Type,
            addExport: true,
            """
            export type UserId = string;

            export type Score = number;

            export type Tags = string[];

            export type Address = {
                street: string;
                zipcode: string;
            };

            export type JsonRootElement = {
                userId: UserId;
                score: Score;
                tags: Tags;
                address: Address;
            };

            """);
    }

    [TestMethod]
    public void ConvertTestUnExportedType()
    {
        this.ConvertTestCore(
            TypescriptDataType.Type,
            addExport: false,
            """
            type UserId = string;

            type Score = number;

            type Tags = string[];

            type Address = {
                street: string;
                zipcode: string;
            };

            type JsonRootElement = {
                userId: UserId;
                score: Score;
                tags: Tags;
                address: Address;
            };

            """);
    }
}
