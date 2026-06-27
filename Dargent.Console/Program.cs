using System.Threading;
using Dargent.Console;
using Dargent.Core;
using Microsoft.Extensions.DependencyInjection;

try
{
    await using var serviceProvider = new ServiceCollection()
        .AddDargentConsole()
        .AddDargentCore()
        .BuildServiceProvider();

    var session = serviceProvider.GetRequiredService<Session>();

    using var cts = new CancellationTokenSource();
    Console.CancelKeyPress += (_, e) =>
    {
        e.Cancel = true; // this ensures the process continues rather than exiting immediately
        // ReSharper disable once AccessToDisposedClosure
        cts.Cancel();
    };

    await session.RunAsync(cts.Token);
}
catch (OperationCanceledException)
{
    AnsiConsole.WriteLine("Session cancelled.");
}
catch (Exception ex)
{
    AnsiConsole.WriteException(ex);
}