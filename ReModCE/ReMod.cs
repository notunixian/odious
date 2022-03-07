using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ExitGames.Client.Photon;
using HarmonyLib;
using Il2CppSystem.Threading;
using MelonLoader;
using Newtonsoft.Json;
using Photon.Realtime;
using ReMod.Core;
using ReMod.Core.Managers;
using ReMod.Core.UI.Wings;
using ReMod.Core.Unity;
using ReModCE.Components;
using ReModCE.Core;
using ReModCE.EvilEyeSDK;
using ReModCE.Loader;
using ReModCE.PhotonExtension;
using UnhollowerBaseLib;
using UnhollowerRuntimeLib;
using UnhollowerRuntimeLib.XrefScans;
using UnityEngine;
using VRC;
using VRC.Core;
using VRC.DataModel;
using VRC.UI.Elements.Menus;
using System.Timers;
using ConfigManager = ReMod.Core.Managers.ConfigManager;
using System.Collections;
using System.Windows.Forms.VisualStyles;
using Il2CppSystem.Runtime.Remoting.Messaging;
using ReMod.Core.VRChat;
using RootMotion.FinalIK;
using Valve.VR;
using Random = System.Random;
using ThreadState = Il2CppSystem.Threading.ThreadState;

namespace ReModCE
{
    public class ReModCE
    {
        private static readonly List<ModComponent> Components = new List<ModComponent>();
        private static UiManager _uiManager;
        private static ConfigManager _configManager;

        public static ReMirroredWingMenu WingMenu;
        public static bool IsEmmVRCLoaded { get; private set; }
        public static bool IsRubyLoaded { get; private set; }
        public static bool IsNocturnalLoaded { get; private set; }
        public static bool IsVoidLoaded { get; private set; }
        public static bool IsAbyssLoaded { get; private set; }
        public static bool IsOculus { get; private set; }
        public string ID { get; set; }
        public MethodBase TargetMethod { get; set; }
        public HarmonyMethod Prefix { get; set; }
        public HarmonyMethod Postfix { get; set; }
        private static int e7SentCount = 0;
        private static int e9SentCount = 0;
        private static int e6SentCount = 0;
        private static string MarkedAvatar = "";

        private static DiscordRPC.EventHandlers handlers;
        private static DiscordRPC.RichPresence presence;

        private unsafe delegate IntPtr AttemptAvatarDownloadDelegate(IntPtr hiddenValueTypeReturn, IntPtr thisPtr, IntPtr apiAvatarPtr, IntPtr multicastDelegatePtr, bool idfk, IntPtr nativeMethodInfo);
        private static AttemptAvatarDownloadDelegate dgAttemptAvatarDownload;
        private static MelonPreferences_Entry<bool> _LogAvi;
        private static MelonPreferences_Entry<bool> _PhotonProtection;
        private static MelonPreferences_Entry<bool> _AvatarProtection;
        private static MelonPreferences_Entry<bool> _WorldSpoof;
        private static MelonPreferences_Entry<string> _WorldIdToSpoof;
        private static MelonPreferences_Entry<string> _InstanceIdToSpoof;
        private static MelonPreferences_Entry<bool> _WorldSpoofWarn;
        public static HashSet<byte> writtenPhotonSamples = new HashSet<byte>();
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr ObjectInstantiateDelegate(IntPtr assetPtr, Vector3 pos, Quaternion rot, byte allowCustomShaders, byte isUI, byte validate, IntPtr nativeMethodPointer);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void VoidDelegate(IntPtr thisPtr, IntPtr nativeMethodInfo);

        private static readonly Dictionary<IntPtr, string> previouslyWhitelisted = new Dictionary<IntPtr, string>();
        public static HarmonyLib.Harmony Harmony { get; private set; }

