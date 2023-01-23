using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Samples.Roadside.Contracts;
using Samples.Roadside.Models;

namespace Samples.Roadside;

public static class Extensions
{
    public static async Task<bool> MigrateDataStoresAsync(this IServiceProvider builtServiceProvider)
    {
        var logFactory = builtServiceProvider.GetRequiredService<ILoggerFactory>();
        var log = logFactory.CreateLogger("DataStoreMigration");

        log.LogInformation("Starting ROADSIDE data migration");

        var migrationServices = builtServiceProvider.GetServices<IMigrateData>();

        var migrationTasks = migrationServices.Select(m => m.MigrateAsync(builtServiceProvider))
                                              .ToArray();

        await Task.WhenAll(migrationTasks);
        
        log.LogInformation("ROADSIDE data migration complete");

        return migrationTasks.Any(t => t.Result);
    }
    
    public static string LastRightPart(this string source, char indexOf)
    {
        if (string.IsNullOrEmpty(source))
        {
            return source;
        }

        var index = source.LastIndexOf(indexOf);

        return index < 0
                   ? source
                   : source[(index + 1)..];
    }

    public static int ToInt(this string value, int defaultValue = 0)
        => string.IsNullOrEmpty(value)
               ? defaultValue
               : int.TryParse(value, out var i)
                   ? i
                   : defaultValue;

    public static IApplicationBuilder RegisterOnApplicationStopping(this IApplicationBuilder app, Action callback)
    {
        var appLifetime = app.ApplicationServices.GetRequiredService<IHostApplicationLifetime>();

        appLifetime.ApplicationStopping.Register(callback);

        return app;
    }

    public static IApplicationBuilder RegisterOnApplicationStarted(this IApplicationBuilder app, Action callback)
    {
        var appLifetime = app.ApplicationServices.GetRequiredService<IHostApplicationLifetime>();

        appLifetime.ApplicationStarted.Register(callback);

        return app;
    }

    public static IApplicationBuilder RegisterOnApplicationStopped(this IApplicationBuilder app, Action callback)
    {
        var appLifetime = app.ApplicationServices.GetRequiredService<IHostApplicationLifetime>();

        appLifetime.ApplicationStopped.Register(callback);

        return app;
    }

    public static string Coalesce(this string first, string second)
        => string.IsNullOrEmpty(first)
               ? second
               : first;

    public static bool IsNullOrEmpty(this string source)
        => string.IsNullOrEmpty(source);

    public static void Exception(this ILogger log, Exception ex, string baseMsg = "",
                                 [CallerMemberName] string memberName = null)
    {
        ex ??= new ApplicationException("Unknown Exception Object Reference");

        var msg = ToLogMessage(ex, baseMsg, memberName);

        log.LogError(ex, msg);
    }

    public static string ToLogMessage(this Exception ex,
                                      string customMessage = null, [CallerMemberName] string memberName = null)
    {
        if (ex == null)
        {
            return null;
        }

        var msg = string.Concat("!!! EXCEPTION !!! ",
                                customMessage.IsNullOrEmpty()
                                    ? string.Concat("Caller [", memberName.Coalesce("N/A"), "]")
                                    : customMessage,
                                " :: Type [", ex.GetType(),
                                "] :: Message [", ex.Message, "]");

        return msg;
    }

    public static ActionResult<SampleApiResult<T>> AsOkSampleApiResult<T>(this T response)
        => new OkObjectResult(AsSampleApiResultResult(response));

    public static ActionResult<SampleApiResults<T>> AsOkSampleApiResults<T>(this IEnumerable<T> response)
        where T : class
        => new OkObjectResult(AsSampleApiResults(response));

    private static SampleApiResults<T> AsSampleApiResults<T>(this IEnumerable<T> response)
        where T : class
        => new()
           {
               Data = response.AsListReadOnly()
           };

    private static SampleApiResult<T> AsSampleApiResultResult<T>(T response)
        => new()
           {
               Data = response
           };

    public static string Left(this string source, int length)
        => string.IsNullOrEmpty(source)
               ? string.Empty
               : source.Length >= length
                   ? source[..length]
                   : source;

    public static IReadOnlyList<T> AsListReadOnly<T>(this IEnumerable<T> source)
        => source == null
               ? null
               : source as IReadOnlyList<T> ?? AsList(source).AsReadOnly();

    public static List<T> AsList<T>(this IEnumerable<T> source)
        => source == null
               ? null
               : source as List<T> ?? source.ToList();
}
