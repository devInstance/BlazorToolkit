// Generic IndexedDB wrapper for BlazorToolkit offline support.
// Source: src/Scripts/blazortoolkit-db.ts. Shipped as
// _content/DevInstance.BlazorToolkit/js/blazortoolkit-db.js.
// Called from C# (IndexedDbObjectStore) via IJSRuntime as window.blazortoolkit.db.*
// The store schema is supplied by the C# layer at open() time.
(function () {
    "use strict";
    var db = null;

    function open(configJson) {
        return new Promise(function (resolve, reject) {
            if (db) { resolve(); return; }
            var config = JSON.parse(configJson);
            var request = indexedDB.open(config.name, config.version);
            request.onupgradeneeded = function (e) {
                var database = e.target.result;
                for (var i = 0; i < config.stores.length; i++) {
                    var store = config.stores[i];
                    var objectStore;
                    if (!database.objectStoreNames.contains(store.name)) {
                        objectStore = database.createObjectStore(store.name, { keyPath: store.keyPath });
                    } else {
                        objectStore = e.target.transaction.objectStore(store.name);
                    }
                    if (store.indexes) {
                        for (var j = 0; j < store.indexes.length; j++) {
                            var idx = store.indexes[j];
                            if (!objectStore.indexNames.contains(idx.name)) {
                                objectStore.createIndex(idx.name, idx.keyPath, { unique: idx.unique });
                            }
                        }
                    }
                }
            };
            request.onsuccess = function (e) { db = e.target.result; resolve(); };
            request.onerror = function (e) { reject(e.target.error); };
        });
    }

    function get(storeName, id) {
        return new Promise(function (resolve, reject) {
            var tx = db.transaction(storeName, "readonly");
            var request = tx.objectStore(storeName).get(id);
            request.onsuccess = function () { resolve(request.result ? JSON.stringify(request.result) : null); };
            request.onerror = function () { reject(request.error); };
        });
    }

    function getAll(storeName) {
        return new Promise(function (resolve, reject) {
            var tx = db.transaction(storeName, "readonly");
            var request = tx.objectStore(storeName).getAll();
            request.onsuccess = function () { resolve(JSON.stringify(request.result || [])); };
            request.onerror = function () { reject(request.error); };
        });
    }

    function getByIndex(storeName, indexName, value) {
        return new Promise(function (resolve, reject) {
            var tx = db.transaction(storeName, "readonly");
            var request = tx.objectStore(storeName).index(indexName).getAll(value);
            request.onsuccess = function () { resolve(JSON.stringify(request.result || [])); };
            request.onerror = function () { reject(request.error); };
        });
    }

    function put(storeName, jsonString) {
        return new Promise(function (resolve, reject) {
            var record = JSON.parse(jsonString);
            var tx = db.transaction(storeName, "readwrite");
            var request = tx.objectStore(storeName).put(record);
            request.onsuccess = function () { resolve(); };
            request.onerror = function () { reject(request.error); };
        });
    }

    function deleteRecord(storeName, id) {
        return new Promise(function (resolve, reject) {
            var tx = db.transaction(storeName, "readwrite");
            var request = tx.objectStore(storeName).delete(id);
            request.onsuccess = function () { resolve(); };
            request.onerror = function () { reject(request.error); };
        });
    }

    function replaceAll(storeName, jsonString) {
        return new Promise(function (resolve, reject) {
            var records = JSON.parse(jsonString);
            var tx = db.transaction(storeName, "readwrite");
            var store = tx.objectStore(storeName);
            store.clear();
            records.forEach(function (r) { store.put(r); });
            tx.oncomplete = function () { resolve(); };
            tx.onerror = function () { reject(tx.error); };
        });
    }

    function clear(storeName) {
        return new Promise(function (resolve, reject) {
            var tx = db.transaction(storeName, "readwrite");
            var request = tx.objectStore(storeName).clear();
            request.onsuccess = function () { resolve(); };
            request.onerror = function () { reject(request.error); };
        });
    }

    function clearAll() {
        return new Promise(function (resolve, reject) {
            var storeNames = Array.from(db.objectStoreNames);
            var tx = db.transaction(storeNames, "readwrite");
            storeNames.forEach(function (name) { tx.objectStore(name).clear(); });
            tx.oncomplete = function () { resolve(); };
            tx.onerror = function () { reject(tx.error); };
        });
    }

    window.blazortoolkit = window.blazortoolkit || {};
    window.blazortoolkit.db = {
        open: open,
        get: get,
        getAll: getAll,
        getByIndex: getByIndex,
        put: put,
        delete: deleteRecord,
        replaceAll: replaceAll,
        clear: clear,
        clearAll: clearAll
    };
})();
