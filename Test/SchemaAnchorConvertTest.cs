using DevToys.JsonToTypescript.Converters;
using DevToys.JsonToTypescript.Models;

namespace Test;

[TestClass]
public class SchemaAnchorConvertTest
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
                        "start": { "$ref": "#/$defs/date" },
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
                        "span": { "$ref": "#span" }
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
                  ],
                  "$defs": {
                    "span": {
                      "$anchor": "span",
                      "type": "object",
                      "properties": {
                        "offset": {
                          "type": "number"
                        },
                        "count": {
                            "type": "number"
                        },
                        "required": [
                          "offset",
                          "count"
                        ]
                      }
                    },
                    "date": {
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
            export interface Span{
                offset: number;
                count: number;
            }

            export interface Date{
                y: number;
                m: number;
                d: number;
            }

            export interface Address{
                street: string;
                zipcode: string;
            }

            export interface Span2{
                start: Date;
                end: Date;
            }

            export interface Metadata{
                span: Span;
            }

            export interface JsonRootElement{
                name: string;
                age: number;
                city: string;
                address: Address;
                hobbies: string[];
                span: Span2;
                customers: Address[];
                metadata: Metadata;
            }

            """);
    }

}
