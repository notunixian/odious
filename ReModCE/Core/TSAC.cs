using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using MelonLoader;
using ReMod.Core;
using ReModCE.Loader;
using UnityEngine.SceneManagement;
using VRC.Core;

namespace ReModCE.Core
{
    internal class TSAC : ModComponent
    {
        private static readonly Dictionary<string, int> OurOffsets = new()
        {
            { "aCEmIwSIcjYriBQDFjQlpTNNW1/kA8Wlbkqelmt1USOMB09cnKwK7QWyOulz9d7DEYJh4+vO0Ldv8gdH+dZCrg==", 0x819130 }, // U2018.4.20 non-dev
            { "5dkhl/dWeTREXhHCIkZK17mzZkbjhTKlxb+IUSk+YaWzZrrV+G+M0ekTOEGjZ4dJuB4O3nU/oE3dycXWeJq9uA==", 0x79B3F0 }, // U2019.4.28 non-dev
            { "MV6xP7theydao4ENbGi6BbiBxdZsgGOBo/WrPSeIqh6A/E00NImjUNZn+gL+ZxzpVbJms7nUb6zluLL3+aIcfg==", 0x79C060 }, // U2019.4.29 non-dev
            { "ccZ4F7iE7a78kWdXdMekJzP7/ktzS5jOOS8IOITxa1C5Jg2TKxC0/ywY8F0o9I1vZHsxAO4eh7G2sOGzsR/+uQ==", 0x79CEE0 }, // U2019.4.30 non-dev
            { "sgZUlX3+LSHKnTiTC+nXNcdtLOTrAB1fNjBLOwDdKzCyndlFLAdL0udR4S1szTC/q5pnFhG3Kdspsj5jvwLY1A==", 0x79F070 }, // U2019.4.31 non-dev
        };

        [UnmanagedFunctionPointer(CallingConvention.FastCall)]
        private delegate void FindAndLoadUnityPlugin(IntPtr name, out IntPtr loadedModule, byte bEnableSomeDebug);

        private readonly ShaderFilterApi _filterApi;
        public TSAC()
        {
            string unityPlayerHash;
            {
                using var sha = SHA512.Create();
                using var unityPlayerStream = File.OpenRead("UnityPlayer.dll");
                unityPlayerHash = Convert.ToBase64String(sha.ComputeHash(unityPlayerStream));
            }

            if (!OurOffsets.TryGetValue(unityPlayerHash, out var offset))
            {
                ReLogger.Error($"Unknown UnityPlayer hash: {unityPlayerHash}");
                ReLogger.Error("The mod will not work");
                return;
            }

            var pluginsPath = MelonUtils.GetGameDataDirectory() + "/Plugins";
            var deeperPluginsPath = Path.Combine(pluginsPath, "x86_64");
            if (Directory.Exists(deeperPluginsPath))
            {
                pluginsPath = deeperPluginsPath;
            }

            const string dllName = ShaderFilterApi.DllName + ".dll";

            try
            {
                var ourAssembly = Assembly.GetExecutingAssembly();
                var resources = ourAssembly.GetManifestResourceNames();
                using var fileStream = File.Open(pluginsPath + "/" + dllName, FileMode.Create, FileAccess.Write);
                foreach (var resource in resources)
                {
                    if (!resource.EndsWith(".dll"))
                        continue;

                    var stream = ourAssembly.GetManifestResourceStream(resource);

                    stream.CopyTo(fileStream);
                }
            }
            catch (IOException ex)
            {
                ReLogger.Warning("Failed to write native unity plugin; will attempt loading it anyway. This is normal if you're running multiple instances of VRChat");
                MelonDebug.Msg(ex.ToString());
            }

            var process = Process.GetCurrentProcess();
            foreach (ProcessModule module in process.Modules)
            {
                if (!module.FileName.Contains("UnityPlayer"))
                    continue;

                var loadLibraryAddress = module.BaseAddress + offset;
                var findAndLoadUnityPlugin = Marshal.GetDelegateForFunctionPointer<FindAndLoadUnityPlugin>(loadLibraryAddress);

                var strPtr = Marshal.StringToHGlobalAnsi(ShaderFilterApi.DllName);

                findAndLoadUnityPlugin(strPtr, out var loaded, 1);

                if (loaded == IntPtr.Zero)
                {
                    ReLogger.Error("Module load failed");
                    return;
                }

                _filterApi = new ShaderFilterApi(loaded);

                Marshal.FreeHGlobal(strPtr);

                break;
            }

            var category = MelonPreferences.CreateCategory("True Shader Anticrash");

            var loopsEnabled = category.CreateEntry("LimitLoops", true, "Limit loops");
            var geometryEnabled = category.CreateEntry("LimitGeometry", true, "Limit geometry shaders");
            var tessEnabled = category.CreateEntry("LimitTesselation", true, "Limit tesselation");

            IEnumerator WaitForRoomManagerAndUpdate()
            {
                while (RoomManager.field_Internal_Static_ApiWorldInstance_0 == null)
                    yield return null;
                UpdateLimiters();
            }

            void UpdateLimiters()
            {
                var room = RoomManager.field_Internal_Static_ApiWorldInstance_0;
                if (room == null)
                {
                    MelonCoroutines.Start(WaitForRoomManagerAndUpdate());
                    return;
                }

                _filterApi.SetFilteringState(loopsEnabled.Value, geometryEnabled.Value, tessEnabled.Value);
            }

            loopsEnabled.OnValueChanged += (_, _) => UpdateLimiters();
            geometryEnabled.OnValueChanged += (_, _) => UpdateLimiters();
            tessEnabled.OnValueChanged += (_, _) => UpdateLimiters();

            var maxLoopIterations = category.CreateEntry("MaxLoopIterations", 128, "Max loop iterations");
            maxLoopIterations.OnValueChanged += (_, value) => _filterApi.SetLoopLimit(value);

            var maxGeometry = category.CreateEntry("MaxGeometryOutputs", 60, "Max geometry shader outputs");
            maxGeometry.OnValueChanged += (_, value) => _filterApi.SetLoopLimit(value);

            var maxTess = category.CreateEntry("MaxTesselation", 5f, "Max tesselation power");
            maxTess.OnValueChanged += (_, value) => _filterApi.SetMaxTesselationPower(value);

            SceneManager.add_sceneLoaded(new Action<Scene, LoadSceneMode>((sc, _) =>
            {
                if (sc.buildIndex == -1)
                    UpdateLimiters();
            }));

            SceneManager.add_sceneUnloaded(new Action<Scene>(_ =>
            {
                _filterApi.SetFilteringState(false, false, false);
            }));

            UpdateLimiters();
            _filterApi.SetMaxTesselationPower(maxTess.Value);
            _filterApi.SetLoopLimit(maxLoopIterations.Value);
            _filterApi.SetGeometryLimit(maxGeometry.Value);
        }


    }
}
