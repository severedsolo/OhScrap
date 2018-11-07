using UnityEngine;
using System;
using System.Text;
using System.Collections.Generic;
using KSP.UI.Screens;
using System.Collections;
using Expansions.Missions;
using ScrapYard;
using ScrapYard.Modules;

namespace OhScrap
{
    //This is the generic Failure Module which all other modules inherit.
    //BaseFailureModule will never be attached directly to a part
    //but this handles the stuff that all modules need (like "will I fail" etc)
    class BaseFailureModule : PartModule
    {
        public bool ready = false;
        public bool willFail = false;
        [KSPField(isPersistant = true, guiActive = false)]
        public bool launched = false;
        [KSPField(isPersistant = true, guiActive = false)]
        public bool endOfLife = false;
        [KSPField(isPersistant = true, guiActive = false)]
        public bool hasFailed = false;
        [KSPField(isPersistant = true, guiActive = false)]
        public string failureType = "none";
        [KSPField(isPersistant = true, guiActive = false)]
        public int expectedLifetime = 2;
        public ModuleSYPartTracker SYP;
        [KSPField(isPersistant = true, guiActive = false)]
        public float chanceOfFailure = 0.1f;
        [KSPField(isPersistant = true, guiActive = false)]
        public float baseChanceOfFailure = 0.1f;
        [KSPField(isPersistant = true, guiActive = false)]
        public int numberOfRepairs = 0;
        [KSPField(isPersistant = false, guiActive = false, guiName = "BaseFailure", guiActiveEditor = false, guiUnits = "%")]
        public int displayChance = 100;
        [KSPField(isPersistant = false, guiActive = true, guiName = "Base Safety Rating", guiActiveEditor = true)]
        public int safetyRating = -1;
        public ModuleUPFMEvents OhScrap;
        public bool remoteRepairable = false;
        public bool isSRB = false;
        bool excluded = false;

#if DEBUG
        [KSPEvent(active = true, guiActive = true, guiActiveUnfocused = true, unfocusedRange = 5.0f, externalToEVAOnly = false, guiName = "Force Failure (DEBUG)")]
        public void ForceFailure()
        {
            FailPart();
        }
#endif
        private void Start()
        {
#if DEBUG
            Fields["displayChance"].guiActive = true;
            Fields["displayChance"].guiActiveEditor = true;
            Fields["safetyRating"].guiActive = true;
#endif
            if (HighLogic.LoadedSceneIsEditor) hasFailed = false;
            //find the ScrapYard Module straight away, as we can't do any calculations without it.
            SYP = part.FindModuleImplementing<ModuleSYPartTracker>();
            chanceOfFailure = baseChanceOfFailure;
            if (expectedLifetime > 12) expectedLifetime = (expectedLifetime / 10) + 2;
            //overrides are defined in each failue Module - stuff that the generic module can't handle.
            Overrides();
            //listen to ScrapYard Events so we can recalculate when needed
            ScrapYardEvents.OnSYTrackerUpdated.Add(OnSYTrackerUpdated);
            ScrapYardEvents.OnSYInventoryAppliedToVessel.Add(OnSYInventoryAppliedToVessel);
            ScrapYardEvents.OnSYInventoryAppliedToPart.Add(OnSYInventoryAppliedToPart);
            OhScrap = part.FindModuleImplementing<ModuleUPFMEvents>();
            //refresh part if we are in the editor and parts never been used before (just returns if not)
            OhScrap.RefreshPart();
            //Initialise the Failure Module.
            if (launched || HighLogic.LoadedSceneIsEditor) Initialise();
        }

        private void OnSYInventoryAppliedToPart(Part p)
        {
            if (p != part) return;
            willFail = false;
            chanceOfFailure = baseChanceOfFailure;
            Initialise();
        }

        private void OnSYInventoryChanged(InventoryPart data0, bool data1)
        {
            willFail = false;
            chanceOfFailure = baseChanceOfFailure;
            Initialise();
        }

        // if SY applies inventory we reset the module as it could be a whole new part now.
        private void OnSYInventoryAppliedToVessel()
        {
#if DEBUG
            Debug.Log("[UPFM]: ScrayYard Inventory Applied. Recalculating failure chance for " + SYP.ID + " " + ClassName);
#endif
            willFail = false;
            chanceOfFailure = baseChanceOfFailure;
            Initialise();
        }

        //likewise for when the SYTracker is updated (usually on VesselRollout). Start() fires before ScrapYard applies the inventory in the flight scene.
        private void OnSYTrackerUpdated(IEnumerable<InventoryPart> data)
        {
#if DEBUG
            Debug.Log("[UPFM]: ScrayYard Tracker updated. Recalculating failure chance for " + SYP.ID + " " + ClassName);
#endif
            willFail = false;
            chanceOfFailure = baseChanceOfFailure;
            Initialise();
        }

