namespace DevToys.JsonToTypescript.Models;

public record class StructCode(
    string ClassName,
    string Code,
    int Hash,
    List<string> PathList
);
