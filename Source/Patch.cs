using HarmonyLib;
using Verse;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;

namespace PerspectivePaths
{   
    [StaticConstructorOnStartup]
    static class Mod_PerspectivePaths
    {
        public const string areaName = "InvertEdges";
        static HashSet<IntVec3> edgeRegistry = new HashSet<IntVec3>();
        public static Dictionary<Map, Area> areaCache = new Dictionary<Map, Area>();
        public static Texture2D iconHardEdgedArea = ContentFinder<Texture2D>.Get("UI/HardEdgedArea", true),
            iconHardEdgedAreaClear = ContentFinder<Texture2D>.Get("UI/HardEdgedAreaClear", true);
        static Mod_PerspectivePaths()
        {
            new Harmony("Owlchemist.PerspectivePaths").PatchAll();
        }
    }

    [HarmonyPatch(typeof(World), nameof(World.FinalizeInit))]
	class ClearCache
    {
        static void Postfix()
        {
            Mod_PerspectivePaths.areaCache = new Dictionary<Map, Area>();
        }
    }

    [HarmonyPatch(typeof(Map), nameof(Map.FinalizeInit))]
	class Patch_FinalizeInit
    {
        static void Postfix(Map __instance)
        {
            if (__instance.areaManager.GetLabeled(Mod_PerspectivePaths.areaName) == null) __instance.areaManager.areas.Add(new Area_InvertEdges(__instance.areaManager, Mod_PerspectivePaths.areaName));
            Mod_PerspectivePaths.areaCache.Add(__instance, __instance.areaManager.GetLabeled(Mod_PerspectivePaths.areaName));
        }
    }
    
    [HarmonyPatch(typeof(SectionLayer_Terrain), nameof(SectionLayer_Terrain.Regenerate))]
	static class Patch_SectionLayer_Terrain
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            int foundOffset = -1;
            var label = generator.DefineLabel();
            foreach (var code in instructions)
            {
                //Grab label
                if (foundOffset != -1 && ++foundOffset < 3)
                {
                    if (foundOffset == 1)
                    {
                        if (code.opcode == OpCodes.Brtrue_S) label = (Label)code.operand;
                    }
                    //Add our injected code
                    else if (foundOffset == 2)
                    {
                        yield return new CodeInstruction(OpCodes.Ldloc_3);
                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(SectionLayer), "get_Map"));
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Patch_SectionLayer_Terrain), nameof(Patch_SectionLayer_Terrain.CheckRegistry)));
                        yield return new CodeInstruction(OpCodes.Brfalse_S, label);
                    }
                }
                //Pass thrugh and sniff out the vanilla codes
                yield return code;
                if (foundOffset == -1 && code.opcode == OpCodes.Callvirt && code.OperandIs(AccessTools.Method(typeof(HashSet<CellTerrain>), nameof(HashSet<CellTerrain>.Contains))))
                {
                    foundOffset++;
                }
            }
        }

        public static bool CheckRegistry(IntVec3 cell, Map map)
        {
            return !Mod_PerspectivePaths.areaCache.TryGetValue(map, out Area area) || !area.innerGrid[cell];
        }
    }
}