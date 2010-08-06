using System;
using System.Linq;
using System.Text;
using DemoGame.Server.Guilds;
using NetGore;
using NetGore.Features.Groups;
using NetGore.Features.Guilds;
using NetGore.World;
using SFML.Graphics;

// ReSharper disable SuggestBaseTypeForParameter
// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedParameter.Local

namespace DemoGame.Server
{
    /// <summary>
    /// Handles processing what Users say.
    /// </summary>
    public class SayHandler : SayHandlerBase<User>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SayHandler"/> class.
        /// </summary>
        /// <param name="server">Server that the commands are coming from.</param>
        public SayHandler(Server server) : base(new SayCommands(server))
        {
        }

        /// <summary>
        /// When overridden in the derived class, handles the output from a command.
        /// </summary>
        /// <param name="user">The user that the command came from.</param>
        /// <param name="text">The output text from the command. Will not be null or empty.</param>
        protected override void HandleCommandOutput(User user, string text)
        {
            ThreadAsserts.IsMainThread();

            using (var pw = ServerPacket.Chat(text))
            {
                user.Send(pw);
            }
        }

        /// <summary>
        /// When overridden in the derived class, handles text that was not a command.
        /// </summary>
        /// <param name="user">The user the <paramref name="text"/> came from.</param>
        /// <param name="text">The text that wasn't a command.</param>
        protected override void HandleNonCommand(User user, string text)
        {
            ThreadAsserts.IsMainThread();

            using (var pw = ServerPacket.ChatSay(user.Name, user.MapEntityIndex, text))
            {
                user.Map.SendToArea(user, pw);
            }
        }

        /// <summary>
        /// The actual class that handles the Say commands.
        /// </summary>
        public class SayCommands : ISayCommands<User>
        {
            static readonly GuildManager _guildManager = GuildManager.Instance;
            static readonly GuildSettings _guildSettings = GuildSettings.Instance;

            readonly Server _server;

            /// <summary>
            /// Initializes a new instance of the <see cref="SayCommands"/> class.
            /// </summary>
            /// <param name="server">The Server that the commands will come from.</param>
            public SayCommands(Server server)
            {
                if (server == null)
                    throw new ArgumentNullException("server");

                _server = server;
            }

            public IGroupManager GroupManager
            {
                get { return Server.GroupManager; }
            }

            /// <summary>
            /// Gets the Server that the commands are coming from.
            /// </summary>
            public Server Server
            {
                get { return _server; }
            }

            /// <summary>
            /// Gets the World that the User belongs to.
            /// </summary>
            public World World
            {
                get { return Server.World; }
            }

            /// <summary>
            /// Sends a message to everyone online.
            /// </summary>
            /// <param name="message">The message to send.</param>
            [SayCommand("Shout")]
            public void Shout(string message)
            {
                using (var pw = ServerPacket.SendMessage(GameMessage.CommandShout, User.Name, message))
                {
                    World.Send(pw);
                }
            }

            /// <summary>
            /// Sends a message to a single user.
            /// </summary>
            /// <param name="userName">The name of the user to whisper to.</param>
            /// <param name="message">The message to send to the user.</param>
            [SayCommand("Tell")]
            [SayCommand("Whisper")]
            public void Tell(string userName, string message)
            {
                // Check for a message to tell
                if (string.IsNullOrEmpty(userName))
                {
                    // Invalid message
                    using (var pw = ServerPacket.SendMessage(GameMessage.CommandTellNoName))
                    {
                        User.Send(pw);
                    }
                    return;
                }

                // Find the user to tell
                if (string.IsNullOrEmpty(message))
                {
                    // No or invalid message
                    using (var pw = ServerPacket.SendMessage(GameMessage.CommandTellNoMessage))
                    {
                        User.Send(pw);
                    }
                    return;
                }

                var target = World.FindUser(userName);

                // Check if the target user is available or not
                if (target != null)
                {
                    // Message to sender ("You tell...")
                    using (var pw = ServerPacket.SendMessage(GameMessage.CommandTellSender, target.Name, message))
                    {
                        User.Send(pw);
                    }

                    // Message to receivd ("X tells you...")
                    using (var pw = ServerPacket.SendMessage(GameMessage.CommandTellReceiver, User.Name, message))
                    {
                        target.Send(pw);
                    }
                }
                else
                {
                    // User not found
                    using (var pw = ServerPacket.SendMessage(GameMessage.CommandTellInvalidUser, userName))
                    {
                        User.Send(pw);
                    }
                }
            }

            #region Helper methods

