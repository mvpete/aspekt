﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aspekt.Contracts
{
    public interface IContractEvaluator
    {
        bool Evaluate(object o);

    }
}
