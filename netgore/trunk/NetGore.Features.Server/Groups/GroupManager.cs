﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using log4net;

namespace NetGore.Features.Groups
{
    /// <summary>
    /// Manages all of the <see cref="IGroup"/>s.
    /// </summary>
    public class GroupManager : IGroupManager
    {
        static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        readonly IGroupEventHandler _groupDisbandHandler;
        readonly List<IGroup> _groups;
        readonly Func<IGroupManager, IGroupable, IGroup> _tryCreateGroup;

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupManager"/> class.
        /// </summary>
        /// <param name="tryCreateGroup">A <see cref="Func{T,U,V}"/> that is used to create a group started by
        /// an <see cref="IGroupable"/>.</param>
        public GroupManager(Func<IGroupManager, IGroupable, IGroup> tryCreateGroup)
        {
            _groupDisbandHandler = Group_Disbanded;
            _tryCreateGroup = tryCreateGroup;
        }

        /// <summary>
        /// Handles when a <see cref="IGroup"/> in this <see cref="GroupManager"/> is disbanded.
        /// </summary>
        /// <param name="group">The group that was disbanded.</param>
        void Group_Disbanded(IGroup group)
        {
            group.Disbanded -= _groupDisbandHandler;
            if (!_groups.Remove(group))
            {
                const string errmsg =
                    "Tried to remove disbanded group `{0}` from group manager `{1}`, but it was not in the _groups list!";
                if (log.IsWarnEnabled)
                    log.WarnFormat(errmsg, group, this);
            }
        }

        /// <summary>
        /// When overridden in the derived class, allows for handling of the
        /// <see cref="IGroupManager.GroupCreated"/> event.
        /// </summary>
        /// <param name="group">The <see cref="IGroup"/> that was created.</param>
        protected virtual void OnCreateGroup(IGroup group)
        {
        }

        #region IGroupManager Members

        /// <summary>
        /// Notifies listeners when a new group has been created.
        /// </summary>
        public event IGroupManagerGroupEventHandler GroupCreated;

        /// <summary>
        /// Gets all of the active <see cref="IGroup"/>s managed by this <see cref="IGroupManager"/>.
        /// </summary>
        public IEnumerable<IGroup> Groups
        {
            get { return _groups; }
        }

        /// <summary>
        /// Creates a new <see cref="IGroup"/>.
        /// </summary>
        /// <param name="founder">The <see cref="IGroupable"/> that will be the founder of the group.</param>
        /// <returns>If the group was successfully created, returns the new <see cref="IGroup"/> with the
        /// <paramref name="founder"/> set as the group's founder. Otherwise, returns null.
        /// A group may not be created by someone who is already in a group.</returns>
        public IGroup TryCreateGroup(IGroupable founder)
        {
            // Make sure not already in a group
            if (founder.Group != null)
                return null;

            // Create the group
            var newGroup = _tryCreateGroup(this, founder);
            if (newGroup == null)
                return null;

            // Add the new group to the list
            _groups.Add(newGroup);

            // Listen for when the group is disbanded so we can remove it
            newGroup.Disbanded += _groupDisbandHandler;

            // Raise events
            OnCreateGroup(newGroup);

            if (GroupCreated != null)
                GroupCreated(this, newGroup);

            return newGroup;
        }

        #endregion
    }
}