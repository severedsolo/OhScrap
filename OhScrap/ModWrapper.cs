using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace OhScrap
{
    //Collection of small helper classes used to support other mods via reflection. 
    public class ModWrapper
    {
        private static readonly BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

        
        public class RemoteTechWrapper
        {
            private static Assembly RT = null;
            private static bool tried = false;
            public static bool available
            {
                get
                {
                    bool loaded = (RT != null);
                    if (!loaded && !tried) 
                    {
                        for (int i = 0; i < AssemblyLoader.loadedAssemblies.Count; i++)
                        {
                            var Asm = AssemblyLoader.loadedAssemblies[i];
                            if (Asm.dllName == "RemoteTech")
                            {
                                loaded = true;
                                //RTAntenna = Asm.assembly.GetType("RemoteTech.Modules.ModuleRTAntenna");
                                RT = Asm.assembly;
                                Debug.Log("[OhScrap]: RemoteTech Detected.");
                            }
                        }
                        tried = true;
                    }
                    return loaded;
                }
            }
            
            public static void SetRTBrokenStatus(PartModule p, bool value)
            {
                SetReflectionField<bool>(p, "IsRTBroken", value);
                SetReflectionField<bool>(p, "IsRTActive", value);
                if (value == false)
                {
                    p.GetType().GetMethod("OnConnectionRefresh").Invoke(p, null);
                }
            }
            public static bool HasConnectionToKSC(Guid vesselGUID)
            {
                object[] parametersArray = new object[1];
                parametersArray[0] = vesselGUID;
                ParameterInfo[] parameters = RT.GetType("RemoteTech.API.API").GetMethod("HasConnectionToKSC").GetParameters();
                return (bool)RT.GetType("RemoteTech.API.API").GetMethod("HasConnectionToKSC").Invoke(RT, parameters.Length == 0 ? null : parametersArray);
            }
            public static bool GetAntennaDeployed(PartModule p)
            {
                if(GetReflectionProperty<bool>(p, "CanAnimate"))
                { 
                    return (bool)p.GetType().GetProperty("AnimOpen", flags).GetValue(p, null);
                }else
                {
                    return true;
                }
            }
        }
     
        public class FerramWrapper
        {
            private static Assembly FAR = null;
            private static bool tried = false;

            public static bool available
            {
                get
                {
                    bool loaded = (FAR != null);
                    if (!loaded && !tried)
                    {
                        for (int i = 0; i < AssemblyLoader.loadedAssemblies.Count; i++)
                        {
                            var Asm = AssemblyLoader.loadedAssemblies[i];
                            if (Asm.dllName == "FerramAerospaceResearch")
                            {
                                loaded = true;
                                FAR = Asm.assembly;
                                Debug.Log("[OhScrap]: FAR Detected.");
                            }
                        }
                        tried = true;
                    }
                    return loaded;
                }
            }
            public static void FailControlSurface(PartModule p)
            {
                
            }
        }

        //Relfection Helpers. 
        private static T GetReflectionField<T>(PartModule p, string field_name)
        {
            return (T)p.GetType().GetField(field_name, flags).GetValue(p);
        }
        private static void SetReflectionField<T>(PartModule p, string value_name, T value)
        {
            p.GetType().GetField(value_name, flags).SetValue(p, value);
        }
        private static T GetReflectionProperty<T>(PartModule p, String property)
        {
            return (T)p.GetType().GetProperty(property, flags).GetValue(p, null);
        }

    }
}
