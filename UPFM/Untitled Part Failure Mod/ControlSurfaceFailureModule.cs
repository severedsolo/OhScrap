using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Untitled_Part_Failure_Mod
{
    class ControlSurfaceFailureModule : BaseFailureModule
    {
        ModuleControlSurface controlSurface;
        System.Random r = new System.Random();
        [KSPField(isPersistant = true, guiActive = false)]
        bool message;

        protected override void Overrides()
        {
            Fields["displayChance"].guiName = "Chance of Control Surface Failure";
            failureType = "stuck control surface";
        }

        protected override bool FailureAllowed()
        {
            return HighLogic.CurrentGame.Parameters.CustomParams<UPFMSettings>().ControlSurfaceFailureModuleAllowed;
        }

        protected override void FailPart()
        {
            if (part.vessel.atmDensity == 0) return;
            controlSurface = part.FindModuleImplementing<ModuleControlSurface>();
            controlSurface.ignorePitch = true;
            controlSurface.ignoreRoll = true;
            controlSurface.ignoreYaw = true;
            SetFailedHighlight();
            if (message) return;
            ScreenMessages.PostScreenMessage("Control Surface Failure!");
            Debug.Log("[UPFM]: " + part.name + " has suffered a control surface failure");
            message = true;
        }

        protected override void RepairPart()
        {
            controlSurface = part.FindModuleImplementing<ModuleControlSurface>();
            controlSurface.ignorePitch = false;
            controlSurface.ignoreRoll = false;
            controlSurface.ignoreYaw = false;
        }
    }
}
