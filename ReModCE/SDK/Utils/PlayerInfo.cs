using TMPro;
using UnityEngine;
using VRC;
using VRC.Core;
using VRC.SDKBase;

namespace ReModCE.SDK.Utils
{
    internal class PlayerInfo
    {
        internal int actorId;

        internal string id;

        internal string displayName;

        internal bool isLocalPlayer;

        internal bool isInstanceMaster;

        internal bool isVRChatStaff;

        internal bool isLegendaryUser;

        internal bool isVRUser;

        internal bool isQuestUser;

        internal bool isClientUser;

        internal bool blockedLocalPlayer;

        internal PlayerRankStatus rankStatus;

        internal Player player;

        internal VRCPlayerApi playerApi;

        internal VRCPlayer vrcPlayer;

        internal APIUser apiUser;

        internal VRCNetworkBehaviour networkBehaviour;

        internal USpeaker uSpeaker;

        internal GamelikeInputController input;

        internal bool detectedFirstGround;

        internal int airstuckDetections;

        internal int lastNetworkedUpdatePacketNumber;

        internal float lastNetworkedUpdateTime;

        internal float lastNetworkedVoicePacket;

        internal int lagBarrier;

        internal GameObject nameplateCanvas;

        internal GameObject customNameplateObject;

        internal RectTransform customNameplateTransform;

        internal TextMeshProUGUI customNameplateText;

        internal bool IsFriends()
        {
            return APIUser.IsFriendsWith(id);
        }

        internal int GetPing()
        {
            return vrcPlayer.prop_PlayerNet_0.prop_Int16_0;
        }

        internal int GetFPS()
        {
            return (int)(1000f / (float)(int)vrcPlayer.prop_PlayerNet_0.field_Private_Byte_0);
        }

        internal bool IsGrounded()
        {
            return playerApi.IsPlayerGrounded();
        }

        internal Vector3 GetVelocity()
        {
            return playerApi.GetVelocity();
        }

        internal GameObject GetAvatar()
        {
            return vrcPlayer.field_Internal_GameObject_0;
        }
	}
}
