using System.Linq;
using System.Xml;
using Verse;

namespace JPTools;

public class PatchOperationCopy : PatchOperationPathed
{
    protected string append;

    protected string toXpath;

    protected override bool ApplyWorker(XmlDocument xml)
    {
        var result = false;
        var enumerable = xml.SelectNodes(toXpath)!.Cast<XmlNode>();
        if (enumerable.EnumerableNullOrEmpty())
        {
            return false;
        }

        foreach (var xmlNode in xml.SelectNodes(xpath)!.Cast<XmlNode>().ToArray())
        {
            foreach (var xmlNode2 in enumerable)
            {
                xmlNode2.InnerXml = xmlNode.InnerXml;
                if (append != null)
                {
                    xmlNode2.InnerText += append;
                }

                result = true;
            }
        }

        return result;
    }
}