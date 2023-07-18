using RimWorld;
using Verse;

namespace SoftWarmBeds;

public class ITab_Bedding : ITab_Storage
{
    public ITab_Bedding()
    {
        labelKey = "TabBedding";
    }

    protected override IStoreSettingsParent SelStoreSettingsParent =>
        (SelObject as Thing).TryGetComp<CompMakeableBed>();

    protected override bool IsPrioritySettingVisible => false;
}