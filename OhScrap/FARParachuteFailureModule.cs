using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace OhScrap
{
    class FARParachuteFailureModule : BaseFailureModule
    {
        PartModule chute;
        private bool allowSpaceDeploy = true;



        public override bool FailureAllowed()
        {
            if(vessel.situation = )

            return HighLogic.CurrentGame.Parameters.CustomParams<UPFMSettings>().ParachuteFailureModuleAllowed;
        }

        protected override void Overrides()
        {
            Fields["displayChance"].guiName = "Chance of Parachute Failure";
            failureType = "Parachute Failure";
            Fields["safetyRating"].guiName = "Parachute Safety Rating";
            foreach (PartModule pm in part.Modules)
            {
                if (pm.moduleName.Equals("RealChuteFAR"))
                {
                    chute = pm;
                }
            }
        }

        //Cuts the chute if it's deployed
        public override void FailPart()
        {

            if (chute == null) return;
            if (OhScrap.highlight) OhScrap.SetFailedHighlight();
            if (chute.vessel != FlightGlobals.ActiveVessel) return;
            if (hasFailed) return;
            if (ModWrapper.FerramWrapper.IsDeployed(chute))
            {
                    ModWrapper.FerramWrapper.CutChute(chute);
            }else
            {
                    ModWrapper.FerramWrapper.DeployChute(chute);
            }
            hasFailed = true;
          
        }
    }
}