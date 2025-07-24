using System.Collections.Generic;
using System.Xml;
using Verse;

namespace JPTools;

public class PatchOperationFindModById : PatchOperation
{
    private PatchOperation match;

    private List<string> mods;

    private PatchOperation nomatch;

    protected override bool ApplyWorker(XmlDocument xml)
    {
        var foundMod = false;
        foreach (var identifier in mods)
        {
            if (ModLister.GetActiveModWithIdentifier(identifier, true) == null)
            {
                continue;
            }

            foundMod = true;
            break;
        }

        if (foundMod)
        {
            if (match != null)
            {
                return match.Apply(xml);
            }
        }
        else if (nomatch != null)
        {
            return nomatch.Apply(xml);
        }

        return true;
    }
}