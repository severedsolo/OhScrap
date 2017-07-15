using ScrapYard.Modules;
using ScrapYard;
using UnityEngine;
using System;
using System.Text;
using System.Collections.Generic;
using KSP.UI.Screens;
using System.Collections;

namespace Untitled_Part_Failure_Mod
{
    class BaseFailureModule : PartModule
    {
        UnityEngine.Random r = new UnityEngine.Random();
        bool ready = false;
        public float randomisation;
        public bool willFail = false;
        [KSPField(isPersistant = true, guiActive = false)]
        public bool launched = false;
        [KSPField(isPersistant = true, guiActive = false)]
        public bool hasFailed = false;
        [KSPField(isPersistant = true, guiActive = false)]
        public bool postMessage = true;
        [KSPField(isPersistant = true, guiActive = false)]
        public string failureType = "none";
        [KSPField(isPersistant = true, guiActive = false)]
        public float expectedLifetime = 2;
        ModuleSYPartTracker SYP;
        public float chanceOfFailure = 0.5f;
        [KSPField(isPersistant = true, guiActive = false)]
        public float baseChanceOfFailure = 0.5f;
        [KSPField(isPersistant = true, guiActive = true, guiName = "BaseFailure" ,guiActiveEditor = true, guiUnits = "%")]
        public int displayChance = 0;
        [KSPField(isPersistant = true, guiActive = false)]
        double failureTime = 0;
        public double maxTimeToFailure = 1800;
        public ModuleUPFMEvents UPFM;


        private void Start()
        {
            chanceOfFailure = baseChanceOfFailure;
            Overrides();
            ScrapYardEvents.OnSYTrackerUpdated.Add(OnSYTrackerUpdated);
            ScrapYardEvents.OnSYInventoryAppliedToVessel.Add(OnSYInventoryAppliedToVessel);
            if (launched || HighLogic.LoadedSceneIsEditor) Initialise();
            GameEvents.onLaunch.Add(onLaunch);
            UPFM = part.FindModuleImplementing<ModuleUPFMEvents>();
        }

        private void OnSYInventoryAppliedToVessel()
        {
            Debug.Log("[UPFM]: ScrayYard Inventory Applied. Recalculating failure chance");
            if(EditorWarnings.instance != null) EditorWarnings.instance.damagedParts.Remove(part);
            willFail = false;
            chanceOfFailure = baseChanceOfFailure;
            Initialise();
        }

        private void onLaunch(EventReport data)
        {
            launched = true;
            Initialise();
            if (!HighLogic.CurrentGame.Parameters.CustomParams<UPFMSettings>().safetyRecover && displayChance < HighLogic.CurrentGame.Parameters.CustomParams<UPFMSettings>().safetyThreshold) return;
            PartModule dontRecover = part.FindModuleImplementing<DontRecoverMe>();
            if (dontRecover == null) return;
            part.RemoveModule(dontRecover);
            Debug.Log("[UPFM]: " + part.name + "marked as recoverable");
        }

        private void OnSYTrackerUpdated(IEnumerable<InventoryPart> data)
        {
            Debug.Log("[UPFM]: ScrayYard Tracker updated. Recalculating failure chance");
            willFail = false;
            chanceOfFailure = baseChanceOfFailure;
            Initialise();
            part.AddModule("DontRecoverMe");
        }

        private void Initialise()
        {
            SYP = part.FindModuleImplementing<ModuleSYPartTracker>();
            ready = SYP.ID != "";
            if (!ready) return;
            randomisation = EditorWarnings.instance.GetRandomisation(part);
            if (hasFailed)
            {
                Events["RepairChecks"].active = true;
                UPFM.Events["ToggleHighlight"].active = true;
                if(EditorWarnings.instance != null)
                {
                    if (!EditorWarnings.instance.brokenParts.ContainsKey(part)) EditorWarnings.instance.brokenParts.Add(part, displayChance);
                    EditorWarnings.instance.damagedParts.Remove(part);
                }
                return;
            }
            if(part != null) Debug.Log("[UPFM]: " + part.name + moduleName+ " has initialised");
            if (FailCheck(true) && !HighLogic.LoadedSceneIsEditor && launched)
            {
                failureTime = Planetarium.GetUniversalTime() + (maxTimeToFailure * UnityEngine.Random.value);
                willFail = true;
                Debug.Log("[UPFM]: " + part.name + " will attempt to fail at " + failureTime);
            }
            displayChance = (int)(chanceOfFailure * 100);
            if(displayChance >= HighLogic.CurrentGame.Parameters.CustomParams<UPFMSettings>().safetyThreshold && EditorWarnings.instance != null)
            {
                int i;
                if(EditorWarnings.instance.damagedParts.TryGetValue(part, out i))
                {
                    if (i >= displayChance) return;
                }
                EditorWarnings.instance.damagedParts.Remove(part);
                EditorWarnings.instance.damagedParts.Add(part, displayChance);
                EditorWarnings.instance.display = true;
            }
        }
        protected virtual void Overrides() { }

