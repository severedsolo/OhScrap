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

        protected override void FailPart()
        {
            battery = part.Resources["ElectricCharge"];
            battery.amount = 0;
            battery.flowState = false;
            SetFailedHighlight();
            if (message) return;
            ScreenMessages.PostScreenMessage("Battery short circuited!");
            Debug.Log("[UPFM]: " + part.name + "has short circuited");
            message = true;
        }

        protected override void RepairPart()
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
