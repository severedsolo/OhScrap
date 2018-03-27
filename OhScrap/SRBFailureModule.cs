using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace OhScrap
{
    class SRBFailureModule : BaseFailureModule
    {
        ModuleEngines engine;
        bool message;

        protected override void Overrides()
        {
            maxTimeToFailure = 0;
            launched = true;
            Fields["displayChance"].guiName = "Chance of SRB Failure";
            Fields["safetyRating"].guiName = "SRB Safety Rating";
            failureType = "ignition failure";
            engine = part.FindModuleImplementing<ModuleEngines>();
            suppressFailure = true;
        }

        protected override bool FailureAllowed()
        {
            return HighLogic.CurrentGame.Parameters.CustomParams<UPFMSettings>().SRBFailureModuleAllowed;
        }

        protected override void FailPart()
        {
            if (engine.currentThrottle == 0) return;
            engine.allowShutdown = true;
            engine.allowRestart = false;
            engine.Shutdown();
            suppressFailure = false;
            if (!message)
            {
                ScreenMessages.PostScreenMessage(part.partInfo.title + " has failed to ignite");
                Debug.Log("[OhScrap]: " + SYP.ID + " has failed to ignite");
                postMessage = true;
                message = true;
            }
        }

        public override void RepairPart()
        {
            ScreenMessages.PostScreenMessage("Igniting an SRB manually doesn't seem like a good idea");
            ModuleUPFMEvents UPFM = part.FindModuleImplementing<ModuleUPFMEvents>();
            UPFM.customFailureEvent = true;
            return;
        }
    }
}
