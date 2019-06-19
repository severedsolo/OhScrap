using System;
using System.Collections.Generic;
using ScrapYard.Utilities;
using UnityEngine;

namespace OhScrap
{
    public class HeatControl : PartModule
    {
        [KSPField(isPersistant = true, guiActive = false)]
        internal bool turnedOn = true;
        [KSPField(isPersistant = false, guiActive = true, guiName = "Cabin Temperature", guiUnits = "C")]
        private double cabinTemp = 20.0f;
        [KSPField(isPersistant = true, guiActive = false)]
        private double cabinTempKelvin = 293.15f;
        [KSPField(isPersistant = false, guiActive = true, guiName = "Internal Temperature", guiUnits = "K")]
        private double internalTemp = 0;
        [KSPField(isPersistant = false, guiActive = true, guiName = "Skin Temperature", guiUnits = "K")]
        private double skinTemp = 0;
        private void Start()
        {
            cabinTemp = Math.Round(cabinTempKelvin - 273.15f, 0);
            if (HighLogic.LoadedScene != GameScenes.FLIGHT) return;
            InvokeRepeating("DrawPower",0.01f,0.01f);
        }

        private void DrawPower()
        {
            double desiredTemp = 293.15f;
            internalTemp = Math.Round(part.temperature, 0);
            skinTemp = Math.Round(part.skinTemperature, 0);
            double requiredEC = 0;
            double providedEC = 0;
            if (turnedOn)
            {
                requiredEC = (Math.Max(cabinTempKelvin, internalTemp) - Math.Min(cabinTempKelvin, internalTemp)) * TimeWarp.CurrentRate / 60 / 60 / 10;
                providedEC = part.vessel.RequestResource(part, PartResourceLibrary.Instance.GetDefinition("ElectricCharge").id, requiredEC, false);
            }

            Debug.Log("[HeatControl]: Need "+requiredEC+"got"+providedEC);
            if (cabinTemp > 15.0f && cabinTemp < 25.0f) turnedOn = false;
            else if (cabinTemp < 10.0f || cabinTemp > 30f) turnedOn = true;
            if (!WithinTolerance(requiredEC, providedEC) || !turnedOn)
            {
                double energyDeficit = providedEC / requiredEC;
                if (providedEC == 0) energyDeficit = 1;
                Debug.Log("[HeatControl]: Deficit is "+energyDeficit);
                double heatLoss = (cabinTempKelvin - internalTemp) / 60 / 60 / 10 * TimeWarp.CurrentRate*energyDeficit;
                Debug.Log("Removing "+heatLoss);
                cabinTempKelvin -= heatLoss;
                cabinTemp = Math.Round(cabinTempKelvin - 273.15f, 0);
            }
            else if (cabinTempKelvin != desiredTemp)
            {
                cabinTempKelvin += (desiredTemp - cabinTempKelvin)/60/60/10*TimeWarp.CurrentRate;
                cabinTemp = Math.Round(cabinTempKelvin - 273.15f, 0);
            }
        }

        private bool WithinTolerance(double num1, double num2)
        {
            if (Math.Max(num1, num2) - Math.Min(num1, num2) < 0.0001) return true;
            return false;
        }
    }
}