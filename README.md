ASPEKT
===

[![Build Status](https://travis-ci.org/mvpete/aspekt.svg?branch=master)](https://travis-ci.org/mvpete/aspekt)


A lightweight (Aspect Oriented Programming) AOP foundation

### Overview
Aspekt is a small AOP foundation library written in C#. Aspect Oriented Programming (AOP) allows to ease the pain associated with cross-cutting concerns. A cross-cutting concern is a pattern that gets across a program that cannot easily be factored into its own module. This raises the so called signal-to-noise ratio. One common cross cutting concern is logging, when logging entry/exit/exception of a function, the code becomes cluttered with log code and the intent can become lost. Aspekt addresses these concerns, by using attributes as annotation of functions. Like PostSharp aspect utilizes Mono.Cecil to post process the .NET binaries, inserting the aspects post build.

### Usage

The foundation of Aspekt is the base Aspect. In order to utilize Aspects, just derive from Aspekt.Aspect and implement the OnEntry, OnExit, OnException methods. Aspekt will only call the calls implemented on the class i.e. OnEntry, OnExit, OnException -- the issue here now becomes inheritence in Aspects...

Also, no PDBs are generated.

```csharp
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
```
Aspekt re-writes methods in the following manner.
```csharp
class Foo
{
    [SampleAspect("Some Value")]
    public void Bar(String s, int i)
    {
       MethodArgs ma = new MethodArgs("Bar", "Assembly.Foo.Bar(String s, int i)", new Arguments(new object[] { s, i }), this);
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
 ```
 Aspekt tries not alter or modify existing code, so if the IL contains multiple returns, Aspekt calls OnExit before each return.

Since Aspekt works post compile, in order to use it you must run the Bootstrap application against your assembly.
    
    > Aspekt.Bootstrap.Host [PathToAssembly] 

This will process the assembly and add in the aspects to their respective members.

