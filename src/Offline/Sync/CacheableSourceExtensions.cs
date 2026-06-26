using DevInstance.BlazorToolkit.Http;
using DevInstance.LogScope;
using Microsoft.Extensions.DependencyInjection;

namespace DevInstance.BlazorToolkit.Offline.Sync;

public static class CacheableSourceExtensions
{
    /// <summary>
    /// Registers a <see cref="CacheableSource{T}"/> as both <see cref="ICacheableSource{T}"/>
    /// (for feature services to read) and the non-generic <see cref="ICacheableSource"/>
    /// (so <see cref="IMasterDataSync"/> can refresh it alongside the others). Both
    /// resolve to the same scoped instance.
    /// </summary>
    public static IServiceCollection AddCacheableSource<T>(
        this IServiceCollection services,
        Func<IServiceProvider, CacheableSourceOptions<T>> optionsFactory)
        where T : class
    {
        services.AddScoped<ICacheableSource<T>>(sp =>
            new CacheableSource<T>(
                optionsFactory(sp),
                sp.GetRequiredService<IScopeManager>(),
                sp.GetService<IHttpApiContextFactory>()));

        services.AddScoped<ICacheableSource>(sp => sp.GetRequiredService<ICacheableSource<T>>());
        return services;
    }
}
