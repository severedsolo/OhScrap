using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Untitled_Part_Failure_Mod
{
    class EngineFailureModule : BaseFailureModule
    {
        ModuleEngines engine;
        double timeBetweenFailureEvents = 0;
        System.Random r = new System.Random();
        [KSPField(isPersistant = true, guiActive = false)]
        string failureType = "none";
        float staticThrust;
        ModuleGimbal gimbal;

        protected override bool FailureAllowed()
        {
            return HighLogic.CurrentGame.Parameters.CustomParams<UPFMSettings>().EngineFailureModuleAllowed;
        }

        protected override void FailPart()
        {
            if (part.Resources.Contains("SolidFuel")) return;
            engine = part.FindModuleImplementing<ModuleEngines>();
            if (engine.currentThrottle == 0) return;
            SetFailedHighlight();
            if (failureType == "none")
            {
                int i = r.Next(1, 5);
                switch (i)
                {
                    case 1:
                        failureType = "Fuel Flow Failure";
                        Debug.Log("[UPFM]: attempted to perform Fuel Flow Failure on " + part.name);
                        break;
                    case 2:
                        failureType = "Fuel Line Leak";
                        Debug.Log("[UPFM]: attempted to perform Fuel Line Leak on " + part.name);
                        break;
                    case 3:
                        failureType = "Underthrust";
                        Debug.Log("[UPFM]: attempted to perform Underthrust on " + part.name);
                        break;
                    case 4:
                        gimbal = part.FindModuleImplementing<ModuleGimbal>();
                        if (gimbal == null) return;
                        failureType = "Gimbal Failure";
                        Debug.Log("[UPFM]: attempted to lock gimbal on" + part.name);
                        break;
                    default:
                        failureType = "none";
                        Debug.Log("[UPFM]: "+part.name+" decided not to fail after all");
                        break;
                }
                ScreenMessages.PostScreenMessage(failureType + " detected on " + part.name);
            }
            switch (failureType)
            {
                case "Fuel Flow Failure":
                    engine.Shutdown();
                    break;
                case "Fuel Line Leak":
                    part.explode();
                    break;
                case "Underthrust":
                    if (timeBetweenFailureEvents <= Planetarium.GetUniversalTime())
                    {
                        engine.thrustPercentage = engine.thrustPercentage * 0.9f;
                        timeBetweenFailureEvents = Planetarium.GetUniversalTime() + r.Next(10,30);
                        staticThrust = engine.thrustPercentage;
                    }
                    engine.thrustPercentage = staticThrust;
                    break;
                case "Gimbal Failure":
                    gimbal.gimbalLock = true;
                    break;
                default:
                    return;
            }
        }

        protected override void RepairPart()
        {
            switch(failureType)
            {
                case "Fuel Flow Failure":
                    engine.Activate();
                    Debug.Log("[UPFM]: Re-activated " + part.name);
                    break;
                case "Underthrust":
                    engine.thrustPercentage = 100;
                    Debug.Log("[UPFM]: Reset Thrust on " + part.name);
                    break;
                case "Gimbal Failure":
                    gimbal.gimbalLock = false;
                    break;

                default:
                    return;
            }
        }
    }
}