            /// <summary>
            /// Checks if the user meets the required guild rank.
            /// </summary>
            /// <param name="requiredRank">The required guild rank.</param>
            /// <returns>If false, the command should be aborted.</returns>
            bool CheckGuildPermissions(GuildRank requiredRank)
            {
                if (((IGuildMember)User).GuildRank < requiredRank)
                {
                    User.Send(GameMessage.GuildInsufficientPermissions, _guildSettings.GetRankName(requiredRank));
                    return false;
                }

                return true;
            }

            /// <summary>
            /// Handles kicking someone out of a guild.
            /// </summary>
            /// <param name="target">The <see cref="IGuildMember"/> being kicked out of the guild.</param>
            /// <param name="userName">The name of the <paramref name="target"/>.</param>
            void GuildMemberPerformer_GuildKick(IGuildMember target, string userName)
            {
                if (target == null)
                {
                    User.Send(GameMessage.GuildKickFailedInvalidUser, userName);
                    return;
                }

                if (target.Guild != User.Guild)
                {
                    User.Send(GameMessage.GuildKickFailedNotInGuild, target.Name);
                    return;
                }

                if (target.GuildRank > ((IGuildMember)User).GuildRank)
                {
                    User.Send(GameMessage.GuildKickFailedTooHighRank, target.Name);
                    return;
                }

                if (!User.Guild.TryKickMember(User, target))
                {
                    User.Send(GameMessage.GuildKickFailedUnknownReason, target.Name);
                    return;
                }

                User.Send(GameMessage.GuildKick, target.Name);
            }

            /// <summary>
            /// Requires the user to not be in a group.
            /// </summary>
            /// <returns>If false, the command should be aborted.</returns>
            bool RequireInGroup()
            {
                if (((IGroupable)User).Group == null)
                {
                    User.Send(GameMessage.InvalidCommandMustBeInGroup);
                    return false;
                }

                return true;
            }

            /// <summary>
            /// Requires the user to not be in a group.
            /// </summary>
            /// <returns>If false, the command should be aborted.</returns>
            bool RequireNotInGroup()
            {
                if (((IGroupable)User).Group != null)
                {
                    User.Send(GameMessage.InvalidCommandMustNotBeInGroup);
                    return false;
                }

                return true;
            }

            /// <summary>
            /// Requires the user to have the specified permission level.
            /// </summary>
            /// <param name="level">The require <see cref="UserPermissions"/> level.</param>
            /// <returns>If false, the command should be aborted.</returns>
            bool RequirePermissionLevel(UserPermissions level)
            {
                if (User.Permissions.IsSet(level))
                    return true;

                User.Send(GameMessage.InsufficientPermissions);
                return false;
            }

            /// <summary>
            /// Requires the user to be in a guild.
            /// </summary>
            /// <returns>If false, the command should be aborted.</returns>
            bool RequireUserInGuild()
            {
                if (User.Guild == null)
                {
                    using (var pw = ServerPacket.SendMessage(GameMessage.InvalidCommandMustBeInGuild))
                    {
                        User.Send(pw);
                    }

                    return false;
                }

                return true;
            }

            /// <summary>
            /// Requires the user to not be in a guild.
            /// </summary>
            /// <returns>If false, the command should be aborted.</returns>
            bool RequireUserNotInGuild()
            {
                if (User.Guild != null)
                {
                    using (var pw = ServerPacket.SendMessage(GameMessage.InvalidCommandMustNotBeInGuild))
                    {
                        User.Send(pw);
                    }

                    return false;
                }

                return true;
            }

            /// <summary>
            /// Sends a chat message to the <see cref="User"/>. This is provided purely for convenience since it can
            /// become quite redundant having to constantly create the <see cref="ServerPacket.Chat"/> calls.
            /// </summary>
            /// <param name="message">The message to send.</param>
            void UserChat(string message)
            {
                using (var pw = ServerPacket.Chat(message))
                {
                    User.Send(pw);
                }
            }

            #endregion

            #region Groups

            /// <summary>
            /// Creates a new group.
            /// </summary>
            [SayCommand("CreateGroup")]
            public void CreateGroup()
            {
                if (!RequireNotInGroup())
                    return;

                var group = GroupManager.TryCreateGroup(User);

                if (group == null)
                    User.Send(GameMessage.GroupCreateFailedUnknownReason);
                else
                    User.Send(GameMessage.GroupCreated);
            }

