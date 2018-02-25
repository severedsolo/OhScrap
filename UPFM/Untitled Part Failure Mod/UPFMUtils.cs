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
        int vesselSafetyRating = 6;
        Part worstPart;
        public bool display = false;
        bool dontBother = false;
        public static UPFMUtils instance;
        Rect Window = new Rect(500, 100, 240, 50);
        ApplicationLauncherButton ToolbarButton;
        ShipConstruct editorConstruct;

        private void Awake()
        {
            instance = this;
        }
        private void Start()
        {
            GameEvents.onPartDie.Add(OnPartDie);
            GameEvents.onGUIApplicationLauncherReady.Add(GUIReady);
            GameEvents.onEditorShipModified.Add(onEditorShipModified);
            GameEvents.OnFlightGlobalsReady.Add(OnFlightGlobalsReady);
            if (!HighLogic.LoadedSceneIsEditor) display = true;
        }

        private void OnFlightGlobalsReady(bool data)
        {
            vesselSafetyRating = 6;
        }

        private void onEditorShipModified(ShipConstruct shipConstruct)
        {
            vesselSafetyRating = 6;
            editorConstruct = shipConstruct;
            for(int i = 0; i< shipConstruct.parts.Count(); i++)
            {
                Part p = shipConstruct.parts.ElementAt(i);
                List<BaseFailureModule> bfmList = p.FindModulesImplementing<BaseFailureModule>();
                for (int b = 0; b < bfmList.Count(); b++)
                {
                    BaseFailureModule bfm = bfmList.ElementAt(b);
                    if (bfm == null) continue;
                    if (bfm.safetyRating < vesselSafetyRating && !bfm.excluded)
                    {
                        vesselSafetyRating = bfm.safetyRating;
                        worstPart = p;
                    }
                }
            }
        }

        void Update()
        {
            if (vesselSafetyRating != 6) return;
            if (!HighLogic.LoadedSceneIsEditor && FlightGlobals.ready)
            {
                for (int i = 0; i < FlightGlobals.ActiveVessel.parts.Count(); i++)
                {
                    Part p = FlightGlobals.ActiveVessel.parts.ElementAt(i);
                    List<BaseFailureModule> bfmList = p.FindModulesImplementing<BaseFailureModule>();
                    for (int b = 0; b < bfmList.Count(); b++)
                    {
                        BaseFailureModule bfm = bfmList.ElementAt(b);
                        if (bfm == null) continue;
                        if (bfm.safetyRating < vesselSafetyRating)
                        {
                            vesselSafetyRating = bfm.safetyRating;
                            worstPart = p;
                        }
                    }
                }
            }
            if (HighLogic.LoadedSceneIsEditor && editorConstruct != null)
            {
                for (int i = 0; i < editorConstruct.parts.Count(); i++)
                {
                    Part p = editorConstruct.parts.ElementAt(i);
                    List<BaseFailureModule> bfmList = p.FindModulesImplementing<BaseFailureModule>();
                    for (int b = 0; b < bfmList.Count(); b++)
                    {
                        BaseFailureModule bfm = bfmList.ElementAt(b);
                        if (bfm == null) continue;
                        if (bfm.safetyRating < vesselSafetyRating)
                        {
                            vesselSafetyRating = bfm.safetyRating;
                            worstPart = p;
                        }
                    }
                }
            }
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
            int randomFactor = 8;
            if (builds > 0) randomFactor = 8 / builds;
            if (randomFactor > 1) f = (Randomiser.instance.RandomInteger(1, randomFactor) / 100.0f);
            float threshold = HighLogic.CurrentGame.Parameters.CustomParams<UPFMSettings>().safetyThreshold / 100.0f;
            if (f > threshold) f = threshold - 0.01f;
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
            if (ToolbarButton == null && HighLogic.LoadedSceneIsEditor)
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
            if (!display) return; ;
            if (FlightGlobals.ActiveVessel != null)
            {
                if (FlightGlobals.ActiveVessel.FindPartModuleImplementing<KerbalEVA>() != null) return;
            }
            Window = GUILayout.Window(98399854, Window, GUIDisplay, "UPFM", GUILayout.Width(200));
        }
        void GUIDisplay(int windowID)
        {
            string s;
            switch(vesselSafetyRating)
            {
                case 5:
                    s = "(Excellent)";
                    break;
                case 4:
                    s = "(Good)";
                    break;
                case 3:
                    s = "(Average)";
                    break;
                case 2:
                    s = "(Poor)";
                    break;
                case 1: s = "(Terrible)";
                    break;
                case 0:
                    s = "(Failure Imminent)";
                    break;
                default:
                    s = "(Something went wrong. Report to the UPFM thread on the forum with a log)";
                    break;
            }
            GUILayout.Label("Vessel Safety Rating: " + vesselSafetyRating+" "+s);
            if(worstPart != null) GUILayout.Label("Worst Part: " + worstPart.name);
            GUILayout.Label("");
            GUILayout.Label("Broken Parts:");
            foreach (var v in brokenParts)
            {
                int repairChance = 0;
                if (v.Key == null) continue;
                if (!v.Key.Modules.Contains("Broken")) repairChance = 100 - v.Value;
                GUILayout.Label(v.Key.name + ": Chance of Repair: " + (repairChance) + "%");
            }
            if (GUILayout.Button("Close"))
            {
                display = false;
            }
            GUI.DragWindow();
        }


        private void OnDestroy()
        {
            display = false;
            GameEvents.onGUIApplicationLauncherReady.Remove(GUIReady);
            GameEvents.onPartDie.Remove(OnPartDie);
            GameEvents.OnFlightGlobalsReady.Remove(OnFlightGlobalsReady);
            if (ToolbarButton == null) return;
            ApplicationLauncher.Instance.RemoveModApplication(ToolbarButton);
        }
    }
}
