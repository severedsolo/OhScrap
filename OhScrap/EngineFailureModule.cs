using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace OhScrap
{
    class EngineFailureModule : BaseFailureModule
    {
        ModuleEngines engine;
        double timeBetweenFailureEvents = 0;
        [KSPField(isPersistant = true, guiActive = false)]
        float staticThrust;
        int fuelLineCounter = 5;
        ModuleGimbal gimbal;
        [KSPField(isPersistant = true, guiActive = false)]
        float originalThrust;
        [KSPField(isPersistant = true, guiActive = false)]
        int spaceEngineExpectedLifetime;
        [KSPField(isPersistant = true, guiActive = false)]
        float spaceEngineBaseChanceOfFailure;


        protected override void Overrides()
        {
            Fields["displayChance"].guiName = "Chance of Engine Failure";
            Fields["safetyRating"].guiName = "Engine Safety Rating";
            engine = part.FindModuleImplementing<ModuleEngines>();
            //If the ISP at sea level suggests this is a space engine, change the lifetime and failure rates accordingly
            float staticPressure = (float)(FlightGlobals.GetHomeBody().GetPressure(0) * PhysicsGlobals.KpaToAtmospheres);
            if (engine.atmosphereCurve.Evaluate(staticPressure) <= 100.0f)
            {
                expectedLifetime = spaceEngineExpectedLifetime;
                baseChanceOfFailure = spaceEngineBaseChanceOfFailure;
            }
        }   

        public override bool FailureAllowed()
        {
            if (part.FindModuleImplementing<ModuleEngines>().currentThrottle == 0) return false;
            return HighLogic.CurrentGame.Parameters.CustomParams<UPFMSettings>().EngineFailureModuleAllowed;
        }

        public override void FailPart()
        {
            engine = part.FindModuleImplementing<ModuleEngines>();
            gimbal = part.FindModuleImplementing<ModuleGimbal>();
            if (engine != null)
            {
                //In the event of a fuel line leak, the chance of explosion will be reset if the engine is shut down.
                if (engine.currentThrottle == 0)
                {
                    fuelLineCounter = 5;
                    return;
                }
            }
            if (OhScrap.highlight) OhScrap.SetFailedHighlight();
            //Randomly pick which failure we will give the player
            if (failureType == "none")
            {
                int i = UPFMUtils.instance._randomiser.Next(1, 5);
                switch (i)
                {
                    case 1:
                        failureType = "Fuel Flow Failure";
                        Debug.Log("[OhScrap]: attempted to perform Fuel Flow Failure on " + SYP.ID);
                        break;
                    case 2:
                        failureType = "Fuel Line Leak";
                        Debug.Log("[OhScrap]: attempted to perform Fuel Line Leak on " + SYP.ID);
                        InvokeRepeating("LeakFuel", 2.0f, 2.0f);
                        break;
                    case 3:
                        failureType = "Underthrust";
                        originalThrust = engine.thrustPercentage;
                        Debug.Log("[OhScrap]: attempted to perform Underthrust on " + SYP.ID);
                        break;
                    case 4:
                        if (gimbal == null) return;
                        failureType = "Gimbal Failure";
                        Debug.Log("[OhScrap]: attempted to lock gimbal on" + SYP.ID);
                        break;
                    default:
                        failureType = "none";
                        Debug.Log("[OhScrap]: " + SYP.ID + " decided not to fail after all");
                        break;
                }
                return;
            }
            switch (failureType)
            {
                //Engine shutdown
                case "Fuel Flow Failure":
                    engine.Shutdown();
                    break;
                 //Fuel line leaks will explode the engine after anywhere between 5 and 50 seconds.
                case "Fuel Line Leak":
                    if (timeBetweenFailureEvents > Planetarium.GetUniversalTime()) break;
                    if (fuelLineCounter < 0) part.explode();
                    else fuelLineCounter--;
                    timeBetweenFailureEvents = Planetarium.GetUniversalTime() + UPFMUtils.instance._randomiser.Next(1, 5);
                    break;
                //Engine will constantly lose thrust
                case "Underthrust":
                    if (timeBetweenFailureEvents <= Planetarium.GetUniversalTime())
                    {
                        engine.thrustPercentage = engine.thrustPercentage * 0.9f;
                        timeBetweenFailureEvents = Planetarium.GetUniversalTime() + UPFMUtils.instance._randomiser.Next(10, 30);
                        staticThrust = engine.thrustPercentage;
                    }
                    engine.thrustPercentage = staticThrust;
                    break;
                 //lock gimbal
                case "Gimbal Failure":
                    gimbal.gimbalLock = true;
                    break;
                default:
                    return;
            }
        }

        public override void RepairPart()
        {
            engine = part.FindModuleImplementing<ModuleEngines>();
            switch (failureType)
            {
                case "Fuel Flow Failure":
                    engine.Activate();
                    Debug.Log("[OhScrap]: Re-activated " + SYP.ID);
                    break;
                case "Underthrust":
                    engine.thrustPercentage = originalThrust;
                    Debug.Log("[OhScrap]: Reset Thrust on " + SYP.ID);
                    break;
                case "Gimbal Failure":
                    gimbal.gimbalLock = false;
                    break;
                case "Fuel Line Leak":
                    CancelInvoke("LeakFuel");
                    break;
                default:
                    return;
            }
        }

        void LeakFuel()
        {
            part.RequestResource("LiquidFuel", 1.0f);
            ScreenMessages.PostScreenMessage("Fuel Line Leaking!");
        }
    }
}
