using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExitGames.Client.Photon;
using MelonLoader;
using Newtonsoft.Json;
using Photon.Pun;
using Photon.Realtime;
using ReMod.Core;
using ReMod.Core.Managers;
using ReMod.Core.UI.QuickMenu;
using ReMod.Core.VRChat;
using ReModCE.Loader;
using UnhollowerBaseLib;
using VRC;
using UnityEngine;
using Player = VRC.Player;

namespace ReModCE.Components
{
    internal class PhotonAntiCrashComponent : ModComponent
    {
        private ConfigValue<bool> PhotonAntiEnabled;
        private static ReMenuToggle _PhotonAntiToggled;
        private static MelonPreferences_Entry<bool> _PhotonProtection;
        public static Dictionary<byte, string> PermaBlockedEvents = new Dictionary<byte, string>();
        public static Dictionary<byte, DateTime> LastEvent = new Dictionary<byte, DateTime>();
        public static Dictionary<byte, int> SpamAmount = new Dictionary<byte, int>();
        public static Dictionary<byte, DateTime> BlockedSpam = new Dictionary<byte, DateTime>();
        public static List<PhotonView> list_8;
        public static int PhotonViewInt = 0;


        public PhotonAntiCrashComponent()
        {
            var category = MelonPreferences.GetCategory("ReModCE");
            _PhotonProtection = (MelonPreferences_Entry<bool>)category.GetEntry("PhotonProtection");

            PhotonAntiEnabled = new ConfigValue<bool>(nameof(PhotonAntiEnabled), true);
            PhotonAntiEnabled.OnValueChanged += () => _PhotonAntiToggled.Toggle(PhotonAntiEnabled);
        }

        public override void OnUiManagerInit(UiManager uiManager)
        {
            var exploitsMenu = uiManager.MainMenu.GetCategoryPage("Safety").GetCategory("Photon Events");
            _PhotonAntiToggled = exploitsMenu.AddToggle("Photon Anti-Crash", "Attempts to prevent against maliciously crafted Photon events.", b => { PhotonAntiEnabled.SetValue(b); PhotonAntiCrash(b); }, PhotonAntiEnabled);
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            if (buildIndex != -1)
            {
                return;
            }
            ReModCE.OurPhotonViews = Resources.FindObjectsOfTypeAll<PhotonView>().ToList();
        }

        public override void OnPlayerJoined(Player player)
        {
            ReModCE.CurrentPhotonPlayers++;
        }

        public override void OnPlayerLeft(Player player)
        {
            ReModCE.CurrentPhotonPlayers--;
        }

        // was thinking of doing requi's method of overriding functions but i'm too lazy to redo my patch for onevent
        public void PhotonAntiCrash(bool enabled)
        {
            if (enabled)
            {
                _PhotonProtection.Value = true;
                MelonPreferences.Save();
            }
            else
            {
                _PhotonProtection.Value = false;
                MelonPreferences.Save();
            }
        }

        public static bool OnEvent(EventData __0)
        {
            // this might not be the greatest idea for performance, but i'm doing it anyway.
            var category = MelonPreferences.GetCategory("ReModCE");
            _PhotonProtection = (MelonPreferences_Entry<bool>)category.GetEntry("PhotonProtection");

            if (_PhotonProtection.Value == false)
            {
                return true;
            }

            try
            {
                //int Sender = __0.sender;
                //var player = PlayerManager.field_Private_Static_PlayerManager_0.GetPlayer(Sender);
                //string SenderPlayer = player != null ? player.prop_APIUser_0.displayName : "";
                byte code = __0.Code;

                if (code == 1 || code == 6 || code == 9 || code == 209 || code == 210)
                {
                    if (BlockedSpam.ContainsKey(code))
                    {
                        if (DateTime.Now.TimeOfDay > BlockedSpam[code].TimeOfDay)
                        {
                            BlockedSpam.Remove(code);
                            if (SpamAmount.ContainsKey(code))
                            {
                                SpamAmount[code] = 0;
                            }
                        }
                        return false;
                    }
                    if (!LastEvent.ContainsKey(code))
                    {
                        LastEvent.Add(code, DateTime.Now);
                    }
                    else
                    {
                        if (!SpamAmount.ContainsKey(code))
                        {
                            SpamAmount.Add(code, 0);
                        }
                        SpamAmount[code]++;
                        if (DateTime.Now.Subtract(LastEvent[code]).TotalSeconds >= 1.0 && !BlockedSpam.ContainsKey(code))
                        {
                            float num = 1f;
                            int num2 = 0;
                            switch (code)
                            {
                                case 6:
                                    num2 = 100;
                                    num = SpamAmount[code] - 100;
                                    break;
                                case 9:
                                    num2 = 10 * ReModCE.OurPhotonViews.Count + ReModCE.CurrentPhotonPlayers;
                                    num = 5f;
                                    break;
                                case 1:
                                    num2 = 20 * ReModCE.CurrentPhotonPlayers;
                                    num = 1.2f;
                                    break;
                                case 210:
                                    num2 = 90;
                                    num = 2.5f;
                                    break;
                                case 209:
                                    num2 = 90;
                                    num = 2.5f;
                                    break;
                            }
                            if (SpamAmount[code] > num2)
                            {
                                DateTime value = DateTime.Now.AddSeconds(num);
                                BlockedSpam.Add(code, value);
                                ReLogger.Msg($"[event {code}] prevented spam for {num} seconds");
                                return false;
                            }
                            SpamAmount[code] = 0;
                            LastEvent[code] = DateTime.Now;
                        }
                    }

                }
            }
            catch (Il2CppException ERROR)
            {
                MelonLogger.Error(ERROR.StackTrace);
                return true;
            }

            return true;
        }
    }
}
