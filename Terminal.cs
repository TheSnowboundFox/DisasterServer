using System;
using System.Runtime.CompilerServices;

namespace DisasterServer
{
  public class Terminal
  {
    static Terminal()
    {
      try
      {
        AppDomain currentDomain = AppDomain.CurrentDomain;
      }
      catch
      {
        Console.WriteLine("Initialisation error.");
      }
    }

    public static void Log(string text)
    {
      string longTimeString = DateTime.Now.ToLongTimeString();
      DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(9, 3);
      interpolatedStringHandler.AppendLiteral("[");
      interpolatedStringHandler.AppendFormatted(longTimeString);
      interpolatedStringHandler.AppendLiteral(" INFO] ");
      interpolatedStringHandler.AppendFormatted(text);
      string stringAndClear = interpolatedStringHandler.ToStringAndClear();
      Console.ForegroundColor = ConsoleColor.White;
      Console.WriteLine(stringAndClear);
    }

    public static void LogDebug(string text)
    {
      string longTimeString = DateTime.Now.ToLongTimeString();
      DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(10, 3);
      interpolatedStringHandler.AppendLiteral("[");
      interpolatedStringHandler.AppendFormatted(longTimeString);
      interpolatedStringHandler.AppendLiteral(" DEBUG] ");
      interpolatedStringHandler.AppendFormatted(text);
      string stringAndClear = interpolatedStringHandler.ToStringAndClear();
      if (!Options.Get<bool>("debug_mode"))
        return;
      Console.ForegroundColor = ConsoleColor.Cyan;
      Console.WriteLine(stringAndClear);
    }
    }
}