        private void ActivateFailures()
        {
            launched = true;
            Initialise();
            UPFMUtils.instance.testedParts.Add(SYP.ID);
        }

            // This is where we "initialise" the failure module and get everything ready
            public void Initialise()
        {
            //ScrapYard isn't always ready when OhScrap is so we check to see if it's returning an ID yet. If not, return and wait until it does.
            ready = SYP.ID != 0;
            if (!ready) return;
            if (UPFMUtils.instance.testedParts.Contains(SYP.ID)) part.FindModuleImplementing<ModuleUPFMEvents>().tested = true;
            OhScrap.generation = UPFMUtils.instance.GetGeneration(SYP.ID, part);
            chanceOfFailure = baseChanceOfFailure;
            if (SYP.TimesRecovered == 0 || !UPFMUtils.instance.testedParts.Contains(SYP.ID)) chanceOfFailure = baseChanceOfFailure - OhScrap.generation*0.01f;
            else chanceOfFailure = (baseChanceOfFailure - (OhScrap.generation*0.01f)) * (SYP.TimesRecovered / (float)expectedLifetime);
            if (chanceOfFailure < 0.01f) chanceOfFailure = 0.01f;
            //if the part has already failed turn the repair and highlight events on.
            if (hasFailed)
            {
                OhScrap.Events["RepairChecks"].active = true;
                OhScrap.Events["ToggleHighlight"].active = true;
            }
            displayChance = (int)(chanceOfFailure * 100);
            //this compares the actual failure rate to the safety threshold and returns a safety calc based on how far below the safety threshold the actual failure rate is.
            //This is what the player actually sees when determining if a part is "failing" or not.
            if (chanceOfFailure <= 0.01f) safetyRating = 10;
            else if (chanceOfFailure < 0.02f) safetyRating = 9;
            else if (chanceOfFailure < 0.03f) safetyRating = 8;
            else if (chanceOfFailure < 0.04f) safetyRating = 7;
            else if (chanceOfFailure < 0.05f) safetyRating = 6;
            else if (chanceOfFailure < 0.06f) safetyRating = 5;
            else if (chanceOfFailure < 0.07f) safetyRating = 4;
            else if (chanceOfFailure < 0.08f) safetyRating = 3;
            else if (chanceOfFailure < 0.09f) safetyRating = 2;
            else safetyRating = 1;
            // if the part is damaged beyond the safety rating (usually only if you've pushed it beyond End Of Life) then it gets a 0
            if (displayChance > HighLogic.CurrentGame.Parameters.CustomParams<UPFMSettings>().safetyThreshold) safetyRating = 0;
            //shows a 1% failure rate as a fallback in case it rounds the float to 0
            if (chanceOfFailure == 0.01f) displayChance = 1;
            if (hasFailed) part.FindModuleImplementing<ModuleUPFMEvents>().SetFailedHighlight();
            ready = true;
            if(HighLogic.LoadedScene == GameScenes.FLIGHT && isSRB && UPFMUtils.instance._randomiser.NextDouble() < chanceOfFailure) InvokeRepeating("FailPart", 0.5f, 0.5f);

        }
        //These methods all are overriden by the failure modules

        //Overrides are things like the UI names, and specific things that we might want to be different for a module
        //For example engines fail after only 2 minutes instead of 30
        protected virtual void Overrides() { }
        //This actually makes the failure happen
        public virtual void FailPart() { }
        //this repairs the part.
        public virtual void RepairPart() { }
        //this should read from the Difficulty Settings.
        protected virtual bool FailureAllowed() { return false; }

        private void FixedUpdate()
        {
            //If ScrapYard didn't return a sensible ID last time we checked, try again.
            if (!ready)
            {
                Initialise();
                return;
            }
            //OnLaunch doesn't fire for rovers, so we do a secondary check for whether the vessel is moving, and fire it manually if it is.
            if (!launched && FlightGlobals.ActiveVessel != null)
            {
                if (FlightGlobals.ActiveVessel.speed > 1) ActivateFailures();
                return;
            }
            if (hasFailed)
            {
                OhScrap.Events["RepairChecks"].active = true;
                OhScrap.Events["ToggleHighlight"].active = true;
                FailPart();
            }
        }

        private void OnDestroy()
        {
            if (ScrapYardEvents.OnSYTrackerUpdated != null) ScrapYardEvents.OnSYTrackerUpdated.Remove(OnSYTrackerUpdated);
            if (ScrapYardEvents.OnSYInventoryAppliedToVessel != null) ScrapYardEvents.OnSYInventoryAppliedToVessel.Remove(OnSYInventoryAppliedToVessel);
        }
    }
}
