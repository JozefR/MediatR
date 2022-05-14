using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.Extensions.DependencyInjection;

namespace MediatR.Examples.PublishStrategies;

class Program
{
    static async Task Main(string[] args)
    {
        var services = new ServiceCollection();
        services.AddScoped<ServiceFactory>(p => p.GetService);

        services.AddSingleton<Publisher>();

        services.AddTransient<INotificationHandler<Pinged>>(sp => new SyncPingedHandler("1"));
        services.AddTransient<INotificationHandler<Pinged>>(sp => new SyncPingedHandler("2"));
        services.AddTransient<INotificationHandler<Pinged>>(sp => new SyncPingedHandler("3"));
        services.AddTransient<INotificationHandler<Pinged>>(sp => new AsyncPingedHandler("4"));
        services.AddTransient<INotificationHandler<Pinged>>(sp => new AsyncPingedHandler("5"));
        services.AddTransient<INotificationHandler<Pinged>>(sp => new AsyncPingedHandler("6"));

        var provider = services.BuildServiceProvider();

        var publisher = provider.GetRequiredService<Publisher>();

        var pinged = new Pinged();

        foreach (PublishStrategy strategy in Enum.GetValues(typeof(PublishStrategy)))
        {
            Console.WriteLine($"Strategy: {strategy}");
            Console.WriteLine("----------");

            var timer = new Stopwatch();
            try
            {
                timer.Start();
                
                await publisher.Publish(pinged, strategy);
                
                timer.Stop();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.GetType()}: {ex.Message}");
            }

            await Task.Delay(1000);
            Console.WriteLine($"Elapsed time: {timer.ElapsedMilliseconds} ms");
            Console.WriteLine("----------");
        }

        Console.WriteLine("done");
    }
}