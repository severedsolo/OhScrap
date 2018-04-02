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

        // Overrides mostly turn generic labels into meaningful ones, and find the Stock Part Module we are going to fail.
        protected override void Overrides()
        {
            Fields["displayChance"].guiName = "Chance of Antenna Failure";
            Fields["safetyRating"].guiName = "Antenna Safety Rating";
            failureType = "communication failure";
            antenna = part.FindModuleImplementing<ModuleDataTransmitter>();
        }
        //Checks if the player has the failures turned on in the menu
        protected override bool FailureAllowed()
        {
            return HighLogic.CurrentGame.Parameters.CustomParams<UPFMSettings>().AntennaFailureModuleAllowed;
        }

        //performs the actual failure.
        protected override void FailPart()
        {
            //if the antenna isn't turned on, don't bother.
            if (!antenna.enabled) return;
            //if this is the first time we've failed this antenna, we need to make a note of the original power for when it's repaired.
            if(firstFail)
            {
                originalPower = antenna.antennaPower;
                firstFail = false;

            }
            //turn the antenna power down to 0
            antenna.antennaPower = 0;
            //post the message if it hasn't already been posted
            if (!message)
            {
                ScreenMessages.PostScreenMessage(part.partInfo.title + " has stopped transmitting");
                Debug.Log("[OhScrap]: " + SYP.ID + " has stopped transmitting");
                postMessage = true;
                message = true;
            }
        }
        //repair just turns the power back to the original power
        public override void RepairPart()
        {
            antenna.antennaPower = originalPower;
        }
    }
}