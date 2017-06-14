using System;
using UnityEngine;

namespace Untitled_Part_Failure_Mod
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
        [GameParameters.CustomIntParameterUI("Safety Threshold", toolTip = "At what failure threshold should UPFM display a warning?")]
        public int safetyThreshold = 66;
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
    }
}
