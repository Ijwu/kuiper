using System;
using System.Collections.Generic;
using System.Text;

using Razorvine.Pickle;

namespace kuiper.Core.Pickle
{
    internal class HintObjectConstructor : IObjectConstructor
    {
        public object construct(object[] args)
        {
            return new MultiDataHint()
            {
                ReceivingPlayer = (int)args[0],
                FindingPlayer = (int)args[1],
                Location = (int)args[2],
                Item = (int)args[3],
                Found = (bool)args[4],
                Entrance = (string)args[5],
                ItemFlags = (int)args[6],
                Status = (MultiDataHintStatus)args[7]
            };
        }
    }
}
