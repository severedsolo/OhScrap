using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace OhScrap
{
    class BatteryFailureModule : BaseFailureModule
    {
        PartResource battery;
        bool message;

        protected override void Overrides()
        {
            Fields["displayChance"].guiName = "Chance of Battery Failure";
            Fields["safetyRating"].guiName = "Battery Safety Rating";
            failureType = "short circuit";
        }

        // Failure will drain the battery and stop it from recharging.
        protected override void FailPart()
        {
            battery = part.Resources["ElectricCharge"];
            battery.amount = 0;
            battery.flowState = false;
            if (OhScrap.highlight) OhScrap.SetFailedHighlight();
            if (message) return;
            if(vessel.vesselType != VesselType.Debris) ScreenMessages.PostScreenMessage("Battery short circuited!");
            Debug.Log("[OhScrap]: " + SYP.ID + " has suffered a short circuit failure");
            message = true;
        }

        //Repair allows it to be charged again.
        public override void RepairPart()
        {
            battery = part.Resources["ElectricCharge"];
            battery.flowState = true;
        }

        protected override bool FailureAllowed()
        {
            return HighLogic.CurrentGame.Parameters.CustomParams<UPFMSettings>().BatteryFailureModuleAllowed;
        }
    }
}
