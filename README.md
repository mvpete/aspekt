[ASPeKT]
===

[![Build status](https://ci.appveyor.com/api/projects/status/ysr9ebr6dwaqamus?svg=true)](https://ci.appveyor.com/project/mvpete/aspekt)

A lightweight (Aspect Oriented Programming) AOP foundation

### Overview
Aspekt is a small AOP foundation library written in C#. Aspect Oriented Programming (AOP) allows to ease the pain associated with cross-cutting concerns. A cross-cutting concern is a pattern that gets across a program that cannot easily be factored into its own module. This raises the so called signal-to-noise ratio. One common cross cutting concern is logging, when logging entry/exit/exception of a function, the code becomes cluttered with log code and the intent can become lost. Aspekt addresses these concerns, by using attributes as annotation of functions. Like PostSharp aspect utilizes post processing of the .NET binaries, inserting the aspects post build. For ASPeKT this is done using Mono.Cecil.

### Usage

The foundation of Aspekt is the base Aspect. In order to utilize Aspects, just derive from Aspekt.Aspect and implement the OnEntry, OnExit, OnException methods. Aspekt will only place the calls implemented on the class i.e. OnEntry, OnExit, OnException


```csharp
class SampleAspect : Aspekt.Aspect
{
   public SampleAspect(String val)
   {
   }

   public void override OnEntry(MethodArgs ma)
   {
      // called before any existing code is ran
   }

   public void override OnExit(MethodArgs ma)
   {
     // called before ANY return statement
   }

   public void override OnException(MethodArgs ma, Exception e)
   {
     // called if existing codes excepts
   }
}
```

When you use the Aspect in your client code, such as

```csharp
class Foo
{
    [SampleAspect("Some Value")]
    public void Bar(String s, int i)
    {
        // Do something here.
    }
}
```

Aspekt will re-write the code, to something along the lines of the following.

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
           // Do Something Here
           sa.OnExit(ma);
       }
       catch(Exception e)
       {
          sa.OnException(ma,e);
          throw;
       }
    }
}
 ```
 As mentioned earlier, Aspekt will only write functions where they've been overridden. This means, only the methods that you want, are added. As well, Aspekt tries not alter or modify existing code, so if the IL contains multiple returns, Aspekt calls OnExit before each return.

If you're using NuGet to get ASPeKT, your project will have the appropriate post build steps. You can ignore anything below.

Since Aspekt works post compile, in order to use it you must run the Bootstrap application against your assembly.
    
    > Aspekt.Bootstrap.Host [PathToAssembly] 

This will process the assembly and add in the aspects to their respective members.


