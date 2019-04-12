

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
    [HasDebugOutput]
    public static class GraphicDatabaseB
    {
        public static Graphic Get<T>(string path, Shader shader, Vector2 drawSize, Color color, Color colorTwo, Color colorThree) where T : Graphic, new()
        {
            GraphicRequest req = new GraphicRequest(typeof(T), path, shader, drawSize, color, colorTwo, colorThree, null, 0, null);
            return GraphicDatabase.GetInner<T>(req);
        }

        public static Graphic Get<T>(string path, Shader shader, Vector2 drawSize, Color color, Color colorTwo, Color colorThree, GraphicData data) where T : Graphic, new()
        {
            GraphicRequest req = new GraphicRequest(typeof(T), path, shader, drawSize, color, colorTwo, colorThree, data, 0, null);
            return GraphicDatabase.GetInner<T>(req);
        }

        public static Graphic Get(Type graphicClass, string path, Shader shader, Vector2 drawSize, Color color, Color colorTwo, Color colorThree)
        {
            return GraphicDatabase.Get(graphicClass, path, shader, drawSize, color, colorTwo, colorThree, null, null);
        }

        public static Graphic Get(Type graphicClass, string path, Shader shader, Vector2 drawSize, Color color, Color colorTwo, GraphicData data, List<ShaderParameter> shaderParameters)
        {
            GraphicRequest graphicRequest = new GraphicRequest(graphicClass, path, shader, drawSize, color, colorTwo, colorThree, data, 0, shaderParameters);
            if (graphicRequest.graphicClass == typeof(Graphic_Single))
            {
                return GraphicDatabase.GetInner<Graphic_Single>(graphicRequest);
            }
            if (graphicRequest.graphicClass == typeof(Graphic_Terrain))
            {
                return GraphicDatabase.GetInner<Graphic_Terrain>(graphicRequest);
            }
            if (graphicRequest.graphicClass == typeof(Graphic_Multi))
            {
                return GraphicDatabase.GetInner<Graphic_Multi>(graphicRequest);
            }
            if (graphicRequest.graphicClass == typeof(Graphic_Mote))
            {
                return GraphicDatabase.GetInner<Graphic_Mote>(graphicRequest);
            }
            if (graphicRequest.graphicClass == typeof(Graphic_Random))
            {
                return GraphicDatabase.GetInner<Graphic_Random>(graphicRequest);
            }
            if (graphicRequest.graphicClass == typeof(Graphic_Flicker))
            {
                return GraphicDatabase.GetInner<Graphic_Flicker>(graphicRequest);
            }
            if (graphicRequest.graphicClass == typeof(Graphic_Appearances))
            {
                return GraphicDatabase.GetInner<Graphic_Appearances>(graphicRequest);
            }
            if (graphicRequest.graphicClass == typeof(Graphic_StackCount))
            {
                return GraphicDatabase.GetInner<Graphic_StackCount>(graphicRequest);
            }
            try
            {
                return (Graphic)GenGeneric.InvokeStaticGenericMethod(typeof(GraphicDatabase), graphicRequest.graphicClass, "GetInner", new object[]
                {
                    graphicRequest
                });
            }
            catch (Exception ex)
            {
                Log.Error(string.Concat(new object[]
                {
                    "Exception getting ",
                    graphicClass,
                    " at ",
                    path,
                    ": ",
                    ex.ToString()
                }), false);
            }
            return BaseContent.BadGraphic;
        }
    }
}
