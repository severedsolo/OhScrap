using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ScrapYard.Modules;
using ScrapYard;

namespace Untitled_Part_Failure_Mod
{
    [KSPScenario(ScenarioCreationOptions.AddToAllGames, GameScenes.FLIGHT, GameScenes.EDITOR)]
    public class UPFMData : ScenarioModule
    {
        public override void OnSave(ConfigNode node)
        {
            ConfigNode temp = node.GetNode("UPFMTracker");
            if (temp == null)
            {
                node.AddNode("UPFMTracker");
                temp = node.GetNode("UPFMTracker");
            }
            if (UPFMUtils.instance.randomisation.Count() == 0) return;
            foreach (var v in UPFMUtils.instance.randomisation)
            {
                if (v.Key == null) continue;
                if (v.Key == "") continue;
                ConfigNode cn = new ConfigNode("PART");
                cn.SetValue("ID", v.Key, true);
                cn.SetValue("RandomFactor", v.Value, true);
                int i;
                if (UPFMUtils.instance.batteryLifetimes.TryGetValue(v.Key, out i)) cn.SetValue("BatteryLifetime", i, true);
                if (UPFMUtils.instance.controlSurfaceLifetimes.TryGetValue(v.Key, out i)) cn.SetValue("ControlSurfaceLifetime", i, true);
                if (UPFMUtils.instance.engineLifetimes.TryGetValue(v.Key, out i)) cn.SetValue("EngineLifetime", i, true);
                if (UPFMUtils.instance.parachuteLifetimes.TryGetValue(v.Key, out i)) cn.SetValue("ParachuteLifetime", i, true);
                if (UPFMUtils.instance.solarPanelLifetimes.TryGetValue(v.Key, out i)) cn.SetValue("SolarPanelLifetime", i, true);
                if (UPFMUtils.instance.reactionWheelLifetimes.TryGetValue(v.Key, out i)) cn.SetValue("ReactionWheelLifetime", i, true);
                if (UPFMUtils.instance.tankLifetimes.TryGetValue(v.Key, out i)) cn.SetValue("TankLifetime", i, true);
                temp.AddNode(cn);
            }
        }

        public override void OnLoad(ConfigNode node)
        {
            ConfigNode temp = node.GetNode("UPFMTracker");
            if (temp == null) return;
            UPFMUtils.instance.randomisation.Clear();
            UPFMUtils.instance.batteryLifetimes.Clear();
            UPFMUtils.instance.controlSurfaceLifetimes.Clear();
            UPFMUtils.instance.engineLifetimes.Clear();
            UPFMUtils.instance.parachuteLifetimes.Clear();
            UPFMUtils.instance.reactionWheelLifetimes.Clear();
            UPFMUtils.instance.solarPanelLifetimes.Clear();
            UPFMUtils.instance.tankLifetimes.Clear();
            ConfigNode[] nodes = temp.GetNodes("PART");
            if (nodes.Count() == 0) return;
            for(int i = 0; i<nodes.Count(); i++)
            {
                ConfigNode cn = nodes.ElementAt(i);
                string s = cn.GetValue("ID");
                int dict;
                if (float.TryParse(cn.GetValue("RandomFactor"),out float f)) UPFMUtils.instance.randomisation.Add(s, f);
                if (int.TryParse(cn.GetValue("ControlSurfaceLifetime"), out dict)) UPFMUtils.instance.controlSurfaceLifetimes.Add(s, dict);
                if (int.TryParse(cn.GetValue("EngineLifetime"), out dict)) UPFMUtils.instance.engineLifetimes.Add(s, dict);
                if (int.TryParse(cn.GetValue("ParachuteLifetime"), out dict)) UPFMUtils.instance.parachuteLifetimes.Add(s, dict);
                if (int.TryParse(cn.GetValue("ReactionWheelLifetime"), out dict)) UPFMUtils.instance.reactionWheelLifetimes.Add(s, dict);
                if (int.TryParse(cn.GetValue("SolarPanelLifetime"), out dict)) UPFMUtils.instance.solarPanelLifetimes.Add(s, dict);
                if (int.TryParse(cn.GetValue("TankLifetime"), out dict)) UPFMUtils.instance.tankLifetimes.Add(s, dict);
                if (int.TryParse(cn.GetValue("BatteryLifetime"), out dict)) UPFMUtils.instance.batteryLifetimes.Add(s, dict);
            }
        }
    }
}
