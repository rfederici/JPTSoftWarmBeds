

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;




using UnityEngine;
//using VerseBase;           // Material/Graphics handling functions are found here
using Verse;
//using Verse.AI;          // Needed when you do something with the AI


using RimWorld;
//using RimWorld.Planet;   // RimWorld specific functions for world creation
//using RimWorld.SquadAI;  // RimWorld specific functions for squad brains 

// Note: If the usings are not found, (red line under it,) look into the folder '/Source-DLLs' and follow the instructions in the text files


// Now the program starts:
namespace SoftWarmBeds
{
    public static class MaterialUtilityB : MaterialUtility
    {
        public static Color GetColorThree(this Material mat)
        {
            if (!mat.HasProperty(ShaderPropertyIDs.ColorThree))
            {
                return Color.white;
            }
            return mat.GetColor(ShaderPropertyIDs.ColorThree);
        }

    }

}
