using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;

namespace OhScrap
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    class Logger : MonoBehaviour
    {
        public List<string> logs = new List<string>();
        public static Logger instance;

        public void Awake()
        {
            logs.Add("Using Oh Scrap 1.4b4");
            instance = this;
        }

        public void Log(string s)
        {
            logs.Add(s);
            Debug.Log("[OhScrap]: " + s);
        }

        public void OnDisable()
        {
            if (logs.Count() == 0) return;
            string path = KSPUtil.ApplicationRootPath + "/GameData/Severedsolo/OhScrap/Logs/" + DateTime.Now.ToString("yyyyMMddHHmmss")+".txt";
            using (StreamWriter writer = File.AppendText(path))
            {
                foreach (string s in logs)
                {
                    writer.WriteLine(s);
                }
            }
        }
    }
}

