using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Untitled_Part_Failure_Mod
{
    class ReactionWheelFailureModule : BaseFailureModule
    {
        ModuleReactionWheel rw;
        bool message = false;

        protected override void Overrides()
        {
            Fields["displayChance"].guiName = "Chance of Reaction Wheel Failure";
            Fields["safetyRating"].guiName = "Reaction Wheel Safety Rating";
            failureType = "reaction wheel failure";
        }

        protected override bool FailureAllowed()
        {
            return HighLogic.CurrentGame.Parameters.CustomParams<UPFMSettings>().ReactionWheelFailureModuleAllowed;
        }

        protected override void FailPart()
        {
            rw = part.FindModuleImplementing<ModuleReactionWheel>();
            if (!rw.isEnabled && rw.wheelState != ModuleReactionWheel.WheelState.Active) return;
            rw.isEnabled = false;
            rw.wheelState = ModuleReactionWheel.WheelState.Broken;
            if(UPFM.highlight)UPFM.SetFailedHighlight();
            if (message) return;
            message = true;
            if (rw.wheelState != ModuleReactionWheel.WheelState.Broken) Debug.Log("[UPFM]: " + SYP.ID + "'s reaction wheels have failed");
            ScreenMessages.PostScreenMessage("A reaction wheel has failed");
        }

        public override void RepairPart()
        {
            rw = part.FindModuleImplementing<ModuleReactionWheel>();
            rw.isEnabled = true;
            rw.wheelState = ModuleReactionWheel.WheelState.Active;
        }
    }
}
