using System;
using System.Collections.Generic;
using System.Text;

using Razorvine.Pickle;

namespace kuiper.Core.Pickle
{
    internal class HintStatusObjectConstructor : IObjectConstructor
    {
        public object construct(object[] args)
        {
            return (MultiDataHintStatus)args[0];
        }
    }
}
