using ScrapYard.Modules;
using ScrapYard;
using UnityEngine;
using System;
using System.Collections.Generic;

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
        float chanceOfFailure = 0.5f;
        [KSPField(isPersistant = true, guiActive = true, guiName = "Generation")]
        public int generation = 0;
        double failureTime = 0;

        private void Start()
        {
            if (HighLogic.LoadedSceneIsEditor) return;
            part.AddModule("DontRecoverMe");
            ScrapYardEvents.OnSYTrackerUpdated.Add(OnSYTrackerUpdated);
            Initialise();
            GameEvents.onStageActivate.Add(onStageActivate);
        }

        private void onStageActivate(int data)
        {
            PartModule dontRecover = part.FindModuleImplementing<DontRecoverMe>();
            if(dontRecover == null) return;
            part.RemoveModule(dontRecover);
            Debug.Log("[UPFM]: " + part.name + "marked as recoverable");
        }

        private void OnSYTrackerUpdated(IEnumerable<InventoryPart> data)
        {
            Debug.Log("[UPFM]: ScrayYard Tracker updated. Recalculating failure chance");
            willFail = false;
            generation = 0;
            chanceOfFailure = 0.5f;
            Initialise();
        }

        private void Initialise()
        {
            SYP = part.FindModuleImplementing<ModuleSYPartTracker>();
            if (generation == 0) generation = (ScrapYardWrapper.GetBuildCount(part, ScrapYardWrapper.TrackType.NEW) - SYP.TimesRecovered);
            if (generation < 1) generation = 1;
            if (hasFailed)
            {
                Events["RepairChecks"].active = true;
                return;
            }
            Debug.Log("[UPFM]: " + part.name + " has initialised");
            if (FailCheck(true))
            {
                failureTime = Planetarium.GetUniversalTime() + (1800*UnityEngine.Random.value);
                willFail = true;
                Debug.Log("[UPFM]: " + part.name + " will attempt to fail at " + failureTime);
            }
        }
        public void SetFailedHighlight()
        {
            part.SetHighlightColor(Color.red);
            part.SetHighlightType(Part.HighlightType.AlwaysOn);
            part.SetHighlight(true, false);
        }
        protected virtual void FailPart() { }

        protected virtual void RepairPart() { }

        protected virtual bool FailureAllowed() { return false; }

        private void FixedUpdate()
        {
            if (HighLogic.LoadedSceneIsEditor) return;
            if (!FailureAllowed()) return;
            if (!willFail) return;
            if (Planetarium.GetUniversalTime() < failureTime) return;
            FailPart();
            if (hasFailed) return;
            if (!willFail) return;
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
            Debug.Log("[UPFM]: Chances of "+part.name+" failing calculated to be " + chanceOfFailure * 100 + "%");
            if (UnityEngine.Random.value < chanceOfFailure) return true;
            return false;
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
            GameEvents.onStageActivate.Remove(onStageActivate);
            if (ScrapYardEvents.OnSYTrackerUpdated == null) return;
            ScrapYardEvents.OnSYTrackerUpdated.Remove(OnSYTrackerUpdated);
        }
    }
}
