using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aspekt.Test;

namespace Aspekt.Target
{
    internal class ClassWithAsync
    {
        public async void AsyncDoSomething()
        {
            await DoSomethingTask();
        }

        public Task DoSomethingTask()
        {
            return null;
        }

        public async Task<int> AsyncDoSomethingReturnInt()
        {
            var args = new MethodArguments(null, null, null, null, this);
            var aspect = new ClassLevelAspect();

            var cont = Aspect.AsyncOnExit<int>(aspect, args);

            return await DoSomethingTaskInt().ContinueWith(cont);
        }

        public Task<int> DoSomethingTaskInt()
        {
            var args = new MethodArguments(null, null, null, null, this);
            var aspect = new ClassLevelAspect();

            var cont = Aspect.AsyncOnExit<int>(aspect, args);

            return Task.Factory
                .StartNew(() => { return 3; })
                .ContinueWith(cont);

            // Get the argument type
            // build the generic call
            // place the stack
        }



    }
}
