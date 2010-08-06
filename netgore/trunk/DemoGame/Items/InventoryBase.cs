using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using log4net;
using NetGore;
using NetGore.World;

namespace DemoGame
{
    /// <summary>
    /// Base class for an Inventory that contains ItemEntities.
    /// </summary>
    /// <typeparam name="T">The type of game item contained in the inventory.</typeparam>
    public abstract class InventoryBase<T> : IInventory<T> where T : ItemEntityBase
    {
        static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        readonly T[] _buffer;

        /// <summary>
        /// Initializes a new instance of the <see cref="InventoryBase&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="slots">The number of slots in the inventory.</param>
        protected InventoryBase(int slots)
        {
            _buffer = new T[slots];
        }

        /// <summary>
        /// Gets or sets (protected) the item in the Inventory at the given <paramref name="slot"/>.
        /// </summary>
        /// <param name="slot">Index of the slot to get the Item from.</param>
        /// <returns>Item in the specified Inventory slot, or null if the slot is empty or invalid.</returns>
        protected T this[int slot]
        {
            get { return this[new InventorySlot(slot)]; }
            set { this[new InventorySlot(slot)] = value; }
        }

        /// <summary>
        /// Completely removes the item in a given slot, including optionally disposing it.
        /// </summary>
        /// <param name="slot">Slot of the item to remove.</param>
        /// <param name="dispose">If true, the item in the slot will also be disposed. If false, the item
        /// will be removed from the Inventory, but not disposed.</param>
        protected void ClearSlot(InventorySlot slot, bool dispose)
        {
            // Get the item at the slot
            var item = this[slot];

            // Check for a valid item
            if (item == null)
            {
                const string errmsg = "Slot `{0}` already contains no item.";
                Debug.Fail(string.Format("Slot `{0}` already contains no item.", slot));
                if (log.IsErrorEnabled)
                    log.ErrorFormat(errmsg, this);
                return;
            }

            // Remove the item reference
            this[slot] = null;

            // Dispose of the item
            if (dispose)
                item.Dispose();
        }

        /// <summary>
        /// When overridden in the derived class, performs additional processing to handle an Inventory slot
        /// changing. This is only called when the object references changes, not when any part of the object
        /// (such as the Item's amount) changes. It is guarenteed that if <paramref name="newItem"/> is null,
        /// <paramref name="oldItem"/> will not be, and vise versa. Both will never be null or non-null.
        /// </summary>
        /// <param name="slot">Slot that the change took place in.</param>
        /// <param name="newItem">The item that was added to the <paramref name="slot"/>, or null if the slot changed to empty.</param>
        /// <param name="oldItem">The item that used to be in the <paramref name="slot"/>,
        /// or null if the slot used to be empty.</param>
        protected virtual void HandleSlotChanged(InventorySlot slot, T newItem, T oldItem)
        {
        }

        /// <summary>
        /// Internal handler for handling adding items.
        /// </summary>
        /// <param name="item">Item that will be added to the Inventory.</param>
        /// <param name="changedSlots">A <see cref="ICollection{InventorySlot}"/> to use to build up the list of slots that
        /// were changed to add the <paramref name="item"/>. If null, changed slots will not be tracked, so its up to
        /// the caller to supply the empty list.</param>
        /// <returns>The remainder of the item that failed to be added to the inventory, or null if all of the
        /// item was added.</returns>
        T InternalAdd(T item, ICollection<InventorySlot> changedSlots)
        {
            Debug.Assert(changedSlots == null || changedSlots.IsEmpty());

            // Try to stack the item in as many slots as possible until it runs out or we run out of slots
            InventorySlot slot;
            while (TryFindStackableSlot(item, out slot))
            {
                var invItem = this[slot];
                if (invItem == null)
                {
                    const string errmsg = "This should never be a null item. If it is, TryFindStackableSlot() may be broken.";
                    Debug.Fail(errmsg);
                    if (log.IsErrorEnabled)
                        log.Error(errmsg);
                }
                else
                {
                    // Stack as much of the item into the existing inventory item
                    var stackAmount = (byte)Math.Min(ItemEntityBase.MaxStackSize - invItem.Amount, item.Amount);
                    invItem.Amount += stackAmount;
                    item.Amount -= stackAmount;

                    if (changedSlots != null && !changedSlots.Contains(slot))
                        changedSlots.Add(slot);

                    // If we stacked all of the item, we're done
                    if (item.Amount == 0)
                        return null;
                }
            }

            // Could not stack, or only some of the item was stacked, so add item to empty slots
            while (TryFindEmptySlot(out slot))
            {
                // Deep-copy the item and set it in the inventory
                var copy = (T)item.DeepCopy();
                this[slot] = copy;

                // Reduce the amount of the item by the amount we took
                var amountTaken = Math.Min(ItemEntityBase.MaxStackSize, item.Amount);
                copy.Amount = amountTaken;
                item.Amount -= amountTaken;

                if (changedSlots != null && !changedSlots.Contains(slot))
                    changedSlots.Add(slot);

                // If we took all of the item, we are done
                if (item.Amount == 0)
                {
                    item.Dispose();
                    return null;
                }
            }

            // Failed to add all of the item
            return item;
        }

