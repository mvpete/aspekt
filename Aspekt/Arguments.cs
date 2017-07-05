using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aspekt
{
    public class Arguments : List<object>
    {
        /// Just a list of objects
        /// 
        public Arguments(Int32 i)
            : base(i)
        {
        }
        public Arguments()
        {
        }
        public static readonly Arguments Empty = new Arguments();
    }
}
