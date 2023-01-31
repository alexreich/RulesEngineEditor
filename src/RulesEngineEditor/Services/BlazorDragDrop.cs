// Copyright (c) Alex Reich.
// Licensed under the CC BY 4.0 License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Append.Blazor.Printing;
using Microsoft.Extensions.DependencyInjection;
using Plk.Blazor.DragDrop;
using System.Net.Http;

namespace RulesEngineEditor.Services
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRulesEngineEditor(this IServiceCollection services)
        {
            Plk.Blazor.DragDrop.ServiceCollectionExtensions.AddBlazorDragDrop(services);
            services.AddScoped<IPrintingService, PrintingService>();
            services.AddSingleton(new WorkflowService());
            return services;
        }
    }
}
