using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace OhScrap
{
    public class RemoteTechWrapper
    {
        private static readonly BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
        public static RemoteTechWrapper instance;
        private static Assembly RT = null;
        private static Type RTAntenna = null;
      
        
        public static bool available
        {
            get
            {
                bool loaded = (RT != null);
                if (!loaded) 
                { 
                    for (int i = 0; i < AssemblyLoader.loadedAssemblies.Count; i++)
                    {
                        var Asm = AssemblyLoader.loadedAssemblies[i];
                        if (Asm.dllName == "RemoteTech")
                        {
                            loaded = true;
                            RTAntenna  = Asm.assembly.GetType("RemoteTech.Modules.ModuleRTAntenna");
                            RT = Asm.assembly;
                        }
                    }
                    
                }
                return loaded;
            }
        }

    

        public static bool getRTBrokenStatus(PartModule p)
        {
           
          return GetReflectionValue<bool>(p, "IsRTBroken");
          
        }
        public static void setRTBrokenStatus(PartModule p, bool value)
        {
                SetReflectionValue<bool>(p, "IsRTBroken", value);
                SetReflectionValue<bool>(p, "IsRTActive", value);
                if (value == false)
                {
                    p.GetType().GetMethod("OnConnectionRefresh").Invoke(p, null);
                }
        }
        public static bool hasConnectionToKSC(Guid vesselGUID)
        {
              object[] parametersArray = new object[1];
              parametersArray[0] = vesselGUID;
              ParameterInfo[] parameters = RT.GetType("RemoteTech.API.API").GetMethod("HasConnectionToKSC").GetParameters();
              return (bool)RT.GetType("RemoteTech.API.API").GetMethod("HasConnectionToKSC").Invoke(RT, parameters.Length == 0 ? null : parametersArray );
        }



        //Relfection Helpers. 
        private static T GetReflectionValue<T>(PartModule p, string field_name)
        {
            return (T)instance.GetType().GetField(field_name, flags).GetValue(instance);
        }
        private static void SetReflectionValue<T>(PartModule p, string value_name, T value)
        {
           p.GetType().GetField(value_name, flags).SetValue(p, value);
        }
        
    }

    
}
