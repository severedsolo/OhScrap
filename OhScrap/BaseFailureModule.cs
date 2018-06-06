using KSP.UI.Screens;
using ScrapYard;
using ScrapYard.Modules;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace OhScrap
{
    //This is the generic Failure Module which all other modules inherit.
    //BaseFailureModule will never be attached directly to a part
    //but this handles the stuff that all modules need (like "will I fail" etc)
    class BaseFailureModule : PartModule
    {
        bool ready = false;
        public float randomisation;
        public bool willFail = false;
        [KSPField(isPersistant = true, guiActive = false)]
        public bool launched = false;
        [KSPField(isPersistant = true, guiActive = false)]
        public bool endOfLife = false;
        [KSPField(isPersistant = true, guiActive = false)]
        public bool hasFailed = false;
        [KSPField(isPersistant = false, guiActive = false)]
        public bool postMessage = true;
        [KSPField(isPersistant = true, guiActive = false)]
        public string failureType = "none";
        [KSPField(isPersistant = true, guiActive = false)]
        public int expectedLifetime = 2;
        [KSPField(isPersistant = true, guiActive = false)]
        public int counter = 0;
        public ModuleSYPartTracker SYP;
        [KSPField(isPersistant = true, guiActive = false)]
        public float chanceOfFailure = 0.5f;
        [KSPField(isPersistant = true, guiActive = false)]
        public float baseChanceOfFailure = 0.01f;
        [KSPField(isPersistant = true, guiActive = false)]
        public int numberOfRepairs = 0;
        [KSPField(isPersistant = false, guiActive = false, guiName = "BaseFailure", guiActiveEditor = false, guiUnits = "%")]
        public int displayChance = 100;
        [KSPField(isPersistant = false, guiActive = true, guiName = "Base Safety Rating", guiActiveEditor = true)]
        public int safetyRating = 6;
        public ModuleUPFMEvents OhScrap;
        public bool remoteRepairable = false;
        public bool excluded = false;
        public bool suppressFailure = false;
        public double nextCheck = 0;
        [KSPField(isPersistant = true, guiActive = false)]
        public int minTimeBetweenFailureChecks = 1;
        [KSPField(isPersistant = true, guiActive = false)]
        public int maxTimeBetweenFailureChecks = 7200;
        

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
            Fields["chanceOfFailure"].guiActive = true;
            Fields["chanceOfFailure"].guiActiveEditor = true;
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
            GameEvents.onLaunch.Add(OnLaunch);
        }

        private void FailureRecurrance()
        {
            if (HighLogic.LoadedSceneIsEditor) return;
            if (Planetarium.GetUniversalTime() < nextCheck) return;
            nextCheck = Planetarium.GetUniversalTime() + Randomiser.instance.RandomInteger(minTimeBetweenFailureChecks, maxTimeBetweenFailureChecks);
            if (hasFailed) return;
            if (willFail) return;
            if (!FailureAllowed()) return;
            if (!launched) return;
            Initialise();        }

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
            OhScrap.doNotRecover = true;
        }

        //when the vessel launches allow the parts to be put back nto the inventory.
        private void OnLaunch(EventReport data)
        {
            ActivateFailures();
        }

        private void ActivateFailures()
        {
            launched = true;
            Initialise();
            if (!hasFailed) OhScrap.doNotRecover = false;
            nextCheck = Planetarium.GetUniversalTime() + Randomiser.instance.RandomInteger(1, maxTimeBetweenFailureChecks);
            InvokeRepeating("FailureRecurrance", 1.0f, 1.0f);
#if DEBUG
            Debug.Log("[UPFM]: " + SYP.ID + "marked as recoverable");
#endif
        }

            // This is where we "initialise" the failure module and get everything ready
            public void Initialise()
        {
            //ScrapYard isn't always ready when OhScrap is so we check to see if it's returning an ID yet. If not, return and wait until it does.
            ready = SYP.ID != 0;
            if (!ready) return;
            OhScrap.generation = UPFMUtils.instance.GetGeneration(SYP.ID, part);
            randomisation = UPFMUtils.instance.GetRandomisation(part, OhScrap.generation);
            //if the part has already failed turn the repair and highlight events on.
            if (hasFailed)
            {
                OhScrap.Events["RepairChecks"].active = true;
                OhScrap.Events["ToggleHighlight"].active = true;
            }
            //otherwise roll the dice to see if it will fail
            else
            {
                if (FailCheck(true) && !HighLogic.LoadedSceneIsEditor && launched)
                {
                    willFail = true;
                    Debug.Log("[OhScrap]: " + SYP.ID + " " + ClassName + " failed");
#if !DEBUG
                    Debug.Log("[OhScrap]: Chance of Failure was "+displayChance+"% (Generation "+OhScrap.generation+", "+SYP.TimesRecovered+ "recoveries)");
#endif
                }
            }
            displayChance = (int)(chanceOfFailure * 100);
            //this compares the actual failure rate to the safety threshold and returns a safety calc based on how far below the safety threshold the actual failure rate is.
            //This is what the player actually sees when determining if a part is "failing" or not.
            if (chanceOfFailure < 0.002) safetyRating = 5;
            else if (chanceOfFailure < 0.0034) safetyRating = 4;
            else if (chanceOfFailure < 0.0048) safetyRating = 3;
            else if (chanceOfFailure < 0.0062) safetyRating = 2;
            else safetyRating = 1;
            // if the part is damaged beyond the safety rating (usually only if you've pushed it beyond End Of Life) then it gets a 0
            if (chanceOfFailure > baseChanceOfFailure) safetyRating = 0;
        }
        //These methods all are overriden by the failure modules
        
        //Overrides are things like the UI names, and specific things that we might want to be different for a module
        //For example engines fail after only 2 minutes instead of 30
        protected virtual void Overrides() { }
        //This actually makes the failure happen
        protected virtual void FailPart() { }
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
            //fails the part and posts the message if needed
            if (hasFailed)
            {
                FailPart();
                if (!suppressFailure)
                {
                    part.FindModuleImplementing<ModuleUPFMEvents>().highlightOverride = true;
                    OhScrap.SetFailedHighlight();
                    if (postMessage)
                    {
                        OhScrap.Events["ToggleHighlight"].active = true;
                        OhScrap.highlight = true;
                        Debug.Log("[OhScrap]: Chance of Failure was " + displayChance + "% (Generation " + OhScrap.generation + ", " + SYP.TimesRecovered + " recoveries)");
                        UPFMUtils.instance.vesselSafetyRating = 6;
                        postMessage = false;
                        if (vessel.vesselType != VesselType.Debris) PostFailureMessage();
                    }
                }
                return;
            }
            if (!willFail) return;
            //Everything below this line only happens when the part first fails. Once "hasFailed" is true this code will not run again
            if (HighLogic.CurrentGame.Parameters.CustomParams<UPFMSettings>().stopOnFailure) TimeWarp.SetRate(0, true);
            hasFailed = true;
            OhScrap.Events["RepairChecks"].active = true;
            CancelInvoke();
            if (FailCheck(false))
            {
                OhScrap.MarkBroken();
                Debug.Log("[OhScrap]: " + SYP.ID + "is too badly damaged to be salvaged");
            }
        }

        //This determines whether or not the part will fail.
        public bool FailCheck(bool recalcChance)
        {
            int standardisedGeneration = OhScrap.generation;
            if (standardisedGeneration > 10) standardisedGeneration = 10;
            if (SYP.TimesRecovered == 0) chanceOfFailure = baseChanceOfFailure/standardisedGeneration + randomisation;
            else chanceOfFailure = ((baseChanceOfFailure/standardisedGeneration) + randomisation) * (SYP.TimesRecovered / (float)expectedLifetime);
            //Chance of Failure can never exceed the safety threshold unless the part has reached "end of life"
            if (chanceOfFailure > baseChanceOfFailure) chanceOfFailure = baseChanceOfFailure;
            //If the part has reached it's "end of life" the failure rate will quickly deteriorate.
            float endOfLifeMultiplier = (SYP.TimesRecovered - expectedLifetime) / 5.0f;
            if (endOfLifeMultiplier > 0)
            {
                if (!endOfLife) endOfLife = Randomiser.instance.NextDouble() < endOfLifeMultiplier;
                if (endOfLife) chanceOfFailure = chanceOfFailure + endOfLifeMultiplier;
            }
            // more repairs = more failure events.
            if (numberOfRepairs > 0) chanceOfFailure = chanceOfFailure * numberOfRepairs;
            displayChance = (int)(chanceOfFailure * 100);
#if DEBUG
            if (part != null) Debug.Log("[UPFM]: Chances of " + SYP.ID + " " + moduleName + " failing calculated to be " + chanceOfFailure * 100 + "%");
#endif
            //No Failures if this is a KRASH simulation, a mission, we are in the editor, or the player has disabled failures for this module.
            if (HighLogic.CurrentGame.Mode == Game.Modes.MISSION) return false;
            if (HighLogic.LoadedSceneIsEditor) return false;
            if (KRASHWrapper.simulationActive()) return false;
            if (!FailureAllowed()) return false;
            //every time a part fails the check, we increment the failure Check multiplier
            if (Randomiser.instance.NextDouble() < chanceOfFailure)
            {
                UPFMUtils.instance.numberOfFailures.TryGetValue(part.name, out int i);
                UPFMUtils.instance.numberOfFailures.Remove(part.name);
                i++;
                UPFMUtils.instance.numberOfFailures.Add(part.name, i);
#if DEBUG
                Debug.Log("[UPFM]: " + part.name + " has now failed " + i + " times");
#endif
                return true;
            }
            return false;
        }

        //Adds the message saying the part has failed to the stock messaging app
        void PostFailureMessage()
        {
            StringBuilder msg = new StringBuilder();
            msg.AppendLine(part.vessel.vesselName);
            msg.AppendLine("");
            msg.AppendLine(part.partInfo.title + " has suffered a " + failureType);
            msg.AppendLine("");
            MessageSystem.Message m = new MessageSystem.Message("OhScrap", msg.ToString(), MessageSystemButton.MessageButtonColor.ORANGE, MessageSystemButton.ButtonIcons.ALERT);
            MessageSystem.Instance.AddMessage(m);
        }

        private void OnDestroy()
        {
            GameEvents.onLaunch.Remove(OnLaunch);
            if (ScrapYardEvents.OnSYTrackerUpdated != null) ScrapYardEvents.OnSYTrackerUpdated.Remove(OnSYTrackerUpdated);
            if (ScrapYardEvents.OnSYInventoryAppliedToVessel != null) ScrapYardEvents.OnSYInventoryAppliedToVessel.Remove(OnSYInventoryAppliedToVessel);
        }
    }
}
