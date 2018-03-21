using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace OhScrap
{
    class UPFMSettings : GameParameters.CustomParameterNode
    {
        public override string Title { get { return "UPFM Options"; } }
        public override GameParameters.GameMode GameMode { get { return GameParameters.GameMode.ANY; } }
        public override string Section { get { return "Allowed Failures"; } }
        public override string DisplaySection { get { return Section; } }
        public override int SectionOrder { get { return 1; } }
        public override bool HasPresets { get { return false; } }
        public bool autoPersistance = true;
        public bool newGameOnly = false;
        [GameParameters.CustomParameterUI("Display Safety Warnings?")]
        public bool safetyWarning = true;
        [GameParameters.CustomParameterUI("Recover parts above Safety Threshold?")]
        public bool safetyRecover = true;
        [GameParameters.CustomParameterUI("Highlight Part Failures?")]
        public bool highlightFailures = true;
        [GameParameters.CustomParameterUI("Stop Timewarp on Failure?")]
        public bool stopOnFailure = true;
        [GameParameters.CustomIntParameterUI("Safety Threshold", toolTip = "At what failure threshold should UPFM display a warning?")]
        public int safetyThreshold = 25;
        [GameParameters.CustomParameterUI("Battery")]
        public bool BatteryFailureModuleAllowed = true;
        [GameParameters.CustomParameterUI("Control Surface")]
        public bool ControlSurfaceFailureModuleAllowed = true;
        [GameParameters.CustomParameterUI("Engine")]
        public bool EngineFailureModuleAllowed = true;
        [GameParameters.CustomParameterUI("Parachutes")]
        public bool ParachuteFailureModuleAllowed = true;
        [GameParameters.CustomParameterUI("Reaction Wheels")]
        public bool ReactionWheelFailureModuleAllowed = true;
        [GameParameters.CustomParameterUI("Resource Tanks")]
        public bool TankFailureModuleAllowed = true;
        [GameParameters.CustomParameterUI("Solar Panels")]
        public bool SolarPanelFailureModuleAllowed = true;
        [GameParameters.CustomParameterUI("RCS")]
        public bool RCSFailureModuleAllowed = true;
    }
}