        public static void OnApplicationStart()
        {
            Harmony = MelonHandler.Mods.First(m => m.Info.Name == "Odious").HarmonyInstance;
            Directory.CreateDirectory("UserData/Odious");
            ReLogger.Msg("Initializing...");

            // static definitions (sorta) if mods are loaded or not, this can easily be broken by the mod author but ¯\_(ツ)_/¯
            IsEmmVRCLoaded = MelonHandler.Mods.Any(m => m.Info.Name == "emmVRCLoader");
            IsRubyLoaded = File.Exists("hid.dll");
            IsNocturnalLoaded = MelonHandler.Mods.Any(m => m.Info.Name == "Nocturnal-V2");
            IsVoidLoaded = File.Exists("glu32.dll");
            IsAbyssLoaded = MelonHandler.Mods.Any(m => m.Info.Name == "AbyssLoader");

            var ourAssembly = Assembly.GetExecutingAssembly();
            var resources = ourAssembly.GetManifestResourceNames();
            foreach (var resource in resources)
            {
                if (!resource.EndsWith(".png"))
                    continue;

                var stream = ourAssembly.GetManifestResourceStream(resource);

                using var ms = new MemoryStream();
                stream.CopyTo(ms);
                var resourceName = Regex.Match(resource, @"([a-zA-Z\d\-_]+)\.png").Groups[1].ToString();
                ResourceManager.LoadSprite("remodce", resourceName, ms.ToArray());
            }

            _configManager = new ConfigManager(nameof(ReModCE));

            EnableDisableListener.RegisterSafe();
            ClassInjector.RegisterTypeInIl2Cpp<WireframeEnabler>();
            ClassInjector.RegisterTypeInIl2Cpp<NamePlates>();

            SetIsOculus();

            ReLogger.Msg($"Running on {(IsOculus ? "Not Steam" : "Steam")}");

            InitializePatches();
            InitializeModComponents();
            InitializeMelonPrefs();

            //handlers = default(DiscordRPC.EventHandlers);
            //DiscordRPC.Initialize("946967250378842112", ref handlers, true, null);
            //presence.details = "Using Odious for VRChat";

            //presence.state += "odious.cf";
            //presence.largeImageKey = "image_large";
            //presence.largeImageText = "Odious";
            //presence.startTimestamp = (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            //presence.partyId = "ae488379-351d-4a4f-ad32-2b9b01c91657";
            //presence.partySize = 1;
            //presence.partyMax = 69420;
            //presence.joinSecret = "MTI4NzM0OjFpMmhuZToxMjMxMjM=";
            //System.Timers.Timer timer = new System.Timers.Timer(15000.0);
            //timer.AutoReset = true;
            //timer.Enabled = true;
            //DiscordRPC.UpdatePresence(ref presence);

            ReLogger.Msg("Done!");
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
            ReLogger.Msg(ConsoleColor.Cyan, "                       Charlie                            ");
            ReLogger.Msg(ConsoleColor.Cyan, "                        Requi                             ");
            ReLogger.Msg(ConsoleColor.Cyan, "                       Stellar                            ");
            ReLogger.Msg(ConsoleColor.Cyan, "                     EvilEye Team                         ");
            ReLogger.Msg(ConsoleColor.Cyan, "                        teddy                             ");
            ReLogger.Msg(ConsoleColor.Cyan, "                        bruce                             ");
            ReLogger.Msg("------------------------------------------------------------");
        }

        private static void SetIsOculus()
        {
            try
            {
                var steamTracking = typeof(VRCTrackingSteam);
            }
            catch (TypeLoadException)
            {
                IsOculus = true;
                return;
            }

            IsOculus = false;
        }

        private static HarmonyMethod GetLocalPatch(string name)
        {
            return typeof(ReModCE).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static).ToNewHarmonyMethod();
        }

