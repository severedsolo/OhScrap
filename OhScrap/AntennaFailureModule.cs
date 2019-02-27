using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace OhScrap
{
    //Handles Antenna Failures if remote tech is not installed.
    class AntennaFailureModule : BaseFailureModule
    {
        ModuleDataTransmitter antenna;
        ModuleDeployableAntenna deployableAntenna;
        [KSPField(isPersistant = true, guiActive = false)]
        double originalPower;


        protected override void Overrides()
        {
            antenna = part.FindModuleImplementing<ModuleDataTransmitter>();
            if (antenna && antenna.CommType == 0) //We wont fail the part if its an internal antenna. They arn't real antenna.
            {
                Fields["displayChance"].guiActive = false;
                Fields["safetyRating"].guiActive = false;
            }else
            {
                Fields["displayChance"].guiName = "Chance of Antenna Failure";
                Fields["safetyRating"].guiName = "Antenna Safety Rating";
            }
            
            failureType = "communication failure";
            
            deployableAntenna = part.FindModuleImplementing<ModuleDeployableAntenna>();
            remoteRepairable = true;
        }

        public override bool FailureAllowed()
        {
            if(deployableAntenna != null)
            {
                if (deployableAntenna.deployState != ModuleDeployablePart.DeployState.EXTENDED) return false;
            }
            return (HighLogic.CurrentGame.Parameters.CustomParams<UPFMSettings>().AntennaFailureModuleAllowed 
                    && CommNet.CommNetScenario.CommNetEnabled
                    && antenna.CommType != 0 ); // Not an internal antenna. Command pods without external antennas should not get an antenna failure.
        }
        public override void FailPart()
        {   
            //if this is the first time we've failed this antenna, we need to make a note of the original power for when it's repaired.
            if(!hasFailed)
            {
                originalPower = antenna.antennaPower;
                Debug.Log("[OhScrap]: " + SYP.ID + " has stopped transmitting");
                hasFailed = true;
            }
            if (OhScrap.highlight) OhScrap.SetFailedHighlight();
            antenna.antennaPower = 0;
        }
        //repair just turns the power back to the original power
        public override void RepairPart()
        {
            antenna.antennaPower = originalPower;
        }
    }
}