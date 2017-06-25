using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Reflection;
using KSP.UI.Screens;

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
        public bool display = false;
        bool dontBother = false;
        public static EditorWarnings instance;
        Rect Window = new Rect(500, 100, 240, 50);

        ApplicationLauncherButton ToolbarButton;

        private void Awake()
        {
            instance = this;
        }

        private void Start()
        {
            GameEvents.onGUIApplicationLauncherReady.Add(GUIReady);
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
            GUILayout.Label("WARNING: The following parts are above the safety threshold");
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