        /// <summary>
        /// Handles when an item is disposed while still in the Inventory.
        /// </summary>
        /// <param name="entity">Entity that was disposed.</param>
        void ItemDisposeHandler(Entity entity)
        {
            var item = (T)entity;

            // Try to get the slot
            InventorySlot slot;
            try
            {
                slot = GetSlot(item);
            }
            catch (ArgumentException)
            {
                const string errmsg = "Inventory item `{0}` was disposed, but was not be found in the Inventory.";
                Debug.Fail(string.Format(errmsg, item));
                if (log.IsWarnEnabled)
                    log.WarnFormat(errmsg, item);
                return;
            }

            // Remove the item from the Inventory (don't dispose since it was already disposed)
            RemoveAt(slot, false);
        }

        /// <summary>
        /// Gets the index of the first unused Inventory slot.
        /// </summary>
        /// <param name="emptySlot">If function returns true, contains the index of the first unused Inventory slot.</param>
        /// <returns>True if an empty slot was found, otherwise false.</returns>
        protected bool TryFindEmptySlot(out InventorySlot emptySlot)
        {
            // Iterate through each slot
            for (var i = 0; i < _buffer.Length; i++)
            {
                // Return on the first null item
                if (this[i] == null)
                {
                    emptySlot = new InventorySlot(i);
                    return true;
                }
            }

            // All slots are in use
            emptySlot = new InventorySlot(0);
            return false;
        }

        /// <summary>
        /// Gets the first slot that the given <paramref name="item"/> can be stacked on.
        /// </summary>
        /// <param name="item">Item that will try to stack on existing items.</param>
        /// <param name="stackableSlot">If function returns true, contains the index of the first slot that
        /// the <paramref name="item"/> can be stacked on. This slot is not guaranteed to be able to hold 
        /// all of the item, but it does guarantee to be able to hold at least one unit of the item.</param>
        /// <returns>True if a stackable slot was found, otherwise false.</returns>
        protected bool TryFindStackableSlot(T item, out InventorySlot stackableSlot)
        {
            // Iterate through each slot
            for (var i = 0; i < _buffer.Length; i++)
            {
                var invItem = this[i];

                // Skip empty slots
                if (invItem == null)
                    continue;

                // Make sure the item isn't already at the stacking limit
                if (invItem.Amount >= ItemEntityBase.MaxStackSize)
                    continue;

                // Check if the item can stack with our item
                if (!invItem.CanStack(item))
                    continue;

                // Stackable slot found
                stackableSlot = new InventorySlot(i);
                return true;
            }

            // No stackable slot found
            stackableSlot = new InventorySlot(0);
            return false;
        }

        #region IInventory<T> Members

        /// <summary>
        /// Gets or sets (protected) the item in the Inventory at the given <paramref name="slot"/>.
        /// </summary>
        /// <param name="slot">Index of the slot to get the Item from.</param>
        /// <returns>Item in the specified Inventory slot, or null if the slot is empty or invalid.</returns>
        public T this[InventorySlot slot]
        {
            get
            {
                // Check for a valid index
                if (!slot.IsLegalValue())
                {
                    const string errmsg = "Tried to get invalid inventory slot `{0}`";
                    if (log.IsErrorEnabled)
                        log.ErrorFormat(errmsg, slot);
                    Debug.Fail(string.Format(errmsg, slot));
                    return null;
                }

                return _buffer[(int)slot];
            }

            protected set
            {
                // Check for a valid index
                if (!slot.IsLegalValue())
                {
                    const string errmsg = "Tried to set invalid inventory slot `{0}`";
                    if (log.IsErrorEnabled)
                        log.ErrorFormat(errmsg, slot);
                    Debug.Fail(string.Format(errmsg, slot));
                    return;
                }

                // Check for a change
                var oldItem = this[slot];
                if (oldItem == value)
                    return;

                // Ensure that, when setting an item into a slot, that no item is already there
                if (oldItem != null && value != null)
                {
                    const string errmsg = "Set an item ({0}) on a slot ({1}) that already contained an item ({2}).";
                    Debug.Fail(string.Format(errmsg, value, slot, oldItem));
                    if (log.IsErrorEnabled)
                        log.ErrorFormat(errmsg, value, slot, oldItem);

                    // Try to resolve the problem by removing and disposing of the old item
                    ClearSlot(slot, true);
                }

                // Attach (if item added) or remove (if item removed) hook to the Dispose event
                if (oldItem != null)
                    oldItem.Disposed -= ItemDisposeHandler;
                else
                    value.Disposed += ItemDisposeHandler;

                // Change the ItemEntity reference
                _buffer[(int)slot] = value;

                // Allow for additional processing
                HandleSlotChanged(slot, value, oldItem);
            }
        }

