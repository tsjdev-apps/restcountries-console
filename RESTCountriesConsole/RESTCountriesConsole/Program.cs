using RESTCountriesConsole.Commands;
using Spectre.Console.Cli;


CommandApp app = new();

app.Configure(configuration =>
{
    configuration.AddCommand<GetCountriesCommand>("get");
});

await app.RunAsync(args);