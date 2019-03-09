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

        public override bool FailureAllowed()
        {
            if (!part.FindModuleImplementing<ModuleRCS>().rcsEnabled) return false;
            return HighLogic.CurrentGame.Parameters.CustomParams<UPFMSettings>().RCSFailureModuleAllowed;
        }

        protected override void Overrides()
        {
            Fields["displayChance"].guiName = "Chance of RCS Failure";
            Fields["safetyRating"].guiName = "RCS Safety Rating";
            failureType = "RCS Failure";
        }

        //turns the RCS off.
        public override void FailPart()
        {
            rcs = part.FindModuleImplementing<ModuleRCS>();
            if (rcs == null) return;
            rcs.rcsEnabled = false;
            if (OhScrap.highlight) OhScrap.SetFailedHighlight();
            if (hasFailed) return;
        }
        //turns it back on again
        public override void RepairPart()
        {
            //rcs = part.FindModuleImplementing<ModuleRCS>();
            rcs.rcsEnabled = true;
        }
    }
}
