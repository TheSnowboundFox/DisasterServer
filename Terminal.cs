// Decompiled with JetBrains decompiler
// Type: DisasterServer.Terminal
// Assembly: DisasterServer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 543F9D20-F969-4EBF-B20C-D2589F4E3C54
// Assembly location: C:\Users\User\Downloads\disasterlauncherwindows\server\DisasterServer.dll

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading;

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
        public static void LogCommand(string text)
        {
            // Сохраняем текущие позиции курсора
            int currentLeft = Console.CursorLeft;
            int currentTop = Console.CursorTop;

            // Печатаем лог административной команды
            string longTimeString = DateTime.Now.ToLongTimeString();
            DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(9, 3);
            interpolatedStringHandler.AppendLiteral("[");
            interpolatedStringHandler.AppendFormatted(longTimeString);
            interpolatedStringHandler.AppendLiteral(" COMMAND] ");
            interpolatedStringHandler.AppendFormatted(text);
            string stringAndClear = interpolatedStringHandler.ToStringAndClear();
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine(stringAndClear);

            // Возвращаемся на строку, где выводится командная строка
            Console.SetCursorPosition(currentLeft, currentTop);
        }
    }
}
