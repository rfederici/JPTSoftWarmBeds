using UnityEngine;
using HugsLib;
using RimWorld;
using Verse;

namespace SoftWarmBeds
{
    internal class ModBaseSoftWarmBeds : ModBase
    {
        private static void Inject()
        {
            SoftWarmBeds_SpecialInjector softWarmBeds_SpecialInjector = new SoftWarmBeds_SpecialInjector();
            softWarmBeds_SpecialInjector.Inject();
        }

        public override string ModIdentifier
        {
            get
            {
                return "JPT_SoftWarmBeds";
            }
        }

        public override void Initialize()
        {
            ModBaseSoftWarmBeds.Inject();
        }
    }
}
