using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Untitled_Part_Failure_Mod
{
    class ParachuteFailureModule : BaseFailureModule
    {
        ModuleParachute chute;
        bool message;

        protected override bool FailureAllowed()
        {
            return HighLogic.CurrentGame.Parameters.CustomParams<UPFMSettings>().ParachuteFailureModuleAllowed;
        }

        protected override void Overrides()
        {
            Fields["displayChance"].guiName = "Chance of Parachute Failure";
            failureType = "parachute failure";
            postMessage = false;
        }

        protected override void FailPart()
        {
            chute = part.FindModuleImplementing<ModuleParachute>();
            if (chute == null) return;
            if (chute.vessel != FlightGlobals.ActiveVessel) return;
            if (chute.deploymentState == ModuleParachute.deploymentStates.SEMIDEPLOYED || chute.deploymentState == ModuleParachute.deploymentStates.DEPLOYED) chute.CutParachute();
            else return;
            if(UPFM.highlight)UPFM.SetFailedHighlight();
            if (message) return;
            message = true;
            postMessage = true;
            ScreenMessages.PostScreenMessage("Parachute Failure!");
            Debug.Log("[UPFM]: " + SYP.ID + " parachute has failed");
        }
    }
}
