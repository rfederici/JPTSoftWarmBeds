using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace SoftWarmBeds
{ 
    public class ITab_Bedding : ITab_Storage
    {
        public ITab_Bedding()
        {
            labelKey = "TabBedding";
        }

        protected override IStoreSettingsParent SelStoreSettingsParent
        {
            get
            {
                IStoreSettingsParent selStoreSettingsParent = base.SelStoreSettingsParent;
                if (selStoreSettingsParent != null)
                {
                    return selStoreSettingsParent;
                }
                Building_SoftWarmBed building_SoftWarmBed = base.SelObject as Building_SoftWarmBed;
                if (building_SoftWarmBed != null)
                {
                    return base.GetThingOrThingCompStoreSettingsParent(building_SoftWarmBed);
                }
                return null;
            }
        }

        protected override bool IsPrioritySettingVisible
        {
            get
            {
                return false;
            }
        }
    }
}
