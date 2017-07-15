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
    public class PartTracker : ScenarioModule
    {
        public override void OnSave(ConfigNode node)
        {
            ConfigNode temp = node.GetNode("UPFMTracker");
            if (temp == null)
            {
                node.AddNode("UPFMTracker");
                temp = node.GetNode("UPFMTracker");
            }
            if (EditorWarnings.instance.randomisation.Count() == 0) return;
            foreach (var v in EditorWarnings.instance.randomisation)
            {
                if (v.Key == null) continue;
                if (v.Key == "") continue;
                ConfigNode cn = new ConfigNode("PART");
                cn.SetValue("ID",v.Key,true);
                cn.SetValue("RandomFactor", v.Value, true);
                temp.AddNode(cn);
            }
        }

        public override void OnLoad(ConfigNode node)
        {
            ConfigNode temp = node.GetNode("UPFMTracker");
            if (temp == null) return;
            EditorWarnings.instance.randomisation.Clear();
            ConfigNode[] nodes = temp.GetNodes("PART");
            if (nodes.Count() == 0) return;
            string s;
            float f;
            for(int i = 0; i<nodes.Count(); i++)
            {
                ConfigNode cn = nodes.ElementAt(i);
                s = cn.GetValue("ID");
                if(!float.TryParse(cn.GetValue("RandomFactor"),out f)) continue;
                EditorWarnings.instance.randomisation.Add(s, f);
            }
        }
    }
}
