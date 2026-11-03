// Generic IndexedDB wrapper for BlazorToolkit offline support.
// Compiled to wwwroot/js/blazortoolkit-db.js and shipped as the static web asset
// _content/DevInstance.BlazorToolkit/js/blazortoolkit-db.js.
// Called from C# (IndexedDbObjectStore) via IJSRuntime as window.blazortoolkit.db.*
//
// The store schema is supplied by the C# layer at open() time, so this file is
// app-agnostic: any app declares its own stores via OfflineStoreOptions.

interface IndexDef {
    name: string;
    keyPath: string;
    unique: boolean;
}

interface StoreDef {
    name: string;
    keyPath: string;
    indexes?: IndexDef[];
}

interface DbConfig {
    name: string;
    version: number;
    stores: StoreDef[];
}

(function () {
    let db: IDBDatabase | null = null;

    function open(configJson: string): Promise<void> {
        return new Promise((resolve, reject) => {
            if (db) { resolve(); return; }

            const config: DbConfig = JSON.parse(configJson);
            const request = indexedDB.open(config.name, config.version);

            request.onupgradeneeded = (e) => {
                const database = (e.target as IDBOpenDBRequest).result;
                for (const store of config.stores) {
                    let objectStore: IDBObjectStore;
                    if (!database.objectStoreNames.contains(store.name)) {
                        objectStore = database.createObjectStore(store.name, { keyPath: store.keyPath });
                    } else {
                        objectStore = (e.target as IDBOpenDBRequest).transaction!.objectStore(store.name);
                    }
                    if (store.indexes) {
                        for (const idx of store.indexes) {
                            if (!objectStore.indexNames.contains(idx.name)) {
                                objectStore.createIndex(idx.name, idx.keyPath, { unique: idx.unique });
                            }
                        }
                    }
                }
            };
            request.onsuccess = (e) => {
                db = (e.target as IDBOpenDBRequest).result;
                resolve();
            };
            request.onerror = (e) => reject((e.target as IDBOpenDBRequest).error);
        });
    }

    function get(storeName: string, id: string): Promise<string | null> {
        return new Promise((resolve, reject) => {
            const tx = db!.transaction(storeName, "readonly");
            const request = tx.objectStore(storeName).get(id);
            request.onsuccess = () => resolve(request.result ? JSON.stringify(request.result) : null);
            request.onerror = () => reject(request.error);
        });
    }

    function getAll(storeName: string): Promise<string> {
        return new Promise((resolve, reject) => {
            const tx = db!.transaction(storeName, "readonly");
            const request = tx.objectStore(storeName).getAll();
            request.onsuccess = () => resolve(JSON.stringify(request.result || []));
            request.onerror = () => reject(request.error);
        });
    }

    function getByIndex(storeName: string, indexName: string, value: unknown): Promise<string> {
        return new Promise((resolve, reject) => {
            const tx = db!.transaction(storeName, "readonly");
            const index = tx.objectStore(storeName).index(indexName);
            const request = index.getAll(value as IDBValidKey);
            request.onsuccess = () => resolve(JSON.stringify(request.result || []));
            request.onerror = () => reject(request.error);
        });
    }

    function put(storeName: string, jsonString: string): Promise<void> {
        return new Promise((resolve, reject) => {
            const record = JSON.parse(jsonString);
            const tx = db!.transaction(storeName, "readwrite");
            const request = tx.objectStore(storeName).put(record);
            request.onsuccess = () => resolve();
            request.onerror = () => reject(request.error);
        });
    }

    function deleteRecord(storeName: string, id: string): Promise<void> {
        return new Promise((resolve, reject) => {
            const tx = db!.transaction(storeName, "readwrite");
            const request = tx.objectStore(storeName).delete(id);
            request.onsuccess = () => resolve();
            request.onerror = () => reject(request.error);
        });
    }

    function replaceAll(storeName: string, jsonString: string): Promise<void> {
        return new Promise((resolve, reject) => {
            const records: unknown[] = JSON.parse(jsonString);
            const tx = db!.transaction(storeName, "readwrite");
            const store = tx.objectStore(storeName);
            store.clear();
            records.forEach((r) => store.put(r));
            tx.oncomplete = () => resolve();
            tx.onerror = () => reject(tx.error);
        });
    }

    function clear(storeName: string): Promise<void> {
        return new Promise((resolve, reject) => {
            const tx = db!.transaction(storeName, "readwrite");
            const request = tx.objectStore(storeName).clear();
            request.onsuccess = () => resolve();
            request.onerror = () => reject(request.error);
        });
    }

    function clearAll(): Promise<void> {
        return new Promise((resolve, reject) => {
            const storeNames = Array.from(db!.objectStoreNames);
            const tx = db!.transaction(storeNames, "readwrite");
            storeNames.forEach((name) => tx.objectStore(name).clear());
            tx.oncomplete = () => resolve();
            tx.onerror = () => reject(tx.error);
        });
    }

    const w = window as any;
    w.blazortoolkit = w.blazortoolkit || {};
    w.blazortoolkit.db = {
        open,
        get,
        getAll,
        getByIndex,
        put,
        delete: deleteRecord,
        replaceAll,
        clear,
        clearAll,
    };
})();
