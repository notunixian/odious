using ExitGames.Client.Photon;
using HarmonyLib;
using MelonLoader;
using Newtonsoft.Json;
using Photon.Pun;
using Photon.Realtime;
using ReMod.Core;
using ReMod.Core.Managers;
using ReMod.Core.UI.Wings;
using ReMod.Core.Unity;
using ReModCE.Components;
using ReModCE.Core;
using ReModCE.Loader;
using ReModCE.PhotonExtension;
using RootMotion.FinalIK;
using SharpNeatLib.Maths;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Il2CppSystem.Threading;
using ReModCE.AvatarPostProcess;
using UnhollowerBaseLib;
using UnhollowerRuntimeLib;
using UnhollowerRuntimeLib.XrefScans;
using UnityEngine;
using VRC.Core;
using VRC.DataModel;
using VRC.UI.Elements.Menus;
using UnityEngine.Animations;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using VRCSDK2;
using ConfigManager = ReMod.Core.Managers.ConfigManager;
using ReModCE.SDK;
using ReModCE.Config;
using System.Net;

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
        public static bool IsMonkeyLoaded { get; private set; }
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
        private static MelonPreferences_Entry<bool> _LogAvi;
        private static MelonPreferences_Entry<bool> _PhotonProtection;
        private static MelonPreferences_Entry<bool> _AvatarProtection;
        private static MelonPreferences_Entry<bool> _WorldSpoof;
        private static MelonPreferences_Entry<string> _WorldIdToSpoof;
        private static MelonPreferences_Entry<string> _InstanceIdToSpoof;
        private static MelonPreferences_Entry<bool> _WorldSpoofWarn;
        private static MelonPreferences_Entry<bool> _WorldSpoofEnabled;
        private static MelonPreferences_Entry<bool> _HWIDSpoof;
        private static Il2CppSystem.Object clientHWID = null;
        private static Il2CppSystem.Object clientHWIDOriginal = null;
        public static HashSet<byte> writtenPhotonSamples = new HashSet<byte>();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr ObjectInstantiateDelegate(IntPtr assetPtr, Vector3 pos, Quaternion rot, byte allowCustomShaders, byte isUI, byte validate, IntPtr nativeMethodPointer);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void VoidDelegate(IntPtr thisPtr, IntPtr nativeMethodInfo);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr OnAvatarDownloadStartDelegate(IntPtr thisPtr, IntPtr apiAvatar, IntPtr downloadContainer, bool unknownBool, IntPtr nativeMethodPointer);
        private static OnAvatarDownloadStartDelegate onAvatarDownloadStart;

        private static readonly Dictionary<IntPtr, string> previouslyWhitelisted = new Dictionary<IntPtr, string>();
        internal static readonly FastRandom fastRandom = new FastRandom();
        public static bool isQuickMenuOpen = false;
        public static string GeneratedHWID = "";
        public static List<PhotonView> OurPhotonViews = new();
        public static int CurrentPhotonPlayers = 0;
        private const string SavePath = "UserData/Odious/Logs";
        internal static VRCAvatarManager currentlyLoadingAvatar;
        private static readonly System.Collections.Generic.List<IntPtr> previouslyCheckedAvatars = new System.Collections.Generic.List<IntPtr>();

        // this prevents some garbage collection bullshit
        private static List<object> ourPinnedDelegates = new List<object>();
        public static HarmonyLib.Harmony Harmony { get; private set; }
        public static List<NameplateModel> nameplateModels;

        // retrives nameplate custom ranks from a file on github
        // this should only be called when it's needed, like when the application starts or when a scene is loaded.
        private static void UpdateNamePlates()
        {
            string url = "https://raw.githubusercontent.com/notunixian/odious/main/Data/Nameplates.json";

            HttpWebRequest WebReq = (HttpWebRequest)WebRequest.Create(url);
            WebReq.Method = "GET";
            HttpWebResponse WebResp = (HttpWebResponse)WebReq.GetResponse();
            string jsonString;
            using (Stream stream = WebResp.GetResponseStream())
            {
                StreamReader reader = new StreamReader(stream, System.Text.Encoding.UTF8);
                jsonString = reader.ReadToEnd();
            }
            NameplateModelList items = JsonConvert.DeserializeObject<NameplateModelList>(jsonString);
            nameplateModels = items.records;
        }

        public static void OnApplicationStart()
        {
            Harmony = MelonHandler.Mods.First(m => m.Info.Name == "Odious").HarmonyInstance;
            Directory.CreateDirectory("UserData/Odious");
            Directory.CreateDirectory("UserData/Odious/Logs");
            if (!File.Exists($"UserData/Odious/odious_favorites.json"))
            {
                File.Create($"UserData/Odious/odious_favorites.json");
            }

            ReLogger.Msg("Initializing...");

            // static definitions (sorta) if mods are loaded or not, this can easily be broken by the mod author but ¯\_(ツ)_/¯
            IsEmmVRCLoaded = MelonHandler.Mods.Any(m => m.Info.Name == "emmVRCLoader");
            IsRubyLoaded = File.Exists("hid.dll");
            IsNocturnalLoaded = MelonHandler.Mods.Any(m => m.Info.Name == "Nocturnal-V2");
            IsAbyssLoaded = MelonHandler.Mods.Any(m => m.Info.Name == "AbyssLoader");
            IsMonkeyLoaded = MelonHandler.Mods.Any(m => m.Info.Name == "FunnyUi");

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
            ClassInjector.RegisterTypeInIl2Cpp<CustomNameplate>();
            MelonCoroutines.Start(UILoggerComponent.MakeUI());
            ReaderPatches.ApplyPatches();

            SetIsOculus();

            ReLogger.Msg($"Running on {(IsOculus ? "Not Steam" : "Steam")}");

            SettingsHandler.Register();
            Configuration.LoadAllConfigs();
            InitializePatches();
            InitializeModComponents();
            UpdateNamePlates();

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

                Harmony.Patch(typeof(RoomManager).GetMethod(nameof(RoomManager.Method_Public_Static_Boolean_ApiWorld_ApiWorldInstance_String_Int32_0)), postfix: GetLocalPatch(nameof(EnterWorldPatch)));

                unsafe
                {
                    MethodInfo method = (from m in typeof(Downloader).GetMethods()
                        where m.Name.StartsWith("Method_Internal_Static_UniTask_1_InterfacePublicAbstractIDisposable") && m.Name.Contains("ApiAvatar")
                        select m).First();
                    IntPtr ptr = *(IntPtr*)(void*)(IntPtr)UnhollowerUtils.GetIl2CppMethodInfoPointerFieldForGeneratedMethod(method).GetValue(null);
                    OnAvatarDownloadStartDelegate onAvatarDownloadStartDelegate = (IntPtr thisPtr, IntPtr apiAvatar, IntPtr downloadContainer, bool unknownBool, IntPtr nativeMethodPointer) => OnAvatarDownloadStartPatch(thisPtr, apiAvatar, downloadContainer, unknownBool, nativeMethodPointer);
                    ourPinnedDelegates.Add(onAvatarDownloadStartDelegate);
                    MelonUtils.NativeHookAttach((IntPtr)(&ptr), Marshal.GetFunctionPointerForDelegate(onAvatarDownloadStartDelegate));
                    onAvatarDownloadStart = Marshal.GetDelegateForFunctionPointer<OnAvatarDownloadStartDelegate>(ptr);
                }

                Harmony.Patch(typeof(LoadBalancingClient).GetMethod(nameof(LoadBalancingClient.OnEvent)), new HarmonyMethod(typeof(PhotonAntiCrashComponent), "OnEvent"));

                Harmony.Patch((from Method in typeof(IKSolverHeuristic).GetMethods()
                               where Method.Name.Equals("IsValid") && Method.GetParameters().Length == 1
                               select Method).First(), new HarmonyMethod(typeof(ReModCE), "FinalIKPatch"));
                Harmony.Patch(typeof(IKSolverAim).GetMethod(nameof(IKSolverAim.GetClampedIKPosition)), GetLocalPatch(nameof(IKSolverAimPatch)));

                Harmony.Patch(typeof(API).GetMethod(nameof(API.SendPutRequest)), new HarmonyMethod(typeof(ReModCE), "PutRequestPatch"));

                //Harmony.Patch(typeof(LoadBalancingClient).GetMethod(nameof(LoadBalancingClient.Method_Public_Virtual_New_Boolean_Byte_Object_RaiseEventOptions_SendOptions_0)), GetLocalPatch(nameof(OnOPRaiseEvent)));

                var matchingMethods = typeof(AssetManagement)
                .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).Where(it =>
                    it.Name.StartsWith("Method_Public_Static_Object_Object_Vector3_Quaternion_Boolean_Boolean_Boolean_") && it.GetParameters().Length == 6).ToList();

                foreach (var matchingMethod in matchingMethods)
                {
                    unsafe
                    {
                        var originalMethodPointer = *(IntPtr*)(IntPtr)UnhollowerUtils.GetIl2CppMethodInfoPointerFieldForGeneratedMethod(matchingMethod).GetValue(null);

                        ObjectInstantiateDelegate originalInstantiateDelegate = null;

                        ObjectInstantiateDelegate replacement = (assetPtr, pos, rot, allowCustomShaders, isUI, validate, nativeMethodPointer) =>
                            OnObjectInstantiatedPatch(assetPtr, pos, rot, allowCustomShaders, isUI, validate, nativeMethodPointer, originalInstantiateDelegate);

                        ourPinnedDelegates.Add(replacement);

                        MelonUtils.NativeHookAttach((IntPtr)(&originalMethodPointer), Marshal.GetFunctionPointerForDelegate(replacement));

                        originalInstantiateDelegate = Marshal.GetDelegateForFunctionPointer<ObjectInstantiateDelegate>(originalMethodPointer);
                    }
                }

                foreach (var nestedType in typeof(VRCAvatarManager).GetNestedTypes())
                {
                    var moveNext = nestedType.GetMethod("MoveNext");
                    if (moveNext == null) continue;
                    var avatarManagerField = nestedType.GetProperties().SingleOrDefault(it => it.PropertyType == typeof(VRCAvatarManager));
                    if (avatarManagerField == null) continue;

                    MelonDebug.Msg($"Patching UniTask type {nestedType.FullName}");

                    var fieldOffset = (int)IL2CPP.il2cpp_field_get_offset((IntPtr)UnhollowerUtils
                        .GetIl2CppFieldInfoPointerFieldForGeneratedFieldAccessor(avatarManagerField.GetMethod)
                        .GetValue(null));

                    unsafe
                    {
                        var originalMethodPointer = *(IntPtr*)(IntPtr)UnhollowerUtils.GetIl2CppMethodInfoPointerFieldForGeneratedMethod(moveNext).GetValue(null);

                        originalMethodPointer = XrefScannerLowLevel.JumpTargets(originalMethodPointer).First();

                        VoidDelegate originalDelegate = null;

                        void TaskMoveNextPatch(IntPtr taskPtr, IntPtr nativeMethodInfo)
                        {
                            var avatarManager = *(IntPtr*)(taskPtr + fieldOffset - 16);
                            currentlyLoadingAvatar = new VRCAvatarManager(avatarManager);
                            originalDelegate(taskPtr, nativeMethodInfo);
                            currentlyLoadingAvatar = null;
                        }

                        var patchDelegate = new VoidDelegate(TaskMoveNextPatch);
                        ourPinnedDelegates.Add(patchDelegate);

                        MelonUtils.NativeHookAttach((IntPtr)(&originalMethodPointer), Marshal.GetFunctionPointerForDelegate(patchDelegate));
                        originalDelegate = Marshal.GetDelegateForFunctionPointer<VoidDelegate>(originalMethodPointer);
                    }
                }

                Harmony.Patch(typeof(NetworkManager).GetMethod(nameof(NetworkManager.OnLeftRoom)), null, new HarmonyMethod(typeof(ReModCE), "LeftRoomPatch"));

                try
                {
                    var category = MelonPreferences.GetCategory("ReModCE");
                    _HWIDSpoof = (MelonPreferences_Entry<bool>)category.GetEntry("HWIDSpoof");
                    if (_HWIDSpoof.Value)
                    {
                        IntPtr getHwidAddress = IL2CPP.il2cpp_resolve_icall("UnityEngine.SystemInfo::GetDeviceUniqueIdentifier");

                        if (getHwidAddress == IntPtr.Zero)
                        {
                            return;
                        }

                        string oldId = SystemInfo.GetDeviceUniqueIdentifier();

                        GenerateHardwareIdentifier();

                        clientHWIDOriginal = new Il2CppSystem.Object(IL2CPP.ManagedStringToIl2Cpp(oldId));
                        clientHWID = new Il2CppSystem.Object(IL2CPP.ManagedStringToIl2Cpp(GeneratedHWID));

                        unsafe
                        {
                            typeof(MelonUtils).GetMethod(nameof(MelonUtils.NativeHookAttach), BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)!.Invoke(null, new object[] { (IntPtr)(&getHwidAddress), GetLocalPatch(nameof(GetHardwareIdentifier)).method.MethodHandle.GetFunctionPointer() });
                        }
                    }
                }
                catch (Exception e)
                {
                    ReLogger.Msg($"failed to spoof hwid, exception: {e}");
                }

                Harmony.Patch(typeof(VRC.UI.Elements.QuickMenu).GetMethod(nameof(VRC.UI.Elements.QuickMenu.OnEnable)), new HarmonyMethod(typeof(ReModCE), "QuickMenuOpenPatch"));
                Harmony.Patch(typeof(VRC.UI.Elements.QuickMenu).GetMethod(nameof(VRC.UI.Elements.QuickMenu.OnDisable)), new HarmonyMethod(typeof(ReModCE), "QuickMenuClosePatch"));
                GeneralWrapper.InitializeWrappers();

                // include native hooks cuz why not
                ReLogger.Msg(ConsoleColor.Green, $"Successfully completed all {Harmony.GetPatchedMethods().Count() + 5} patches, running client...");
            }
            catch (Exception e)
            {
                ReLogger.Error($"Patching failed! Exception: \n {e}");
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
            visualPage.AddCategory("Menu");

            _uiManager.MainMenu.AddMenuPage("Dynamic Bones", "Access your global dynamic bone settings", ResourceManager.GetSprite("remodce.bone"));
            _uiManager.MainMenu.AddMenuPage("Avatars", "Access avatar related settings", ResourceManager.GetSprite("remodce.hanger"));

            var utilityPage = _uiManager.MainMenu.AddCategoryPage("Utility", "Access miscellaneous settings", ResourceManager.GetSprite("remodce.tools"));
            utilityPage.AddCategory("Quality of Life");

            _uiManager.MainMenu.AddMenuPage("Logging", "Access logging related settings", ResourceManager.GetSprite("remodce.log"));
            _uiManager.MainMenu.AddMenuPage("Hotkeys", "Access hotkey related settings", ResourceManager.GetSprite("remodce.keyboard"));

            var exploitsPage = _uiManager.MainMenu.AddCategoryPage("Exploits", "haha funny vrchat game", ResourceManager.GetSprite("remodce.exploits"));
            exploitsPage.AddCategory("USpeak");
            exploitsPage.AddCategory("Events");
            exploitsPage.AddCategory("Items");
            exploitsPage.AddCategory("Avatar");

            var safetyPage = _uiManager.MainMenu.AddCategoryPage("Safety", "Access protection/safety settings", ResourceManager.GetSprite("remodce.safety"));
            safetyPage.AddCategory("Avatars");
            safetyPage.AddCategory("Photon Events");

            var spoofingPage = _uiManager.MainMenu.AddCategoryPage("Spoofing", "Access settings related to spoofing certain things", ResourceManager.GetSprite("remodce.spoofing"));
            spoofingPage.AddCategory("Hardware");
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
            UpdateNamePlates();
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

        public static void QuickMenuOpenPatch()
        {
            isQuickMenuOpen = true;
        }

        public static void QuickMenuClosePatch()
        {
            isQuickMenuOpen = false;
        }

        public static bool FinalIKPatch(IKSolverHeuristic __instance)
        {
            if (__instance.maxIterations > 64 && Configuration.GetAvatarProtectionsConfig().AntiFinalIKCrash)
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
                var category = MelonPreferences.GetCategory("ReModCE");
                _WorldSpoofEnabled = (MelonPreferences_Entry<bool>)category.GetEntry("WorldSpoofEnabled");
                _WorldSpoof = (MelonPreferences_Entry<bool>)category.GetEntry("WorldSpoof");
                _WorldIdToSpoof = (MelonPreferences_Entry<string>)category.GetEntry("WorldIdToSpoof");
                _InstanceIdToSpoof = (MelonPreferences_Entry<string>)category.GetEntry("InstanceIdToSpoof");
                _WorldSpoofWarn = (MelonPreferences_Entry<bool>)category.GetEntry("WorldSpoofWarn");

                if (_WorldSpoof.Value == false || _WorldSpoofEnabled.Value == false)
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

        private static bool OnOPRaiseEvent(byte __0, ref Il2CppSystem.Object __1, ref RaiseEventOptions __2, ref SendOptions __3)
        {
            try
            {
                ReLogger.Msg($"[OPRaiseEvent {__0}]");
                ReLogger.Msg($"-------------------");
                ReLogger.Msg("RaiseEventOptions:");
                ReLogger.Msg($"Payload: {JsonConvert.SerializeObject(Serialization.FromIL2CPPToManaged<object>(__1))}");
                ReLogger.Msg($"Caching Type: {__2.field_Public_EventCaching_0}");
                ReLogger.Msg($"Receiver Group: {__2.field_Public_ReceiverGroup_0}");
                ReLogger.Msg($"Target Actors {__2.field_Public_ArrayOf_Int32_0}");
                ReLogger.Msg($"Unknown Byte 1: {__2.field_Public_Byte_0}");
                ReLogger.Msg($"Unknown Byte 2: {__2.field_Public_Byte_1}");
                ReLogger.Msg($"Webflag byte: {__2.field_Public_WebFlags_0.field_Public_Byte_0}");
                ReLogger.Msg($"-------------------");
                ReLogger.Msg($"SendOptions:");
                ReLogger.Msg($"Channel: {__3.Channel}");
                ReLogger.Msg($"Delivery Mode: {__3.DeliveryMode}");
                ReLogger.Msg($"Encrypt: {__3.Encrypt}");
                ReLogger.Msg($"Reliable?: {__3.Reliability}");
                ReLogger.Msg($"-------------------");
            }
            catch // this is caused 99.99% of the time because of the event containing something i can't grab (like the payload)
            {
            }

            return true;
        }

        private static bool ReturnFalse()
        {
            return false;
        }

        internal static void GenerateHardwareIdentifier()
        {
            string oldId = SystemInfo.GetDeviceUniqueIdentifier();

            byte[] bytes = new byte[SystemInfo.deviceUniqueIdentifier.Length / 2];

            fastRandom.NextBytes(bytes);
            string newhwid = string.Join("", bytes.Select(it => it.ToString("x2")));

            clientHWID = new Il2CppSystem.Object(IL2CPP.ManagedStringToIl2Cpp(newhwid));
            GeneratedHWID = newhwid;

            ReLogger.Msg($"Generated HWID:");
            ReLogger.Msg($"HWID before changing: {oldId}");
            ReLogger.Msg($"HWID after changing: {newhwid}");
        }

        private static IntPtr GetHardwareIdentifier()
        {
            return true ? clientHWID.Pointer : clientHWIDOriginal.Pointer;
        }

        // this is from decompiled code, that's why a lot of the names look weird.
        // i had to spend A FUCK TON of time fixing decompiled code, mostly explains why 1.0.6 so long to come out.
        // ilspy output also takes values and puts like allowDestroyingAssets: true when it can just be true.
        // weird, but decompiled code funnies below.
        private static IntPtr OnObjectInstantiatedPatch(IntPtr assetPtr, Vector3 pos, Quaternion rot,
            byte allowCustomShaders, byte isUI, byte validate, IntPtr nativeMethodPointer,
            ObjectInstantiateDelegate originalInstantiateDelegate)
        {
            if (WorldUtils.GetCurrentInstance() == null)
            {
                return originalInstantiateDelegate(assetPtr, pos, rot, allowCustomShaders, isUI, validate,
                    nativeMethodPointer);
            }

            if (assetPtr == IntPtr.Zero)
            {
                return originalInstantiateDelegate(assetPtr, pos, rot, allowCustomShaders, isUI, validate,
                    nativeMethodPointer);
            }

            GameObject gameObject = new UnityEngine.Object(assetPtr).TryCast<GameObject>();
            if (gameObject == null)
            {
                return originalInstantiateDelegate(assetPtr, pos, rot, allowCustomShaders, isUI, validate,
                    nativeMethodPointer);
            }

            if (gameObject.name.StartsWith("UserUi") || gameObject.name.StartsWith("WorldUi") ||
                gameObject.name.StartsWith("AvatarUi") || gameObject.name.StartsWith("Holoport"))
            {
                return originalInstantiateDelegate(assetPtr, pos, rot, allowCustomShaders, isUI, validate,
                    nativeMethodPointer);
            }

            bool flag2 = gameObject.name.StartsWith("prefab");
            if (!gameObject.name.StartsWith("_CustomAvatar") && !gameObject.name.Equals("Avatar") &&
                !gameObject.name.Equals("AvatarPrefab") && !flag2)
            {
                return originalInstantiateDelegate(assetPtr, pos, rot, allowCustomShaders, isUI, validate,
                    nativeMethodPointer);
            }

            string newValue = "None";
            string text = "Unknown";
            string newValue2 = "Unknown";
            string newValue3 = "Unknown";
            if (currentlyLoadingAvatar != null)
            {
                newValue =
                    currentlyLoadingAvatar.field_Private_VRCPlayer_0?.prop_Player_0?.prop_APIUser_0?.displayName ??
                    "None";
                text = currentlyLoadingAvatar.field_Private_ApiAvatar_0?.id ?? "Unknown";
                newValue2 = currentlyLoadingAvatar.field_Private_ApiAvatar_0?.name ?? "Unknown";
                newValue3 = currentlyLoadingAvatar.field_Private_ApiAvatar_0?.assetUrl ?? "Unknown";
            }
            else if (flag2)
            {
                int num = gameObject.name.IndexOf('_') + 1;
                int num2 = gameObject.name.LastIndexOf('_');
                text = gameObject.name.Substring(num, num2 - num);

                if (Configuration.GetAvatarProtectionsConfig().WhitelistedAvatars.ContainsKey(text))
                {
                    if (!previouslyCheckedAvatars.Contains(gameObject.Pointer))
                    {
                        previouslyCheckedAvatars.Add(gameObject.Pointer);
                    }
                    return originalInstantiateDelegate(assetPtr, pos, rot, allowCustomShaders, isUI, validate, nativeMethodPointer);
                }
                if (previouslyCheckedAvatars.Contains(gameObject.Pointer))
                {
                    return originalInstantiateDelegate(assetPtr, pos, rot, allowCustomShaders, isUI, validate, nativeMethodPointer);
                }
            }

            if (previouslyCheckedAvatars.Contains(gameObject.Pointer))
            {
                return originalInstantiateDelegate(assetPtr, pos, rot, allowCustomShaders, isUI, validate,
                    nativeMethodPointer);
            }
            previouslyCheckedAvatars.Add(gameObject.Pointer);

            if (Configuration.GetAvatarProtectionsConfig().WhitelistedAvatars.ContainsKey(text))
            {
                ReLogger.Msg($"[Avatar Anti-Crash] Skipping avatar ({text}) due to it being whitelisted.");
                return originalInstantiateDelegate(assetPtr, pos, rot, allowCustomShaders, isUI, validate,
                    nativeMethodPointer);
            }

            bool activeSelf = gameObject.activeSelf;
            gameObject.SetActive(value: false);
            IntPtr intPtr = originalInstantiateDelegate(assetPtr, pos, rot, allowCustomShaders, isUI, validate,
                nativeMethodPointer);
            gameObject.SetActive(activeSelf);
            if (intPtr == IntPtr.Zero)
            {
                return intPtr;
            }

            GameObject gameObject2 = new GameObject(intPtr);
            if (gameObject2 == null)
            {
                return intPtr;
            }

            if (gameObject2.transform.lossyScale.y > Configuration.GetAvatarProtectionsConfig().MaxHeight)
            {
                AntiCrashUtils.DisposeAvatar(gameObject2);
                ReLogger.Msg($"[Anti-Crash] Deleted avatar worn by {newValue} for being too big");
            }

            System.Collections.Generic.List<Component> list =
                GeneralUtils.FindAllComponentsInGameObject<Component>(gameObject2);
            if (list.Count > Configuration.GetAvatarProtectionsConfig().MaxTotalComponents)
            {
                AntiCrashUtils.DisposeAvatar(gameObject2);
                list.Clear();
                ReLogger.Msg($"[Anti-Crash] Deleted avatar worn by {newValue} for having too many components");
            }

            int num3 = 0;
            int num4 = 0;
            int num5 = 0;
            int nukedColliders = 0;
            int num6 = 0;
            int num7 = 0;
            int num8 = 0;
            int nukedConstraints = 0;
            int nukedRigidbodies = 0;
            int currentTransforms = 0;
            int currentColliders = 0;
            int currentSpringJoints = 0;
            int num9 = 0;
            int currentRigidbodies = 0;
            int num10 = 0;
            int currentConstraints = 0;
            int num11 = 0;
            AntiCrashClothPostProcess antiCrashClothPostProcess = new AntiCrashClothPostProcess();
            AntiCrashParticleSystemPostProcess post = new AntiCrashParticleSystemPostProcess();
            AntiCrashDynamicBoneColliderPostProcess antiCrashDynamicBoneColliderPostProcess =
                new AntiCrashDynamicBoneColliderPostProcess();
            AntiCrashDynamicBonePostProcess antiCrashDynamicBonePostProcess = new AntiCrashDynamicBonePostProcess();
            AntiCrashLightSourcePostProcess antiCrashLightSourcePostProcess = new AntiCrashLightSourcePostProcess();
            AntiCrashRendererPostProcess previousProcess = new AntiCrashRendererPostProcess();

            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] == null)
                {
                    continue;
                }

                if (GeneralUtils.GetChildDepth(list[i].transform, gameObject2.transform) > Configuration.GetAvatarProtectionsConfig().MaxDepth)
                {
                    num5++;
                    UnityEngine.Object.DestroyImmediate(list[i].gameObject, allowDestroyingAssets: true);
                    continue;
                }

                for (int j = Configuration.GetAvatarProtectionsConfig().MaxChildren; j < list[i].transform.childCount; j++)
                {
                    num5++;
                    UnityEngine.Object.DestroyImmediate(list[i].transform.GetChild(j).gameObject,
                        allowDestroyingAssets: true);
                }

                MonoBehaviour monoBehaviour = list[i].TryCast<MonoBehaviour>();
                if (monoBehaviour != null)
                {
                    num11++;
                    if (num11 > Configuration.GetAvatarProtectionsConfig().MaxMonobehaviours)
                    {
                        AntiCrashUtils.DisposeAvatar(gameObject2);
                        ReLogger.Msg($"[Anti-Crash] Deleted avatar worn by {newValue} for having too many monobehaviours.");
                        break;
                    }
                }

                Transform transform = list[i].TryCast<Transform>();
                Rigidbody rigidbody = list[i].TryCast<Rigidbody>();
                Collider collider = list[i].TryCast<Collider>();
                Joint joint = list[i].TryCast<Joint>();
                AudioSource audioSource = list[i].TryCast<AudioSource>();
                Cloth cloth = list[i].TryCast<Cloth>();
                ParticleSystem particleSystem = list[i].TryCast<ParticleSystem>();
                DynamicBoneCollider dynamicBoneCollider = list[i].TryCast<DynamicBoneCollider>();
                DynamicBone dynamicBone = list[i].TryCast<DynamicBone>();
                Light light = list[i].TryCast<Light>();
                Renderer renderer = list[i].TryCast<Renderer>();
                Animator animator = list[i].TryCast<Animator>();
                ParentConstraint parentConstraint = list[i].TryCast<ParentConstraint>();
                RotationConstraint rotationConstraint = list[i].TryCast<RotationConstraint>();
                PositionConstraint positionConstraint = list[i].TryCast<PositionConstraint>();
                ScaleConstraint scaleConstraint = list[i].TryCast<ScaleConstraint>();
                LookAtConstraint lookAtConstraint = list[i].TryCast<LookAtConstraint>();
                AimConstraint aimConstraint = list[i].TryCast<AimConstraint>();
                VRCAvatarDescriptor vRCAvatarDescriptor = list[i].TryCast<VRCAvatarDescriptor>();
                VRC_AvatarDescriptor vRC_AvatarDescriptor = list[i].TryCast<VRC_AvatarDescriptor>();
                if (transform != null && Configuration.GetAvatarProtectionsConfig().AntiPhysicsCrash)
                {
                    if (AntiCrashUtils.ProcessTransform(transform, ref currentTransforms))
                    {
                        AntiCrashUtils.DisposeAvatar(gameObject2);
                        ReLogger.Msg($"[Anti-Crash] Deleted avatar worn by {newValue} for being suspected as malicious");
                        break;
                    }
                }
                else if (rigidbody != null && Configuration.GetAvatarProtectionsConfig().AntiPhysicsCrash)
                {
                    AntiCrashUtils.ProcessRigidbody(rigidbody, ref currentRigidbodies, ref nukedRigidbodies);
                }
                else if (collider != null && Configuration.GetAvatarProtectionsConfig().AntiPhysicsCrash)
                {
                    AntiCrashUtils.ProcessCollider(collider, ref currentColliders, ref nukedColliders);
                }
                else if (joint != null && Configuration.GetAvatarProtectionsConfig().AntiPhysicsCrash)
                {
                    if (AntiCrashUtils.ProcessJoint(joint, ref currentSpringJoints))
                    {
                        num6++;
                    }
                }
                else if (audioSource != null && Configuration.GetAvatarProtectionsConfig().AntiAudioCrash)
                {
                    if (num9 >= Configuration.GetAvatarProtectionsConfig().MaxAudioSources)
                    {
                        num7++;
                        UnityEngine.Object.DestroyImmediate(audioSource, allowDestroyingAssets: true);
                    }
                    else
                    {
                        num9++;
                    }
                }
                else if (cloth != null && Configuration.GetAvatarProtectionsConfig().AntiClothCrash)
                {
                    antiCrashClothPostProcess = AntiCrashUtils.ProcessCloth(cloth, antiCrashClothPostProcess);
                }
                else if (particleSystem != null && Configuration.GetAvatarProtectionsConfig().AntiParticleSystemCrash)
                {
                    AntiCrashUtils.ProcessParticleSystem(particleSystem, ref post);
                }
                else if (dynamicBoneCollider != null && Configuration.GetAvatarProtectionsConfig().AntiDynamicBoneCrash)
                {
                    antiCrashDynamicBoneColliderPostProcess = AntiCrashUtils.ProcessDynamicBoneCollider(
                        dynamicBoneCollider, antiCrashDynamicBoneColliderPostProcess.nukedDynamicBoneColliders,
                        antiCrashDynamicBoneColliderPostProcess.dynamicBoneColiderCount);
                }
                else if (dynamicBone != null && Configuration.GetAvatarProtectionsConfig().AntiDynamicBoneCrash)
                {
                    antiCrashDynamicBonePostProcess = AntiCrashUtils.ProcessDynamicBone(dynamicBone,
                        antiCrashDynamicBonePostProcess.nukedDynamicBones,
                        antiCrashDynamicBonePostProcess.dynamicBoneCount);
                }
                else if (light != null && Configuration.GetAvatarProtectionsConfig().AntiLightSourceCrash)
                {
                    antiCrashLightSourcePostProcess = AntiCrashUtils.ProcessLight(light,
                        antiCrashLightSourcePostProcess.nukedLightSources,
                        antiCrashLightSourcePostProcess.lightSourceCount);
                }
                else if (renderer != null)
                {
                    AntiCrashUtils.ProcessRenderer(renderer, ref previousProcess);
                }
                else if (animator != null)
                {
                    if (num10 >= Configuration.GetAvatarProtectionsConfig().MaxAnimators)
                    {
                        num8++;
                        UnityEngine.Object.DestroyImmediate(animator, allowDestroyingAssets: true);
                    }
                    else
                    {
                        num10++;
                    }
                }
                else if (parentConstraint != null && Configuration.GetAvatarProtectionsConfig().AntiConstraintsCrash)
                {
                    AntiCrashUtils.ProcessConstraint(parentConstraint, ref currentConstraints, ref nukedConstraints);
                }
                else if (rotationConstraint != null && Configuration.GetAvatarProtectionsConfig().AntiConstraintsCrash)
                {
                    AntiCrashUtils.ProcessConstraint(rotationConstraint, ref currentConstraints, ref nukedConstraints);
                }
                else if (positionConstraint != null && Configuration.GetAvatarProtectionsConfig().AntiConstraintsCrash)
                {
                    AntiCrashUtils.ProcessConstraint(positionConstraint, ref currentConstraints, ref nukedConstraints);
                }
                else if (scaleConstraint != null && Configuration.GetAvatarProtectionsConfig().AntiConstraintsCrash)
                {
                    AntiCrashUtils.ProcessConstraint(scaleConstraint, ref currentConstraints, ref nukedConstraints);
                }
                else if (lookAtConstraint != null && Configuration.GetAvatarProtectionsConfig().AntiConstraintsCrash)
                {
                    AntiCrashUtils.ProcessConstraint(lookAtConstraint, ref currentConstraints, ref nukedConstraints);
                }
                else if (aimConstraint != null && Configuration.GetAvatarProtectionsConfig().AntiConstraintsCrash)
                {
                    AntiCrashUtils.ProcessConstraint(aimConstraint, ref currentConstraints, ref nukedConstraints);
                }
                else if (vRCAvatarDescriptor != null)
                {
                    if (vRCAvatarDescriptor.expressionsMenu != null)
                    {
                        for (int k = 0; k < vRCAvatarDescriptor.expressionsMenu.controls.Count; k++)
                        {
                            VRCExpressionsMenu.Control control = vRCAvatarDescriptor.expressionsMenu.controls[k];
                            if (control.name.Length > 200)
                            {
                                control.name = control.name.Substring(0, 200);
                                num3++;
                            }

                            if (control.parameter.name.Length > 200)
                            {
                                control.parameter.name = control.parameter.name.Substring(0, 200);
                                num3++;
                            }
                        }
                    }

                    int num12 = 0;
                    Il2CppSystem.Collections.Generic.List<VRCAvatarDescriptor.DebugHash>.Enumerator enumerator =
                        vRCAvatarDescriptor.animationHashSet.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        VRCAvatarDescriptor.DebugHash current = enumerator.Current;
                        if (current.name.Length > 50)
                        {
                            num12++;
                            if (num12 > 15)
                            {
                                gameObject2.GetComponent<Animator>().enabled = false;
                                num8++;
                                break;
                            }
                        }
                    }
                }
                else
                {
                    if (!(vRC_AvatarDescriptor != null) || !(vRC_AvatarDescriptor.CustomStandingAnims != null))
                    {
                        continue;
                    }

                    for (int l = 0; l < vRC_AvatarDescriptor.CustomStandingAnims.animationClips.Count; l++)
                    {
                        AnimationClip animationClip = vRC_AvatarDescriptor.CustomStandingAnims.animationClips[l];
                        if (animationClip.name.Length > 200)
                        {
                            animationClip.name = animationClip.name.Substring(0, 200);
                            num4++;
                        }
                    }
                }
            }

            // god please end my suffering
            // might as well call me yanderedev v2

            if (num5 > 0 || nukedColliders > 0 || num6 > 0 || num7 > 0 || nukedRigidbodies > 0 ||
                nukedConstraints > 0 || antiCrashClothPostProcess.nukedCloths > 0 || post.nukedParticleSystems > 0 ||
                antiCrashDynamicBonePostProcess.nukedDynamicBones > 0 ||
                antiCrashDynamicBoneColliderPostProcess.nukedDynamicBoneColliders > 0 ||
                antiCrashLightSourcePostProcess.nukedLightSources > 0 || num3 > 0 || previousProcess.nukedMeshes > 0 ||
                previousProcess.nukedMaterials > 0 || previousProcess.nukedShaders > 0 ||
                previousProcess.removedBlendshapeKeys)
            {
                ReLogger.Msg($"[Anti-Crash] Checked avatar worn by {newValue}, named {newValue2}, ID: {text}");
                if (num5 > 0)
                {
                    ReLogger.Msg($"Removed Transforms: {num5} total", ConsoleColor.Blue);
                }

                if (nukedColliders > 0)
                {
                    ReLogger.Msg($"Removed Colliders: {nukedColliders} total", ConsoleColor.Blue);
                }

                if (num6 > 0)
                {
                    ReLogger.Msg($"Removed SpringJoints: {num6} total", ConsoleColor.Blue);
                }

                if (num7 > 0)
                {
                    ReLogger.Msg($"Removed AudioSources: {num7} total", ConsoleColor.Blue);
                }

                if (num8 > 0)
                {
                    ReLogger.Msg($"Removed Animators: {num8} total", ConsoleColor.Blue);
                }

                if (nukedConstraints > 0)
                {
                    ReLogger.Msg($"Removed Constraints: {nukedConstraints} total", ConsoleColor.Blue);
                }

                if (antiCrashClothPostProcess.nukedCloths > 0)
                {
                    ReLogger.Msg($"Removed Cloth: {antiCrashClothPostProcess.nukedCloths} total", ConsoleColor.Blue);
                }

                if (post.nukedParticleSystems > 0)
                {
                    ReLogger.Msg($"Removed ParticleSystems: {post.nukedParticleSystems} total", ConsoleColor.Blue);
                }

                if (antiCrashDynamicBonePostProcess.nukedDynamicBones > 0)
                {
                    ReLogger.Msg($"Removed DynamicBones: {antiCrashDynamicBonePostProcess.nukedDynamicBones} total",
                        ConsoleColor.Blue);
                }

                if (antiCrashDynamicBoneColliderPostProcess.nukedDynamicBoneColliders > 0)
                {
                    ReLogger.Msg(
                        $"Removed DynamicBoneColliders: {antiCrashDynamicBoneColliderPostProcess.nukedDynamicBoneColliders} total",
                        ConsoleColor.Blue);
                }

                if (antiCrashLightSourcePostProcess.nukedLightSources > 0)
                {
                    ReLogger.Msg($"Removed LightSources: {antiCrashLightSourcePostProcess.nukedLightSources} total",
                        ConsoleColor.Blue);
                }

                if (num3 > 0)
                {
                    ReLogger.Msg($"Removed Expression Menus: {num3} total", ConsoleColor.Blue);
                }

                if (num4 > 0)
                {
                    ReLogger.Msg($"Removed Animation Clips: {num4} total", ConsoleColor.Blue);
                }

                if (previousProcess.nukedMeshes > 0)
                {
                    ReLogger.Msg($"Removed Meshes: {previousProcess.nukedMeshes} total", ConsoleColor.Blue);
                }

                if (previousProcess.nukedMaterials > 0)
                {
                    ReLogger.Msg($"Removed Materials: {previousProcess.nukedMaterials} total", ConsoleColor.Blue);
                }

                if (previousProcess.nukedShaders > 0)
                {
                    ReLogger.Msg($"Removed Shaders: {previousProcess.nukedShaders} total", ConsoleColor.Blue);
                }

                if (previousProcess.removedBlendshapeKeys)
                {
                    ReLogger.Msg("Avatar contained invalid blendshape keys.", ConsoleColor.Blue);
                }
            }

            return intPtr;
        }

        public static void LeftRoomPatch()
        {
            previouslyCheckedAvatars.Clear();
        }

        private static IntPtr OnAvatarDownloadStartPatch(IntPtr thisPtr, IntPtr apiAvatar, IntPtr downloadContainer,
            bool unknownBool, IntPtr nativeMethodPointer)
        {
            try
            {
                var category = MelonPreferences.GetCategory("ReModCE");
                _LogAvi = (MelonPreferences_Entry<bool>)category.GetEntry("LogAvi");

                ApiAvatar apiAvatar2 = ((apiAvatar != IntPtr.Zero) ? new ApiAvatar(apiAvatar) : null);
                if (apiAvatar2 == null)
                {
                    return onAvatarDownloadStart(thisPtr, apiAvatar, downloadContainer, unknownBool, nativeMethodPointer);
                }

                if (Configuration.GetAvatarProtectionsConfig().WhitelistedAvatars.ContainsKey(apiAvatar2.id))
                {
                    ReLogger.Msg("Downloading whitelisted avatar: " + apiAvatar2.id + " (" + apiAvatar2.name + ") | " + apiAvatar2.authorId + " (" + apiAvatar2.authorName + ")", ConsoleColor.Cyan);
                    return onAvatarDownloadStart(thisPtr, apiAvatar, downloadContainer, unknownBool, nativeMethodPointer);
                }

                if (Configuration.GetAvatarProtectionsConfig().BlacklistedAvatars.ContainsKey(apiAvatar2.id))
                {
                    ReLogger.Msg("Prevented blacklisted avatar from loading: " + apiAvatar2.id + " (" + apiAvatar2.name + ") | " + apiAvatar2.authorId + " (" + apiAvatar2.authorName + ")", ConsoleColor.Cyan);
                    return onAvatarDownloadStart(thisPtr, GeneralUtils.robotAvatar.Pointer, downloadContainer, unknownBool, nativeMethodPointer);
                }

                if (_LogAvi.Value)
                {
                    ReLogger.Msg("Downloading avatar: " + apiAvatar2.id + " (" + apiAvatar2.name + ") | " + apiAvatar2.authorId + " (" + apiAvatar2.authorName + ")");
                }
            }
            catch (Exception e)
            {
                ReLogger.Error("Download avatar patch had an error! Exception:", e);
            }
            return onAvatarDownloadStart(thisPtr, apiAvatar, downloadContainer, unknownBool, nativeMethodPointer);
        }

        private static List<string> DebugLogs = new List<string>();
        private static int duplicateCount = 1;
        private static string lastMsg = "";
        public static void LogDebug(string message)
        {
            if (message == lastMsg)
            {
                DebugLogs.RemoveAt(DebugLogs.Count - 1);
                duplicateCount++;
                DebugLogs.Add($"<color=white><b>[<color=#8c99e1>Odious</color>] {message} <color=red><i>x{duplicateCount}</i></color></b></color>");
            }
            else
            {
                lastMsg = message;
                duplicateCount = 1;
                DebugLogs.Add($"<color=white><b>[<color=#8c99e1>Odious</color>] {message}</b></color>");
                if (DebugLogs.Count == 25)
                {
                    DebugLogs.RemoveAt(0);
                }
            }
            DebugMenuComponent.debugLog.text.text = string.Join("\n", DebugLogs.Take(25));
            DebugMenuComponent.debugLog.text.enableWordWrapping = false;
            DebugMenuComponent.debugLog.text.fontSizeMin = 25;
            DebugMenuComponent.debugLog.text.fontSizeMax = 30;
            DebugMenuComponent.debugLog.text.alignment = TMPro.TextAlignmentOptions.Left;
            DebugMenuComponent.debugLog.text.verticalAlignment = TMPro.VerticalAlignmentOptions.Top;
        }
    }
}
