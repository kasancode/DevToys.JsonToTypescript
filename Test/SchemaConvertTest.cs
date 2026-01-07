using DevToys.JsonToTypescript.Converters;

namespace Test;

[TestClass]
public class SchemaConvertTest
{
    internal void ConvertTestCore(TypescriptDataType dataType, bool addExport, string expected)
    {
        var json = """
                {
                  "$schema": "http://json-schema.org/draft-07/schema#",
                  "title": "Generated schema for Root",
                  "type": "object",
                  "properties": {
                    "name": {
                      "type": "string"
                    },
                    "age": {
                      "type": "number"
                    },
                    "city": {
                      "type": "string"
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
                      "required": [
                        "street",
                        "zipcode"
                      ]
                    },
                    "hobbies": {
                      "type": "array",
                      "items": {
                        "type": "string"
                      }
                    },
                    "span": {
                      "type": "object",
                      "properties": {
                        "start": {
                          "type": "object",
                          "properties": {
                            "y": {
                              "type": "number"
                            },
                            "m": {
                              "type": "number"
                            },
                            "d": {
                              "type": "number"
                            }
                          },
                          "required": [
                            "y",
                            "m",
                            "d"
                          ]
                        },
                        "end": {
                          "type": "object",
                          "properties": {
                            "m": {
                              "type": "number"
                            },
                            "d": {
                              "type": "number"
                            },
                            "y": {
                              "type": "number"
                            }
                          },
                          "required": [
                            "m",
                            "d",
                            "y"
                          ]
                        }
                      },
                      "required": [
                        "start",
                        "end"
                      ]
                    },
                    "customers": {
                      "type": "array",
                      "items": {
                        "type": "object",
                        "properties": {
                          "street": {
                            "type": "string"
                          },
                          "zipcode": {
                            "type": "string"
                          }
                        },
                        "required": [
                          "street",
                          "zipcode"
                        ]
                      }
                    },
                    "metadata": {
                      "type": "object",
                      "properties": {
                        "span": {
                          "type": "object",
                          "properties": {
                            "offset": {
                              "type": "number"
                            },
                            "count": {
                              "type": "number"
                            }
                          },
                          "required": [
                            "offset",
                            "count"
                          ]
                        }
                      },
                      "required": [
                        "span"
                      ]
                    }
                  },
                  "required": [
                    "name",
                    "age",
                    "city",
                    "address",
                    "hobbies",
                    "span",
                    "customers",
                    "metadata"
                  ]
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
            export interface Address{
                street: string;
                zipcode: string;
            }

            export interface Start{
                y: number;
                m: number;
                d: number;
            }

            export interface Span{
                start: Start;
                end: Start;
            }

            export interface Span2{
                offset: number;
                count: number;
            }

            export interface Metadata{
                span: Span2;
            }

            export interface JsonRootElement{
                name: string;
                age: number;
                city: string;
                address: Address;
                hobbies: string[];
                span: Span;
                customers: Address[];
                metadata: Metadata;
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
            interface Address{
                street: string;
                zipcode: string;
            }

            interface Start{
                y: number;
                m: number;
                d: number;
            }

            interface Span{
                start: Start;
                end: Start;
            }

            interface Span2{
                offset: number;
                count: number;
            }

            interface Metadata{
                span: Span2;
            }

            interface JsonRootElement{
                name: string;
                age: number;
                city: string;
                address: Address;
                hobbies: string[];
                span: Span;
                customers: Address[];
                metadata: Metadata;
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
            export type Address = {
                street: string;
                zipcode: string;
            };

            export type Start = {
                y: number;
                m: number;
                d: number;
            };

            export type Span = {
                start: Start;
                end: Start;
            };

            export type Span2 = {
                offset: number;
                count: number;
            };

            export type Metadata = {
                span: Span2;
            };

            export type JsonRootElement = {
                name: string;
                age: number;
                city: string;
                address: Address;
                hobbies: string[];
                span: Span;
                customers: Address[];
                metadata: Metadata;
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
            type Address = {
                street: string;
                zipcode: string;
            };

            type Start = {
                y: number;
                m: number;
                d: number;
            };

            type Span = {
                start: Start;
                end: Start;
            };

            type Span2 = {
                offset: number;
                count: number;
            };

            type Metadata = {
                span: Span2;
            };

            type JsonRootElement = {
                name: string;
                age: number;
                city: string;
                address: Address;
                hobbies: string[];
                span: Span;
                customers: Address[];
                metadata: Metadata;
            };

            """);
    }

    [TestMethod]
    public void IsJsonSchemaTest()
    {
        var json = """
            {
              "$schema": "http://json-schema.org/draft-07/schema#",
              "title": "Generated schema for Root",
              "type": "object",
              "properties": {
                "name": {
                  "type": "string"
                }
              },
              "required": [
                "name"
              ]
            }
            """;
        Assert.IsTrue(JsonSchemaToTypescriptConverter.IsJsonSchema(json));

        json = """
            {
              "type": "object",
              "properties": {
                "name": {
                  "type": "string"
                }
              }
            }
            """;
        Assert.IsTrue(JsonSchemaToTypescriptConverter.IsJsonSchema(json));

        json = """
            {
              "type": "object"
            }
            """;
        Assert.IsFalse(JsonSchemaToTypescriptConverter.IsJsonSchema(json));

        json = """
            {
              "name": "John Doe",
              "age": 30
            }
            """;

        Assert.IsFalse(JsonSchemaToTypescriptConverter.IsJsonSchema(json));
    }
}
