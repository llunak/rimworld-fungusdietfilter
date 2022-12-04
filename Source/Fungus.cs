using RimWorld;
using Verse;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace FungusDietFilter
{
    public static class FungusHelper
    {
        public static bool IsFungus(Thing t)
        {
            if(!ModsConfig.IdeologyActive)
                return false;
            if(t.def.IsFungus)
                return true;
            CompIngredients compIngredients = t.TryGetComp<CompIngredients>();
            if (compIngredients != null)
            {
                for (int j = 0; j < compIngredients.ingredients.Count; j++)
                    if(compIngredients.ingredients[j].IsFungus)
                        return true;
            }
            return false;
        }
    }

    public class SpecialThingFilterWorker_Fungus : SpecialThingFilterWorker
    {
        public override bool Matches(Thing t)
        {
            return FungusHelper.IsFungus(t);
        }

        public override bool CanEverMatch(ThingDef def)
        {
            if (!def.HasComp(typeof(CompIngredients)))
                return ModsConfig.IdeologyActive && def.IsFungus;
            return true;
        }
    }

    [StaticConstructorOnStartup]
    public class HarmonyPatches
    {
        static HarmonyPatches()
        {
            var harmony = new Harmony("llunak.FungusDietFilter");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }

    [HarmonyPatch(typeof(SpecialThingFilterWorker_Vegetarian))]
    public static class SpecialThingFilterWorker_Vegetarian_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(Matches))]
        public static bool Matches(ref bool __result, Thing t)
        {
            if(FungusHelper.IsFungus(t))
            {
                __result = false;
                return false;
            }
            return true;
        }
    }

    [DefOf]
    public static class SpecialThingFilterDefOf
    {
        public static SpecialThingFilterDef AllowVegetarian;
        public static SpecialThingFilterDef FDF_AllowFungus;

        static SpecialThingFilterDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(SpecialThingFilterDefOf));
        }
    }

    [HarmonyPatch(typeof(Dialog_BillConfig))]
    public static class Dialog_BillConfig_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(HiddenSpecialThingFilters), MethodType.Getter)]
        public static void HiddenSpecialThingFilters(ref IEnumerable<SpecialThingFilterDef> __result)
        {
            if(__result != null && __result.Contains(SpecialThingFilterDefOf.AllowVegetarian))
            {
                List<SpecialThingFilterDef> l = __result.ToList();
                l.Add(SpecialThingFilterDefOf.FDF_AllowFungus);
                __result = l;
            }
        }
    }

    [HarmonyPatch(typeof(ITab_Storage))]
    public static class ITab_Storage_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(HiddenSpecialThingFilters))]
        private static void HiddenSpecialThingFilters(ref IEnumerable<SpecialThingFilterDef> __result)
        {
            if(__result != null && __result.Contains(SpecialThingFilterDefOf.AllowVegetarian))
            {
                List<SpecialThingFilterDef> l = __result.ToList();
                l.Add(SpecialThingFilterDefOf.FDF_AllowFungus);
                __result = l;
            }
        }
    }
}
