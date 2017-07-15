using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Untitled_Part_Failure_Mod
{
    class SolarPanelFailureModule : BaseFailureModule
    {
        ModuleDeployableSolarPanel panel;
        bool originallyRetractable;
        protected override void Overrides()
        {
            Fields["displayChance"].guiName = "Chance of Solar Panel Failure";
            failureType = "retraction error";
            postMessage = false;
        }
        protected override bool FailureAllowed()
        {
            return HighLogic.CurrentGame.Parameters.CustomParams<UPFMSettings>().SolarPanelFailureModuleAllowed;
        }
        protected override void FailPart()
        {

            panel = part.FindModuleImplementing<ModuleDeployableSolarPanel>();
            if (panel == null) return;
            if (!panel.CanMove) return;
            if (panel.retractable && panel.deployState == ModuleDeployablePart.DeployState.EXTENDED)
            {
                panel.retractable = false;
                originallyRetractable = true;
                ScreenMessages.PostScreenMessage(part.name + " retraction mechanism jammed");
                Debug.Log("[UPFM]: " + part.name + " retraction mechanism has jammed");
                if(UPFM.highlight)UPFM.SetFailedHighlight();
                postMessage = true;
            }
        }

        protected override void RepairPart()
        {
            panel = part.FindModuleImplementing<ModuleDeployableSolarPanel>();
            if (originallyRetractable) panel.retractable = true;
        }
    }
}
