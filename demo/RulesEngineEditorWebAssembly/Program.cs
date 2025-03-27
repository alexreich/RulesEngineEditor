// Copyright (c) Alex Reich.
// Licensed under the CC BY 4.0 License.

using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using RulesEngineEditor.Services;
using System.Text.Json;
using RulesEngineEditor.Shared;

namespace RulesEngineEditorWebAssembly
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

            builder.Services.AddRulesEngineEditor();

            builder.Services.AddScoped<JsonSerializerOptions>(sp =>
            {
                return RulesEngineJsonSourceContext.Default.Options;
            });

            await builder.Build().RunAsync();
        }
    }
}