        /// <summary>
        /// Gets the number of inventory slots that are currently unoccupied (contains no item).
        /// </summary>
        public int FreeSlots
        {
            get
            {
                var count = 0;

                for (var i = 0; i < _buffer.Length; i++)
                {
                    if (this[i] == null)
                        ++count;
                }

                return count;
            }
        }

        /// <summary>
        /// Gets the number of inventory slots that are currently occupied (contains an item).
        /// </summary>
        public int OccupiedSlots
        {
            get
            {
                var count = 0;

                for (var i = 0; i < _buffer.Length; i++)
                {
                    if (this[i] != null)
                        ++count;
                }

                return count;
            }
        }

        /// <summary>
        /// Gets the total number of slots in this inventory (both free and occupied slots).
        /// </summary>
        public int TotalSlots
        {
            get { return _buffer.Length; }
        }

        /// <summary>
        /// Adds as much of the item, if not all, to the inventory as possible. The actual 
        /// item object is not given to the Inventory, but rather a deep copy of it. 
        /// The passed item's amount will be reduced either to 0 if the Inventory took all
        /// of the item, or reduced if only some of the item could be taken.
        /// </summary>
        /// <param name="item">Item that will be added to the Inventory.</param>
        /// <returns>The remainder of the item that failed to be added to the inventory, or null if all of the
        /// item was added.</returns>
        public T Add(T item)
        {
            return InternalAdd(item, null);
        }

        /// <summary>
        /// Adds as much of the item, if not all, to the inventory as possible. The actual 
        /// item object is not given to the Inventory, but rather a deep copy of it. 
        /// The passed item's amount will be reduced either to 0 if the Inventory took all
        /// of the item, or reduced if only some of the item could be taken.
        /// </summary>
        /// <param name="item">Item that will be added to the Inventory.</param>
        /// <param name="changedSlots">Contains the <see cref="InventorySlot"/>s that the <paramref name="item"/> was added to.</param>
        /// <returns>The remainder of the item that failed to be added to the inventory, or null if all of the
        /// item was added.</returns>
        public T Add(T item, out IEnumerable<InventorySlot> changedSlots)
        {
            var changeList = new List<InventorySlot>();
            changedSlots = changeList;
            return InternalAdd(item, changeList);
        }

        /// <summary>
        /// Checks if the specified <paramref name="item"/> can be added to the inventory completely
        /// and successfully, but does not actually add the item.
        /// </summary>
        /// <param name="item">Item to try to fit into the inventory.</param>
        /// <returns>True if the <paramref name="item"/> can be added to the inventory properly; false if the <paramref name="item"/>
        /// is invalid or cannot fully fit into the inventory.</returns>
        public bool CanAdd(T item)
        {
            // NOTE: All CanAdd() implementations use a hack to check if the items can fit

            // FUTURE: Can prevent creating an ItemEntity just to figure out it won't fit by passing the item template and amount
            // and using that to check if there is a free slot. Then, when stacking is supported, the method can create the
            // ItemEntity to test if it will stack. Will want to also have an out parameter to be able to reuse/dispose of the
            // created temporary ItemEntity.

            if (item == null)
                return false;

            return FreeSlots >= 1;
        }

        /// <summary>
        /// Checks if the specified <paramref name="items"/> can be added to the inventory completely
        /// and successfully, but does not actually add the items.
        /// </summary>
        /// <param name="items">Items to try to fit into the inventory.</param>
        /// <returns>True if the <paramref name="items"/> can be added to the inventory properly; false if the <paramref name="items"/>
        /// is invalid or cannot fully fit into the inventory.</returns>
        public bool CanAdd(IEnumerable<T> items)
        {
            // NOTE: All CanAdd() implementations use a hack to check if the items can fit

            if (items == null)
                return false;

            return FreeSlots >= items.Count();
        }

