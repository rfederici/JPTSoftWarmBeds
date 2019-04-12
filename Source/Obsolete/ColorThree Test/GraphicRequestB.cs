

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
    public struct GraphicRequestB : IEquatable<Verse.GraphicRequest>
    {
        public GraphicRequest(Type graphicClass, string path, Shader shader, Vector2 drawSize, Color color, Color colorTwo, GraphicData graphicData, int renderQueue, List<ShaderParameter> shaderParameters)
        {
            this.graphicClass = graphicClass;
            this.path = path;
            this.shader = shader;
            this.drawSize = drawSize;
            this.color = color;
            this.colorTwo = colorTwo;
            this.colorThree = colorThree;
            this.graphicData = graphicData;
            this.renderQueue = renderQueue;
            this.shaderParameters = ((!shaderParameters.NullOrEmpty<ShaderParameter>()) ? shaderParameters : null);
        }

        public override int GetHashCode()
        {
            if (this.path == null)
            {
                this.path = BaseContent.BadTexPath;
            }
            int seed = 0;
            seed = Gen.HashCombine<Type>(seed, this.graphicClass);
            seed = Gen.HashCombine<string>(seed, this.path);
            seed = Gen.HashCombine<Shader>(seed, this.shader);
            seed = Gen.HashCombineStruct<Vector2>(seed, this.drawSize);
            seed = Gen.HashCombineStruct<Color>(seed, this.color);
            seed = Gen.HashCombineStruct<Color>(seed, this.colorTwo);
            seed = Gen.HashCombine<GraphicData>(seed, this.graphicData);
            seed = Gen.HashCombine<int>(seed, this.renderQueue);
            return Gen.HashCombine<List<ShaderParameter>>(seed, this.shaderParameters);
        }

        public override bool Equals(object obj)
        {
            return obj is GraphicRequest && this.Equals((GraphicRequest)obj);
        }

        public bool Equals(GraphicRequest other)
        {
            return this.graphicClass == other.graphicClass && this.path == other.path && this.shader == other.shader && this.drawSize == other.drawSize && this.color == other.color && this.colorTwo == other.colorTwo && this.colorThree == other.colorThree && this.graphicData == other.graphicData && this.renderQueue == other.renderQueue && this.shaderParameters == other.shaderParameters;
        }

        public static bool operator ==(GraphicRequest lhs, GraphicRequest rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(GraphicRequest lhs, GraphicRequest rhs)
        {
            return !(lhs == rhs);
        }

        public Type graphicClass;

        public string path;

        public Shader shader;

        public Vector2 drawSize;

        public Color color;

        public Color colorTwo;

        public Color colorThree;

        public GraphicData graphicData;

        public int renderQueue;

        public List<ShaderParameter> shaderParameters;
    }
}
