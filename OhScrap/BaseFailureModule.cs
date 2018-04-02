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
        bool ready = false;
        public float randomisation;
        public bool willFail = false;
        [KSPField(isPersistant = true, guiActive = false)]
        public bool launched = false;
        [KSPField(isPersistant = true, guiActive = false)]
        public bool endOfLife = false;
        [KSPField(isPersistant = true, guiActive = false)]
        public bool hasFailed = false;
        [KSPField(isPersistant = true, guiActive = false)]
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
        double failureTime = 0;
        public double maxTimeToFailure = 1800;
        public ModuleUPFMEvents OhScrap;
        public bool remoteRepairable = false;
        public bool excluded = false;
        public bool suppressFailure = false;

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
            //find the ScrapYard Module straight away, as we can't do any calculations without it.
            SYP = part.FindModuleImplementing<ModuleSYPartTracker>();
            chanceOfFailure = baseChanceOfFailure;
            if (expectedLifetime > 12) expectedLifetime = (expectedLifetime / 10) + 2;
            //overrides are defined in each failue Module - stuff that the generic module can't handle.
            Overrides();
            //listen to ScrapYard Events so we can recalculate when needed
            ScrapYardEvents.OnSYTrackerUpdated.Add(OnSYTrackerUpdated);
            ScrapYardEvents.OnSYInventoryAppliedToVessel.Add(OnSYInventoryAppliedToVessel);
            OhScrap = part.FindModuleImplementing<ModuleUPFMEvents>();
            //refresh part if we are in the editor and parts never been used before (just returns if not)
            OhScrap.RefreshPart();
            //Initialise the Failure Module.
            if (launched || HighLogic.LoadedSceneIsEditor) Initialise();
            GameEvents.onLaunch.Add(OnLaunch);

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
            launched = true;
            Initialise();
            if (!HighLogic.CurrentGame.Parameters.CustomParams<UPFMSettings>().safetyRecover && displayChance < HighLogic.CurrentGame.Parameters.CustomParams<UPFMSettings>().safetyThreshold) return;
            if (!hasFailed) OhScrap.doNotRecover = false;
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
                    //Most failures will happen up to 30 minutes in the future, but the more damaged a part is, the shorter it can hold on.
                    double timeToFailure = (maxTimeToFailure * (1 - chanceOfFailure)) * Randomiser.instance.NextDouble();
                    failureTime = Planetarium.GetUniversalTime() + timeToFailure;
                    willFail = true;
                    Debug.Log("[OhScrap]: " + SYP.ID + " " + ClassName + " will attempt to fail in " + timeToFailure + " seconds");
#if !DEBUG
                    Debug.Log("[OhScrap]: Chance of Failure was "+displayChance+"% (Generation "+OhScrap.generation+", "+SYP.TimesRecovered+ "recoveries)");
#endif
                }
            }
            displayChance = (int)(chanceOfFailure * 100);
            //this compares the actual failure rate to the safety threshold and returns a safety calc based on how far below the safety threshold the actual failure rate is.
            //This is what the player actually sees when determining if a part is "failing" or not.
            float safetyCalc = 1.0f - ((float)displayChance / HighLogic.CurrentGame.Parameters.CustomParams<UPFMSettings>().safetyThreshold);
            if (safetyCalc > 0.95) safetyRating = 5;
            else if (safetyCalc > 0.9) safetyRating = 4;
            else if (safetyCalc > 0.8) safetyRating = 3;
            else if (safetyCalc > 0.7) safetyRating = 2;
            else safetyRating = 1;
            // if the part is damaged beyond the safety rating (usually only if you've pushed it beyond End Of Life) then it gets a 0
            if (displayChance > HighLogic.CurrentGame.Parameters.CustomParams<UPFMSettings>().safetyThreshold) safetyRating = 0;
            //shows a 1% failure rate as a fallback in case it rounds the float to 0
            if (chanceOfFailure == 0.01f) displayChance = 1;
            //deprecated and will be removed soon.
            if (UPFMUtils.instance != null && hasFailed)
            {
                if (!UPFMUtils.instance.brokenParts.ContainsKey(part)) UPFMUtils.instance.brokenParts.Add(part, displayChance);
            }
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
        protected virtual bool FailureAllowed() { return true; }

        private void FixedUpdate()
        {
            //If ScrapYard didn't return a sensible ID last time we checked, try again.
            if (!ready) Initialise();
            //No point trying to fail parts in the editor.
            if (HighLogic.LoadedSceneIsEditor) return;
            //We don't want to interfere with MH Missions - they can have their own failures if the author wants.
            if (HighLogic.CurrentGame.Mode == Game.Modes.MISSION) return;
            //No Failures if this is a KRASH simulation
            if (KRASHWrapper.simulationActive()) return;
            if (!FailureAllowed()) return;
            //fails the part and posts the message if needed
            if (hasFailed)
            {
                FailPart();
                if (!suppressFailure)
                {
                    OhScrap.SetFailedHighlight();
                    if (postMessage)
                    {
                        PostFailureMessage();
                        postMessage = false;
                        OhScrap.Events["ToggleHighlight"].active = true;
                        OhScrap.highlight = true;
                        Debug.Log("[OhScrap]: Chance of Failure was " + displayChance + "% (Generation " + OhScrap.generation + ", " + SYP.TimesRecovered + " recoveries)");
                    }
                }
                return;
            }
            if (!willFail)
            {
                return;
            }
            if (Planetarium.GetUniversalTime() < failureTime) return;
            //Everything below this line only happens when the part first fails. Once "hasFailed" is true this code will not run again
            if (HighLogic.CurrentGame.Parameters.CustomParams<UPFMSettings>().stopOnFailure) TimeWarp.SetRate(0, true);
            hasFailed = true;
            if (UPFMUtils.instance != null)
            {
                if (!UPFMUtils.instance.brokenParts.ContainsKey(part)) UPFMUtils.instance.brokenParts.Add(part, displayChance);
            }
            OhScrap.Events["RepairChecks"].active = true;
            if (FailCheck(false))
            {
                OhScrap.MarkBroken();
                Debug.Log("[OhScrap]: " + SYP.ID + "is too badly damaged to be salvaged");
            }
        }

        //This determines whether or not the part will fail.
        public bool FailCheck(bool recalcChance)
        {
            if (SYP.TimesRecovered == 0) chanceOfFailure = baseChanceOfFailure/OhScrap.generation + randomisation;
            else chanceOfFailure = ((baseChanceOfFailure/OhScrap.generation) + randomisation) * (SYP.TimesRecovered / (float)expectedLifetime);
            if (chanceOfFailure * 100 > HighLogic.CurrentGame.Parameters.CustomParams<UPFMSettings>().safetyThreshold) chanceOfFailure = HighLogic.CurrentGame.Parameters.CustomParams<UPFMSettings>().safetyThreshold / 100.0f;
            float endOfLifeMultiplier = (SYP.TimesRecovered - expectedLifetime) / 5.0f;
            if (endOfLifeMultiplier > 0)
            {
                if (!endOfLife) endOfLife = Randomiser.instance.NextDouble() < endOfLifeMultiplier;
                if (endOfLife) chanceOfFailure = chanceOfFailure + endOfLifeMultiplier;
            }
            if (numberOfRepairs > 0) chanceOfFailure = chanceOfFailure * numberOfRepairs;
            displayChance = (int)(chanceOfFailure * 100);
#if DEBUG
            if (part != null) Debug.Log("[UPFM]: Chances of " + SYP.ID + " " + moduleName + " failing calculated to be " + chanceOfFailure * 100 + "%");
#endif
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
