using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReModCE.SDK.Utils
{
    public enum PlayerRankStatus : short
    {
        Unknown,
        Local,
        Visitor,
        NewUser,
        User,
        Known,
        Trusted,
        Veteran,
        VRChatTeam,
        Friend
    }
}
