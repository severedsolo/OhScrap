using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Reflection;

namespace Untitled_Part_Failure_Mod
{
    [KSPAddon(KSPAddon.Startup.EditorAny, false)]
    class EditorWarnings : MonoBehaviour
    {
        public Dictionary<Part,int> damagedParts = new Dictionary<Part, int>();
        public bool display = false;
        bool dontBother = false;
        public static EditorWarnings instance;
        Rect Window = new Rect(500, 100, 240, 50);

        private void Awake()
        {
            instance = this;
        }

        private void OnGUI()
        {
            if (!HighLogic.CurrentGame.Parameters.CustomParams<UPFMSettings>().safetyWarning) return;
            if (dontBother) return;
            if (!display) return;
            if (damagedParts.Count == 0) return;
            Window = GUILayout.Window(98399854, Window, GUIDisplay, "UPFM", GUILayout.Width(200));
        }
        void GUIDisplay(int windowID)
        {
            int counter = 0;
            GUILayout.Label("WARNING: The following parts are above the safety threshold");
            foreach (var v in damagedParts)
            {
                if (v.Key == null) continue;
                GUILayout.Label(v.Key.name + ": " + v.Value+"%");
                counter++;
            }
            if (counter == 0) display = false;
            if(GUILayout.Button("Dismiss"))
            {
                display = false;
            }
            if(GUILayout.Button("Stop Bothering Me"))
            {
                display = false;
                dontBother = true;
            }
            GUI.DragWindow();
        }

        private void OnDestroy()
        {
            display = false;
        }
    }
}
