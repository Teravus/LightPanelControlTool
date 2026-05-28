using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace NeewerLightPanelTool.Services;

public sealed class StreamDeckHttpServer : IAsyncDisposable
{
    private WebApplication? _app;

    public bool IsRunning => _app is not null;

    public async Task StartAsync(string ipAddress, int port, Func<StreamDeckLightRequest, Task<StreamDeckLightResponse>> handler)
    {
        if (_app is not null)
        {
            return;
        }

        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.Logging.ClearProviders();
        builder.WebHost.UseUrls($"http://{ipAddress}:{port}");

        WebApplication app = builder.Build();
        MapLightEndpoint(app, "/neewerbt_RGBSet", "rgb", handler);
        MapLightEndpoint(app, "/neewerbt_CCTToneSet", "cct", handler);
        MapLightEndpoint(app, "/neewerbt_SceneSet", "scene", handler);
        MapLightEndpoint(app, "/neewerbt_brightnessSet", "brightness", handler);
        MapLightEndpoint(app, "/neewerbt_PowerSet", "power", handler);
        MapLightEndpoint(app, "/neewerbt_reconnect", "connect", handler);
        MapLightEndpoint(app, "/neewerbt_connect", "connect", handler);

        await app.StartAsync().ConfigureAwait(false);
        _app = app;
    }

    public async Task StopAsync()
    {
        if (_app is null)
        {
            return;
        }

        WebApplication app = _app;
        _app = null;
        await app.StopAsync().ConfigureAwait(false);
        await app.DisposeAsync().ConfigureAwait(false);
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync().ConfigureAwait(false);
    }

    private static void MapLightEndpoint(WebApplication app, string path, string action, Func<StreamDeckLightRequest, Task<StreamDeckLightResponse>> handler)
    {
        app.MapMethods(path, ["GET", "POST"], async context =>
        {
            StreamDeckLightRequest request = await CreateRequestAsync(action, context).ConfigureAwait(false);
            StreamDeckLightResponse response = await handler(request).ConfigureAwait(false);
            context.Response.StatusCode = response.Success ? StatusCodes.Status200OK : StatusCodes.Status400BadRequest;
            context.Response.ContentType = "text/plain";
            await context.Response.WriteAsync(response.Message).ConfigureAwait(false);
        });
    }

    private static async Task<StreamDeckLightRequest> CreateRequestAsync(string action, HttpContext context)
    {
        Dictionary<string, string> values = new(StringComparer.OrdinalIgnoreCase);
        foreach ((string key, Microsoft.Extensions.Primitives.StringValues value) in context.Request.Query)
        {
            values[key] = value.ToString();
        }

        if (context.Request.HasFormContentType)
        {
            IFormCollection form = await context.Request.ReadFormAsync().ConfigureAwait(false);
            foreach ((string key, Microsoft.Extensions.Primitives.StringValues value) in form)
            {
                values[key] = value.ToString();
            }
        }

        return new StreamDeckLightRequest(
            action,
            Get(values, "group"),
            Get(values, "light"),
            TryInt(values, "r"),
            TryInt(values, "g"),
            TryInt(values, "b"),
            TryInt(values, "tone"),
            TryFloat(values, "brightness"),
            Get(values, "scenename"),
            Get(values, "power"));
    }

    private static string Get(Dictionary<string, string> values, string key)
    {
        return values.TryGetValue(key, out string? value) ? value : string.Empty;
    }

    private static int? TryInt(Dictionary<string, string> values, string key)
    {
        return values.TryGetValue(key, out string? value) && int.TryParse(value, out int result) ? result : null;
    }

    private static float? TryFloat(Dictionary<string, string> values, string key)
    {
        return values.TryGetValue(key, out string? value) && float.TryParse(value, out float result) ? result : null;
    }
}

public sealed record StreamDeckLightRequest(
    string Action,
    string GroupName,
    string LightId,
    int? Red,
    int? Green,
    int? Blue,
    int? Tone,
    float? Brightness,
    string SceneName,
    string Power);

public sealed record StreamDeckLightResponse(bool Success, string Message);
