using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace OhScrap
{
    class ReactionWheelFailureModule : BaseFailureModule
    {
        ModuleReactionWheel rw;

        protected override void Overrides()
        {
            Fields["displayChance"].guiName = "Chance of Reaction Wheel Failure";
            Fields["safetyRating"].guiName = "Reaction Wheel Safety Rating";
            failureType = "Reaction Wheel Failure";
            remoteRepairable = true;
        }

        protected override bool FailureAllowed()
        {
            return HighLogic.CurrentGame.Parameters.CustomParams<UPFMSettings>().ReactionWheelFailureModuleAllowed;
        }

        // Reaction wheel stops working
        public override void FailPart()
        {
            rw = part.FindModuleImplementing<ModuleReactionWheel>();
            if (!rw.isEnabled && rw.wheelState != ModuleReactionWheel.WheelState.Active)
            {
                hasFailed = false;
                return;
            }
            rw.isEnabled = false;
            rw.wheelState = ModuleReactionWheel.WheelState.Broken;
            if (OhScrap.highlight) OhScrap.SetFailedHighlight();
        }
        //Turns it back on again,
        public override void RepairPart()
        {
            rw = part.FindModuleImplementing<ModuleReactionWheel>();
            rw.isEnabled = true;
            rw.wheelState = ModuleReactionWheel.WheelState.Active;
        }
    }
}
