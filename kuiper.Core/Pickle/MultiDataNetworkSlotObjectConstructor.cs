using kuiper.Core.Pickle;

using Razorvine.Pickle;

internal class MultiDataNetworkSlotObjectConstructor : IObjectConstructor
{
    public object construct(object[] args)
    {
        var groupMembers = Array.ConvertAll<object, long>((object[])args[3], Convert.ToInt64);
        return new MultiDataNetworkSlot()
        {
            Name = (string)args[0],
            Game = (string)args[1],
            Type = (MultiDataSlotType)args[2],
            GroupMembers = groupMembers
        };
    }
}