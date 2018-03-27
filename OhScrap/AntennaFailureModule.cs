using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace OhScrap
{
    class AntennaFailureModule : BaseFailureModule
    {
        ModuleDataTransmitter antenna;
        [KSPField(isPersistant = true, guiActive = false)]
        bool firstFail = false;
        [KSPField(isPersistant = true, guiActive = false)]
        double originalPower;
        bool message;

        protected override void Overrides()
        {
            Fields["displayChance"].guiName = "Chance of Antenna Failure";
            Fields["safetyRating"].guiName = "Antenna Safety Rating";
            failureType = "communication failure";
            antenna = part.FindModuleImplementing<ModuleDataTransmitter>();
        }

        protected override bool FailureAllowed()
        {
            return HighLogic.CurrentGame.Parameters.CustomParams<UPFMSettings>().AntennaFailureModuleAllowed;
        }

        protected override void FailPart()
        {
            if (!antenna.enabled) return;
            if(firstFail)
            {
                originalPower = antenna.antennaPower;
                firstFail = false;
            }
            antenna.antennaPower = 0;
            if (!message)
            {
                ScreenMessages.PostScreenMessage(part.partInfo.title + " has stopped transmitting");
                Debug.Log("[OhScrap]: " + SYP.ID + " has stopped transmitting");
                postMessage = true;
                message = true;
            }
        }

        public override void RepairPart()
        {
            antenna.antennaPower = originalPower;
        }
    }
}