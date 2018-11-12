using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ScrapYard.Modules;

namespace OhScrap
{
    //This handles all events that need to be in a PartModule but if we put it in the main BaseFailureModule would appear multiple times on a part.
    // This module is attached to any part that has at least one other Failure Module
    class ModuleUPFMEvents : PartModule
    {
        [KSPField(isPersistant = true, guiActive = false)]
        public bool highlight = true;
        bool doNotRecover = false;
        [KSPField(isPersistant = false, guiActiveEditor = true, guiName = "Tested")]
        public bool tested = false;
        BaseFailureModule repair;
        ModuleSYPartTracker SYP;
        public bool refreshed = false;
        [KSPField(isPersistant = false, guiActive = true, guiName = "Generation", guiActiveEditor = true)]
        public int generation = 0;
        public bool customFailureEvent = false;
        public bool highlightOverride = false;
        public bool repairTried = false;

        private void Start()
        {
            SYP = part.FindModuleImplementing<ModuleSYPartTracker>();
#if DEBUG
            Debug.Log("[UPFM]: UPFMEvents.Start" + SYP.ID);
#endif
        }
        //forces ScrapYard to refresh the part if it's needed.
        public void RefreshPart()
        {
            if (!HighLogic.LoadedSceneIsEditor || refreshed) return;
            SYP = part.FindModuleImplementing<ModuleSYPartTracker>();
            if (SYP.TimesRecovered == 0)
            {
                SYP.MakeFresh();
            }
            refreshed = true;
        }
        
        //This allows the player to not recover the part to the SY Inventory
        [KSPEvent(active = true, guiActive = true, guiActiveUnfocused = false, externalToEVAOnly = false, guiName = "Trash Part")]
        public void TrashPart()
        {
            if (!doNotRecover)
            {
                doNotRecover = true;
                ScreenMessages.PostScreenMessage(part.partInfo.title + " will not be recovered");
            }
            else
            {
                List<BaseFailureModule> modules = part.FindModulesImplementing<BaseFailureModule>();
                if (modules.Count == 0) return;
                for(int i = 0; i<modules.Count; i++)
                {
                    BaseFailureModule bfm = modules.ElementAt(i);
                    if(bfm.hasFailed)
                    {
                        ScreenMessages.PostScreenMessage(part.partInfo.title + " cannot be saved");
                        return;
                    }
                }
                doNotRecover = false;
                ScreenMessages.PostScreenMessage(part.partInfo.title + " will be recovered");
            }
            Debug.Log("[OhScrap]: TrashPart " + SYP.ID+" "+doNotRecover);
        }
        //This marks the part as not recoverable to ScrapYard
        public void MarkBroken()
        {
            doNotRecover = true;
        }

        //This toggles the part failure highlight on and off (player initiated)
        [KSPEvent(active = false, guiActive = true, guiActiveUnfocused = false, externalToEVAOnly = false, guiName = "Toggle Failure Highlight")]
        public void ToggleHighlight()
        {
            if (highlight)
            {
                part.SetHighlight(false, false);
                part.highlightType = Part.HighlightType.OnMouseOver;
                highlight = false;
            }
            else highlight = true;
        }

        //This sets the initial highlighting when a part fails (mod initiated)
        public void SetFailedHighlight()
        {
            if (!HighLogic.CurrentGame.Parameters.CustomParams<UPFMSettings>().highlightFailures) return;
            if (!highlight) return;
            part.SetHighlightColor(Color.red);
            part.SetHighlightType(Part.HighlightType.AlwaysOn);
            part.SetHighlight(true, false);
        }

