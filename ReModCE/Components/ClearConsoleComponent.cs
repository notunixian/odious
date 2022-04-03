using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReMod.Core;
using ReMod.Core.Managers;
using ReMod.Core.UI.QuickMenu;
using ReModCE.Loader;
using ReModCE.SDK;
using UnityEngine.UI;

namespace ReModCE.Components
{
    internal class ClearConsoleComponent : ModComponent
    {
        private ReMenuButton ClearConsole;
        public ClearConsoleComponent()
        {
        }

        public override void OnUiManagerInit(UiManager uiManager)
        {
            var utilMenu = uiManager.MainMenu.GetCategoryPage("Utility").AddCategory("Application");
            ClearConsole = utilMenu.AddButton("Clear Console", "Clears the console.", () =>
            {
                Console.Clear();
                ReLogger.Msg("------------------------------------------------------------");
                ReLogger.Msg(ConsoleColor.DarkMagenta, "                     d8b   d8,                            ");
                ReLogger.Msg(ConsoleColor.DarkMagenta, "                     88P  `8P                             ");
                ReLogger.Msg(ConsoleColor.DarkMagenta, "                   d88                                    ");
                ReLogger.Msg(ConsoleColor.DarkMagenta, "       d8888b  d888888    88b d8888b ?88   d8P .d888b,    ");
                ReLogger.Msg(ConsoleColor.DarkMagenta, "      d8P' ?88d8P' ?88    88Pd8P' ?88d88   88  ?8b,       ");
                ReLogger.Msg(ConsoleColor.DarkMagenta, "      88b  d8888b  ,88b  d88 88b  d88?8(  d88    `?8b     ");
                ReLogger.Msg(ConsoleColor.DarkMagenta, "      `?8888P'`?88P'`88bd88' `?8888P'`?88P'?8b`?888P'     ");
                ReLogger.Msg(ConsoleColor.Cyan, "                                                          ");
                ReLogger.Msg(ConsoleColor.Cyan, "                                                          ");
                ReLogger.Msg(ConsoleColor.Cyan, "              Made & Pasted by Unixian#4669               ");
                ReLogger.Msg(ConsoleColor.Cyan, "             Most pasted client known to man              ");
                ReLogger.Msg(ConsoleColor.Cyan, "                                                          ");
                ReLogger.Msg(ConsoleColor.Cyan, "                       Credits:                           ");
                ReLogger.Msg(ConsoleColor.Cyan, "                                                          ");
                ReLogger.Msg(ConsoleColor.Cyan, "                        Requi                             ");
                ReLogger.Msg(ConsoleColor.Cyan, "                       Stellar                            ");
                ReLogger.Msg(ConsoleColor.Cyan, "                     EvilEye Team                         ");
                ReLogger.Msg(ConsoleColor.Cyan, "                        lenoob                            ");
                ReLogger.Msg("------------------------------------------------------------");
                ReLogger.Msg("Cleared Console!");
            });
        }
    }
}
