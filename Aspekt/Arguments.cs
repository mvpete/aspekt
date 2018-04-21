using System;
using System.Collections.Generic;
using System.Linq;

namespace Aspekt
{
    public class Argument
    {
        public string Name { get; set; }
        public Type Type { get; set; }

        public object Value { get; set; }

        public Argument(string name, object value)
        {
            Name = name;
            Type = value?.GetType();
            Value = value;
        }
    }


    public class Arguments : List<Argument>
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

        public IEnumerable<object> Values
        {
            get
            {
                return this.Select(a => a.Value);
            }
        }

        public object GetArgumentByIndex(int index)
        {
            if (index >= Count)
                throw new IndexOutOfRangeException($"Argument '{index}' out of range");
            return this[index].Value;
        }

        public object GetArgumentValueByName(String argumentName)
        {
            var argument = this.FirstOrDefault(i => i.Name == argumentName);
            if(argument == null)
                throw new ArgumentException($"Argument '{argumentName}' does not exist");
            return argument.Value;
        } 

        public static readonly Arguments Empty = new Arguments();
    }
}
