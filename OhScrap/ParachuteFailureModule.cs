using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace OhScrap
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
            Fields["safetyRating"].guiName = "Parachute Safety Rating";
            postMessage = false;
        }

        //Cuts the chute if it's deployed
        protected override void FailPart()
        {
            chute = part.FindModuleImplementing<ModuleParachute>();
            if (chute == null) return;
            if (chute.vessel != FlightGlobals.ActiveVessel) return;
            if (chute.deploymentState == ModuleParachute.deploymentStates.SEMIDEPLOYED || chute.deploymentState == ModuleParachute.deploymentStates.DEPLOYED) chute.CutParachute();
            if (OhScrap.highlight) OhScrap.SetFailedHighlight();
            if (message) return;
            message = true;
            postMessage = true;
            if(vessel.vesselType != VesselType.Debris) ScreenMessages.PostScreenMessage("Parachute Failure!");
            Debug.Log("[OhScrap]: " + SYP.ID + " parachute has failed");
        }
    }
}
