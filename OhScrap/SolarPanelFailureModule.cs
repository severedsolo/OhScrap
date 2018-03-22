using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace OhScrap
{
    class SolarPanelFailureModule : BaseFailureModule
    {
        ModuleDeployableSolarPanel panel;
        bool originallyRetractable;
        bool trackingFailure;
        bool trackingSet = false;

        protected override void Overrides()
        {
            Fields["displayChance"].guiName = "Chance of Solar Panel Failure";
            Fields["safetyRating"].guiName = "Solar Panel Safety Rating";
            postMessage = false;
            remoteRepairable = true;
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
            if (!postMessage && !trackingSet)
            {
                if (Randomiser.instance.NextDouble() < 0.5) trackingFailure = true;
                else trackingFailure = false;
                trackingSet = true;
            }
            if (panel.isTracking && panel.retractable && panel.deployState == ModuleDeployablePart.DeployState.EXTENDED && !trackingFailure)
            {
                panel.retractable = false;
                originallyRetractable = true;
                if (!postMessage)
                {
                    failureType = "retraction error";
                    ScreenMessages.PostScreenMessage(part.name + " retraction mechanism jammed");
                    Debug.Log("[OhScrap]: " + SYP.ID + " retraction mechanism has jammed");
                    postMessage = true;
                }
                if (OhScrap.highlight) OhScrap.SetFailedHighlight();
            }
            else if (panel.isTracking && panel.deployState == ModuleDeployablePart.DeployState.EXTENDED && !originallyRetractable)
            {
                panel.isTracking = false;
                if (!postMessage)
                {
                    failureType = "sun tracking error";
                    ScreenMessages.PostScreenMessage(part.name + " sun tracking mechanism jammed");
                    Debug.Log("[OhScrap]: " + SYP.ID + " sun tracking mechanism has jammed");
                    postMessage = true;
                }
                if (OhScrap.highlight) OhScrap.SetFailedHighlight();
            }
        }

        public override void RepairPart()
        {
            panel = part.FindModuleImplementing<ModuleDeployableSolarPanel>();
            if (originallyRetractable) panel.retractable = true;
            panel.isTracking = true;
        }
    }
}
