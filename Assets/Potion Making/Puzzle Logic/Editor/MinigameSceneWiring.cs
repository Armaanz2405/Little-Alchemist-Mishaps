using System;
using System.Linq;
using Nocturne.PotionMaking.Energy;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Nocturne.PotionMaking.Minigames.Editor
{
    // Wires the minigames into whatever scene is open: the games go on the "Minigames" object, the cutting board and
    // cauldron stations get their A/B alternates, and the mortar gets a grind station. Safe to run again, it only adds
    // what's missing. Nocturne > Minigames > Wire into open scene, or from the command line:
    //   Unity -batchmode -projectPath <repo> -executeMethod Nocturne.PotionMaking.Minigames.Editor.MinigameSceneWiring.WireSceneCli -scene "Assets/Scenes/X.unity"
    public static class MinigameSceneWiring
    {
        [MenuItem("Nocturne/Minigames/Wire into open scene")]
        public static void WireOpenScene()
        {
            var report = Wire();
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("[Minigames] " + report);
        }

        public static void WireSceneCli()
        {
            string scenePath = null;
            var args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++) if (args[i] == "-scene") scenePath = args[i + 1];
            if (string.IsNullOrEmpty(scenePath)) throw new Exception("pass -scene <path.unity>");
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            var report = Wire();
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[Minigames] " + report);
            EditorApplication.Exit(0);
        }

        private static string Wire()
        {
            var launcher = UnityEngine.Object.FindAnyObjectByType<MinigameLauncher>(FindObjectsInactive.Include);
            var host = launcher != null ? launcher.gameObject : GameObject.Find("Minigames");
            if (host == null)
            {
                host = new GameObject("Minigames");
                Undo.RegisterCreatedObjectUndo(host, "Minigames host");
            }
            if (launcher == null) launcher = host.AddComponent<MinigameLauncher>();

            var knife = Ensure<KnifeSliceMinigame>(host);
            var stir = Ensure<StirTimingMinigame>(host);
            var grind = Ensure<GrindMinigame>(host);
            var rhythm = Ensure<RhythmChopMinigame>(host);
            var brew = Ensure<CauldronBrewMinigame>(host);
            var flameflies = Ensure<SkyFishingMinigame>(host);
            Set(launcher, ("knife", knife), ("stir", stir), ("grind", grind), ("rhythm", rhythm), ("brew", brew), ("flameflies", flameflies));

            var report = "";
            var board = UnityEngine.Object.FindAnyObjectByType<CuttingBoard>(FindObjectsInactive.Include);
            if (board != null)
            {
                var station = Ref<MinigameStation>(board, "chopMinigameStation") ?? NewStation(board.gameObject, knife, "E: chop", MinigameStation.EnergyRule.Block, EnergyAction.Refine, 1.8f);
                Set(board, ("chopMinigameStation", station));
                Set(station, ("minigame", knife), ("alternate", rhythm));
                SetInt(station, "slot", (int)MinigameStation.VariantSlot.Chop);
                Listen(knife.onFinished, board, board.Chop_Trigger);
                Listen(rhythm.onFinished, board, board.Chop_Trigger);
                report += "cutting board: knife + chop to the beat. ";
            }
            else report += "no CuttingBoard in the scene. ";

            var cauldron = UnityEngine.Object.FindAnyObjectByType<PuzzleLogic_Cauldron>(FindObjectsInactive.Include);
            if (cauldron != null)
            {
                var station = Ref<MinigameStation>(cauldron, "stirMinigameStation") ?? NewStation(cauldron.gameObject, stir, "E: stir", MinigameStation.EnergyRule.ZeroScore, EnergyAction.Bottle, 1f);
                Set(cauldron, ("stirMinigameStation", station));
                Set(station, ("minigame", stir), ("alternate", brew));
                SetInt(station, "slot", (int)MinigameStation.VariantSlot.Cauldron);
                Listen(stir.onFinished, cauldron, cauldron.Brew_Trigger);
                Listen(brew.onFinished, cauldron, cauldron.Brew_Trigger);
                report += "cauldron: stir ring + brew. ";
            }
            else report += "no PuzzleLogic_Cauldron in the scene. ";

            var mortar = UnityEngine.Object.FindAnyObjectByType<MortarNPestle>(FindObjectsInactive.Include);
            if (mortar != null)
            {
                var station = Ref<MinigameStation>(mortar, "grindMinigameStation") ?? NewStation(mortar.gameObject, grind, "E: grind", MinigameStation.EnergyRule.Block, EnergyAction.Refine, 1.8f);
                Set(mortar, ("grindMinigameStation", station));
                Set(station, ("minigame", grind));
                var prompt = mortar.GetComponent<MortarPrompt>();
                if (prompt != null) Set(prompt, ("station", station));
                Listen(grind.onFinished, mortar, mortar.Grind_Trigger);
                report += "mortar: grind. ";
            }
            else report += "no MortarNPestle in the scene. ";

            return report;
        }

        private static T Ensure<T>(GameObject go) where T : Component
        {
            var c = go.GetComponent<T>();
            if (c == null) c = Undo.AddComponent<T>(go);
            return c;
        }

        private static MinigameStation NewStation(GameObject go, Minigame game, string prompt, MinigameStation.EnergyRule rule, EnergyAction action, float range)
        {
            var station = Undo.AddComponent<MinigameStation>(go);
            var player = GameObject.FindWithTag("Player");
            var so = new SerializedObject(station);
            so.FindProperty("minigame").objectReferenceValue = game;
            so.FindProperty("player").objectReferenceValue = player != null ? player.transform : null;
            so.FindProperty("holdPoint").objectReferenceValue = player != null ? player.transform : null;
            so.FindProperty("range").floatValue = range;
            so.FindProperty("key").intValue = (int)Key.E;
            so.FindProperty("prompt").stringValue = prompt;
            so.FindProperty("showLabel").boolValue = false;
            so.FindProperty("waitUntilReady").boolValue = true;
            so.FindProperty("energyRule").intValue = (int)rule;
            so.FindProperty("energyAction").intValue = (int)action;
            so.ApplyModifiedPropertiesWithoutUndo();
            return station;
        }

        private static T Ref<T>(UnityEngine.Object owner, string field) where T : UnityEngine.Object
        {
            var p = new SerializedObject(owner).FindProperty(field);
            return p != null ? p.objectReferenceValue as T : null;
        }

        private static void Set(UnityEngine.Object owner, params (string field, UnityEngine.Object value)[] refs)
        {
            var so = new SerializedObject(owner);
            foreach (var (field, value) in refs)
            {
                var p = so.FindProperty(field);
                if (p == null) Debug.LogWarning($"[Minigames] {owner.GetType().Name} has no field {field}");
                else p.objectReferenceValue = value;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetInt(UnityEngine.Object owner, string field, int value)
        {
            var so = new SerializedObject(owner);
            so.FindProperty(field).intValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // adds target.method to the event unless it's already there
        private static void Listen(UnityEvent<float> evt, UnityEngine.Object target, UnityAction method)
        {
            for (int i = 0; i < evt.GetPersistentEventCount(); i++)
                if (evt.GetPersistentTarget(i) == target && evt.GetPersistentMethodName(i) == method.Method.Name) return;
            UnityEventTools.AddVoidPersistentListener(evt, method);
        }
    }
}
