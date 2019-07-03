using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlayTable
{
    /// <summary>
    /// Non-player things/items/objects
    /// </summary>
    public abstract class PTProperty : PTZone    {
        #region fields
        /// <summary>
        /// The list of players who has a name on this
        /// </summary>
        public List<PTPlayer> owners { get; protected set; }
        #endregion

        #region delegates
        public PTDelegatePlayer OnOwnershipAdded;
        public PTDelegatePlayer OnOwnershipRemoved;
        public PTDelegatePlayer OnOwnershipReplacedWith;
        #endregion

        #region Unity built-in
        protected void Awake()
        {
            owners = new List<PTPlayer>();
        }
        #endregion

        #region api
        protected abstract bool allowOwnershipAdd(PTPlayer player);
        protected abstract bool allowOwnershipRemove(PTPlayer player);
        protected abstract bool allowOwnershipReplaceWith(PTPlayer player);
        public bool RequestOwnershipAdd(PTPlayer player)
        {
            if (allowOwnershipAdd(player))
            {
                owners.Add(player);
                if (OnOwnershipAdded != null)
                {
                    OnOwnershipAdded(player);
                }
                return true;
            }
            return false;
        }
        public bool RequestOwnershipRemoved(PTPlayer player)
        {
            if (allowOwnershipRemove(player))
            {
                owners.Remove(player);

                if (OnOwnershipRemoved != null)
                {
                    OnOwnershipRemoved(player);
                }
                return true;
            }
            return false;
        }
        public bool RequestOwnershipReplaceWith (PTPlayer player)
        {
            if (allowOwnershipReplaceWith(player))
            {
                owners = new List<PTPlayer>();
                owners.Add(player);

                if (OnOwnershipReplacedWith != null)
                {
                    OnOwnershipReplacedWith(player);
                }
                return true;
            }
            return false;
        }
        #endregion
    }
}

