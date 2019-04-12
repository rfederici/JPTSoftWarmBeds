

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
    public class GraphicB : Graphic
    {

        public Color ColorThree
        {
            get
            {
                return this.colorThree;
            }
        }
        public override virtual Graphic GetColoredVersion(Shader newShader, Color newColor, Color newColorThree)
        {
            Log.ErrorOnce("CloneColored not implemented on this subclass of Graphic: " + base.GetType().ToString(), 66300, false);
            return BaseContent.BadGraphic;
        }
        public virtual Graphic GetCopy(Vector2 newDrawSize)
        {
            return GraphicDatabase.Get(base.GetType(), this.path, this.Shader, newDrawSize, this.color, this.colorThree);
        }
        public Color colorTwo = Color.white;
    }
}