        private static void InitializePatches()
        {
            try
            {
                Harmony.Patch(typeof(VRCPlayer).GetMethod(nameof(VRCPlayer.Awake)), GetLocalPatch(nameof(VRCPlayerAwakePatch)));
                ReLogger.Msg(ConsoleColor.Green, $"Succesfully patched VRCPlayerAwake!");
            }
            catch (Exception e)
            {
                ReLogger.Error($"Unable to patch VRCPlayerAwake!\n Exception (please send this to unixian: \n{e}");
            }

            try
            {
                Harmony.Patch(typeof(RoomManager).GetMethod(nameof(RoomManager.Method_Public_Static_Boolean_ApiWorld_ApiWorldInstance_String_Int32_0)), postfix: GetLocalPatch(nameof(EnterWorldPatch)));
                ReLogger.Msg(ConsoleColor.Green, $"Successfully patched EnterWorld!");
            }
            catch (Exception e)
            {
                ReLogger.Error($"Unable to patch EnterWorld!\n Exception (please send this to unixian): \n{e}");
            }

            // hook from bundlebouncer
            try
            {
                unsafe
                {
                    var originalMethodPointer = *(IntPtr*)(IntPtr)UnhollowerUtils
                        .GetIl2CppMethodInfoPointerFieldForGeneratedMethod(typeof(AssetBundleDownloadManager).GetMethod(
                            nameof(AssetBundleDownloadManager.Method_Internal_UniTask_1_InterfacePublicAbstractIDisposableGaObGaUnique_ApiAvatar_MulticastDelegateNInternalSealedVoUnUnique_Boolean_0)))
                        .GetValue(null);

                    MelonUtils.NativeHookAttach((IntPtr)(&originalMethodPointer), typeof(ReModCE).GetMethod(nameof(OnAttemptAvatarDownload), BindingFlags.Static | BindingFlags.NonPublic).MethodHandle.GetFunctionPointer());

                    dgAttemptAvatarDownload = Marshal.GetDelegateForFunctionPointer<AttemptAvatarDownloadDelegate>(originalMethodPointer);
                    ReLogger.Msg(ConsoleColor.Green, $"Successfully able to hook AssetBundleDownLoadManager for logging.");
                }
            }
            catch (Exception e)
            {
                ReLogger.Error($"Unable to hook AssetBundleDownloadManager for logging!\n Exception (please send this to unixian): \n{e}");
            }

            try
            {
                Harmony.Patch(typeof(AssetBundleDownload).GetMethod(nameof(AssetBundleDownload.Method_Private_Static_String_String_String_Int32_String_String_String_0)), GetLocalPatch(nameof(OnCreateAssetBundleDownload)));
                ReLogger.Msg(ConsoleColor.Green, $"Successfully able to patch AssetBundleDownload for logging.");
            }
            catch (Exception e)
            {
                ReLogger.Error($"Unable to patch AssetBundleDownload for logging!\n Exception (please send this to unixian): \n{e}");
            }

            try
            {
                Harmony.Patch(typeof(LoadBalancingClient).GetMethod(nameof(LoadBalancingClient.OnEvent)), GetLocalPatch(nameof(OnEvent)));
                ReLogger.Msg(ConsoleColor.Green, $"Successfully able to patch OnEvent for Photon Anti-Crash.");
            }
            catch (Exception e)
            {
                ReLogger.Error($"Unable to patch OnEvent! Photon Anti-Crash will not work!\n Exception (please send this to unixian): \n{e}");
            }

            try
            {
                Harmony.Patch((from Method in typeof(IKSolverHeuristic).GetMethods()
                    where Method.Name.Equals("IsValid") && Method.GetParameters().Length == 1
                    select Method).First(), new HarmonyMethod(typeof(ReModCE), "FinalIKPatch"));
                Harmony.Patch(typeof(IKSolverAim).GetMethod(nameof(IKSolverAim.GetClampedIKPosition)), GetLocalPatch(nameof(IKSolverAimPatch)));
                ReLogger.Msg(ConsoleColor.Green, $"Successfully able to patch IK to prevent IK-based avatar crashers.");
            }
            catch (Exception e)
            {
                ReLogger.Error($"Unable to patch FinalIK! You will not be protected against IK-based crashers!\n Exception (please send this to unixian): \n{e}");
                throw;
            }

            try
            {
                Harmony.Patch(typeof(API).GetMethod(nameof(API.SendPutRequest)), new HarmonyMethod(typeof(ReModCE), "PutRequestPatch"));
                ReLogger.Msg(ConsoleColor.Green, $"Successfully able to patch SendPutRequest for world spoofing.");
            }
            catch (Exception e)
            {
                ReLogger.Error($"Unable to patch SendPutRequest! World spoofing will not work!\n Exception (please send this to unixian): \n{e}");
            }

            foreach (var method in typeof(SelectedUserMenuQM).GetMethods())
            {
                if (!method.Name.StartsWith("Method_Private_Void_IUser_PDM_"))
                    continue;

                if (XrefScanner.XrefScan(method).Count() < 3)
                    continue;

                Harmony.Patch(method, postfix: GetLocalPatch(nameof(SetUserPatch)));
            }
        }

