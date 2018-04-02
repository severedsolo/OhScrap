using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace OhScrap
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
            Fields["safetyRating"].guiName = "RCS Safety Rating";
            failureType = "RCS failure";
            postMessage = false;
        }

        //turns the RCS off.
        protected override void FailPart()
        {
            rcs = part.FindModuleImplementing<ModuleRCS>();
            if (rcs == null) return;
            if (rcs.vessel != FlightGlobals.ActiveVessel) return;
            rcs.rcsEnabled = false;
            if (OhScrap.highlight) OhScrap.SetFailedHighlight();
            if (message) return;
            message = true;
            postMessage = true;
            ScreenMessages.PostScreenMessage("RCS Failure!");
            Debug.Log("[OhScrap]: " + SYP.ID + " RCS has failed");
        }

        public override void RepairPart()
        {
            rcs = part.FindModuleImplementing<ModuleRCS>();
            rcs.rcsEnabled = true;
        }
    }
}
