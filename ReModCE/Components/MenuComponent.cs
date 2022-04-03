using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MelonLoader;
using ReMod.Core;
using ReMod.Core.Managers;
using ReMod.Core.UI.QuickMenu;
using UnityEngine;

namespace ReModCE.Components
{
    // soon...

    //internal class MenuComponent : ModComponent
    //{
    //    private ConfigValue<bool> MenuEnabled;
    //    private static ReMenuToggle _MenuToggled;
    //    private bool toggleBool = true;
    //    public Texture2D icon;

    //    public MenuComponent()
    //    {
    //        MenuEnabled = new ConfigValue<bool>(nameof(MenuEnabled), true);
    //        MenuEnabled.OnValueChanged += () => _MenuToggled.Toggle(MenuEnabled);
    //    }

    //    public override void OnUiManagerInit(UiManager uiManager)
    //    {
    //        var visuals = uiManager.MainMenu.GetCategoryPage("Visuals").GetCategory("Menu");
    //        _MenuToggled = visuals.AddToggle("Custom Menu",
    //            "Custom Menu for Odious (contains notifications, join/leave logs, etc.)", MenuEnabled.SetValue,
    //            MenuEnabled);
    //    }

    //    public override void OnGUI()
    //    {
    //        if (GUI.Button(new Rect(10, 10, 100, 50), icon))
    //        {
    //            MelonLogger.Msg("you clicked the icon");
    //        }
    //    }
    //}
}
