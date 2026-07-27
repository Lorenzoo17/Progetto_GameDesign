#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Tool di editor per le stanze multi-cella.
///
/// A runtime la forma viene gia' ricavata da sola dalla gerarchia
/// (vedi RoomBehaviour.BuildCellsFromHierarchy), quindi questo tool NON e' obbligatorio:
/// serve a "cristallizzare" la configurazione nell'inspector e a riparare un prefab
/// in cui la RoomBehaviour manca o e' mal configurata.
///
/// Convenzione: il root della stanza ha un figlio per ogni cella occupata, chiamato
///   Cell_&lt;x&gt;_&lt;y&gt;      (x = destra, y = basso)
/// e dentro ogni cella devono esistere gli oggetti (a qualsiasi profondita'):
///   ClosedUp, ClosedDown, ClosedRight, ClosedLeft        -> muri
///   DoorUp, DoorBottom (o DoorDown), DoorRight, DoorLeft -> porte
///   CameraBounds                                         -> BoxCollider2D (opzionale)
/// </summary>
public static class RoomMultiCellSetup {

    private const string MENU = "Tools/Dungeon/Configura celle multi-cella";
    private const string MENU_LOG = "Tools/Dungeon/Log forma stanza selezionata";

    [MenuItem(MENU)]
    private static void ConfigureSelection() {

        GameObject[] selection = Selection.gameObjects;

        if (selection.Length == 0) {
            Debug.LogWarning("[MultiCell] Seleziona il prefab della stanza (nel Project o aperto in Prefab Mode).");
            return;
        }

        foreach (GameObject selected in selection) {

            string path = AssetDatabase.GetAssetPath(selected);

            if (!string.IsNullOrEmpty(path) && path.EndsWith(".prefab")) {

                GameObject contents = PrefabUtility.LoadPrefabContents(path);

                Configure(contents);

                PrefabUtility.SaveAsPrefabAsset(contents, path);
                PrefabUtility.UnloadPrefabContents(contents);

                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            }
            else {
                Configure(selected);
                EditorUtility.SetDirty(selected);
            }
        }

        AssetDatabase.SaveAssets();
    }

    [MenuItem(MENU_LOG)]
    private static void LogSelection() {

        foreach (GameObject selected in Selection.gameObjects) {

            RoomBehaviour room = selected.GetComponent<RoomBehaviour>();

            if (room == null) {
                Debug.LogError($"[MultiCell] '{selected.name}' NON ha una RoomBehaviour. " +
                               "Lancia 'Configura celle multi-cella' per aggiungerla.", selected);
                continue;
            }

            Vector2Int[] shape = room.GetShapeOffsets();

            string text = "";
            for (int i = 0; i < shape.Length; i++) text += (i > 0 ? " " : "") + shape[i];

            Debug.Log($"[MultiCell] '{selected.name}' -> {shape.Length} cella/e: {text}", selected);
        }
    }

    // ===============================

    private static void Configure(GameObject root) {

        RoomBehaviour room = root.GetComponent<RoomBehaviour>();

        if (room == null) {
            room = root.AddComponent<RoomBehaviour>();
            Debug.LogWarning($"[MultiCell] '{root.name}': RoomBehaviour mancante, aggiunta ora. " +
                             "Controlla 'Room Type' nell'inspector.");
        }

        RoomCellSetup[] built = RoomBehaviour.BuildCellsFromHierarchy(root.transform);

        SerializedObject so = new SerializedObject(room);

        // ---- cells ----
        SerializedProperty cellsProperty = so.FindProperty("cells");

        if (cellsProperty == null) {
            Debug.LogError("[MultiCell] Campo 'cells' non trovato: RoomBehaviour non e' aggiornata / non compila.");
            return;
        }

        if (built == null || built.Length == 0) {
            cellsProperty.arraySize = 0;
            Debug.Log($"[MultiCell] '{root.name}': nessun figlio 'Cell_x_y', resta una stanza 1x1.");
        }
        else {
            cellsProperty.arraySize = built.Length;

            for (int i = 0; i < built.Length; i++) {

                SerializedProperty element = cellsProperty.GetArrayElementAtIndex(i);

                element.FindPropertyRelative("label").stringValue = built[i].label;
                element.FindPropertyRelative("offset").vector2IntValue = built[i].offset;

                SerializedProperty blocks = element.FindPropertyRelative("blocks");
                SerializedProperty doors = element.FindPropertyRelative("doors");

                blocks.arraySize = 4;
                doors.arraySize = 4;

                for (int d = 0; d < 4; d++) {
                    blocks.GetArrayElementAtIndex(d).objectReferenceValue = built[i].blocks[d];
                    doors.GetArrayElementAtIndex(d).objectReferenceValue = built[i].doors[d];
                }

                element.FindPropertyRelative("cameraBounds").objectReferenceValue = built[i].cameraBounds;
            }
        }

        // ---- roomBounds ----
        SerializedProperty roomBounds = so.FindProperty("roomBounds");

        if (roomBounds.objectReferenceValue == null) {
            Transform bounds = FindTransform(root.transform, "RoomBounds");

            if (bounds != null) {
                roomBounds.objectReferenceValue = bounds.GetComponent<BoxCollider2D>();
            }
            else {
                Transform camera = FindTransform(root.transform, RoomBehaviour.CameraBoundsName);
                if (camera != null) roomBounds.objectReferenceValue = camera.GetComponent<BoxCollider2D>();
            }
        }

        // ---- roomCentre ----
        SerializedProperty roomCentre = so.FindProperty("roomCentre");

        if (roomCentre.objectReferenceValue == null) {
            Transform centre = FindTransform(root.transform, "RoomCentre");
            if (centre != null) roomCentre.objectReferenceValue = centre;
        }

        // ---- enemiesSpawnPoints ----
        SerializedProperty spawns = so.FindProperty("enemiesSpawnPoints");

        if (spawns.arraySize == 0) {
            List<Transform> points = new List<Transform>();
            CollectSpawnPoints(root.transform, points);

            spawns.arraySize = points.Count;
            for (int i = 0; i < points.Count; i++) {
                spawns.GetArrayElementAtIndex(i).objectReferenceValue = points[i];
            }
        }

        so.ApplyModifiedProperties();

        int cellCount = built == null ? 1 : built.Length;
        bool perCellCamera = built != null && built.Length > 0 && built[0].cameraBounds != null;

        Debug.Log($"[MultiCell] '{root.name}' configurata: {cellCount} cella/e, " +
                  $"{spawns.arraySize} spawn point" +
                  (perCellCamera ? ", camera bounds per cella" : ", camera sull'intera stanza"), root);
    }

    private static void CollectSpawnPoints(Transform root, List<Transform> result) {

        if (root.name == "EnemiesSpawnPoints") {
            foreach (Transform child in root) result.Add(child);
            return;
        }

        foreach (Transform child in root) CollectSpawnPoints(child, result);
    }

    private static Transform FindTransform(Transform root, string name) {

        if (root.name == name) return root;

        foreach (Transform child in root) {
            Transform found = FindTransform(child, name);
            if (found != null) return found;
        }

        return null;
    }
}
#endif
