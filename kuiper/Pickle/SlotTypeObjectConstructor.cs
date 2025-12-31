using kuiper.Pickle;

using Razorvine.Pickle;

internal class SlotTypeObjectConstructor : IObjectConstructor
{
    public object construct(object[] args)
    {
        return (SlotType)args[0];
    }
}