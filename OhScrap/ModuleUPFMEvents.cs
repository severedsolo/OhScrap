using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ScrapYard.Modules;

namespace OhScrap
{
    class ModuleUPFMEvents : PartModule
    {
        [KSPField(isPersistant = true, guiActive = false)]
        public bool highlight = false;
        BaseFailureModule repair;
        ModuleSYPartTracker SYP;
        public bool refreshed = false;
        [KSPField(isPersistant = true, guiActive = false)]
        public bool doNotRecover = true;
        [KSPField(isPersistant = false, guiActive = true, guiName = "Generation", guiActiveEditor = true)]
        public int generation = 0;
        public bool srb = false;

        private void Start()
        {
            SYP = part.FindModuleImplementing<ModuleSYPartTracker>();
#if DEBUG
            Debug.Log("[UPFM]: UPFMEvents.Start" + SYP.ID);
#endif
        }
        public void RefreshPart()
        {
            if (!HighLogic.LoadedSceneIsEditor || refreshed) return;
            SYP = part.FindModuleImplementing<ModuleSYPartTracker>();
            if (SYP.TimesRecovered == 0) SYP.MakeFresh();
            refreshed = true;
        }
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
        public void MarkBroken()
        {
            doNotRecover = true;
        }
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

        public void SetFailedHighlight()
        {
            if (!HighLogic.CurrentGame.Parameters.CustomParams<UPFMSettings>().highlightFailures) return;
            if (!highlight) return;
            part.SetHighlightColor(Color.red);
            part.SetHighlightType(Part.HighlightType.AlwaysOn);
            part.SetHighlight(true, false);
        }

        [KSPEvent(active = false, guiActive = true, guiActiveUnfocused = true, unfocusedRange = 5.0f, externalToEVAOnly = false, guiName = "Repair ")]
        public void RepairChecks()
        {
            Debug.Log("[OhScrap]: Attempting repairs");
            bool repairAllowed = true;
            List<BaseFailureModule> bfm = part.FindModulesImplementing<BaseFailureModule>();
            if (FlightGlobals.ActiveVessel.FindPartModuleImplementing<KerbalEVA>() == null)
            {
                Debug.Log("[OhScrap]: Attempting Remote Repair");
                if (!FlightGlobals.ActiveVessel.Connection.IsConnectedHome)
                {
                    ScreenMessages.PostScreenMessage("Vessel must be connected to Homeworld before remote repair can be attempted");
                    Debug.Log("[OhScrap]: Remote Repair aborted. Vessel not connected home");
                    return;
                }
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
            while (!Repaired())
            {
                if (repair.FailCheck(false) || doNotRecover)
                {
                    ScreenMessages.PostScreenMessage("This part is beyond repair");
                    doNotRecover = true;
                    Debug.Log("[OhScrap]: " + SYP.ID + " is too badly damaged to be fixed");
                    return;
                }
                repair.hasFailed = false;
                repair.willFail = false;
                Events["RepairChecks"].active = false;
                repair.RepairPart();
                if (!srb) ScreenMessages.PostScreenMessage("The part should be ok to use now");
                repair.numberOfRepairs++;
                Debug.Log("[OhScrap]: " + SYP.ID + " " + repair.moduleName + " was successfully repaired");
                part.highlightType = Part.HighlightType.OnMouseOver;
                part.SetHighlight(false, false);
                UPFMUtils.instance.brokenParts.Remove(part);
                repair.postMessage = true;
            }
            for (int i = 0; i < bfm.Count; i++)
            {
                BaseFailureModule bf = bfm.ElementAt(i);
                bf.Initialise();
            }
        }

        bool Repaired()
        {
            List<BaseFailureModule> failedList = part.FindModulesImplementing<BaseFailureModule>();
            if (failedList.Count() == 0) return true;
            for (int i = 0; i < failedList.Count(); i++)
            {
                BaseFailureModule bfm = failedList.ElementAt(i);
                if (bfm == null) continue;
                if (!bfm.hasFailed) continue;
                repair = bfm;
                return false;
            }
            return true;
        }
    }
}
