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

        protected override void FailPart()
        {
            rw = part.FindModuleImplementing<ModuleReactionWheel>();
            rw.isEnabled = false;
            if (rw.wheelState != ModuleReactionWheel.WheelState.Broken) Debug.Log("[UPFM]: " + part.name + "'s reaction wheels have failed");
            rw.wheelState = ModuleReactionWheel.WheelState.Broken;
        }

        protected override void RepairPart()
        {
            rw.isEnabled = true;
            rw.wheelState = ModuleReactionWheel.WheelState.Active;
        }
    }
}
