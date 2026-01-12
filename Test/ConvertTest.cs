using DevToys.JsonToTypescript.Converters;
using DevToys.JsonToTypescript.Models;

namespace Test;

[TestClass]
public sealed class ConvertTest
{
    internal void ConvertTestCore(TypescriptDataType dataType, bool addExport, string expected)
    {
        var json = """
            {
              "name": "John Doe",
              "age": 30,
              "city": "New York",
              "address": {
                "street": "123 Main St",
                "zipcode": "10001"
              },
              "hobbies": ["reading", "hiking", "cooking"],
              "span": {
                "start": {"y":1990, "m":8, "d":2},
                "end": {"m":2, "d":15, "y":2020}
              },
              "customers": [
                  {
                    "street": "123 Main St",
                    "zipcode": "10001"
                  },
                  {
                    "zipcode": "10001",
                    "street": "123 Main St"
                    },
                  {
                    "street": "123 Main St",
                    "zipcode": "10001"
                  }
              ],
              "metadata": {
                "span": {
                    "offset": 10,
                    "count": 50
                }
              }
            }
            """;
        var converter = new JsonToTypescriptConverter(dataType, addExport);
        var convertedPython = converter.Convert(json);

        Assert.AreEqual(expected, convertedPython);
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
}