        /// <summary>
        /// Checks if the specified <paramref name="items"/> can be added to the inventory completely
        /// and successfully, but does not actually add the items.
        /// </summary>
        /// <param name="items">Items to try to fit into the inventory.</param>
        /// <returns>True if the <paramref name="items"/> can be added to the inventory properly; false if the <paramref name="items"/>
        /// is invalid or cannot fully fit into the inventory.</returns>
        public bool CanAdd(IInventory<T> items)
        {
            // NOTE: All CanAdd() implementations use a hack to check if the items can fit

            if (items == null)
                return false;

            return FreeSlots >= items.OccupiedSlots;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<KeyValuePair<InventorySlot, T>> GetEnumerator()
        {
            for (var i = new InventorySlot(0); i < _buffer.Length; i++)
            {
                var item = this[i];
                if (item != null)
                    yield return new KeyValuePair<InventorySlot, T>(i, item);
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Gets the slot for the specified <paramref name="item"/>.
        /// </summary>
        /// <param name="item">Item to find the slot for.</param>
        /// <returns>Slot for the specified <paramref name="item"/>.</returns>
        /// <exception cref="ArgumentException">The specified item is not in the Inventory.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="item"/> was null.</exception>
        public InventorySlot GetSlot(T item)
        {
            if (item == null)
                throw new ArgumentNullException("item");

            for (var i = 0; i < _buffer.Length; i++)
            {
                if (this[i] == item)
                    return new InventorySlot(i);
            }

            throw new ArgumentException("The specified item is not in the Inventory.", "item");
        }

        /// <summary>
        /// Removes all items from the inventory.
        /// </summary>
        /// <param name="dispose">If true, then all of the items in the InventoryBase will be disposed of. If false,
        /// they will only be removed from the InventoryBase, but could still referenced by other objects.</param>
        public void RemoveAll(bool dispose)
        {
            for (var i = 0; i < _buffer.Length; i++)
            {
                if (this[i] != null)
                    ClearSlot(new InventorySlot(i), dispose);
            }
        }

        /// <summary>
        /// Removes the item in the given <paramref name="slot"/> from the inventory. The removed item is
        /// not disposed, so if the ItemEntity must be disposed (that is, it won't be used anywhere else), be
        /// sure to dispose of it!
        /// </summary>
        /// <param name="slot">Slot of the item to remove.</param>
        /// <param name="dispose">If true, the item at the given <paramref name="slot"/> will be disposed. If false,
        /// the item will not be disposed and will still be referenceable.</param>
        public void RemoveAt(InventorySlot slot, bool dispose)
        {
            ClearSlot(slot, dispose);
        }

        /// <summary>
        /// Swaps the items in two inventory slots.
        /// </summary>
        /// <param name="a">The first <see cref="InventorySlot"/> to swap.</param>
        /// <param name="b">The second <see cref="InventorySlot"/> to swap.</param>
        /// <returns>True if the swapping was successful; false if either of the <see cref="InventorySlot"/>s contained
        /// an invalid value or if the slots were the same slot.</returns>
        public bool SwapSlots(InventorySlot a, InventorySlot b)
        {
            // Check for valid slots
            if (!a.IsLegalValue() || !b.IsLegalValue())
                return false;

            // Don't swap to the same slot
            if (a == b)
                return false;

            // We do a little hack here to swap much quicker than using the indexer

            // Store the items in the slots
            var tmpA = this[a];
            var tmpB = this[b];

            // Only swap if there is an item in either of or both of the slots
            if (tmpA != null || tmpB != null)
            {
                // Swap the items in the slots in the buffer directly
                _buffer[(int)a] = tmpB;
                _buffer[(int)b] = tmpA;

                // Raise the slot change notification for both the swaps
                HandleSlotChanged(a, tmpB, tmpA);
                HandleSlotChanged(b, tmpA, tmpB);
            }

            return true;
        }

        /// <summary>
        /// Gets the slot for the specified <paramref name="item"/>.
        /// </summary>
        /// <param name="item">Item to find the slot for.</param>
        /// <returns>Slot for the specified <paramref name="item"/>, or null if the <paramref name="item"/> is invalid or not in
        /// the inventory.</returns>
        public InventorySlot? TryGetSlot(T item)
        {
            if (item == null)
                return null;

            for (var i = 0; i < _buffer.Length; i++)
            {
                if (this[i] == item)
                    return new InventorySlot(i);
            }

            return null;
        }

        #endregion
    }
}