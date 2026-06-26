using DevInstance.BlazorToolkit.Http;
using DevInstance.BlazorToolkit.Http.Extensions;
using DevInstance.BlazorToolkit.Services;
using DevInstance.LogScope;
using DevInstance.WebServiceToolkit.Common.Model;

namespace DevInstance.BlazorToolkit.Offline.Sync;

/// <summary>
/// Default <see cref="ICacheableSource{T}"/> implementation. Reads from the local
/// store; refreshes from a list endpoint that returns
/// <c>ServiceActionResult&lt;ModelList&lt;T&gt;&gt;</c>. Concurrent refreshes are
/// de-duplicated and rate-limited so multiple pages binding the same source don't
/// stampede the server.
/// </summary>
public class CacheableSource<T> : ICacheableSource<T> where T : class
{
    private readonly CacheableSourceOptions<T> options;
    private readonly IHttpApiContextFactory? apiFactory;
    private readonly IScopeLog log;

    private Task<bool>? inFlight;
    private readonly object inFlightLock = new();
    private DateTime? lastCompletedAt;
    private static readonly TimeSpan MinRefreshInterval = TimeSpan.FromSeconds(2);

    public string Name => options.Endpoint;
    public bool IsRefreshing { get; private set; }

    public event Action? OnCacheUpdated;
    public event Action? OnRefreshStateChanged;

    public CacheableSource(
        CacheableSourceOptions<T> options,
        IScopeManager scopeManager,
        IHttpApiContextFactory? apiFactory = null)
    {
        this.options = options;
        this.apiFactory = apiFactory;
        log = scopeManager.CreateLogger($"CacheableSource<{typeof(T).Name}>");
    }

    public Task<List<T>> GetCachedAsync() => options.LoadLocal();

    public Task<bool> RefreshAsync()
    {
        lock (inFlightLock)
        {
            if (inFlight != null) return inFlight;
            if (lastCompletedAt.HasValue && DateTime.Now - lastCompletedAt.Value < MinRefreshInterval)
                return Task.FromResult(true);
            inFlight = DoRefreshAsync();
            return inFlight;
        }
    }

    private async Task<bool> DoRefreshAsync()
    {
        using var l = log.TraceScope();

        SetRefreshing(true);
        try
        {
            if (apiFactory == null)
            {
                l.D($"No API factory — skipping refresh for {options.Endpoint}");
                return false;
            }

            var api = apiFactory.CreateDefault<T>(options.Endpoint);
            // The server wraps list responses in ServiceActionResult<ModelList<T>>;
            // ExecuteListAsync would see an envelope-shaped payload and quietly return
            // an empty list, so deserialize and unwrap the envelope explicitly.
            var envelope = await api.Get().Top(options.PageSize).ExecuteAsync<ServiceActionResult<ModelList<T>>>();
            if (envelope == null || !envelope.Success || envelope.Result == null)
            {
                l.D($"No data returned for {options.Endpoint}");
                return false;
            }

            var items = envelope.Result.Items?.ToList() ?? new List<T>();
            items = ApplyCacheConditions(items);
            await options.SaveLocal(items);

            l.D($"Refreshed {items.Count} items from {options.Endpoint}");
            lastCompletedAt = DateTime.Now;
            OnCacheUpdated?.Invoke();
            return true;
        }
        catch (Exception ex)
        {
            l.E($"Refresh failed for {options.Endpoint}: {ex.Message}");
            return false;
        }
        finally
        {
            lock (inFlightLock) { inFlight = null; }
            SetRefreshing(false);
        }
    }

    private void SetRefreshing(bool value)
    {
        if (IsRefreshing == value) return;
        IsRefreshing = value;
        OnRefreshStateChanged?.Invoke();
    }

    private List<T> ApplyCacheConditions(List<T> items)
    {
        if (options.UpdateDateSelector == null) return items;

        IEnumerable<T> filtered = items;

        if (options.MaxAge.HasValue)
        {
            var threshold = DateTime.Now - options.MaxAge.Value;
            filtered = filtered.Where(i => options.UpdateDateSelector!(i) >= threshold);
        }

        if (options.MaxCount.HasValue)
        {
            filtered = filtered
                .OrderByDescending(i => options.UpdateDateSelector!(i))
                .Take(options.MaxCount.Value);
        }

        return filtered.ToList();
    }
}
