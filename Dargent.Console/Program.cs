using Dargent.Console;
using Dargent.Core;
using Microsoft.Extensions.DependencyInjection;

try
{
    var serviceProvider = new ServiceCollection()
        .AddDargentConsole()    
        .AddDargentCore()
        .BuildServiceProvider();
    
    var session = serviceProvider.GetRequiredService<Session>();
    await session.RunAsync();
}
catch (Exception ex)
{
    AnsiConsole.WriteException(ex);
}