        private static void InitializeNetworkManager()
        {
            var playerJoinedDelegate = NetworkManager.field_Internal_Static_NetworkManager_0.field_Internal_VRCEventDelegate_1_Player_0;
            var playerLeftDelegate = NetworkManager.field_Internal_Static_NetworkManager_0.field_Internal_VRCEventDelegate_1_Player_1;
            playerJoinedDelegate.field_Private_HashSet_1_UnityAction_1_T_0.Add(new Action<VRC.Player>(p =>
            {
                if (p != null) OnPlayerJoined(p);
            }));

            playerLeftDelegate.field_Private_HashSet_1_UnityAction_1_T_0.Add(new Action<VRC.Player>(p =>
            {
                if (p != null) OnPlayerLeft(p);
            }));
        }

        private static void InitializeMelonPrefs()
        {
            var category = MelonPreferences.GetCategory("ReModCE");
            _LogAvi = category.CreateEntry("LogAvi", false, "Excessive Avatar Logging",
                "When enabled, will excessively log avatars. Useful for identifying crashers or yoinking avatars from people.",
                true);
            _PhotonProtection = category.CreateEntry("PhotonProtection", true, "Photon Event Protections",
                "When enabled, will try to prevent malicious Photon exploits from affecting you. (freezing/crashing you.)",
                true);
            _AvatarProtection = category.CreateEntry("AvatarProtection", true, "Avatar Protections",
                "When enabled, will try to prevent malicious avatars from crashing you.",
                true);
            _WorldSpoof = category.CreateEntry("WorldSpoof", true, "World Spoofing",
                "When enabled, will spoof the world to a selected World ID (by default, VRChat Home World with instance id 1337.) Configurable under WorldIdToSpoof and InstanceIdToSpoof.",
                true);
            _WorldIdToSpoof = category.CreateEntry("WorldIdToSpoof", "wrld_4432ea9b-729c-46e3-8eaf-846aa0a37fdd", "World Spoofing",
                "World ID to spoof, WorldSpoof must be enabled for this to work.",
                true);
            _InstanceIdToSpoof = category.CreateEntry("InstanceIdToSpoof", "1337", "World Spoofing",
                "Instance ID to spoof, WorldSpoof must be enabled for this to work.",
                true);
            _WorldSpoofWarn = category.CreateEntry("WorldSpoofWarn", true, "World Spoofing",
                "Nags you about the world you are spoofing, just incase. WorldSpoof must be enabled for this to work.",
                true);
        }

