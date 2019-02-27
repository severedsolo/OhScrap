using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;


namespace OhScrap
{
    //Allows For failures on Antennas when remote tech is installed. Can fail Parts with ModuleRTAntenna.
    class RTAntennaFailureModule : BaseFailureModule
    {
      

        private PartModule antenna;
        [KSPField(isPersistant = true, guiActive = false)]
        bool RTAvailable = ModWrapper.RemoteTechWrapper.available;



        protected override void Overrides()
        {
            Fields["displayChance"].guiName = "Chance of Antenna Failure";
            Fields["safetyRating"].guiName = "Antenna Safety Rating";
            failureType = "communication failure";
            if (!antenna)
            {
                foreach (PartModule pm in part.Modules)
                {
                    if (pm.moduleName.Equals("ModuleRTAntenna"))
                    {
                       antenna = pm;
                    }

                }
            }
            remoteRepairable = true;
        }

        public override bool FailureAllowed()
        {
            if (antenna)
            {
                if (!RTAvailable) return false;
                if (part.FindModuleImplementing<ModuleDeployableAntenna>() != null)
                {
                    if (part.FindModuleImplementing<ModuleDeployableAntenna>().deployState != ModuleDeployablePart.DeployState.EXTENDED) return false;
                }
                return HighLogic.CurrentGame.Parameters.CustomParams<UPFMSettings>().AntennaFailureModuleAllowed;
            }
            else
            {
                return false;
            }
        }
        public override void FailPart()
        {
                ModWrapper.RemoteTechWrapper.SetRTBrokenStatus(antenna, true);
                ScreenMessages.PostScreenMessage(antenna.part.partName + "is not responding. Connection Failure.");
                if (OhScrap.highlight) OhScrap.SetFailedHighlight();
                if (!hasFailed) Debug.Log("[OhScrap](RemoteTech): " + SYP.ID + " has stopped transmitting");
                hasFailed = true;
        }
        
        public override void RepairPart()
        {
            if (antenna)
            {
                ModWrapper.RemoteTechWrapper.SetRTBrokenStatus(antenna, false);
            }
        }

    
    }
}