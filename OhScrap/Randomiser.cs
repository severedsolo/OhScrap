using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace OhScrap
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, true)]
    public class Randomiser : MonoBehaviour
    {
        System.Random r = new System.Random();
        public static Randomiser instance;
        List<double> shuffleBag1 = new List<double>();
        List<double> shuffleBag2 = new List<double>();
        List<double> shuffleBag3 = new List<double>();

        private void Awake()
        {
            instance = this;
            DontDestroyOnLoad(this);
        }

        public double NextDouble()
        {
            int i = RandomInteger(1, 4);
            double d = 1.0f;
            int remaining = 0;
            switch (i)
            {
                case 1:
                    if (shuffleBag1.Count() < 20) PopulateShuffleBag(shuffleBag1, i);
                    d = shuffleBag1.ElementAt(RandomInteger(0, shuffleBag1.Count()));
                    shuffleBag1.Remove(d);
                    remaining = shuffleBag1.Count();
                    break;
                case 2:
                    if (shuffleBag2.Count() < 20) PopulateShuffleBag(shuffleBag2, i);
                    d = shuffleBag2.ElementAt(RandomInteger(0, shuffleBag2.Count()));
                    shuffleBag2.Remove(d);
                    remaining = shuffleBag2.Count();
                    break;
                case 3:
                    if (shuffleBag3.Count() < 20) PopulateShuffleBag(shuffleBag3, i);
                    d = shuffleBag3.ElementAt(RandomInteger(0, shuffleBag3.Count()));
                    shuffleBag3.Remove(d);
                    remaining = shuffleBag3.Count();
                    break;
            }
#if DEBUG
            Debug.Log("[UPFM]: Shufflebag returned " + d + " from shuffle bag " + i + " (" + remaining + " left)");
#endif
            return d;
        }

        private void PopulateShuffleBag(List<double> shuffleBag, int i)
        {
            for (double d = 0.01f; d < 1.0; d += 0.01f)
            {
                if (shuffleBag.Contains(d)) continue;
                shuffleBag.Add(d);
            }
#if DEBUG
            Debug.Log("[UPFM]: Refilled shufflebag " + i);
#endif
        }

        public int RandomInteger(int min, int max)
        {
            int i = r.Next(min, max);
#if DEBUG
            Debug.Log("[UPFM]: RandomInteger returned " + i);
#endif 
            return i;
        }
    }
}
