using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Untitled_Part_Failure_Mod
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, true)]
    public class Randomiser : MonoBehaviour
    {
        System.Random r = new System.Random();
        public static Randomiser instance;

        private void Awake()
        {
            instance = this;
            DontDestroyOnLoad(this);
        }

        public double NextDouble()
        {
            return r.NextDouble();
        }

        public int RandomInteger(int min, int max)
        {
            return r.Next(min, max);
        }
    }
}
