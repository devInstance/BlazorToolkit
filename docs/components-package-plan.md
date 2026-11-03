# `DevInstance.BlazorToolkit.Components` — implementation brief

**Status: Step 0 done 2026-11-03 (10.6.0); HDataGrid port not started.** Consumer-side plan and research: `ThreadIQ/docs/blazortoolkit-components-plan.md`.

A second NuGet package in this repo for reusable **UI components**. The core `DevInstance.BlazorToolkit`
package stays infrastructure (DI, service execution, HTTP, offline); visual components with their own
CSS/JS go in the new one, so consumers that only want the infrastructure do not pull styling.

First content: the `HDataGrid` family, ported from ThreadIQ
(`src/Client/ThreadIQ.Client.Desktop/Core/UI/Components` + `Core/UI/Model/Grid`). ThreadIQ's copy is
the newest of four (DevCoreApp, Tentrie, coreblog are older); port **that** one.

## Step 0: move the existing UI components out of core (done)

Decided 2026-11-03. Can ship on its own, before the HDataGrid port (no WST.Common 10.4.0 needed).

**Dependency check.** Core does not reference anything in `src/Components/` (only the `@using` in
`src/_Imports.razor`); the components depend on core (`IServiceExecutionHost`), WST.Common
(`IModelList<T>`) and ASP.NET Core Components only, none have CSS/JS of their own. A single
`ProjectReference` to core covers every dependency — no cycle.

**What moves** (`git mv`, namespace unchanged `DevInstance.BlazorToolkit.Components`):
`BlazorToolkitPageLayout.razor`, `DataPager.razor` + `.razor.cs`, `DataPageUtils.cs` (internal, used only
by the pagers), `ErrorMessageBanner.razor`, `LoadingProgress.razor`, `ModelDataPager.razor`,
`PageProgress.razor`, `SortableHeader.razor`.

**What stays in core:** `ServiceExecutionHostComponent.razor` — it has no markup and is the base class of
the core service-execution pattern; moving it would force infrastructure-only consumers onto the UI
package. The namespace is therefore split across both assemblies (legal; keeps consumers' `@using`
lines working). Keep the `@using DevInstance.BlazorToolkit.Components` in core's `_Imports.razor`.

**Breaking change — no type forwarding possible** (`[TypeForwardedTo]` would need core to reference the
Components assembly: circular; shipping the types in both assemblies gives CS0433). Release as **10.6.0** for both packages
(breaking for consumers of the moved components). Release notes must say: add the `DevInstance.BlazorToolkit.Components`
package. Warn that Razor does **not** error on a missing component — `<ErrorMessageBanner>`,
`<DataPager>`, `<ModelDataPager>` become unknown HTML elements with only warning RZ10012 and silently
render nothing; recommend `<WarningsAsErrors>RZ10012</WarningsAsErrors>` while upgrading.
(`@layout BlazorToolkitPageLayout` does fail to compile.)

**Folder layout.** `src/` is the core project's directory, so `src/DevInstance.BlazorToolkit.Components/**`
would be globbed into core. Add `<Compile Remove>`, `<Content Remove>`, `<None Remove>` for that folder
to the core csproj (or move core into `src/DevInstance.BlazorToolkit/` — then update the pipelines and
`CLAUDE.md` paths).

**Checklist**
1. Create the project (see "Project layout" below for csproj settings); `_Imports.razor` with
   `@using Microsoft.AspNetCore.Components.Web` and `@using DevInstance.BlazorToolkit.Services`.
2. Move the files listed above.
3. Add the project to the `.sln`; add a `ProjectReference` from `Samples.Client` (its `Home.razor` and
   `HomePrerender.razor` use `ErrorMessageBanner` and `ModelDataPager`). Build with RZ10012 as an error.
4. Release pipeline: pack and push both projects with the same `$(version)`. CI needs no change.
5. Optional: `tests/DevInstance.BlazorToolkit.Components.Tests` covering `DataPageUtils.GetPageRange`
   via `InternalsVisibleTo`.
6. Docs: README (`BlazorToolkitPageLayout` example), `docs/USAGE.md` (`ErrorMessageBanner`,
   `DataPager` sections name the package), `CLAUDE.md` (two packages, shared version,
   `ServiceExecutionHostComponent` stays in core).
7. Bump both packages to **10.6.0**; verify the Components `.nupkg` depends on
   `DevInstance.BlazorToolkit` at that version.

## Prerequisite: WebServiceToolkit.Common 10.4.0

Before porting, add to `WebServiceToolkit/src/DevInstance.WebServiceToolkit.Common/Model/`:

