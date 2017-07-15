using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Reflection;
using KSP.UI.Screens;
using ScrapYard;
using ScrapYard.Modules;

namespace Untitled_Part_Failure_Mod
{
    [KSPAddon(KSPAddon.Startup.EditorAny, false)]
    class EditorAnyWarnings : EditorWarnings
    {

    }
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    class FlightWarnings : EditorWarnings
    {
        
    }
    class EditorWarnings : MonoBehaviour
    {
        public Dictionary<Part,int> damagedParts = new Dictionary<Part, int>();
        public Dictionary<Part, int> brokenParts = new Dictionary<Part, int>();
        public Dictionary<string, float> randomisation = new Dictionary<string, float>();
        public bool display = false;
        bool dontBother = false;
        public static EditorWarnings instance;
        Rect Window = new Rect(500, 100, 240, 50);
        StringBuilder s = new StringBuilder();

        ApplicationLauncherButton ToolbarButton;

        private void Awake()
        {
            instance = this;
        }
        public float GetRandomisation(Part p)
        {
            ModuleSYPartTracker SYP = p.FindModuleImplementing<ModuleSYPartTracker>();
            if (SYP == null) return 0;
            float f;
            if (randomisation.TryGetValue(SYP.ID, out f)) return f;
            int builds;
            if (HighLogic.LoadedSceneIsEditor) builds = ScrapYardWrapper.GetBuildCount(p, ScrapYardWrapper.TrackType.NEW) + 1;
            else builds = ScrapYardWrapper.GetBuildCount(p, ScrapYardWrapper.TrackType.NEW);
            f = (UnityEngine.Random.value / 3) / builds;
            f = (float)Math.Round(f, 2);
            randomisation.Add(SYP.ID, f);
            Debug.Log("[UPFM]: Applied Random Factor of " + f + " to part " + p.partName);
            return f;
        }


        private void Start()
        {
            GameEvents.onGUIApplicationLauncherReady.Add(GUIReady);
            s.Append("WARNING: The following parts are above the safety threshold");
            if (!HighLogic.CurrentGame.Parameters.CustomParams<UPFMSettings>().safetyRecover) s.Append(" and won't be recovered");
        }

        public void GUIReady()
        {
            if (HighLogic.LoadedSceneIsEditor) return;
            if (ToolbarButton == null)
            {
                ToolbarButton = ApplicationLauncher.Instance.AddModApplication(GUISwitch, GUISwitch, null, null, null, null, ApplicationLauncher.AppScenes.ALWAYS, GameDatabase.Instance.GetTexture("UntitledFailures/Icon", false));
            }
        }
        public void GUISwitch()
        {
            if (display)
            {
                display = false;
            }
            else
            {
                display = true;
            }
        }
        private void OnGUI()
        {
            if (!HighLogic.CurrentGame.Parameters.CustomParams<UPFMSettings>().safetyWarning) return;
            if (dontBother) return;
            if (!display) return;
            if (damagedParts.Count == 0 && HighLogic.LoadedSceneIsEditor) return;
            Window = GUILayout.Window(98399854, Window, GUIDisplay, "UPFM", GUILayout.Width(200));
        }
        void GUIDisplay(int windowID)
        {
            int counter = 0;
            GUILayout.Label(s.ToString());
            foreach (var v in damagedParts)
            {
                if (v.Key == null) continue;
                GUILayout.Label(v.Key.name + ": " + v.Value+"%");
                counter++;
            }
            GUILayout.Label("Broken Parts:");
            foreach(var v in brokenParts)
            {
                int repairChance = 0;
                if (v.Key == null) continue;
                if (!v.Key.Modules.Contains("Broken")) repairChance = 100-v.Value;
                GUILayout.Label(v.Key.name + ": Chance of Repair: " + (repairChance) + "%");
            }
            if (HighLogic.LoadedSceneIsEditor)
            {
                if (counter == 0) display = false;
                if (GUILayout.Button("Dismiss"))
                {
                    display = false;
                }
                if (GUILayout.Button("Stop Bothering Me"))
                {
                    display = false;
                    dontBother = true;
                }
            }
            GUI.DragWindow();
        }

        private void OnDestroy()
        {
            display = false;
            GameEvents.onGUIApplicationLauncherReady.Remove(GUIReady);
            if (ToolbarButton == null) return;
            ApplicationLauncher.Instance.RemoveModApplication(ToolbarButton);
        }
    }
}
