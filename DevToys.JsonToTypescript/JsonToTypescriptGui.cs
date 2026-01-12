using DevToys.Api;
using DevToys.JsonToTypescript.Converters;
using DevToys.JsonToTypescript.Models;
using System.ComponentModel.Composition;
using static DevToys.Api.GUI;

namespace DevToys.JsonToTypescript;

[Export(typeof(IGuiTool))]
[Name("JsonToTypescriptExtension")]
[ToolDisplayInformation(
    IconFontName = "FluentSystemIcons",
    IconGlyph = '\uF0E4',// '\uEE79', '\uF0CC' F0E1 E0DD
    GroupName = PredefinedCommonToolGroupNames.Converters,
    ResourceManagerAssemblyIdentifier = nameof(JsonToTypescriptAssemblyIdentifier),
    ResourceManagerBaseName = "DevToys.JsonToTypescript.JsonToTypescriptExtension",
    ShortDisplayTitleResourceName = nameof(JsonToTypescriptExtension.ShortDisplayTitle),
    LongDisplayTitleResourceName = nameof(JsonToTypescriptExtension.LongDisplayTitle),
    DescriptionResourceName = nameof(JsonToTypescriptExtension.Description),
    AccessibleNameResourceName = nameof(JsonToTypescriptExtension.AccessibleName))]
internal sealed class JsonToTypescriptGui : IGuiTool
{
    private readonly ISettingsProvider _settingsProvider;
    private readonly IUIMultiLineTextInput _inputTextArea = MultiLineTextInput("json-to-typescript-input-text-area");
    private readonly IUIMultiLineTextInput _outputTextArea = MultiLineTextInput("json-to-typescript-output-text-area");
    private static readonly SettingDefinition<TypescriptDataType> _typescriptDataTypeDefinition = new(name: "Output type", defaultValue: TypescriptDataType.Interface);
    private static readonly SettingDefinition<bool> _addExport = new(name: "Add export keyword", defaultValue: true);
    private static readonly SettingDefinition<JsonType> _jsonTypeDefinition = new(name: "JSON type", defaultValue: JsonType.AutoDetect);

    [ImportingConstructor]
    public JsonToTypescriptGui(ISettingsProvider settingsProvider)
    {
        this._settingsProvider = settingsProvider;
    }

    private enum GridColumn
    {
        Content
    }

    private enum GridRow
    {
        Header,
        Content,
        Footer
    }

    public UIToolView View => new
        (
            isScrollable: true,
            Grid()
                .ColumnLargeSpacing()
                .RowLargeSpacing()
                .Rows(
                    (GridRow.Header, Auto),
                    (GridRow.Content, new UIGridLength(1, UIGridUnitType.Fraction))
                )
                .Columns(
                    (GridColumn.Content, new UIGridLength(1, UIGridUnitType.Fraction))
                )
            .Cells(
                Cell(
                    GridRow.Header,
                    GridColumn.Content,
                    Stack()
                        .Vertical()
                        .LargeSpacing()
                        .WithChildren(
                            Label().Text(JsonToTypescriptExtension.ConvertJsonToTypescriptConfigurationTitle),
                            SettingGroup("json-to-typescript-settings")
                                .Icon("FluentSystemIcons", '\uF6A9')
                                .Title("Settings")
                                .WithSettings(
                                    Setting()
                                        .Icon("FluentSystemIcons", '\uEA71')
                                        .Title("JSON type")
                                        .Description("Select JSON type")
                                        .Handle(
                                            this._settingsProvider,
                                            _jsonTypeDefinition,
                                            this.OnChanged,
                                            Item("Auto detect", JsonType.AutoDetect),
                                            Item("JSON data", JsonType.Data),
                                            Item("JSON schema", JsonType.Shema)
                                        ),
                                    Setting()
                                        .Icon("FluentSystemIcons", '\uECF4')
                                        .Title("Output type")
                                        .Description("Select Typescript data type")
                                        .Handle(
                                            this._settingsProvider,
                                            _typescriptDataTypeDefinition,
                                            this.OnChanged,
                                            Item("Interface", TypescriptDataType.Interface),
                                            Item("Type", TypescriptDataType.Type)
                                        ),
                                    Setting()
                                        .Icon("FluentSystemIcons", '\uF581')
                                        .Title("Add export keyword")
                                        .Description("Add export keyword before interfaces.")
                                        .Handle(
                                            this._settingsProvider,
                                            _addExport,
                                            this.OnChanged
                                        )
                                )
                        )
                ),
                Cell(
                    GridRow.Content,
                    GridColumn.Content,
                    SplitGrid()
                        .Vertical()
                        .WithLeftPaneChild(
                            this._inputTextArea
                                .Title(JsonToTypescriptExtension.ConvertJsonToTypescriptInputTitle)
                                .Language("json")
                                .OnTextChanged(this.OnChanged))
                        .WithRightPaneChild(
                            this._outputTextArea
                                .Title(JsonToTypescriptExtension.ConvertJsonToTypescriptOutputTitle)
                                .Language("typescript")
                                .ReadOnly()
                                .Extendable()
                        )
                )
            )
        );

    public void OnDataReceived(string dataTypeName, object? parsedData) => throw new NotImplementedException();

    private void OnChanged<T>(T _) => this.Convert();

    private void Convert()
    {
        var json = this._inputTextArea.Text;
        var outputType = _settingsProvider.GetSetting(_typescriptDataTypeDefinition);
        var addExport = _settingsProvider.GetSetting(_addExport);
        var jsonType = _settingsProvider.GetSetting(_jsonTypeDefinition);



        if (string.IsNullOrEmpty(json))
        {
            this._outputTextArea.Text(string.Empty);
            return;
        }

        try
        {
            var asSchema = jsonType == JsonType.Shema;
            if (jsonType == JsonType.AutoDetect)
            {
                asSchema = JsonSchemaToTypescriptConverter.IsJsonSchema(json);
            }

            if (asSchema)
            {
                var converter = new JsonSchemaToTypescriptConverter(outputType, addExport);
                this._outputTextArea.Text(converter.Convert(json));

            }
            else
            {
                var converter = new JsonToTypescriptConverter(outputType, addExport);
                this._outputTextArea.Text(converter.Convert(json));
            }
        }
        catch
        {
            this._outputTextArea.Text("Please provide a valid JSON");
        }
    }
}