- `HighlightRange` — copy ThreadIQ's `src/Shared/Model/Core/Common/HighlightRange.cs` verbatim
  (`Start`, `Length`, static `Segments(text, ranges)` with clip/merge). Namespace
  `DevInstance.WebServiceToolkit.Common.Model`. Bring its tests.
- `IModelListHighlights`:
  ```csharp
  public interface IModelListHighlights
  {
      /// item Id → DTO property name → ranges. Null when not searched.
      Dictionary<string, Dictionary<string, HighlightRange[]>> Highlights { get; set; }
  }
  ```
  Separate from `IModelList<T>` on purpose: adding a member to `IModelList<T>` breaks every implementer.

Then bump the core BlazorToolkit package's `WebServiceToolkit.Common` reference to 10.4.0.

## Project layout

```
src/DevInstance.BlazorToolkit.Components/
├── DevInstance.BlazorToolkit.Components.csproj
├── _Imports.razor
├── DataGrid/        HDataGrid.razor(.cs), HColumnTemplate.razor, HSortableHeader.razor,
│                    HSortableHeaderSortArgs.cs, GridSettings.razor(.cs), ColumnDescriptor.cs,
│                    GroupContext.cs, ColumnWidthChange.cs
├── Search/          HSearchBar.razor(.cs,.css), SearchHighlight.cs
├── Scripts/         blazortoolkit-components.ts   (source; keep in sync by hand, as with blazortoolkit-db.ts)
└── wwwroot/
    ├── js/blazortoolkit-components.js            (ES module)
    └── css/blazortoolkit-components.css
```

csproj: copy the core package's (`Microsoft.NET.Sdk.Razor`, `net10.0`, `Nullable`, `ImplicitUsings`,
`SupportedPlatform browser`, `GeneratePackageOnBuild`, license/icon/readme packing). Differences:

- `AssemblyName`/`PackageId` `DevInstance.BlazorToolkit.Components`, `RootNamespace` the same.
- `ProjectReference` to `../DevInstance.BlazorToolkit.csproj` (for `ModelDataPager`). It packs as a
  NuGet dependency on the same version — **release both packages with one version number**.
- Package references: only `Microsoft.AspNetCore.Components.Web` + `DevInstance.WebServiceToolkit.Common`
  (transitively via the core package is fine). No FluentValidation, no LogScope needed.
- Add the project to `DevInstance.BlazorToolkit.sln`, and a second pack/push step to
  `azure-pipeline-release.yml`.

**Namespaces.** After Step 0 the new assembly owns `DevInstance.BlazorToolkit.Components` (pagers,
banner, layout, etc.), shared with core's `ServiceExecutionHostComponent`. Put the new grid/search types
in `DevInstance.BlazorToolkit.Components.DataGrid` and `….Components.Search`; consumers add those two
`@using` lines.

## Porting rules — what to change while copying

