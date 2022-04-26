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
                ReLogger.Msg("==============================================================");
                ReLogger.Msg(ConsoleColor.DarkMagenta, "                        ,o888888o.                          ");
                ReLogger.Msg(ConsoleColor.DarkMagenta, "                     . 8888     `88.                        ");
                ReLogger.Msg(ConsoleColor.DarkMagenta, "                    ,8 8888       `8b                       ");
                ReLogger.Msg(ConsoleColor.DarkMagenta, "                    88 8888        `8b                      ");
                ReLogger.Msg(ConsoleColor.DarkMagenta, "                    88 8888         88                      ");
                ReLogger.Msg(ConsoleColor.DarkMagenta, "                    88 8888         88                      ");
                ReLogger.Msg(ConsoleColor.DarkMagenta, "                    88 8888        ,8P                      ");
                ReLogger.Msg(ConsoleColor.DarkMagenta, "                    `8 8888       ,8P                       ");
                ReLogger.Msg(ConsoleColor.DarkMagenta, "                     ` 8888     ,88'                        ");
                ReLogger.Msg(ConsoleColor.DarkMagenta, "                        `8888888P'                          ");
                ReLogger.Msg(ConsoleColor.Cyan, "                                                            ");
                ReLogger.Msg(ConsoleColor.Cyan, "               Made & Pasted by Unixian#4669                ");
                ReLogger.Msg(ConsoleColor.Cyan, "        Stop paying for features that are open source.      ");
                ReLogger.Msg(ConsoleColor.Cyan, "                                                            ");
                ReLogger.Msg(ConsoleColor.Cyan, "                         Credits:                           ");
                ReLogger.Msg(ConsoleColor.Cyan, "              Requi, Stellar, Evileye, lenoob               ");
                ReLogger.Msg("==============================================================");
                ReLogger.Msg("Cleared Console!");
            });
        }
    }
}
