

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;




using UnityEngine;

using Verse;
//using Verse.AI;          // Needed when you do something with the AI


using RimWorld;
//using RimWorld.Planet;   // RimWorld specific functions for world creation
//using RimWorld.SquadAI;  // RimWorld specific functions for squad brains 

// Note: If the usings are not found, (red line under it,) look into the folder '/Source-DLLs' and follow the instructions in the text files


// Now the program starts:
namespace SoftWarmBeds
{
    public class CompProperties_MakeableBed : CompProperties
    {
        public CompProperties_MakeableBed()
        {
            this.compClass = typeof(CompMakeableBed);
        }
    
    public ThingDef blanketDef;

    public StorageSettings defaultStorageSettings;
    
    }
}