1. **Data type.** `HDataGrid.Data` and `HSortableHeader.Model` become `IModelList<TItem>?`
   (were ThreadIQ's `PagedList<TItem>`). Keep `where TItem : IModelItem`.
   - `Data.SortField()` (a ThreadIQ extension): inline as a private helper — first `SortOrder` entry with
     `+`/`-` trimmed.
   - Highlights: `(Data as IModelListHighlights)?.Highlights` → look up `[itemId][field]`; null when absent.
2. **No app design classes.** Replace `tiq-eyebrow` → `hdg-eyebrow`, `tiq-chip` → `hdg-chip`,
   `tiq-count` → `hdg-count` (group bar, grouped header row, GridSettings "Columns" heading).
3. **CSS.** Collect into `wwwroot/css/blazortoolkit-components.css` the rules that today live in
   ThreadIQ's `Styles/scss/_components.scss` (`thead th.hdg-th`, `.hdg-resizer` incl. `::after`,
   `body.hdg-resizing`, `.hdg-groupbar`, `.hdg-groupbar-remove`, `mark.search-mark`) and `_crm.scss`
   (`.gs-columns`, `.gs-col` incl. `.column-dragging`/`.column-can-drop`, `.gs-grip`, `.gs-ipp-label`,
   `.gs-footer`), plus minimal `.hdg-eyebrow/.hdg-chip/.hdg-count`. Write them in plain CSS (no SCSS
   build here). Every colour goes through a `--btk-*` custom property with a Bootstrap fallback:
   ```css
   :root {
     --btk-accent:    var(--bs-primary);
     --btk-accent-bg: var(--bs-primary-bg-subtle);
     --btk-border:    var(--bs-border-color);
     --btk-surface:   var(--bs-tertiary-bg);
     --btk-bg:        var(--bs-body-bg);
     --btk-text:      var(--bs-body-color);
     --btk-text-2:    var(--bs-secondary-color);
     --btk-text-3:    var(--bs-tertiary-color);
   }
   ```
   Do **not** carry over the generic `table`/`thead th` theming — that is the app's look, not the grid's.
   `HSearchBar.razor.css` stays scoped CSS (it bundles automatically into the consumer's `*.styles.css`).
4. **JS as a module.** Port ThreadIQ's `Scripts/data-grid.ts` into `blazortoolkit-components.ts`, but
   export functions instead of assigning `window.hDataGrid`:
   `export function registerGrid(id, dotnetRef)`, `unregisterGrid(id)`, `showOffcanvas(id)`.
   In `HDataGrid`, lazily `JS.InvokeAsync<IJSObjectReference>("import", "./_content/DevInstance.BlazorToolkit.Components/js/blazortoolkit-components.js")`
   in `OnAfterRenderAsync(firstRender)`, and dispose the module reference in `DisposeAsync`
   (swallow `JSDisconnectedException` — Server circuits). Keep the `[JSInvokable] OnColumnsResized` contract.
5. **No `eval`, no fixed id.** `ContextMenuShowSettings` currently runs
   `eval("bootstrap.Offcanvas.getOrCreateInstance(document.querySelector('#offcanvasGridSettings')).show()")`.
   Replace with `module.showOffcanvas(SettingsId)`, where `showOffcanvas` uses `window.bootstrap?.Offcanvas`
   and no-ops if Bootstrap JS is absent. Add `[Parameter] string SettingsId = "offcanvasGridSettings"`
   to `HDataGrid` and `[Parameter] string Id = "offcanvasGridSettings"` to `GridSettings`
   (used for `id`, `data-bs-target`, `aria-controls`, `aria-labelledby`). Defaults keep today's behaviour.
6. **Server + WASM.** Nothing may assume WASM: no `IJSInProcessRuntime`, no synchronous JS calls, all
   JS calls after first render. Two consumers (Tentrie, coreblog) are interactive Server.
7. **Leave behind:** `GridProfileColumnExtensions` (depends on the app's `GridProfileItem`).
8. Strings stay English for now; do not add a localisation layer in this release.

## Consumer prerequisites (put in README + USAGE)

- Bootstrap 5.3 CSS **and** JS bundle (context menu, offcanvas, placeholders), `bootstrap-icons`.
- `<link href="_content/DevInstance.BlazorToolkit.Components/css/blazortoolkit-components.css" rel="stylesheet" />`
  after Bootstrap and before the app's own CSS (so the app can override `--btk-*`).
- No `<script>` tag — the module loads itself.
- `@using DevInstance.BlazorToolkit.Components.DataGrid` / `.Search` in `_Imports.razor`.

## Docs, sample, tests

- Port ThreadIQ's `HDataGrid.md` to `docs/components.md`; update every `ModelList<TItem>`/`PagedList`
  mention to `IModelList<TItem>`, and document `SettingsId`, the `--btk-*` variables and the prerequisites.
- Add a grid page to `example/` (Samples.Client) with in-memory data: sort, page, group, resize,
  select, search highlight, two grids + two `GridSettings` on one page (proves the id fix).
- Tests (new `tests/DevInstance.BlazorToolkit.Components.Tests`, xUnit v3; bUnit if you want render tests):
  at minimum the pure logic — `HeaderWidthStyle` percent scaling, `GroupKeyComparer` ordering,
  sort-field parsing, `HighlightRange.Segments` (lives in WST tests). Make the helpers `internal` +
  `InternalsVisibleTo` rather than public.
- Update `CLAUDE.md` here: second package, its namespaces, the module/CSS asset paths, and that both
  packages share a version.

## Definition of done

- `dotnet build -c Release` produces both `.nupkg`s with matching versions; the Components package
  lists the core package and WST.Common as dependencies.
- The sample page works in the hosted WASM sample with no extra `<script>` tags.
- ThreadIQ builds against a local feed of the package with only the changes listed in its plan.

## Wave 2 (later, not this release)

Candidates that are generic but still carry app coupling: `CopyButton` (needs a `copyText` export in
the same module), `TimeSeriesChart`/`ChartLegend`/`ChartSeries` (pure SVG, `--chart-*` variables already
namespaced), `DateRangeStepper`. Anything that drags a heavy dependency (Markdig for the markdown
viewers) gets its own package rather than joining this one.
