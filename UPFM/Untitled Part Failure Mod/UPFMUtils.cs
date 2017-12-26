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
    class EditorAnyWarnings : UPFMUtils
    {

    }
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    class FlightWarnings : UPFMUtils
    {
        
    }
    class UPFMUtils : MonoBehaviour
    {
        public Dictionary<Part,int> damagedParts = new Dictionary<Part, int>();
        public Dictionary<Part, int> brokenParts = new Dictionary<Part, int>();
        public Dictionary<string, float> randomisation = new Dictionary<string, float>();
        public Dictionary<string, int> batteryLifetimes = new Dictionary<string, int>();
        public Dictionary<string, int> controlSurfaceLifetimes = new Dictionary<string, int>();
        public Dictionary<string, int> engineLifetimes = new Dictionary<string, int>();
        public Dictionary<string, int> parachuteLifetimes = new Dictionary<string, int>();
        public Dictionary<string, int> reactionWheelLifetimes = new Dictionary<string, int>();
        public Dictionary<string, int> solarPanelLifetimes = new Dictionary<string, int>();
        public Dictionary<string, int> tankLifetimes = new Dictionary<string, int>();
        public Dictionary<string, int> RCSLifetimes = new Dictionary<string, int>();

        public bool display = false;
        bool dontBother = false;
        public static UPFMUtils instance;
        Rect Window = new Rect(500, 100, 240, 50);
        StringBuilder s = new StringBuilder();
        ApplicationLauncherButton ToolbarButton;

        private void Awake()
        {
            instance = this;
        }
        private void Start()
        {
            GameEvents.onPartDie.Add(OnPartDie);
            GameEvents.onGUIApplicationLauncherReady.Add(GUIReady);
            s.Append("WARNING: The following parts are above the safety threshold");
            if (!HighLogic.CurrentGame.Parameters.CustomParams<UPFMSettings>().safetyRecover) s.Append(" and won't be recovered");
        }

        private void OnPartDie(Part part)
        {
            ModuleSYPartTracker SYP = part.FindModuleImplementing<ModuleSYPartTracker>();
            if (SYP == null) return;
            randomisation.Remove(SYP.ID);
            batteryLifetimes.Remove(SYP.ID);
            controlSurfaceLifetimes.Remove(SYP.ID);
            engineLifetimes.Remove(SYP.ID);
            parachuteLifetimes.Remove(SYP.ID);
            reactionWheelLifetimes.Remove(SYP.ID);
            solarPanelLifetimes.Remove(SYP.ID);
            tankLifetimes.Remove(SYP.ID);
#if DEBUG
            Debug.Log("[UPFM]: Stopped Tracking " + SYP.ID);
#endif
        }

        public float GetRandomisation(Part p)
        {
            ModuleSYPartTracker SYP = p.FindModuleImplementing<ModuleSYPartTracker>();
            if (SYP == null) return 0;
            float f = 0;
            if (randomisation.TryGetValue(SYP.ID, out f)) return f;
            int builds;
            if (HighLogic.LoadedSceneIsEditor) builds = ScrapYardWrapper.GetBuildCount(p, ScrapYardWrapper.TrackType.NEW) + 1;
            else builds = ScrapYardWrapper.GetBuildCount(p, ScrapYardWrapper.TrackType.NEW);
            f = (Randomiser.instance.RandomInteger(1, 10)/100)/builds;
            float threshold = HighLogic.CurrentGame.Parameters.CustomParams<UPFMSettings>().safetyThreshold/100.0f;
            if (f > threshold) f = threshold-0.1f;
            if (!float.IsNaN(f))
            {
                randomisation.Add(SYP.ID, f);
#if DEBUG
                Debug.Log("[UPFM]: Applied Random Factor of " + f + " to part " + SYP.ID);
#endif
            }
            return f;
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
            if (!HighLogic.LoadedSceneIsEditor)
            {
                if (FlightGlobals.ActiveVessel.FindPartModuleImplementing<KerbalEVA>() != null) return;
            }
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
                if (GUILayout.Button("Replace unsafe parts"))
                {
                    List<Part> repairedList = new List<Part>();
                    foreach (var v in damagedParts)
                    {
                        ModuleSYPartTracker SYP = v.Key.FindModuleImplementing<ModuleSYPartTracker>();
                        SYP.MakeFresh();
                        repairedList.Add(v.Key);
                        ScrapYardWrapper.RemovePartFromInventory(v.Key);
                    }
                    damagedParts.Clear();
                    if (repairedList.Count() == 0) return;
                    for(int d = 0; d<repairedList.Count; d++)
                    {
                        Part p = repairedList.ElementAt(d);
                        List<BaseFailureModule> failureModules = p.FindModulesImplementing<BaseFailureModule>();
                        if (failureModules.Count() == 0) continue;
                        for(int i = 0; i<failureModules.Count(); i++)
                        {
                            BaseFailureModule bfm = failureModules.ElementAt(i);
                            if (bfm == null) continue;
                            bfm.chanceOfFailure = bfm.baseChanceOfFailure;
                            bfm.Initialise();
                        }
                    }
                }
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
            GameEvents.onPartDie.Remove(OnPartDie);
            if (ToolbarButton == null) return;
            ApplicationLauncher.Instance.RemoveModApplication(ToolbarButton);
        }
    }
}
