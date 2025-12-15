using DevToys.Api;
using System.ComponentModel.Composition;

namespace DevToys.JsonToTypescript;

[Export(typeof(IResourceAssemblyIdentifier))]
[Name(nameof(JsonToTypescriptAssemblyIdentifier))]
internal sealed class JsonToTypescriptAssemblyIdentifier : IResourceAssemblyIdentifier
{
    public ValueTask<FontDefinition[]> GetFontDefinitionsAsync()
    {
        throw new NotImplementedException();
    }
}