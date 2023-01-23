using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Samples.Roadside.Middleware;

public class SampleLogMiddleware
{
    private const string _startRequestFormatMsg = "### START API REQUEST ### :: Source [{HttpRequestMethod} {HttpRequestPath}] :: ContentLength [{HttpRequestContentLength}] :: Referer [{HttpRequestReferer}]";
    private const string _endRequestFormatMsg = "### END API REQUEST ### :: Source [{HttpRequestMethod} {HttpRequestPath}] :: Status [{RequestStatus} {HttpResponseStatusCode}] :: Duration [{ElapsedMilliseconds}]ms";

    private readonly RequestDelegate _next;
    private readonly ILogger<SampleLogMiddleware> _log;

    public SampleLogMiddleware(RequestDelegate next, ILogger<SampleLogMiddleware> log)
    {
        _next = next;
        _log = log;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var success = true;
        var start = DateTime.UtcNow;

        try
        {
            _log.LogInformation(_startRequestFormatMsg, context.Request.Method, context.Request.Path, context.Request.ContentLength ?? -1, context.Request.Headers["Referer"]);

            await _next(context).ConfigureAwait(false);
        }
        catch(Exception x)
        {
            success = false;

            if (context.Response.StatusCode < 400)
            {
                context.Response.StatusCode = 500;
            }

            _log.Exception(x);

            throw;
        }
        finally
        {
            _log.LogInformation(_endRequestFormatMsg, context.Request.Method, context.Request.Path,
                                success
                                    ? "SUCCESS"
                                    : "FAILED",
                                context.Response.StatusCode,
                                (int)(DateTime.UtcNow - start).TotalMilliseconds);
        }
    }
}
