using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aspekt
{
    [AttributeUsage(AttributeTargets.Method)]
    public class IgnoreWarningAttribute : Attribute
    {
    }
}