        protected virtual void FailPart() { }

        protected virtual void RepairPart() { }

        protected virtual bool FailureAllowed() { return false; }

        private void FixedUpdate()
        {
            if (!ready) Initialise();
            if (HighLogic.LoadedSceneIsEditor) return;
            if (!FailureAllowed()) return;
            if (hasFailed)
            {
                FailPart();
                if (postMessage)
                {
                    PostFailureMessage();
                    postMessage = false;
                    UPFMEvents["ToggleHighlight"].active = true;
                }                    
                return;
            }
            if (!willFail) return;
            if (Planetarium.GetUniversalTime() < failureTime) return;
            hasFailed = true;
            if (!hasFailed) return;
            if(EditorWarnings.instance != null)
            {
                if (!EditorWarnings.instance.brokenParts.ContainsKey(part)) EditorWarnings.instance.brokenParts.Add(part, displayChance);
                EditorWarnings.instance.damagedParts.Remove(part);
            }
            Events["RepairChecks"].active = true;
            if (FailCheck(false))
            {
                part.AddModule("Broken");
                Debug.Log("[UPFM]: " + part.name + "is too badly damaged to be salvaged");
            }
        }

        bool FailCheck(bool recalcChance)
        {
            if (SYP.TimesRecovered == 0) chanceOfFailure = baseChanceOfFailure + randomisation;
            else chanceOfFailure = (SYP.TimesRecovered / expectedLifetime)+randomisation;
            if (part != null) Debug.Log("[UPFM]: Chances of " + part.name + moduleName +" failing calculated to be " + chanceOfFailure * 100 + "%");
            if (UnityEngine.Random.value < chanceOfFailure) return true;
            return false;
        }

        [KSPEvent(active = false, guiActiveUnfocused = true, unfocusedRange = 5.0f, externalToEVAOnly = true, guiName = "Repair ")]
        public void RepairChecks()
        {
            Debug.Log("[UPFM]: Attempting EVA repairs");
            if (FailCheck(false) || part.Modules.Contains("Broken"))
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
            Debug.Log("[UPFM]: " + part.name + moduleName + " was successfully repaired");
            part.highlightType = Part.HighlightType.OnMouseOver;
        }

        void PostFailureMessage()
        {
            StringBuilder msg = new StringBuilder();
            msg.AppendLine(part.vessel.vesselName);
            msg.AppendLine("");
            msg.AppendLine(part.name + " has suffered a " + failureType);
            msg.AppendLine("");
            if (part.FindModuleImplementing<Broken>() != null) msg.AppendLine("The part is damaged beyond repair");
            else msg.AppendLine("Chance of a successful repair is " + (100 - displayChance)+"%");
            MessageSystem.Message m = new MessageSystem.Message("UPFM", msg.ToString(), MessageSystemButton.MessageButtonColor.ORANGE,MessageSystemButton.ButtonIcons.ALERT);
            MessageSystem.Instance.AddMessage(m);
        }

        private void OnDestroy()
        {
            GameEvents.onLaunch.Remove(onLaunch);
            if (ScrapYardEvents.OnSYTrackerUpdated != null) ScrapYardEvents.OnSYTrackerUpdated.Remove(OnSYTrackerUpdated);
            if (ScrapYardEvents.OnSYInventoryAppliedToVessel != null) ScrapYardEvents.OnSYInventoryAppliedToVessel.Remove(OnSYInventoryAppliedToVessel);
        }
    }
}
