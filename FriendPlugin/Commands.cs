using Sandbox.Game.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Torch.Commands;
using Torch.Commands.Permissions;
using VRage.Game;
using VRage.Game.ModAPI;
using VRageMath;

namespace FriendCommand
{
    [Category("friend")]
    public class Commands : CommandModule
    {

        public static Dictionary<long, List<long>> friendRequests = new Dictionary<long, List<long>>();

        [Command("add", "send friend request")]
        [Permission(MyPromoteLevel.None)]
        public void SendFriendRequest(string factiontag)
        {
            MyFaction target = MySession.Static.Factions.TryGetFactionByTag(factiontag);
            if (target == null)
            {
                Context.Respond("Target faction does not exist", "FP");
                return;
            }
            if (FacUtils.GetPlayersFaction(Context.Player.Identity.IdentityId) != null)
            {
                MyFaction fac = FacUtils.GetPlayersFaction(Context.Player.Identity.IdentityId) as MyFaction;

                if (friendRequests.TryGetValue(target.FactionId, out List<long> temp))
                {
                    if (temp.Contains(fac.FactionId))
                    {
                        Context.Respond("Target faction already has a request!", "FP");
                        return;
                    }
                }
                if (IsLeaderOrFounder(fac, Context.Player.Identity.IdentityId))
                {
                    if (friendRequests.TryGetValue(target.FactionId, out List<long> temp2))
                    {
                        temp2.Add(fac.FactionId);
                        friendRequests[target.FactionId] = temp2;
                    }
                    else
                    {
                        List<long> t = new List<long>();
                        t.Add(fac.FactionId);
                        friendRequests.Add(target.FactionId, t);
                    }
                    Context.Respond("Friend request sent!", "FP");
                    foreach (MyFactionMember mb in target.Members.Values)
                    {


                        ulong steamid = MySession.Static.Players.TryGetSteamId(mb.PlayerId);
                        if (steamid != 0)
                        {
                            Core.SendMessage("FP", String.Format("{0} has sent a friend request, use !friend requests to see requests", fac.Tag) , Color.DarkGreen, (long)steamid);
                        }

                    }
                }
                else
                {
                    Context.Respond("You are not a leader or founder!", "FP");
                }
            }
        }

        public static Boolean IsLeaderOrFounder(MyFaction fac, long id)
        {
            if (fac.IsLeader(id) || fac.IsFounder(id))
            {
                return true;
            }

            return false;
        }

        [Command("requests", "list friend requests")]
        [Permission(MyPromoteLevel.None)]
        public void ListFriendRequest()
        {
            if (FacUtils.GetPlayersFaction(Context.Player.Identity.IdentityId) != null)
            {
                MyFaction fac = FacUtils.GetPlayersFaction(Context.Player.Identity.IdentityId) as MyFaction;
                if (friendRequests.TryGetValue(fac.FactionId, out List<long> temp))
                {
                    int i = 0;
                    foreach (long l in temp)
                    {
                        //this is a stupid way to do this, but im too lazy to write the other way
                        i++;
                        MyFaction otherFac = MySession.Static.Factions.TryGetFactionById(l) as MyFaction;
                        if (otherFac != null)
                        {
                            Context.Respond(String.Format("[{0}] : {1}", i, otherFac.Tag), "FP");
                        }
                    }
                }
                Context.Respond("Accept a request with !friend accept #", Color.Green, "FP");
            }
            else
            {
                Context.Respond("You are not a member of a faction!");
            }

        }

        [Command("accept", "accept friend request")]
        [Permission(MyPromoteLevel.None)]
        public void AcceptFriendRequest(string number)
        {
            int num = 0;
            try
            {
                num = int.Parse(number);
            }
            catch (Exception)
            {
                Context.Respond("You must enter the number of the request from !friend requests", "FP");
                return;
            }


            if (FacUtils.GetPlayersFaction(Context.Player.Identity.IdentityId) != null)
            {
                MyFaction fac = FacUtils.GetPlayersFaction(Context.Player.Identity.IdentityId) as MyFaction;
                if (IsLeaderOrFounder(fac, Context.Player.Identity.IdentityId))
                {
                    if (friendRequests.TryGetValue(fac.FactionId, out List<long> temp))
                    {
                        if (num <= 0)
                        {
                            Context.Respond("Number must be positive!", "FP");
                            return;
                        }
                        if (temp.Count >= num)
                        {
                            MyFaction target = MySession.Static.Factions.TryGetFactionById(temp[num - 1]) as MyFaction;
                            if (target != null)
                            {
                                Core.DoFriendlyUpdate(fac.FactionId, target.FactionId);
                                MySession.Static.Factions.SetReputationBetweenFactions(fac.FactionId, target.FactionId, 1500);
                                temp.Remove(num - 1);
                                friendRequests[fac.FactionId] = temp;
                                Context.Respond("Accepted request!", "FP");
                                foreach (MyFactionMember mb in target.Members.Values)
                                {


                                    ulong steamid = MySession.Static.Players.TryGetSteamId(mb.PlayerId);
                                    if (steamid != 0)
                                    {
                                        Core.SendMessage("Friend Plugin", String.Format("{0} has accepted the friend request!", fac.Tag), Color.DarkGreen, (long)steamid);
                                    }

                                }
                            }
                            else
                            {
                                Context.Respond("That request is no longer valid.", "FP");
                                return;
                            }
                        }
                        else
                        {
                            Context.Respond("You do not have a request for that number!", "FP");
                            return;
                        }
                    }
                    else
                    {
                        Context.Respond("You do not have any requests.", "FP");
                        return;
                    }
                }
                else
                {
                    Context.Respond("You are not a leader or founder.", "FP");
                }
            }

        }
    }
}

