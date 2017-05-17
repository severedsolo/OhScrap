using ScrapYard.Modules;
using ScrapYard;
using UnityEngine;
using System;
using System.Collections.Generic;

namespace Untitled_Part_Failure_Mod
{
    class BaseFailureModule : PartModule
    {
        System.Random r = new System.Random();
        public bool willFail = false;
        [KSPField(isPersistant = true, guiActive = false)]
        public bool hasFailed = false;
        [KSPField(isPersistant = true, guiActive = true, guiName = "Expected Lifetime", guiUnits = " Flights")]
        public float expectedLifetime = 2;
        ModuleSYPartTracker SYP;
        float chanceOfFailure = 0.5f;
        [KSPField(isPersistant = true, guiActive = true, guiName = "Generation")]
        public int generation = 0;
        double failureTime = 0;

        private void Start()
        {
            ScrapYardEvents.OnSYTrackerUpdated.Add(OnSYTrackerUpdated);
            Initialise();
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
            if (FailCheck(true))
            {
                failureTime = Planetarium.GetUniversalTime() + r.Next(1, 1800);
                willFail = true;
                Debug.Log("[UPFM]: " + part.name + " will attempt to fail at " + failureTime);
            }
        }

        protected virtual void FailPart() { }

        protected virtual void RepairPart() { }

        private void FixedUpdate()
        {
            if (hasFailed)
            {
                FailPart();
                return;
            }
            if (!willFail) return;
            if (Planetarium.GetUniversalTime() < failureTime) return;
            FailPart();
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
            if (r.NextDouble() < chanceOfFailure) return true;
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
        }

        private void OnDestroy()
        {
            if (ScrapYardEvents.OnSYTrackerUpdated == null) return;
            ScrapYardEvents.OnSYTrackerUpdated.Remove(OnSYTrackerUpdated);
        }
    }
}