            /// <summary>
            /// Sends an invite to another user to join the group.
            /// </summary>
            /// <param name="userName">The name of the user to invite to the group.</param>
            [SayCommand("GroupInvite")]
            public void GroupInvite(string userName)
            {
                if (!RequireInGroup())
                    return;

                var target = World.FindUser(userName);

                if (target == null)
                {
                    User.Send(GameMessage.GroupInviteFailedInvalidUser, userName);
                    return;
                }

                if (target == User)
                {
                    User.Send(GameMessage.GroupInviteFailedCannotInviteSelf);
                    return;
                }

                if (!(((IGroupable)User).Group.TryInvite(target)))
                {
                    // Invite failed
                    if (((IGroupable)target).Group != null)
                        User.Send(GameMessage.GroupInviteFailedAlreadyInGroup, target.Name);
                    else
                        User.Send(GameMessage.GroupInviteFailedUnknownReason, target.Name);
                }
                else
                {
                    // Invite successful
                    using (var pw = ServerPacket.SendMessage(GameMessage.GroupInvite, User.Name, target.Name))
                    {
                        foreach (var u in ((IGroupable)User).Group.Members.OfType<User>())
                        {
                            u.Send(pw);
                        }
                    }
                }
            }

            /// <summary>
            /// Accepts an invitation to join a group.
            /// </summary>
            [SayCommand("JoinGroup")]
            public void JoinGroup()
            {
                if (!RequireNotInGroup())
                    return;

                User.TryJoinGroup();
            }

            /// <summary>
            /// Leaves the current group.
            /// </summary>
            [SayCommand("LeaveGroup")]
            public void LeaveGroup()
            {
                if (!RequireInGroup())
                    return;

                ((IGroupable)User).Group.RemoveMember(User);
            }

            #endregion

            #region Guilds

            /// <summary>
            /// Creates a new guild.
            /// </summary>
            /// <param name="name">The name of the guild.</param>
            /// <param name="tag">The guild tag.</param>
            [SayCommand("CreateGuild")]
            public void CreateGuild(string name, string tag)
            {
                if (!RequireUserNotInGuild())
                    return;

                // Valid name
                if (!_guildSettings.IsValidName(name))
                {
                    User.Send(GameMessage.GuildCreationFailedNameInvalid, name);
                    return;
                }

                if (!_guildManager.IsNameAvailable(name))
                {
                    User.Send(GameMessage.GuildCreationFailedNameNotAvailable, name);
                    return;
                }

                // Valid tag
                if (!_guildSettings.IsValidTag(tag))
                {
                    User.Send(GameMessage.GuildCreationFailedTagInvalid, tag);
                    return;
                }

                if (!_guildManager.IsTagAvailable(tag))
                {
                    User.Send(GameMessage.GuildCreationFailedTagNotAvailable, tag);
                    return;
                }

                // Create
                var guild = _guildManager.TryCreateGuild(User, name, tag);
                if (guild == null)
                    User.Send(GameMessage.GuildCreationFailedUnknownReason, name, tag);
                else
                    User.Send(GameMessage.GuildCreationSuccessful, name, tag);
            }

            /// <summary>
            /// Demotes the guild rank of another member in the guild.
            /// </summary>
            /// <param name="userName">The name of the fellow guild member to demote.</param>
            [SayCommand("Demote")]
            public void Demote(string userName)
            {
                if (!RequireUserInGuild() || !CheckGuildPermissions(_guildSettings.MinRankDemote))
                    return;

                var success = false;
                World.GuildMemberPerformer.Perform(userName, x => success = User.Guild.TryDemoteMember(User, x));

                if (success)
                    User.Send(GameMessage.GuildDemote, userName);
                else
                    User.Send(GameMessage.GuildDemoteFailed, userName);
            }

            /// <summary>
            /// Displays the guild commands.
            /// </summary>
            [SayCommand("GuildHelp")]
            public void GuildHelp()
            {
                var sb = new StringBuilder();
                sb.AppendLine("Guild commands:");
                sb.AppendLine("/JoinGuild [name]");
                sb.AppendLine("/CreateGuild [name] [symbol]");
                sb.AppendLine("/LeaveGuild");
                sb.AppendLine("/GuildMembers");
                sb.AppendLine("/GuildOnline");
                sb.AppendLine("/GuildKick [user]");
                sb.AppendLine("/Promote [user]");
                sb.AppendLine("/Demote [user]");
                sb.AppendLine("/RenameGuild [name]");
                sb.AppendLine("/RetagGuild [tag]");
                sb.AppendLine("/GuildInvite [user]");
                sb.AppendLine("/GuildLog");
                sb.AppendLine("/GuildSay [message]");

                UserChat(sb.ToString());
            }

