using System;
using System.Linq;

namespace NetGore.Features.PeerTrading
{
    /// <summary>
    /// Contains the settings for peer trading.
    /// </summary>
    public class PeerTradingSettings
    {
        /// <summary>
        /// The settings instance.
        /// </summary>
        static PeerTradingSettings _instance;

        readonly int _maxDistance;
        readonly byte _maxTradeSlots;

        /// <summary>
        /// Initializes a new instance of the <see cref="PeerTradingSettings"/> class.
        /// </summary>
        /// <param name="maxTradeSlots">The max number of items able to be traded in a single transaction. Should not be more
        /// than the number of inventory slots a user has.</param>
        /// <param name="maxDistance">The maximum distance two characters may be from one another to be able to trade.</param>
        public PeerTradingSettings(byte maxTradeSlots, int maxDistance)
        {
            _maxTradeSlots = maxTradeSlots;
            _maxDistance = maxDistance;
        }

        /// <summary>
        /// Gets the <see cref="PeerTradingSettings"/> instance.
        /// </summary>
        public static PeerTradingSettings Instance
        {
            get { return _instance; }
        }

        /// <summary>
        /// Gets the maximum distance two characters may be from one another to be able to trade.
        /// </summary>
        public int MaxDistance
        {
            get { return _maxDistance; }
        }

        /// <summary>
        /// Gets the max number of items able to be traded in a single transaction.
        /// </summary>
        public ushort MaxTradeSlots
        {
            get { return _maxTradeSlots; }
        }

        /// <summary>
        /// Initializes the <see cref="PeerTradingSettings"/>. This must only be called once and called as early as possible.
        /// </summary>
        /// <param name="settings">The settings instance.</param>
        public static void Initialize(PeerTradingSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException("settings");
            if (_instance != null)
                throw new MethodAccessException("This method must be called once and only once.");

            _instance = settings;
        }
    }
}