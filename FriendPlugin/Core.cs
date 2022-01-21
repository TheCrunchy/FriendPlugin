using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Torch;
using Torch.API;
using Torch.API.Plugins;
using Torch.Managers;
using Torch.API.Managers;
using Torch.Session;
using Torch.API.Session;
using Torch.Managers.PatchManager;
using System.Reflection;
using Sandbox.Game.Entities.Cube;
using System.IO;
using Sandbox.Game.GameSystems;
using Sandbox.Game.World;
using VRage.Game.ModAPI;
using Sandbox.Engine.Multiplayer;
using Sandbox.Game.Multiplayer;
using NLog;
using VRageMath;

namespace FriendCommand
{
    public class Core : TorchPluginBase
    {
        public override void Init(ITorchBase torch)
        {
            base.Init(torch);

            var sessionManager = Torch.Managers.GetManager<TorchSessionManager>();

            if (sessionManager != null)
            {
                sessionManager.SessionStateChanged += SessionChanged;
            }

   

        }
        public static void SendMessage(string author, string message, Color color, long steamID)
        {


            Logger _chatLog = LogManager.GetLogger("Chat");
            ScriptedChatMsg scriptedChatMsg1 = new ScriptedChatMsg();
            scriptedChatMsg1.Author = author;
            scriptedChatMsg1.Text = message;
            scriptedChatMsg1.Font = "White";
            scriptedChatMsg1.Color = color;
            scriptedChatMsg1.Target = Sync.Players.TryGetIdentityId((ulong)steamID);
            ScriptedChatMsg scriptedChatMsg2 = scriptedChatMsg1;
            MyMultiplayerBase.SendScriptedChatMessage(ref scriptedChatMsg2);



        }


        public static MethodInfo sendChange;

        public void SetupFriendMethod()
        {
            Type FactionCollection = MySession.Static.Factions.GetType().Assembly.GetType("Sandbox.Game.Multiplayer.MyFactionCollection");
            sendChange = FactionCollection?.GetMethod("SendFactionChange", BindingFlags.NonPublic | BindingFlags.Static);
        }

        public static void DoFriendlyUpdate(long firstId, long SecondId)
        {
            MyFactionStateChange change = MyFactionStateChange.SendFriendRequest;
            MyFactionStateChange change2 = MyFactionStateChange.AcceptFriendRequest;
            List<object[]> Input = new List<object[]>();
            object[] MethodInput = new object[] { change, firstId, SecondId, 0L };
            sendChange?.Invoke(null, MethodInput);
            object[] MethodInput2 = new object[] { change2, SecondId, firstId, 0L };
            sendChange?.Invoke(null, MethodInput2);

        }

        private void SessionChanged(ITorchSession session, TorchSessionState newState)
        {
            if (newState == TorchSessionState.Loaded)
            {
                SetupFriendMethod();
            }
        }



    }
}

