using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Untitled_Part_Failure_Mod
{
    class DontRecoverMe : PartModule
    {
        public override void OnSave(ConfigNode node)
        {
            node.AddValue("MM_DYNAMIC", true);
        }
    }
}
