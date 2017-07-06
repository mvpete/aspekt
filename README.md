ASPEKT
===
A lightweight (Aspect Oriented Programming) AOP foundation

### Overview
Aspekt is a small AOP foundation library written in C#. Aspect Oriented Programming (AOP) allows to ease the pain associated with cross-cutting concerns. A cross-cutting concern is a pattern that gets across a program that cannot easily be factored into its own module. This raises the so called signal-to-noise ratio. One common cross cutting concern is logging, when logging entry/exit/exception of a function, the code becomes cluttered with log code and the intent can become lost. Aspekt addresses these concerns, by using attributes as annotation of functions. Like PostSharp aspect utilizes Mono.Cecil to post process the .NET binaries, inserting the aspects post build.

### Usage

The foundation of Aspekt is the base Aspect. In order to utilize Aspects, just derive from Aspekt.Aspect and implement the OnEntry, OnExit, OnException methods. Currently, Aspekt only supports method processing but I'm hoping to implement class and property aspects.

Also, no PDBs are generated.

    class SampleAspect : Aspekt.Aspect
    {
       public SampleAspect(String val)
       {
       }
       
       public OnEntry(MethodArgs ma)
       {
          // called before any existing code is ran
       }
       
       public OnExit(MethodArgs ma)
       {
         // called before ANY return statement
       }
       
       public OnException(MethodArgs ma, Exception e)
       {
         // called if existing codes excepts
       }
    }


### Information
Aspekt re-writes methods in the following manner.

    class Foo
    {
        [SampleAspect("Some Value")]
        public void Bar(String s, int i)
        {
           MethodArgs ma = new MethodArgs("Foo.Bar()", new Arguments(new object[] { s, i }));
           SampleAspect sa = new SampleAspect("Some Value");
           sa.OnEntry(ma);
           try
           {
               // original code
               sa.OnExit(ma);
           }
           catch(Exception e)
           {
              sa.OnException(ma,e);
           }
        }
    }
    
 Aspekt tries not alter or modify existing code, so if the IL contains multiple returns, Aspekt calls OnExit before each return.


### TODO

0. Clean up -- I just threw this together this week. I need to clean up the host program and clean up some of the code. Also, manage multiple Aspects, etc...

0.5. Tests -- While I clean up, I should implement some tests... You know TDD... :) 

1. Pay for what you use. --  Only call functions that are overridden from the base Aspect. Currently Aspect is an abstract class for all three functions. The plan is to change that and only call overridden functions.

2. PDBs -- I'm not sure about this. I'm sure I missed something with Cecil for the recompile.

3. Class attributes -- Implement class attribute processing. If an attribute exists on a class, then implement each method in the class with the Aspect.

4. UseThis Property attribute -- UseThis as an attribute on the Aspect, allow for assignment of "this" to a property on the Aspect. This allows for inspection of the targeted class.
