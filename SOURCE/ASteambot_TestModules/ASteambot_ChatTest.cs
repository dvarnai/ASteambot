﻿using ASteambotInterfaces;
using SteamKit2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASteambot_TestModules
{
    public class ASteambot_ChatTest : IASteambotChat
    {
        public bool HandleInvitation(SteamFriends steamFriends, SteamID InvitedID, SteamID ChatRoomID, SteamID PatronID, EChatRoomType ChatRoomType, SteamID FriendChatID, string ChatRoomName, GameID GameID, out string translationSentence)
        {
            if (ChatRoomType == EChatRoomType.Lobby)
            {
                translationSentence = "BOT_GAME_INVITE";

                steamFriends.RemoveFriend(PatronID);

                return false;
            }

            translationSentence = "";
            return false;
        }

        public bool HandleMessage(SteamFriends steamFriends, SteamID partenar, EChatEntryType msgtype, string message, out string translationSentence)
        {
            translationSentence = "";
            return false;
        }
    }
}