        public static void OnUiManagerInit()
        {
            ReLogger.Msg("Initializing UI...");

            _uiManager = new UiManager("<color=#8c99e1>Odious</color>", ResourceManager.GetSprite("remodce.remod"));
            WingMenu = ReMirroredWingMenu.Create("Odious", "Open the Odious menu", ResourceManager.GetSprite("remodce.remod"));

            _uiManager.MainMenu.AddMenuPage("Movement", "Access movement related settings", ResourceManager.GetSprite("remodce.running"));

            var visualPage = _uiManager.MainMenu.AddCategoryPage("Visuals", "Access anything that will affect your game visually", ResourceManager.GetSprite("remodce.eye"));
            visualPage.AddCategory("ESP/Highlights");
            visualPage.AddCategory("Wireframe");
            visualPage.AddCategory("Nametags");
            visualPage.AddCategory("Cursor");

            _uiManager.MainMenu.AddMenuPage("Dynamic Bones", "Access your global dynamic bone settings", ResourceManager.GetSprite("remodce.bone"));
            _uiManager.MainMenu.AddMenuPage("Avatars", "Access avatar related settings", ResourceManager.GetSprite("remodce.hanger"));

            var utilityPage = _uiManager.MainMenu.AddCategoryPage("Utility", "Access miscellaneous settings", ResourceManager.GetSprite("remodce.tools"));
            utilityPage.AddCategory("Quality of Life");

            _uiManager.MainMenu.AddMenuPage("Logging", "Access logging related settings", ResourceManager.GetSprite("remodce.log"));
            _uiManager.MainMenu.AddMenuPage("Hotkeys", "Access hotkey related settings", ResourceManager.GetSprite("remodce.keyboard"));

            var exploitsPage = _uiManager.MainMenu.AddCategoryPage("Exploits", "haha funny vrchat game", ResourceManager.GetSprite("remodce.exploits"));
            exploitsPage.AddCategory("USpeak");
            exploitsPage.AddCategory("Events");
            exploitsPage.AddCategory("Udon");
            exploitsPage.AddCategory("Avatar");

            var safetyPage = _uiManager.MainMenu.AddCategoryPage("Safety", "Access protection/safety settings", ResourceManager.GetSprite("remodce.safety"));
            // TODO: make an actual good avatar anti-crash.
            // safetyPage.AddCategory("Avatars");
            safetyPage.AddCategory("Photon Events");

            var spoofingPage = _uiManager.MainMenu.AddCategoryPage("Spoofing", "Access settings related to spoofing certain things", ResourceManager.GetSprite("remodce.spoofing"));
            // TODO: add hwid spoofer
            // spoofingPage.AddCategory("Hardware");
            spoofingPage.AddCategory("Worlds");

            foreach (var m in Components)
            {
                try
                {
                    m.OnUiManagerInit(_uiManager);
                }
                catch (Exception e)
                {
                    ReLogger.Error($"{m.GetType().Name} had an error during UI initialization:\n{e}");
                }
            }
        }
        public static void OnUiManagerInitEarly()
        {
            ReLogger.Msg("Initializing early UI...");

            InitializeNetworkManager();

            foreach (var m in Components)
            {
                try
                {
                    m.OnUiManagerInitEarly();
                }
                catch (Exception e)
                {
                    ReLogger.Error($"{m.GetType().Name} had an error during early UI initialization:\n{e}");
                }
            }
        }

        public static void OnFixedUpdate()
        {
            foreach (var m in Components)
            {
                m.OnFixedUpdate();
            }
        }

        public static void OnUpdate()
        {
            foreach (var m in Components)
            {
                m.OnUpdate();
            }
        }

        public static void OnLateUpdate()
        {
            foreach (var m in Components)
            {
                m.OnLateUpdate();
            }
        }

        public static void OnGUI()
        {
            foreach (var m in Components)
            {
                m.OnGUI();
            }
        }

