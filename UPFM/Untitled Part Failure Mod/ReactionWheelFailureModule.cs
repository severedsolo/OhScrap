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

        protected override bool FailureAllowed()
        {
            return HighLogic.CurrentGame.Parameters.CustomParams<UPFMSettings>().ReactionWheelFailureModuleAllowed;
        }

        protected override void FailPart()
        {
            rw = part.FindModuleImplementing<ModuleReactionWheel>();
            rw.isEnabled = false;
            rw.wheelState = ModuleReactionWheel.WheelState.Broken;
            SetFailedHighlight();
            if (message) return;
            message = true;
            if (rw.wheelState != ModuleReactionWheel.WheelState.Broken) Debug.Log("[UPFM]: " + part.name + "'s reaction wheels have failed");
            ScreenMessages.PostScreenMessage("A reaction wheel has failed");
        }

        protected override void RepairPart()
        {
            rw.isEnabled = true;
            rw.wheelState = ModuleReactionWheel.WheelState.Active;
        }
    }
}
