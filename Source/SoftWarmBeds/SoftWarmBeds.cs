using System.Reflection;
using HarmonyLib;
using Verse;

namespace SoftWarmBeds;

[StaticConstructorOnStartup]
public static class SoftWarmBeds
{
    static SoftWarmBeds()
    {
        new SoftWarmBeds_SpecialInjector().Inject();
        new Harmony("Mlie.SoftWarmBeds").PatchAll(Assembly.GetExecutingAssembly());
    }
}