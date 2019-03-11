using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace OhScrap
{
    //Failures for parachutes when Ferram Aerospace is installed. FAR uses it's own implementation of realchute. 
    class FARParachuteFailureModule : BaseFailureModule
    {
        PartModule chute;
        
        public override bool FailureAllowed()
        {
           // if (vessel.atmDensity <= 0.001f) return false; 
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

        
        public override void FailPart()
        {

            if (chute == null) return;
            if (OhScrap.highlight) OhScrap.SetFailedHighlight();
            if (chute.vessel != FlightGlobals.ActiveVessel) return;
            if (hasFailed) return;
            if (ModWrapper.FerramWrapper.IsDeployed(chute))
            {
                    ModWrapper.FerramWrapper.CutChute(chute);
                   chute.Events["GUIRepack"].active = false;
            }
            else
            {
                    ModWrapper.FerramWrapper.DeployChute(chute); //Will deploy the chute right away, ignoring chutes min altitude/pressure. 
                    chute.Events["GUIDisarm"].active = false;
                    chute.Events["GUIRepack"].active = false;
                    
                    
            }
            hasFailed = true;
          
        }

        public override void RepairPart() //turn off highlight and repack with one of realchutes spares.
        {
                chute.Events["GUIDisarm"].active = true;
                chute.Events["GUIRepack"].active = true;
        }
    }
}