using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace CruiserSkinNARPatch
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInDependency("EffMapis.CruiserSkin", BepInDependency.DependencyFlags.HardDependency)]
    public class CruiserSkinNARPatch : BaseUnityPlugin
    {
        public static CruiserSkinNARPatch Instance { get; private set; } = null!;
        internal new static ManualLogSource Logger { get; private set; } = null!;
        internal static Harmony? Harmony { get; set; }

        private void Awake()
        {
            Logger = base.Logger;
            Instance = this;
            Patch();
            Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} has loaded!");
        }

        internal static void Patch()
        {
            Harmony ??= new Harmony(MyPluginInfo.PLUGIN_GUID);

            Logger.LogDebug("Patching...");

            Harmony.PatchAll();
            Logger.LogDebug("Finished patching!");
        }

        internal static void Unpatch()
        {
            Logger.LogDebug("Unpatching...");

            Harmony?.UnpatchSelf();

            Logger.LogDebug("Finished unpatching!");
        }
    }
}
namespace CruiserSkinNARPatch.Patches
{
    [HarmonyPatch(typeof(VehicleController))]
    public class Patching
    {
        private static Texture2D main;
        private static Texture2D destroyed;
        [HarmonyPatch(typeof(VehicleController), "Start")] //Patch VehicleController when the Start method is ran
        [HarmonyPrefix]
        private static void StartPatch(VehicleController __instance) //Runs when vehicle is created
        {
            main = CruiserSkin.Plugin.defaults[0];
            destroyed = CruiserSkin.Plugin.destroyeds[0];
            Transform[] componentsInChildren = ((Component)__instance).GetComponentsInChildren<Transform>();
            foreach (Transform val in componentsInChildren)
            {
                string TheObjectName = ((Object)val).name;
                //CruiserSkinNARPatch.Logger.LogDebug(TheObjectName);
                /*Wheel Structure is 
                    Wheels
                    ├── BackWheelAxisContainer
                    │   ├── BackLeftTire
                    │   └── BackRightTire
                    └── FrontWheelAxisContainer
                        ├── FrontLeftTire
                        └── FrontRightTire*/
                if (TheObjectName == "Wheels")
                {
                    foreach (MeshRenderer WheelMesh in val.GetComponentsInChildren<MeshRenderer>())
                    {
                        ((Renderer)((Component)WheelMesh).GetComponent<MeshRenderer>()).materials[0].mainTexture = (Texture)(object)main;
                    }
                }
                else if (TheObjectName == "CabinWindowContainer")
                {
                    ((Renderer)((Component)val).GetComponentInChildren<MeshRenderer>()).materials[0].mainTexture = (Texture)(object)main;
                }
                else if (TheObjectName == "DriverSeatContainer")
                {
                    ((Renderer)((Component)val).GetComponentInChildren<MeshRenderer>()).materials[0].mainTexture = (Texture)(object)main;
                }
            }
        }
        [HarmonyPatch(typeof(VehicleController), "DestroyCar")] //Patch VehicleController when the DestroyCar is ran
        [HarmonyPostfix]
        private static void DestroyCarPatch(VehicleController __instance)
        {
            Transform[] componentsInChildren = ((Component)__instance).GetComponentsInChildren<Transform>();
            foreach (Transform val in componentsInChildren)
            {
                string TheObjectName = ((Object)val).name;
                //Tires are not in the destroyed car so we don't care about the tires
                if (TheObjectName == "CabinWindowContainer")
                {
                    ((Renderer)((Component)val).GetComponentInChildren<MeshRenderer>()).materials[0].mainTexture = (Texture)(object)destroyed;
                }
                else if (TheObjectName == "DriverSeatContainer")
                {
                    ((Renderer)((Component)val).GetComponentInChildren<MeshRenderer>()).materials[0].mainTexture = (Texture)(object)destroyed;
                }
            }
        }
    }
}