        //This loops through every failure module on this part and runs the "Repair Part" method.
        [KSPEvent(active = false, guiActive = true, guiActiveUnfocused = true, unfocusedRange = 5.0f, externalToEVAOnly = false, guiName = "Repair ")]
        public void RepairChecks()
        {
            Debug.Log("[OhScrap]: Attempting repairs");
            bool repairAllowed = true;
            List<BaseFailureModule> bfm = part.FindModulesImplementing<BaseFailureModule>();
            //If no kerbal on EVA we can assume this is a remote repair.
            if (FlightGlobals.ActiveVessel.FindPartModuleImplementing<KerbalEVA>() == null)
            {
                Debug.Log("[OhScrap]: Attempting Remote Repair");
                //If CommNet is enabled check if vessel is connected (can't upload a software fix with no connection)
                if (CommNet.CommNetScenario.CommNetEnabled && !FlightGlobals.ActiveVessel.Connection.IsConnectedHome)
                {
                    ScreenMessages.PostScreenMessage("Vessel must be connected to Homeworld before remote repair can be attempted");
                    Debug.Log("[OhScrap]: Remote Repair aborted. Vessel not connected home");
                    return;
                }
                //Check if the part is actually remote repairable. This will fail if any of the failed modules are not remote repairable.
                for (int i = 0; i < bfm.Count(); i++)
                {
                    BaseFailureModule b = bfm.ElementAt(i);
                    if (b.hasFailed)
                    {
                        if (!b.remoteRepairable)
                        {
                            ScreenMessages.PostScreenMessage(part.partInfo.title + "cannot be repaired remotely");
                            repairAllowed = false;
                            Debug.Log("[OhScrap]: Remote Repair not allowed on " + SYP.ID + " " + b.ClassName);
                            continue;
                        }
                    }
                    else continue;
                }
            }
            if (!repairAllowed) return;
            //Repaired loops through all modules and checks them for the hasFailed == true flag
            //If it finds it, it will return false and the repair checks will continue.
            //This will carry on until either a module cant be repaired, or all modules are repaired.
            while (!Repaired())
            {
                //If the module fails the check or it's already been marked as irrepairable will stop trying.
                if (RepairFailCheck() || repairTried)
                {
                    ScreenMessages.PostScreenMessage("This part is beyond repair");
                    repairTried = true;
                    Debug.Log("[OhScrap]: " + SYP.ID + " is too badly damaged to be fixed");
                    return;
                }
                //reset the failure status on the module and disables the highlight.
                repair.hasFailed = false;
                repair.willFail = false;
                repair.RepairPart();
                if (!customFailureEvent) ScreenMessages.PostScreenMessage("The part should be ok to use now");
                repair.numberOfRepairs++;
                Debug.Log("[OhScrap]: " + SYP.ID + " " + repair.moduleName + " was successfully repaired");
                part.highlightType = Part.HighlightType.OnMouseOver;
                part.SetHighlightColor(Color.green);
                part.SetHighlight(false, false);
            }
            //Once the part has been repaired run the Initialise Event again (possibly another fail)
            for (int i = 0; i < bfm.Count; i++)
            {
                BaseFailureModule bf = bfm.ElementAt(i);
                bf.Initialise();
            }
        }

        //Determines whether a repair will be successful
        private bool RepairFailCheck()
        {
            //base success chance is 20%
            float repairChance = 0.2f;
            if(FlightGlobals.ActiveVessel.GetCrewCount() >0)
            {
                //if repair is done by EVA success is 40%
                if (FlightGlobals.ActiveVessel.FindPartModuleImplementing<KerbalEVA>() != null) repairChance = 0.4f;
                for(int i = 0; i<FlightGlobals.ActiveVessel.GetVesselCrew().Count(); i++)
                {
                    //Engineers give a 10% bonus per level to repair rates
                    ProtoCrewMember p = FlightGlobals.ActiveVessel.GetVesselCrew().ElementAt(i);
                    if(p.trait == "Engineer")
                    {
                        repairChance += p.experienceLevel*0.1f;
                        break;
                    }
                }
            }
            return UPFMUtils.instance._randomiser.NextDouble() < repairChance;
        }

        bool Repaired()
        {
            List<BaseFailureModule> failedList = part.FindModulesImplementing<BaseFailureModule>();
            if (failedList.Count() == 0) return true;
            //Loop through everything that inherits BaseFailureModule on this part and check the hasFailed flag. If true, return false, otherwise return true.
            for (int i = 0; i < failedList.Count(); i++)
            {
                BaseFailureModule bfm = failedList.ElementAt(i);
                if (bfm == null) continue;
                if (!bfm.hasFailed) continue;
                repair = bfm;
                return false;
            }
            Events["RepairChecks"].active = false;
            Events["ToggleHighlight"].active = false;
            doNotRecover = false;
            return true;
        }
    }
}
