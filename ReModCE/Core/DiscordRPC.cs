using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ReModCE.Core
{
    internal class DiscordRPC
    {
        [DllImport("VRChat_Data/Managed/discord-rpc", CallingConvention = CallingConvention.Cdecl, EntryPoint = "Discord_Initialize")]
        public static extern void Initialize(string applicationId, ref DiscordRPC.EventHandlers handlers, bool autoRegister, string optionalSteamId);

        [DllImport("VRChat_Data/Managed/discord-rpc", CallingConvention = CallingConvention.Cdecl, EntryPoint = "Discord_RunCallbacks")]
        public static extern void RunCallbacks();

        [DllImport("VRChat_Data/Managed/discord-rpc", CallingConvention = CallingConvention.Cdecl, EntryPoint = "Discord_Shutdown")]
        public static extern void Shutdown();

        [DllImport("VRChat_Data/Managed/discord-rpc", CallingConvention = CallingConvention.Cdecl, EntryPoint = "Discord_UpdatePresence")]
        public static extern void UpdatePresence(ref DiscordRPC.RichPresence presence);


        internal static void Initialize(string v1, ref object handlers, bool v2, object p)
        {
            throw new NotImplementedException();
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void DisconnectedCallback(int errorCode, string message);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void ErrorCallback(int errorCode, string message);

        public struct EventHandlers
        {
            public DiscordRPC.ReadyCallback readyCallback;

            public DiscordRPC.DisconnectedCallback disconnectedCallback;

            public DiscordRPC.ErrorCallback errorCallback;
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void ReadyCallback();

        [Serializable]
        public struct RichPresence
        {
            public string state;

            public string details;

            public long startTimestamp;

            public long endTimestamp;

            public string largeImageKey;

            public string largeImageText;

            public string smallImageKey;

            public string smallImageText;

            public string partyId;

            public int partySize;

            public int partyMax;

            public string matchSecret;

            public string joinSecret;

            public string spectateSecret;
            public string button;

            public bool instance;
        }
    }
}
