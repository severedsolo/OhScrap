using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ScrapYard.Modules;
using ScrapYard;

namespace OhScrap
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
                if (v.Key == 0) continue;
                ConfigNode cn = new ConfigNode("PART");
                cn.SetValue("ID", v.Key, true);
                cn.SetValue("RandomFactor", v.Value, true);
                int i;
                if (UPFMUtils.instance.generations.TryGetValue(v.Key, out i)) cn.SetValue("Generation", i, true);
                temp.AddNode(cn);
            }
            foreach (var v in UPFMUtils.instance.numberOfFailures)
            {
                ConfigNode cn = new ConfigNode("FAILURE");
                cn.SetValue("Name", v.Key, true);
                cn.SetValue("Failures", v.Value, true);
                temp.AddNode(cn);
            }
            temp.SetValue("FlightWindow", UPFMUtils.instance.flightWindow, true);
            temp.SetValue("EditorWindow", UPFMUtils.instance.editorWindow, true);
            Debug.Log("[OhScrap]: Saved");
        }

        public override void OnLoad(ConfigNode node)
        {
            ConfigNode temp = node.GetNode("UPFMTracker");
            if (temp == null) return;
            UPFMUtils.instance.randomisation.Clear();
            UPFMUtils.instance.numberOfFailures.Clear();
            UPFMUtils.instance.generations.Clear();
            bool.TryParse(temp.GetValue("FlightWindow"), out UPFMUtils.instance.flightWindow);
            bool.TryParse(temp.GetValue("EditorWindow"), out UPFMUtils.instance.editorWindow);
            ConfigNode[] nodes = temp.GetNodes("PART");
            if (nodes.Count() == 0) return;
            for (int i = 0; i < nodes.Count(); i++)
            {
                ConfigNode cn = nodes.ElementAt(i);
                string s = cn.GetValue("ID");
                uint.TryParse(s, out uint u);
                if (float.TryParse(cn.GetValue("RandomFactor"), out float f)) UPFMUtils.instance.randomisation.Add(u, f);
                if (int.TryParse(cn.GetValue("Generation"), out int g)) UPFMUtils.instance.generations.Add(u, g);
            }
            nodes = temp.GetNodes("FAILURE");
            if (nodes.Count() == 0) return;
            for (int i = 0; i < nodes.Count(); i++)
            {
                ConfigNode cn = nodes.ElementAt(i);
                string s = cn.GetValue("Name");
                if (int.TryParse(cn.GetValue("Failures"), out int failures)) UPFMUtils.instance.numberOfFailures.Add(s, failures);
            }
            Debug.Log("[OhScrap]: Loaded");
        }
    }
}
