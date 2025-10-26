using Aspekt.Test;

namespace Aspekt.Target
{
    internal class ClassWithAsync
    {
        public async void AsyncDoSomething()
        {
            await DoSomethingTask().ConfigureAwait(false);
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

            return await DoSomethingTaskInt().ContinueWith(cont).ConfigureAwait(false);
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