        public static void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            foreach (var m in Components)
            {
                m.OnSceneWasLoaded(buildIndex, sceneName);
            }
        }

        public static void OnSceneWasInitialized(int buildIndex, string sceneName)
        {
            foreach (var m in Components)
            {
                m.OnSceneWasInitialized(buildIndex, sceneName);
            }
        }

        public static void OnApplicationQuit()
        {
            foreach (var m in Components)
            {
                m.OnApplicationQuit();
            }

            MelonPreferences.Save();
            Process.GetCurrentProcess().Kill();
        }

        public static void OnPreferencesLoaded()
        {
            foreach (var m in Components)
            {
                m.OnPreferencesLoaded();
            }
        }

        public static void OnPreferencesSaved()
        {
            foreach (var m in Components)
            {
                m.OnPreferencesSaved();
            }
        }

        private static void OnPlayerJoined(VRC.Player player)
        {
            foreach (var m in Components)
            {
                m.OnPlayerJoined(player);
            }
        }

        private static void OnPlayerLeft(VRC.Player player)
        {
            foreach (var m in Components)
            {
                m.OnPlayerLeft(player);
            }
        }

        private static void AddModComponent(Type type)
        {
            try
            {
                var newModComponent = Activator.CreateInstance(type) as ModComponent;
                Components.Add(newModComponent);
            }
            catch (Exception e)
            {
                ReLogger.Error($"Failed creating {type.Name}:\n{e}");
            }
        }

        private class LoadableModComponent
        {
            public int Priority;
            public Type Component;
        }

        private static void InitializeModComponents()
        {
            var assembly = Assembly.GetExecutingAssembly();
            IEnumerable<Type> types;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException reflectionTypeLoadException)
            {
                types = reflectionTypeLoadException.Types.Where(t => t != null);
            }

            var loadableModComponents = new List<LoadableModComponent>();
            foreach (var t in types)
            {
                if (t.IsAbstract)
                    continue;
                if (t.BaseType != typeof(ModComponent))
                    continue;
                if (t.IsDefined(typeof(ComponentDisabled), false))
                    continue;

                var priority = 0;
                if (t.IsDefined(typeof(ComponentPriority)))
                {
                    priority = ((ComponentPriority)Attribute.GetCustomAttribute(t, typeof(ComponentPriority)))
                        .Priority;
                }

                loadableModComponents.Add(new LoadableModComponent
                {
                    Component = t,
                    Priority = priority
                });
            }

            var sortedComponents = loadableModComponents.OrderBy(component => component.Priority);
            foreach (var modComp in sortedComponents)
            {
                AddModComponent(modComp.Component);
            }

            ReLogger.Msg(ConsoleColor.Cyan, $"Created {Components.Count} mod components.");
        }

        private static void EnterWorldPatch(ApiWorld __0, ApiWorldInstance __1)
        {
            if (__0 == null || __1 == null)
                return;

            foreach (var m in Components)
            {
                m.OnEnterWorld(__0, __1);
            }
        }

        private static void VRCPlayerAwakePatch(VRCPlayer __instance)
        {
            if (__instance == null) return;

            __instance.Method_Public_add_Void_OnAvatarIsReady_0(new Action(() =>
            {
                foreach (var m in Components)
                {
                    m.OnAvatarIsReady(__instance);
                }
            }));
        }
        private static void SetUserPatch(SelectedUserMenuQM __instance, IUser __0)
        {
            if (__0 == null)
                return;

            foreach (var m in Components)
            {
                m.OnSelectUser(__0, __instance.field_Public_Boolean_0);
            }
        }

        static unsafe IntPtr OnAttemptAvatarDownload(IntPtr hiddenStructReturn, IntPtr thisPtr, IntPtr pApiAvatar, IntPtr pMulticastDelegate, bool param_3, IntPtr nativeMethodInfo)
        {
            try
            {
                using (var ctx = new AttemptAvatarDownloadContext(pApiAvatar == IntPtr.Zero ? null : new ApiAvatar(pApiAvatar)))
                {
                    var av = AttemptAvatarDownloadContext.apiAvatar;

                    if (_LogAvi.Value == false)
                    {
                        // crasher avi id
                        if (av.id == "avtr_da56b2e0-134e-465f-813d-707d284ab159")
                        {
                            av.assetUrl = null; // this should just make it an error robot
                        }
                        return dgAttemptAvatarDownload(hiddenStructReturn, thisPtr, pApiAvatar, pMulticastDelegate, param_3, nativeMethodInfo);
                    }
                    ReLogger.Msg($"Attempting to download avatar... {av.id} ({av.name}) by {av.authorName}");

                    // same thing here
                    if (av.id == "avtr_da56b2e0-134e-465f-813d-707d284ab159")
                    {
                        av.assetUrl = null;
                    }

                    return dgAttemptAvatarDownload(hiddenStructReturn, thisPtr, pApiAvatar, pMulticastDelegate, param_3, nativeMethodInfo);
                }
            }
            catch (Exception e)
            {
                ReLogger.Error($"avatar download threw an exception? {e}");
                return dgAttemptAvatarDownload(hiddenStructReturn, thisPtr, pApiAvatar, pMulticastDelegate, param_3, nativeMethodInfo);
            }
        }

        private struct AttemptAvatarDownloadContext : IDisposable
        {
            internal static ApiAvatar apiAvatar;

            public AttemptAvatarDownloadContext(ApiAvatar iApiAvatar)
            {
                apiAvatar = iApiAvatar;
            }

            public void Dispose()
            {
                apiAvatar = null;
            }
        }

        public static void QuickMenuOpenPatch()
        {
            // this will be used for some stuff later
            bool isQuickMenuOpen = true;
        }


        // also from bundle bouncer
        private static bool OnCreateAssetBundleDownload(string __0, string __1, int __2, string __3, string __4, string __5, ref string __result)
        {
            if (_LogAvi.Value == false)
            {
                return true;
            }

            string asseturl = __0;
            string id = __1;
            string category = __4;
            string ext = __5;

            // do this explicit check since this also has the chance to log world downloading, which isn't really useful.
            if (ext == "vrca" || category == "Avatars")
            {
                ReLogger.Msg($"\nAttempting to load VRCA!\nDetails: \nAsset URL: {asseturl} \nAvatar ID: {id}");
            }

            return true;
        }

        // took this from bundle bouncer since it's useful
        internal static string Params2JSON(ParameterDictionary paramDict)
        {
            var p = new Dictionary<byte, object>();
            foreach (var kvp in paramDict)
            {
                p[kvp.Key] = PhotonExtension.Serialization.FromIL2CPPToManaged<object>(kvp.Value);
            }
            return JsonConvert.SerializeObject(p);
        }

        // you should use networksanity instead of this, just a little thing i was trying to do on my own and it kind of works? 
        internal static bool OnEvent(EventData __0)
        {
            if (_PhotonProtection.Value == false)
            {
                return true;
            }

            // hey, person snooping though my pasted code.
            // want to hear a little background info about me since i'm bored while trying to paste photon anti-crash?

            // i used to do a lot of csgo cheating related things before i started doing stuff on vrchat, i've only been on this game for around 1-2 months.
            // i've met a lot of nice people on here, and probably some of the most fun i've had in a while.
            // so if you're one of the friends i've met over the past 1-2 months, thank you very much.
            // enough rambling, more pasting.

            try
            {
                int Sender = __0.sender;
                var player = PlayerManager.field_Private_Static_PlayerManager_0.GetPlayer(Sender);
                string SenderPlayer = player != null ? player.prop_APIUser_0.displayName : "VRC Server";

                switch (__0.Code)
                {

                    case 9: // avatar sync reliable, also used for some triggers i think?
                        // event 9 exploits were fixed (at least byte[8]) but 
                        if (__0.CustomData.Cast<Il2CppArrayBase<byte>>().Length >= 150 ||
                            __0.CustomData.Cast<Il2CppArrayBase<byte>>().Length <= 8)
                        {
                            // i know this is a bad way of doing this, but i really can't think of a different way of doing this.
                            if (SenderPlayer != "VRC Server" && SenderPlayer == player.prop_APIUser_0.displayName && Sender != -1 && Sender != 0)
                            {
                                e9SentCount++;
                            }
                            else
                            {
                                e9SentCount = 0;
                            }

                            if (e9SentCount > 1500)
                            {
                                ReLogger.Msg($"[photon anti-crash] {SenderPlayer} sent invalid event 9 data, blocking. size: [{__0.CustomData.Cast<Il2CppArrayBase<byte>>().Length}]"); ;
                                e9SentCount = 0;
                            }

                            return false;
                        }

                        break;

                    // i hate doing if trees like this but i'm too lazy to do a proper way of doing this
                    // also comparing event payloads like this is really bad, but i don't understand other ways of preventing these.
                    case 6: // rpc event, it is required to send the encrypt flag while using this (i think)
                        var payload = Params2JSON(__0.Parameters);

                        // EnableMeshRPC, no idea where this gets used normally so ima just return this false
                        if (payload.Contains("AEVuYWJsZU1lc2hSUEM"))
                        {
                            if (SenderPlayer != "VRC Server" && SenderPlayer == player.prop_APIUser_0.displayName && Sender != -1 && Sender != 0)
                            {
                                e6SentCount++;
                            }
                            else
                            {
                                e6SentCount = 0;
                            }

                            if (e6SentCount > 1500)
                            {
                                ReLogger.Msg($"[photon anti-crash] {SenderPlayer} sent invalid event 6 data, blocking.");
                                e6SentCount = 0;
                            }

                            return false;
                        }

                        // small chunk of an event 6 exploit that uses SanityCheck RPC, should only trigger if the exploit is being sent.
                        if (payload.Contains("wA6Mi8OAP8AAAAACQAAAAsAU2FuaXR5Q2hlY2sAAAAABBAAAgoDAAsAAAULAAAJBQIAAAA"))
                        {
                            if (SenderPlayer != "VRC Server" && SenderPlayer == player.prop_APIUser_0.displayName && Sender != -1 && Sender != 0)
                            {
                                e6SentCount++;
                            }
                            else
                            {
                                e6SentCount = 0;
                            }

                            if (e6SentCount > 1500)
                            {
                                ReLogger.Msg($"[photon anti-crash] {SenderPlayer} sent invalid event 6 data, blocking.");
                                e6SentCount = 0;
                            }


                            return false;
                        }

                        break;
                    case 209: // item transfer
                        return false;

                    case 210: // item ownership
                        return false;

                }
            }
            catch (Il2CppException ERROR)
            {
                MelonLogger.Error(ERROR.StackTrace);
                return true;
            }

            return true;
        }

        public static bool FinalIKPatch(IKSolverHeuristic __instance)
        {
            if (__instance.maxIterations > 64)
            {
                return false;
            }

            return true;
        }

        private static bool IKSolverAimPatch(ref IKSolverAim __instance)
        {
            __instance.clampSmoothing = MelonUtils.Clamp(__instance.clampSmoothing, 0, 2);
            return true;
        }

        public static bool PutRequestPatch(string __0, ApiContainer __1, Il2CppSystem.Collections.Generic.Dictionary<string, Il2CppSystem.Object> __2, API.CredentialsBundle __3)
        {
            try
            {
                if (_WorldSpoof.Value == false)
                {
                    return true;
                }

                if (__0.Contains("visits") || __0.Contains("joins"))
                {
                    // idiot proof checks ig
                    if (!_WorldIdToSpoof.Value.Contains("wrld_"))
                    {
                        __2["worldId"] = $"wrld_4432ea9b-729c-46e3-8eaf-846aa0a37fdd:1337";
                    }
                    else
                    {
                        if (!Regex.IsMatch(_InstanceIdToSpoof.Value, "^[A-Za-z0-9]*$"))
                        {
                            __2["worldId"] = $"{_WorldIdToSpoof.Value}:1337";
                        }
                    }

                    if (_WorldSpoofWarn.Value)
                    {
                        ReLogger.Warning($"You are currently spoofing your world, currently spoofed world: {_WorldIdToSpoof.Value}, currently spoofed InstanceID: {_InstanceIdToSpoof.Value}");
                    }
                    __2["worldId"] = $"{_WorldIdToSpoof.Value}:{_InstanceIdToSpoof.Value}";
                }
            }
            catch (Exception e)
            {
                ReLogger.Error($"[SendPutRequest] had an error! \n {e}");
            }

            return true;
        }
    }
}
