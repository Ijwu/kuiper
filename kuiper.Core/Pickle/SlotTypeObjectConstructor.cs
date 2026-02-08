using kuiper.Core.Pickle;

using Razorvine.Pickle;

internal class SlotTypeObjectConstructor : IObjectConstructor
{
    public object construct(object[] args)
    {
        return (MultiDataSlotType)args[0];
    }
}