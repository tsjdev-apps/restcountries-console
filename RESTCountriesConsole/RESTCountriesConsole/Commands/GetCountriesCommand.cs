using RESTCountriesSharp;
using RESTCountriesSharp.Models;
using Spectre.Console;
using Spectre.Console.Cli;

namespace RESTCountriesConsole.Commands;

internal class GetCountriesCommand : AsyncCommand<GetCountriesCommandSettings>
{
    private readonly RESTCountriesService _service;
    private readonly List<string> _possibleOperations = new() { "all", "name", "fullname", "code", "codes", "currency", "language", "capital", "region", "subregion" };

    public GetCountriesCommand()
    {
        _service = new RESTCountriesService();
    }

    public override async Task<int> ExecuteAsync(
        CommandContext context,
        GetCountriesCommandSettings settings)
    {
        // write header
        AnsiConsole.Write(new FigletText("REST Countries").Centered().Color(Color.Red));

        // validate settings
        EnsureSettings(settings);

        IEnumerable<Country> countries = new List<Country>();
        await AnsiConsole.Status().StartAsync("Getting countries from restcountries.com...", async ctx =>
        {
            countries = await GetCountriesAsync(settings);
            countries = countries.OrderBy(c => c.Name.Common);
        });

        if (countries.Any())
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"[green]Here are {countries.Count()} countries:[/]");

            // create table
            Table table = new Table()
                .AddColumn(new TableColumn("Name").LeftAligned())
                .AddColumn(new TableColumn("Region").LeftAligned())
                .AddColumn(new TableColumn("Subregion").LeftAligned())
                .AddColumn(new TableColumn("Latitude").LeftAligned())
                .AddColumn(new TableColumn("Longitude").LeftAligned())
                .AddColumn(new TableColumn("Capital").LeftAligned());

            // write table
            AnsiConsole.Live(table)
               .Start(ctx =>
               {
                   foreach (Country country in countries)
                   {
                       table.AddRow(country.Name.Common, country.Region, country.Subregion ?? "-", country.Latitude?.ToString() ?? "-", country.Longitude?.ToString() ?? "-", country.Capitals is not null ? string.Join(", ", country.Capitals) : "-");
                       ctx.Refresh();
                       Thread.Sleep(250);
                   }
               });
        }
        else
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"[red]There is no matching country for the provided parameter.[/]");
        }

        return 0;
    }

    private async Task<IEnumerable<Country>> GetCountriesAsync(GetCountriesCommandSettings settings)
    {
        switch (settings.Operation)
        {
            case "all":
                return await _service.GetAllCountriesAsync();
            case "name":
                return await _service.GetCountriesByNameAsync(settings.Parameter);
            case "fullname":
                Country fullNameCountry = await _service.GetCountryByFullNameAsync(settings.Parameter);
                return fullNameCountry is null ? Enumerable.Empty<Country>() : new List<Country>() { fullNameCountry };
            case "code":
                Country codeCountry = await _service.GetCountryByCodeAsync(settings.Parameter);
                return codeCountry is null ? Enumerable.Empty<Country>() : new List<Country>() { codeCountry };
            case "codes":
                if (settings.Parameter is null)
                {
                    return await _service.GetCountriesByCodesAsync(Enumerable.Empty<string>());
                }
                else
                {
                    var parameters = settings.Parameter.Split(",");
                    return await _service.GetCountriesByCodesAsync(parameters);
                }
            case "currency":
                return await _service.GetCountriesByCurrencyAsync(settings.Parameter);
            case "language":
                return await _service.GetCountriesByLanguageAsync(settings.Parameter);
            case "capital":
                return await _service.GetCountriesByCapitalAsync(settings.Parameter);
            case "region":
                return await _service.GetCountriesByRegionAsync(settings.Parameter);
            case "subregion":
                return await _service.GetCountriesBySubregionAsync(settings.Parameter);
            default:
                return await _service.GetAllCountriesAsync();
        }

    }

    private void EnsureSettings(GetCountriesCommandSettings settings)
    {
        if (string.IsNullOrEmpty(settings.Operation))
        {
            settings.Operation = AnsiConsole.Prompt(
               new SelectionPrompt<string>()
                   .Title("Please select the operation.")
                   .PageSize(10)
                   .AddChoices(_possibleOperations));
        }
        else if (!_possibleOperations.Contains(settings.Operation.ToLower()))
        {
            settings.Operation = AnsiConsole.Prompt(
               new SelectionPrompt<string>()
                   .Title("The provided operation was not recognized. Please select the operation.")
                   .PageSize(15)
                   .AddChoices(_possibleOperations));
        }

        if (string.IsNullOrEmpty(settings.Parameter))
        {
            if (settings.Operation == "All")
            {
                return;
            }

            settings.Parameter = AnsiConsole.Ask<string>("Please insert the search parameter:");
        }
    }
}