            /// <summary>
            /// Invites a user to join the guild.
            /// </summary>
            /// <param name="toInvite">The name of the user to invite into the guild.</param>
            [SayCommand("GuildInvite")]
            public void GuildInvite(string toInvite)
            {
                if (!RequireUserInGuild() || !CheckGuildPermissions(_guildSettings.MinRankInvite))
                    return;

                var invitee = World.FindUser(toInvite);
                if (invitee == null)
                {
                    User.Send(GameMessage.GuildInviteFailedInvalidUser, toInvite);
                    return;
                }

                if (invitee == User)
                {
                    User.Send(GameMessage.GuildInviteFailedCannotInviteSelf);
                    return;
                }

                if (invitee.Guild != null)
                {
                    User.Send(GameMessage.GuildInviteFailedAlreadyInGuild, invitee.Name);
                    return;
                }

                var success = User.Guild.TryInviteMember(User, invitee);

                if (!success)
                    User.Send(GameMessage.GuildInviteFailedUnknownReason, invitee.Name);
                else
                    User.Send(GameMessage.GuildInviteSuccess, invitee.Name);
            }

            /// <summary>
            /// Kicks a fellow guild member out of the guild.
            /// </summary>
            /// <param name="userName">The name of the user to kick out of the guild.</param>
            [SayCommand("GuildKick")]
            public void GuildKick(string userName)
            {
                if (!RequireUserInGuild() || !CheckGuildPermissions(_guildSettings.MinRankKick))
                    return;

                World.GuildMemberPerformer.Perform(userName, x => GuildMemberPerformer_GuildKick(x, userName));
            }

            /// <summary>
            /// Displays the latest entries of the guild log.
            /// </summary>
            [SayCommand("GuildLog")]
            public void GuildLog()
            {
                if (!RequireUserInGuild() || !CheckGuildPermissions(_guildSettings.MinRankViewLog))
                    return;

                User.Guild.TryViewEventLog(User);
            }

            /// <summary>
            /// Displays all of the guild members.
            /// </summary>
            [SayCommand("GuildMembers")]
            public void GuildMembers()
            {
                if (!RequireUserInGuild())
                    return;

                User.Guild.TryViewMembers(User);
            }

            /// <summary>
            /// Displays the guild members that are currently online.
            /// </summary>
            [SayCommand("GuildOnline")]
            public void GuildOnline()
            {
                if (!RequireUserInGuild())
                    return;

                User.Guild.TryViewOnlineMembers(User);
            }

            /// <summary>
            /// Sends a message to all online fellow guild members.
            /// </summary>
            /// <param name="message">The message to send.</param>
            [SayCommand("GuildSay")]
            public void GuildSay(string message)
            {
                if (!RequireUserInGuild())
                    return;

                foreach (var guildMember in User.Guild.GetMembers().OfType<User>())
                {
                    guildMember.Send(GameMessage.GuildSay, User.Name, message);
                }
            }

            /// <summary>
            /// Accepts an invitation to join a guild.
            /// </summary>
            /// <param name="guildName">The name of the guild to join.</param>
            [SayCommand("JoinGuild")]
            public void JoinGuild(string guildName)
            {
                if (!RequireUserNotInGuild())
                    return;

                if (!User.TryJoinGuild(guildName))
                    User.Send(GameMessage.GuildJoinFailedInvalidOrNoInvite, guildName);
            }

            /// <summary>
            /// Leaves the current guild.
            /// </summary>
            [SayCommand("LeaveGuild")]
            public void LeaveGuild()
            {
                if (!RequireUserInGuild())
                    return;

                User.Guild = null;
            }

            /// <summary>
            /// Promotes the guild rank of another member in the guild.
            /// </summary>
            /// <param name="userName">The name of the fellow guild member to promote.</param>
            [SayCommand("Promote")]
            public void Promote(string userName)
            {
                if (!RequireUserInGuild() || !CheckGuildPermissions(_guildSettings.MinRankPromote))
                    return;

                var success = false;
                World.GuildMemberPerformer.Perform(userName, x => success = User.Guild.TryPromoteMember(User, x));

                if (success)
                    User.Send(GameMessage.GuildPromote, userName);
                else
                    User.Send(GameMessage.GuildPromoteFailed, userName);
            }

