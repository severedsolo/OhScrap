using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ScrapYard.Modules;

namespace Untitled_Part_Failure_Mod
{
    class ModuleUPFMEvents : PartModule
    {
        public bool highlight = false;
        BaseFailureModule repair;
        ModuleSYPartTracker SYP;
        public bool refreshed = false;
        
        private void Start()
        {
            SYP = part.FindModuleImplementing<ModuleSYPartTracker>();
            Debug.Log("[UPFM]: UPFMEvents.Start "+SYP.ID);
        }
        public void RefreshPart()
        {
            if (!HighLogic.LoadedSceneIsEditor || refreshed) return;
            SYP = part.FindModuleImplementing<ModuleSYPartTracker>();
            if(SYP.TimesRecovered == 0) SYP.MakeFresh();
            refreshed = true;
        }
        [KSPEvent(active = true, guiActive = true, guiActiveUnfocused = false, externalToEVAOnly = false, guiName = "Trash Part")]
        public void TrashPart()
        {
            if (part.FindModuleImplementing<Broken>() == null) part.AddModule("Broken");
            ScreenMessages.PostScreenMessage(part.name + " will not be recovered");
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

        [KSPEvent(active = false, guiActiveUnfocused = true, unfocusedRange = 5.0f, externalToEVAOnly = true, guiName = "Repair ")]
        public void RepairChecks()
        {
            Debug.Log("[UPFM]: Attempting EVA repairs");
            while (!Repaired())
            {
                if (repair.FailCheck(false) || part.Modules.Contains("Broken"))
                {
                    ScreenMessages.PostScreenMessage("This part is beyond repair");
                    if (!part.Modules.Contains("Broken")) part.AddModule("Broken");
                    Debug.Log("[UPFM]: " + SYP.ID + " is too badly damaged to be fixed");
                    return;
                }
                repair.hasFailed = false;
                repair.willFail = false;
                ScreenMessages.PostScreenMessage("The part should be ok to use now");
                Events["RepairChecks"].active = false;
                repair.RepairPart();
                Debug.Log("[UPFM]: " + SYP.ID+" " + moduleName + " was successfully repaired");
                part.highlightType = Part.HighlightType.OnMouseOver;
            }
        }

        bool Repaired()
        {
            List<BaseFailureModule> failedList = part.FindModulesImplementing<BaseFailureModule>();
            if (failedList.Count() == 0) return true;
            for(int i = 0; i <failedList.Count(); i++)
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
