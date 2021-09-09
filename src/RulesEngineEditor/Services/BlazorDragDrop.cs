using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Plk.Blazor.DragDrop;

namespace RulesEngineEditor.Services
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBlazorDragDrop(this IServiceCollection services) => Plk.Blazor.DragDrop.ServiceCollectionExtensions.AddBlazorDragDrop(services);
    }
}
