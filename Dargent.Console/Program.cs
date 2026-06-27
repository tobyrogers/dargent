using Dargent.Console;

try
{
    var session = new Session();
    await session.RunAsync();
}
catch (Exception ex)
{
    AnsiConsole.WriteException(ex);
}