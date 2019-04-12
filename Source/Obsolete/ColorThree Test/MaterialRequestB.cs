

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
    public struct MaterialRequestB : IEquatable<MaterialRequest>
    {
        public MaterialRequest(Texture2D tex)
        {
            this.shader = ShaderDatabase.Cutout;
            this.mainTex = tex;
            this.color = Color.white;
            this.colorTwo = Color.white;
            this.colorThree = Color.white;
            this.maskTex = null;
            this.renderQueue = 0;
            this.shaderParameters = null;
        }
        public MaterialRequest(Texture2D tex, Shader shader)
        {
            this.shader = shader;
            this.mainTex = tex;
            this.color = Color.white;
            this.colorTwo = Color.white;
            this.colorThree = Color.white;
            this.maskTex = null;
            this.renderQueue = 0;
            this.shaderParameters = null;
        }
        public MaterialRequest(Texture2D tex, Shader shader, Color color)
        {
            this.shader = shader;
            this.mainTex = tex;
            this.color = color;
            this.colorTwo = Color.white;
            this.colorThree = Color.white;
            this.maskTex = null;
            this.renderQueue = 0;
            this.shaderParameters = null;
        }
        public override int GetHashCode()
        {
            int seed = 0;
            seed = Gen.HashCombine<Shader>(seed, this.shader);
            seed = Gen.HashCombineStruct<Color>(seed, this.color);
            seed = Gen.HashCombineStruct<Color>(seed, this.colorTwo);
            seed = Gen.HashCombineStruct<Color>(seed, this.colorThree);
            seed = Gen.HashCombine<Texture2D>(seed, this.mainTex);
            seed = Gen.HashCombine<Texture2D>(seed, this.maskTex);
            seed = Gen.HashCombineInt(seed, this.renderQueue);
            return Gen.HashCombine<List<ShaderParameter>>(seed, this.shaderParameters);
        }
        public bool Equals(MaterialRequest other)
        {
            return other.shader == this.shader && other.mainTex == this.mainTex && other.color == this.color && other.colorTwo == this.colorTwo && other.colorTthree == this.colorThree && other.maskTex == this.maskTex && other.renderQueue == this.renderQueue && other.shaderParameters == this.shaderParameters;
        }
        public override string ToString()
        {
            return string.Concat(new string[]
            {
                "MaterialRequest(",
                this.shader.name,
                ", ",
                this.mainTex.name,
                ", ",
                this.color.ToString(),
                ", ",
                this.colorTwo.ToString(),
                ", ",
                this.colorThree.ToString(),
                ", ",
                this.maskTex.ToString(),
                ", ",
                this.renderQueue.ToString(),
                ")"
            });
        }

        public Shader shader;

        public Texture2D mainTex;

        public Color color;

        public Color colorTwo;

        public Texture2D maskTex;

        public int renderQueue;

        public List<ShaderParameter> shaderParameters;

        public Color colorThree;
    }
}
