using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace OhScrap
{
    class FARControlSurfaceFailureModule : BaseFailureModule
    {

        PartModule FARControlSurface;
        private System.Random _randomizer = new System.Random();

        
        private int failMode;
        public float failTimeBrakeRudder { get; set; }

        //Weights for chances of different failures. -Need to load from cfg
        private const int stuckWeight = 20;
        private const int hingeWeight = 80;

        private const int weightTotal = stuckWeight + hingeWeight;

        //Hinge Failure additional random weights. - load from cfg and balance. 
        private const int hingePitchWeight = 20;
        private const int hingeYawWeight = hingePitchWeight;
        private const int hingeRollWeight = hingePitchWeight;
        private const int hingeResetWeight = 5;
        private const int hingeStuckWeight = 2;
        
        private const int hingeWeightTotal = (hingePitchWeight * 3) + hingeResetWeight + hingeStuckWeight;


        
        private StuckScenario stuckScenario;
        private ResetScenario resetScenario;
        private List<Scenario> hingeFailureScenarios = new List<Scenario>(); 

        protected override void Overrides()
        {
            Fields["displayChance"].guiName = "Chance of Control Surface Failure";
            Fields["safetyRating"].guiName = "Control Surface Safety Rating";
            failureType = "Control Surface Failure";
            foreach (PartModule pm in part.Modules)
            {
                if (pm.moduleName.Equals("FARControllableSurface"))
                {
                    FARControlSurface = pm;
                }
            }
            //Part is mechanical so can be repaired remotely.
            remoteRepairable = true;

            //Setup Weights and references. 
            hingeFailureScenarios.Add(new PitchChangeScenario(hingePitchWeight));
            hingeFailureScenarios.Add(new YawChangeScenario(hingeYawWeight));
            hingeFailureScenarios.Add(new RollChangeScenario(hingeRollWeight));
            resetScenario = new ResetScenario(hingeResetWeight);
            hingeFailureScenarios.Add(resetScenario);
            stuckScenario = new StuckScenario(hingeStuckWeight);
            hingeFailureScenarios.Add(stuckScenario);
            hingeFailureScenarios = hingeFailureScenarios.OrderBy(i => (i.Weight)).ToList();
        }

        public override bool FailureAllowed()
        {
            if (part.vessel.atmDensity == 0) return false;
            return (HighLogic.CurrentGame.Parameters.CustomParams<UPFMSettings>().ControlSurfaceFailureModuleAllowed
            && ModWrapper.FerramWrapper.available
            && FARControlSurface);
        }
        
        public override void FailPart()
        {
            if (!FARControlSurface) return;
            if (!hasFailed) 
            {
                failMode = _randomizer.Next(1, weightTotal + 1);
                //failTimeBrakeRudder = ModWrapper.FerramWrapper.GetCtrlSurfBrakeRudder(FARControlSurface);
                resetScenario.failTimeYaw = ModWrapper.FerramWrapper.GetCtrlSurfYaw(FARControlSurface);
                resetScenario.failTimePitch = ModWrapper.FerramWrapper.GetCtrlSurfPitch(FARControlSurface);
                resetScenario.failTimeRoll = ModWrapper.FerramWrapper.GetCtrlSurfRoll(FARControlSurface);
                //Debug.Log("[OhScrap](FAR): " + SYP.ID + " has suffered a control surface failure");
            }


            if (failMode <= stuckWeight) //Stuck Surface.
            {
                if (!hasFailed)
                {
                    Debug.Log("[OhScrap](FAR): " + SYP.ID + " has a stuck control surface");
                    
                    hasFailed = true;
                }
                stuckScenario.Run(FARControlSurface);
            }
            else //Hinge Failure. 
            {
                if (!hasFailed)
                {
                    Debug.Log("[OhScrap](FAR): " + SYP.ID + " hinge failure.");
                    hasFailed = true;
                }

                //Load from Config? randomize once or every time? 
                int adjustmnetAmount = (_randomizer.Next(-10, 10)) / 100;
                int adjustmentRoll = _randomizer.Next(1, hingeWeightTotal + 1);

                int counter = hingeFailureScenarios.Count - 1;
                Scenario s = null;
                while (counter >= 0)
                {
                    s = hingeFailureScenarios.ElementAt(counter);
                    if ((adjustmentRoll -= s.Weight) <= 0)
                    {
                        counter = 0;
                    }
                    counter--;
                }
                s.Run(FARControlSurface);
                if (OhScrap.highlight) OhScrap.SetFailedHighlight();
            }
        }
        //restores control to the control surface
        public override void RepairPart()
        {
            resetScenario.Run(FARControlSurface);
        }

        private abstract class Scenario
        {
            public int Weight { get; set; }
            public String Name { get; set; }
            public abstract void Run(PartModule surface);
        }

    
        private class PitchChangeScenario : Scenario
        {
            int adjust_amount { get; set; }
            private float curr_amount;
            public PitchChangeScenario(int weight)
            {
                Weight = weight;
            }
            public override void Run(PartModule surface)
            {
                curr_amount = ModWrapper.FerramWrapper.GetCtrlSurfPitch(surface);
                ModWrapper.FerramWrapper.SetCtrlSurfPitch(surface, (curr_amount + (curr_amount * adjust_amount)));
            }
           
        }
        private class YawChangeScenario : Scenario
        {
            int adjust_amount { get; set; }
            private float curr_amount;
            public YawChangeScenario(int weight)
            {
                Weight = weight;
            }
            public override void Run(PartModule surface)
            {
                curr_amount = ModWrapper.FerramWrapper.GetCtrlSurfYaw(surface);
                ModWrapper.FerramWrapper.SetCtrlSurfYaw(surface, (curr_amount + (curr_amount * adjust_amount)));
            }

        }
        private class RollChangeScenario : Scenario
        {
            int adjust_amount { get; set; }
            private float curr_amount;
            public RollChangeScenario(int weight)
            {
                Weight = weight;
            }
            public override void Run(PartModule surface)
            {
                curr_amount = ModWrapper.FerramWrapper.GetCtrlSurfRoll(surface);
                ModWrapper.FerramWrapper.SetCtrlSurfRoll(surface, (curr_amount + (curr_amount * adjust_amount)));
            }

        }

        private class ResetScenario : Scenario
        {
            public float failTimePitch { get; set; }
            public float failTimeYaw { get; set; }
            public float failTimeRoll { get; set; }
            public ResetScenario(int weight)
            {
                Weight = weight;
            }
            public override void Run(PartModule surface)
            {
                ModWrapper.FerramWrapper.SetCtrlSurfPitch(surface, failTimePitch);
                ModWrapper.FerramWrapper.SetCtrlSurfRoll(surface, failTimeRoll);
                ModWrapper.FerramWrapper.SetCtrlSurfYaw(surface, failTimeYaw);
            }

        }

        private class StuckScenario : Scenario
        {
            public StuckScenario(int weight)
            {
                Weight = weight;
            }
            public override void Run(PartModule surface)
            {
                ModWrapper.FerramWrapper.SetCtrlSurfPitch(surface, 0.0f);
                ModWrapper.FerramWrapper.SetCtrlSurfRoll(surface, 0.0f);
                ModWrapper.FerramWrapper.SetCtrlSurfYaw(surface, 0.0f);
            }
        }
    }

}

