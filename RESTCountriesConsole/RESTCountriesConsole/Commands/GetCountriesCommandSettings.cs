using Spectre.Console.Cli;
using System.ComponentModel;

namespace RESTCountriesConsole.Commands;

internal class GetCountriesCommandSettings : CommandSettings
{
    [CommandArgument(0, "[operation]")]
    [Description("The operation to get the countries.")]
    public string? Operation { get; set; }

    [CommandOption("-p|--parameter")]
    [Description("Specifies the parameter for the operation.")]
    public string? Parameter { get; set; }
}
