using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Untitled_Part_Failure_Mod
{
    class BatteryFailureModule : BaseFailureModule
    {
        PartResource battery;
        bool message;

        protected override void Overrides()
        {
            Fields["displayChance"].guiName = "Chance of Battery Failure";
            failureType = "short circuit";
        }
        protected override void FailPart()
        {
            battery = part.Resources["ElectricCharge"];
            battery.amount = 0;
            battery.flowState = false;
            if(UPFM.highlight)UPFM.SetFailedHighlight();
            if (message) return;
            ScreenMessages.PostScreenMessage("Battery short circuited!");
            Debug.Log("[UPFM]: " + SYP.ID + " has suffered a short circuit failure");
            message = true;
        }

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
