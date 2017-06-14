using ScrapYard.Modules;
using ScrapYard;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Untitled_Part_Failure_Mod
{
    class BaseFailureModule : PartModule
    {
        UnityEngine.Random r = new UnityEngine.Random();
        public bool willFail = false;
        [KSPField(isPersistant = true, guiActive = false)]
        public bool hasFailed = false;
        public float expectedLifetime = 2;
        ModuleSYPartTracker SYP;
        public float chanceOfFailure = 0.5f;
        [KSPField(isPersistant = true, guiActive = true, guiName = "BaseFailure" ,guiActiveEditor = true, guiUnits = "%")]
        public int displayChance = 0;
        [KSPField(isPersistant = true, guiActive = false)]
        public int generation = 0;
        [KSPField(isPersistant = true, guiActive = false)]
        double failureTime = 0;
        public double maxTimeToFailure = 1800;

        private void Start()
        {
            Overrides();
            ScrapYardEvents.OnSYTrackerUpdated.Add(OnSYTrackerUpdated);
            ScrapYardEvents.OnSYInventoryAppliedToVessel.Add(OnSYInventoryAppliedToVessel);
            Initialise();
            GameEvents.onLaunch.Add(onLaunch);
        }

        private void OnSYInventoryAppliedToVessel()
        {
            Debug.Log("[UPFM]: ScrayYard Inventory Applied. Recalculating failure chance");
            if(EditorWarnings.instance != null) EditorWarnings.instance.damagedParts.Remove(part);
            willFail = false;
            generation = 0;
            chanceOfFailure = 0.5f;
            Initialise();
        }

        private void onLaunch(EventReport data)
        {
            PartModule dontRecover = part.FindModuleImplementing<DontRecoverMe>();
            if (dontRecover == null) return;
            part.RemoveModule(dontRecover);
            Debug.Log("[UPFM]: " + part.name + "marked as recoverable");
        }

        private void OnSYTrackerUpdated(IEnumerable<InventoryPart> data)
        {
            Debug.Log("[UPFM]: ScrayYard Tracker updated. Recalculating failure chance");
            willFail = false;
            generation = 0;
            chanceOfFailure = 0.5f;
            part.AddModule("DontRecoverMe");
            Initialise();
        }

        private void Initialise()
        {
            SYP = part.FindModuleImplementing<ModuleSYPartTracker>();
            if (generation == 0)
            {
                generation = (ScrapYardWrapper.GetBuildCount(part, ScrapYardWrapper.TrackType.NEW) - SYP.TimesRecovered);
                if (HighLogic.LoadedSceneIsEditor && SYP.TimesRecovered == 0) generation++;
            }
            if (generation < 1) generation = 1;
            if (hasFailed)
            {
                Events["RepairChecks"].active = true;
                return;
            }
            if(part != null) Debug.Log("[UPFM]: " + part.name + " has initialised");
                if (FailCheck(true) && !HighLogic.LoadedSceneIsEditor)
                {
                    failureTime = Planetarium.GetUniversalTime() + (maxTimeToFailure * UnityEngine.Random.value);
                    willFail = true;
                    Debug.Log("[UPFM]: " + part.name + " will attempt to fail at " + failureTime);
                }
            displayChance = (int)(chanceOfFailure * 100);
            if(displayChance >= HighLogic.CurrentGame.Parameters.CustomParams<UPFMSettings>().safetyThreshold && EditorWarnings.instance != null)
            {
                if (EditorWarnings.instance.damagedParts.ContainsKey(part)) return;
                EditorWarnings.instance.damagedParts.Add(part, displayChance);
                EditorWarnings.instance.display = true;
            }
        }
        public void SetFailedHighlight()
        {
            part.SetHighlightColor(Color.red);
            part.SetHighlightType(Part.HighlightType.AlwaysOn);
            part.SetHighlight(true, false);
        }
        protected virtual void Overrides() { }

        protected virtual void FailPart() { }

        protected virtual void RepairPart() { }

        protected virtual bool FailureAllowed() { return false; }

        private void FixedUpdate()
        {
            if (HighLogic.LoadedSceneIsEditor) return;
            if (!FailureAllowed()) return;
            if (hasFailed)
            {
                FailPart();
                return;
            }
            if (!willFail) return;
            if (Planetarium.GetUniversalTime() < failureTime) return;
            hasFailed = true;
            Events["RepairChecks"].active = true;
            if (!FailCheck(false)) return;
            part.AddModule("Broken");
            Debug.Log("[UPFM]: " + part.name + "is too badly damaged to be salvaged");
        }

        bool FailCheck(bool recalcChance)
        {
            if (recalcChance)
            {
                chanceOfFailure = chanceOfFailure / generation;
                if (SYP.TimesRecovered > 0) chanceOfFailure = chanceOfFailure * ((SYP.TimesRecovered / expectedLifetime));
            }
            if(part != null) Debug.Log("[UPFM]: Chances of "+part.name+" failing calculated to be " + chanceOfFailure * 100 + "%");
            if (UnityEngine.Random.value < chanceOfFailure) return true;
            return false;
        }
        [KSPEvent(active = true, guiActiveUnfocused = false, unfocusedRange = 5.0f, externalToEVAOnly = false, guiName = "Trash Part ")]
        public void TrashPart()
        {
            if (part.FindModuleImplementing<Broken>() == null) part.AddModule("Broken");
            ScreenMessages.PostScreenMessage("This part will not be recovered");
        }
        [KSPEvent(active = false, guiActiveUnfocused = true, unfocusedRange = 5.0f, externalToEVAOnly = true, guiName = "Repair ")]
        public void RepairChecks()
        {
            Debug.Log("[UPFM]: Attempting EVA repairs");
            if(FailCheck(false) || part.Modules.Contains("Broken"))
            {
                ScreenMessages.PostScreenMessage("This part is beyond repair");
                if (!part.Modules.Contains("Broken")) part.AddModule("Broken");
                Debug.Log("[UPFM]: " + part.name + " is too badly damaged to be fixed");
                return;
            }
            hasFailed = false;
            willFail = false;
            ScreenMessages.PostScreenMessage("The part should be ok to use now");
            Events["RepairChecks"].active = false;
            RepairPart();
            Debug.Log("[UPFM]: " + part.name + " was successfully repaired");
            part.highlightType = Part.HighlightType.OnMouseOver;
        }

        private void OnDestroy()
        {
            GameEvents.onLaunch.Remove(onLaunch);
            if (ScrapYardEvents.OnSYTrackerUpdated == null) return;
            ScrapYardEvents.OnSYTrackerUpdated.Remove(OnSYTrackerUpdated);
        }
    }
}
