using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Untitled_Part_Failure_Mod
{
    class RCSFailureModule : BaseFailureModule
    {
        ModuleRCS rcs;
        bool message;

        protected override bool FailureAllowed()
        {
            return HighLogic.CurrentGame.Parameters.CustomParams<UPFMSettings>().RCSFailureModuleAllowed;
        }

        protected override void Overrides()
        {
            Fields["displayChance"].guiName = "Chance of RCS Failure";
            failureType = "RCS failure";
            postMessage = false;
        }

        protected override void FailPart()
        {
            rcs = part.FindModuleImplementing<ModuleRCS>();
            if (rcs == null) return;
            if (rcs.vessel != FlightGlobals.ActiveVessel) return;
            rcs.rcsEnabled = false;
            if (UPFM.highlight) UPFM.SetFailedHighlight();
            if (message) return;
            message = true;
            postMessage = true;
            ScreenMessages.PostScreenMessage("RCS Failure!");
            Debug.Log("[UPFM]: " + SYP.ID + " RCS has failed");
        }
    }
}