            /// <summary>
            /// Changes the name of the guild.
            /// </summary>
            /// <param name="newName">The new guild name.</param>
            [SayCommand("RenameGuild")]
            public void RenameGuild(string newName)
            {
                if (!RequireUserInGuild() || !CheckGuildPermissions(_guildSettings.MinRankRename))
                    return;

                if (!_guildSettings.IsValidName(newName))
                {
                    User.Send(GameMessage.GuildRenameFailedInvalidValue, newName);
                    return;
                }

                if (!_guildManager.IsNameAvailable(newName))
                {
                    User.Send(GameMessage.GuildRenameFailedNameNotAvailable, newName);
                    return;
                }

                if (!User.Guild.TryChangeName(User, newName))
                    User.Send(GameMessage.GuildRenameFailedUnknownReason, newName);
            }

            /// <summary>
            /// Changes the tag of the guild.
            /// </summary>
            /// <param name="newTag">The new guild tag.</param>
            [SayCommand("RetagGuild")]
            public void RetagGuild(string newTag)
            {
                if (!RequireUserInGuild() || !CheckGuildPermissions(_guildSettings.MinRankRename))
                    return;

                if (!_guildSettings.IsValidTag(newTag))
                {
                    User.Send(GameMessage.GuildRetagFailedInvalidValue, newTag);
                    return;
                }

                if (!_guildManager.IsTagAvailable(newTag))
                {
                    User.Send(GameMessage.GuildRetagFailedNameNotAvailable, newTag);
                    return;
                }

                if (!User.Guild.TryChangeTag(User, newTag))
                    User.Send(GameMessage.GuildRetagFailedUnknownReason, newTag);
            }

            #endregion

            #region Test/development commands

            /// <summary>
            /// Creates a new map instance and places the user on that map.
            /// </summary>
            /// <param name="mapID">The ID of the map to create the instance of.</param>
            [SayCommand("CreateMapInstance")]
            public void CreateMapInstance(MapID mapID)
            {
                if (!RequirePermissionLevel(UserPermissions.Admin))
                    return;

                // Check for a valid map
                if (!MapBase.IsMapIDValid(mapID))
                {
                    UserChat("Invalid map ID: " + mapID);
                    return;
                }

                // Try to create the map
                MapInstance instance;
                try
                {
                    instance = new MapInstance(mapID, World);
                }
                catch (Exception ex)
                {
                    UserChat("Failed to create instance: " + ex);
                    return;
                }

                // Add the user to the map
                User.ChangeMap(instance, new Vector2(50, 50));
            }

            /// <summary>
            /// Leaves an instanced map if the map the user is on is for an instanced map. The user is warped
            /// to their respawn position.
            /// </summary>
            [SayCommand("LeaveMapInstance")]
            public void LeaveMapInstance()
            {
                if (!RequirePermissionLevel(UserPermissions.Admin))
                    return;

                // Check for a valid map
                if (User.Map == null || !User.Map.IsInstanced)
                {
                    UserChat("You must be on an instanced map to do that.");
                    return;
                }

                // Get the map to respawn on
                var mapID = User.RespawnMapID;
                Map map = null;

                if (mapID.HasValue)
                    map = World.GetMap(mapID.Value);

                if (map == null)
                {
                    UserChat("Could not teleport you to your respawn location - your respawn map is null for some reason...");
                    return;
                }

                // Teleport to respawn map/position
                User.ChangeMap(map, User.RespawnPosition);
            }

            /// <summary>
            /// Causes you to kill yourself.
            /// </summary>
            [SayCommand("Suicide")]
            [SayCommand("Seppuku")]
            public void Suicide()
            {
                if (!RequirePermissionLevel(UserPermissions.Moderator))
                    return;

                User.Kill();
            }

            #endregion

            #region Lesser Admin commands

            /// <summary>
            /// Creates an instance of an item from a template and adds it to your inventory. Any items that cannot
            /// fit into the caller's inventory are destroyed.
            /// </summary>
            /// <param name="id">The ID of the item template to use.</param>
            /// <param name="amount">The number of items to create.</param>
            [SayCommand("CreateItem")]
            public void CreateItem(ItemTemplateID id, byte amount)
            {
                if (!RequirePermissionLevel(UserPermissions.LesserAdmin))
                    return;

                // Get the item template
                var template = ItemTemplateManager.Instance[id];
                if (template == null)
                {
                    UserChat("Invalid item template ID: " + id);
                    return;
                }

                // Create the item
                var item = new ItemEntity(template, amount);

                // Give to user
                var remainder = User.Inventory.Add(item);

                // Delete any that failed to be added
                if (remainder != null)
                {
                    UserChat(remainder.Amount + " units could not be added to your inventory.");
                    remainder.Dispose();
                }
            }

            #endregion

            #region ISayCommands<User> Members

            /// <summary>
            /// Gets or sets the User that the current command came from.
            /// </summary>
            public User User { get; set; }

            #endregion
        }
    }
}