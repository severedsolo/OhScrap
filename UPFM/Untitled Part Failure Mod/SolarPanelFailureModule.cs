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
        bool trackingFailure;

        protected override void Overrides()
        {
            Fields["displayChance"].guiName = "Chance of Solar Panel Failure";
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
            if (!panel.isTracking) return;
            if (!postMessage)
            {
                if (Randomiser.instance.NextDouble() < 0.5) trackingFailure = true;
                else trackingFailure = false;
            }
            if (panel.retractable && panel.deployState == ModuleDeployablePart.DeployState.EXTENDED && !trackingFailure)
            {
                panel.retractable = false;
                originallyRetractable = true;
                if (!postMessage)
                {
                    failureType = "retraction error";
                    ScreenMessages.PostScreenMessage(part.name + " retraction mechanism jammed");
                    Debug.Log("[UPFM]: " + SYP.ID + " retraction mechanism has jammed");
                    postMessage = true;
                }
                if (UPFM.highlight) UPFM.SetFailedHighlight();
            }
            else if (panel.isTracking && panel.deployState == ModuleDeployablePart.DeployState.EXTENDED)
            {
                panel.isTracking = false;
                if (!postMessage)
                {
                    failureType = "sun tracking error";
                    ScreenMessages.PostScreenMessage(part.name + " sun tracking mechanism jammed");
                    Debug.Log("[UPFM]: " + SYP.ID + " sun tracking mechanism has jammed");
                    postMessage = true;
                }
                if (UPFM.highlight) UPFM.SetFailedHighlight();
            }
        }

        public override void RepairPart()
        {
            panel = part.FindModuleImplementing<ModuleDeployableSolarPanel>();
            if (originallyRetractable) panel.retractable = true;
        }
    }
}
