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

        public override bool FailureAllowed()
        {
            return HighLogic.CurrentGame.Parameters.CustomParams<UPFMSettings>().ParachuteFailureModuleAllowed;
        }

        protected override void Overrides()
        {
            Fields["displayChance"].guiName = "Chance of Parachute Failure";
            failureType = "Parachute Failure";
            Fields["safetyRating"].guiName = "Parachute Safety Rating";
        }

        //Cuts the chute if it's deployed
        public override void FailPart()
        {
            chute = part.FindModuleImplementing<ModuleParachute>();
            if (chute == null) return;
            if (OhScrap.highlight) OhScrap.SetFailedHighlight();
            if (chute.vessel != FlightGlobals.ActiveVessel) return;
            if (chute.deploymentState == ModuleParachute.deploymentStates.SEMIDEPLOYED || chute.deploymentState == ModuleParachute.deploymentStates.DEPLOYED) chute.CutParachute();
        }
    }
}
