

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
    public class GraphicDataB : GraphicData
    {
        public void CopyFrom(GraphicData other)
        {
            this.texPath = other.texPath;
            this.graphicClass = other.graphicClass;
            this.shaderType = other.shaderType;
            this.color = other.color;
            this.colorTwo = other.colorTwo;
            this.colorThree = other.colorThree;
            this.drawSize = other.drawSize;
            this.onGroundRandomRotateAngle = other.onGroundRandomRotateAngle;
            this.drawRotated = other.drawRotated;
            this.allowFlip = other.allowFlip;
            this.flipExtraRotation = other.flipExtraRotation;
            this.shadowData = other.shadowData;
            this.damageData = other.damageData;
            this.linkType = other.linkType;
            this.linkFlags = other.linkFlags;
        }
        private void Init()
        {
            if (this.graphicClass == null)
            {
                this.cachedGraphic = null;
                return;
            }
            ShaderTypeDef cutout = this.shaderType;
            if (cutout == null)
            {
                cutout = ShaderTypeDefOf.Cutout;
            }
            Shader shader = cutout.Shader;
            this.cachedGraphic = GraphicDatabase.Get(this.graphicClass, this.texPath, shader, this.drawSize, this.color, this.colorTwo, this.colorThree, this, this.shaderParameters);
            if (this.onGroundRandomRotateAngle > 0.01f)
            {
                this.cachedGraphic = new Graphic_RandomRotated(this.cachedGraphic, this.onGroundRandomRotateAngle);
            }
            if (this.Linked)
            {
                this.cachedGraphic = GraphicUtility.WrapLinked(this.cachedGraphic, this.linkType);
            }
        }
        public Graphic GraphicColoredFor(Thing t)
        {
            if (t.DrawColor.IndistinguishableFrom(this.Graphic.Color) && t.DrawColorThree.IndistinguishableFrom(this.Graphic.ColorThree))
            {
                return this.Graphic;
            }
            return this.Graphic.GetColoredVersion(this.Graphic.Shader, t.DrawColor, t.DrawColorThree);

        }
        public Color colorThree = Color.white;
    }
}
