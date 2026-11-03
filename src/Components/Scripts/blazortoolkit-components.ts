// ES module for DevInstance.BlazorToolkit.Components. HDataGrid imports it itself
// ("./_content/DevInstance.BlazorToolkit.Components/js/blazortoolkit-components.js"); consumers add no <script> tag.
//
// Source of wwwroot/js/blazortoolkit-components.js. There is no TypeScript build step in this repo: keep the
// two in sync by hand.

interface DotNetObjectReference {
    invokeMethodAsync(methodName: string, ...args: unknown[]): Promise<unknown>;
}

interface BootstrapOffcanvas {
    getOrCreateInstance(element: Element): { show(): void };
}

// ---------------------------------------------------------------------------------------------------------
// Column resizing for HDataGrid. Each grid renders data-hdg-id on its <table> and registers a .NET callback
// under that id; one document-level listener (added once, when the module is first imported) serves every
// grid on the page.
//
// Dragging a header's handle moves the border between that column and its right-hand neighbour, so the
// table's total width never changes. On release every data column's width is reported back as a percentage
// of the data columns' total, which HDataGrid stores in ColumnDescriptor.ResizedWidth.
// ---------------------------------------------------------------------------------------------------------

const minColumnWidth = 48;
const grids = new Map<string, DotNetObjectReference>();

function dataHeaders(table: HTMLTableElement): HTMLTableCellElement[] {
    return Array.from(table.querySelectorAll<HTMLTableCellElement>("thead th[data-hdg-field]"));
}

document.addEventListener("pointerdown", (e: PointerEvent) => {
    if (e.button !== 0 || !(e.target instanceof Element)) return;
    const handle = e.target.closest<HTMLElement>(".hdg-resizer");
    const th = handle?.closest<HTMLTableCellElement>("th[data-hdg-field]");
    const table = th?.closest<HTMLTableElement>("table[data-hdg-id]");
    if (!handle || !th || !table) return;

    const headers = dataHeaders(table);
    const index = headers.indexOf(th);
    const next = headers[index + 1];
    if (index < 0 || !next) return;

    e.preventDefault();
    e.stopPropagation();

    // Freeze every column at its rendered pixel width so only the two either side of the border move.
    const startWidths = headers.map(h => h.getBoundingClientRect().width);
    headers.forEach((h, i) => h.style.width = `${startWidths[i]}px`);
    const pairWidth = startWidths[index] + startWidths[index + 1];
    const startX = e.clientX;

    handle.setPointerCapture(e.pointerId);
    document.body.classList.add("hdg-resizing");

    const onMove = (ev: PointerEvent) => {
        const left = Math.min(Math.max(startWidths[index] + ev.clientX - startX, minColumnWidth), pairWidth - minColumnWidth);
        th.style.width = `${left}px`;
        next.style.width = `${pairWidth - left}px`;
    };

    const onEnd = () => {
        handle.removeEventListener("pointermove", onMove);
        handle.removeEventListener("pointerup", onEnd);
        handle.removeEventListener("pointercancel", onEnd);
        document.body.classList.remove("hdg-resizing");

        const widths = headers.map(h => h.getBoundingClientRect().width);
        const total = widths.reduce((sum, w) => sum + w, 0);
        if (total <= 0) return;

        const result = headers.map((h, i) => {
            const width = `${(widths[i] * 100 / total).toFixed(2)}%`;
            h.style.width = width;
            return { field: h.dataset.hdgField ?? "", width };
        });
        void grids.get(table.dataset.hdgId ?? "")?.invokeMethodAsync("OnColumnsResized", result);
    };

    handle.addEventListener("pointermove", onMove);
    handle.addEventListener("pointerup", onEnd);
    handle.addEventListener("pointercancel", onEnd);
});

export function registerGrid(gridId: string, dotNetRef: DotNetObjectReference): void {
    grids.set(gridId, dotNetRef);
}

export function unregisterGrid(gridId: string): void {
    grids.delete(gridId);
}

// ---------------------------------------------------------------------------------------------------------
// Opens a Bootstrap offcanvas (GridSettings) by element id. No-op when the element or Bootstrap's JS bundle
// is missing.
// ---------------------------------------------------------------------------------------------------------

export function showOffcanvas(id: string): void {
    const element = document.getElementById(id);
    const offcanvas = (window as unknown as { bootstrap?: { Offcanvas?: BootstrapOffcanvas } }).bootstrap?.Offcanvas;
    if (!element || !offcanvas) return;
    offcanvas.getOrCreateInstance(element).show();
}
