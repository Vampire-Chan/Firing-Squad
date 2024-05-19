using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using GTA;
using GTA.Native;

namespace FiringSquad
{
    internal class Extension
    {
        public static void ShowSubtitle(string message)
        {
            ShowSubtitle(message, 2500);
        }
        public static void ShowSubtitle(string message, int duration)
        {
            Function.Call(Hash.BEGIN_TEXT_COMMAND_PRINT, "CELL_EMAIL_BCON");
            
            Function.Call(Hash.END_TEXT_COMMAND_PRINT, duration, true);
        }

        public static void ShowHelpMessage(string message)
        {
            ShowHelpMessage(message, 5000, true);
        }
        public static void ShowHelpMessage(string message, bool sound)
        {
            ShowHelpMessage(message, 5000, sound);
        }
        public static void ShowHelpMessage(string message, int duration)
        {
            ShowHelpMessage(message, duration, true);
        }
        public static void ShowHelpMessage(string message, int duration, bool sound)
        {
            Function.Call(Hash.BEGIN_TEXT_COMMAND_DISPLAY_HELP, "CELL_EMAIL_BCON");
            
            Function.Call(Hash.END_TEXT_COMMAND_DISPLAY_HELP, 0, false, sound, duration);
        }

        public static void FadeScreenIn(int duration)
        {
            Function.Call(Hash.DO_SCREEN_FADE_IN, duration);
        }
        
        public static void FadeScreenOut(int duration)
        {
            Function.Call(Hash.DO_SCREEN_FADE_OUT, duration);
        }
    }
}
