using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace OhScrap
{
    class ControlSurfaceFailureModule : BaseFailureModule
    {
        ModuleControlSurface controlSurface;
        [KSPField(isPersistant = true, guiActive = false)]
        bool message;

        protected override void Overrides()
        {
            Fields["displayChance"].guiName = "Chance of Control Surface Failure";
            Fields["safetyRating"].guiName = "Control Surface Safety Rating";
            failureType = "stuck control surface";
            //Part is mechanical so can be repaired remotely.
            remoteRepairable = true;
        }

        protected override bool FailureAllowed()
        {
            return HighLogic.CurrentGame.Parameters.CustomParams<UPFMSettings>().ControlSurfaceFailureModuleAllowed;
        }
        //control surface will stick and not respond to input
        protected override void FailPart()
        {
            if (part.vessel.atmDensity == 0) return;
            controlSurface = part.FindModuleImplementing<ModuleControlSurface>();
            controlSurface.ignorePitch = true;
            controlSurface.ignoreRoll = true;
            controlSurface.ignoreYaw = true;
            if (OhScrap.highlight) OhScrap.SetFailedHighlight();
            if (message) return;
            if(vessel.vesselType != VesselType.Debris) ScreenMessages.PostScreenMessage("Control Surface Failure!");
            Debug.Log("[OhScrap]: " + SYP.ID + " has suffered a control surface failure");
            message = true;
        }
        //restores control to the control surface
        public override void RepairPart()
        {
            controlSurface = part.FindModuleImplementing<ModuleControlSurface>();
            controlSurface.ignorePitch = false;
            controlSurface.ignoreRoll = false;
            controlSurface.ignoreYaw = false;
        }
    }
}
