using DevInstance.BlazorToolkit.Http;
using DevInstance.BlazorToolkit.Offline.Connectivity;
using DevInstance.BlazorToolkit.Offline.Storage;
using DevInstance.BlazorToolkit.Offline.Sync;
using DevInstance.LogScope;
using Microsoft.Extensions.DependencyInjection;

namespace DevInstance.BlazorToolkit.Offline.Extensions;

/// <summary>
/// DI wiring for the BlazorToolkit offline stack: IndexedDB object store,
/// connectivity tracking, the outbound sync queue, and the master-data orchestrator.
/// </summary>
public static class OfflineServiceExtensions
{
    /// <summary>
    /// Registers the offline core. Declare the app's IndexedDB stores in
    /// <paramref name="configure"/>; register cacheable sources via
    /// <c>AddCacheableSource&lt;T&gt;</c> and sync handlers via
    /// <c>AddCrudSyncHandler&lt;T&gt;</c>.
    /// </summary>
    public static IServiceCollection AddBlazorOffline(
        this IServiceCollection services,
        Action<OfflineStoreOptions> configure)
    {
        var options = new OfflineStoreOptions();
        configure(options);

        services.AddSingleton(options);
        services.AddScoped<IObjectStore, IndexedDbObjectStore>();
        services.AddScoped<IConnectivityService, ConnectivityService>();
        services.AddScoped<IOutboxQueue, OutboxProcessor>();
        services.AddScoped<IMasterDataSync, MasterDataSync>();

        return services;
    }

    /// <summary>
    /// Registers a <see cref="CrudSyncHandler{T}"/> as an <see cref="ISyncOperationHandler"/>
    /// for one entity type. Multiple handlers coexist; the outbox picks by entity type.
    /// </summary>
    public static IServiceCollection AddCrudSyncHandler<T>(
        this IServiceCollection services,
        Func<IServiceProvider, CrudSyncHandlerOptions<T>> optionsFactory)
        where T : class
    {
        services.AddScoped<ISyncOperationHandler>(sp =>
            new CrudSyncHandler<T>(
                optionsFactory(sp),
                sp.GetRequiredService<IScopeManager>(),
                sp.GetService<IHttpApiContextFactory>()));
        return services;
    }
}
