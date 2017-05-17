using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Untitled_Part_Failure_Mod
{
    class TankFailureModule : BaseFailureModule
    {
        PartResource leaking;
        [KSPField(isPersistant = true, guiActive = false)]
        public string leakingName = "None";
        private readonly string savedFile = KSPUtil.ApplicationRootPath + "/GameData/UntitledFailures/MM Patches/DontLeak.cfg";
        System.Random r = new System.Random();
        [KSPField(isPersistant = true, guiActive = false)]
        public string FailureType = "None";

        protected override void FailPart()
        {
            if (FailureType == "None")
            {
                switch (r.Next(1, 3))
                {
                    case 1:
                        FailureType = "Leak";
                        break;
                    case 2:
                        FailureType = "Valve";
                        break;
                    default:
                        return;
                }
            }
            if (leaking == null)
            {
                if (leakingName != "None")
                {
                    leaking = part.Resources[leakingName];
                    return;
                }
                List<PartResource> potentialLeaks = part.Resources.ToList();
                if (potentialLeaks.Count == 0) return;
                ConfigNode cn = ConfigNode.Load(savedFile);
                if (cn != null)
                {
                    ConfigNode[] blackListNode = cn.GetNodes("BLACKLISTED");
                    if (blackListNode.Count() > 0)
                    {
                        for (int i = 0; i < blackListNode.Count(); i++)
                        {
                            ConfigNode node = blackListNode.ElementAt(i);
                            for (int p = 0; p < potentialLeaks.Count(); p++)
                            {
                                PartResource pr = potentialLeaks.ElementAt(i);
                                if (pr.resourceName == node.GetValue("name")) potentialLeaks.Remove(pr);
                            }
                        }
                        if (potentialLeaks.Count == 0)
                        {
                            FailureType = "None";
                            leaking = null;
                            leakingName = "None";
                            hasFailed = false;
                            willFail = false;
                            Debug.Log("[UPFM]: "+part.name + "has no resources that could fail. Failure aborted");
                            return;
                        }
                    }
                }
                switch (FailureType)
                {
                    case "Leak":
                        leaking = potentialLeaks.ElementAt(r.Next(0, potentialLeaks.Count()));
                        leakingName = leaking.resourceName;
                        Debug.Log("[UPFM}: " + leaking.resourceName + " started leaking from " + part.name);
                        ScreenMessages.PostScreenMessage("A tank of " + leaking.resourceName + " started to leak!");
                        break;
                    case "Valve":
                        leaking = potentialLeaks.ElementAt(r.Next(0, potentialLeaks.Count()));
                        leakingName = leaking.resourceName;
                        Debug.Log("[UPFM}: The flow valve on " + leaking.resourceName + " siezed on " + part.name);
                        ScreenMessages.PostScreenMessage("The " + leaking.resourceName + " valve has failed on " + part.name + "!");
                        break;
                }
            }

            switch (FailureType)
            {
                case "Leak":
                    leaking.amount = leaking.amount * 0.999f;
                    break;
                case "Valve":
                    leaking._flowState = false;
                    break;
            }
        }

        protected override void RepairPart()
        {
            if (FailureType == "Valve")
            {
                leaking._flowState = true;
            }
        }
    }
}
    
 

