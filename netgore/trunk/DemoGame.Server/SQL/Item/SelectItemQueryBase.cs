using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;

using NetGore.Db;

namespace DemoGame.Server
{
    public abstract class SelectItemQueryBase<T> : DbQueryReader<T>
    {
        protected SelectItemQueryBase(DbConnectionPool connectionPool, string commandText) : base(connectionPool, commandText)
        {
        }

        protected static ItemValues GetItemValues(IDataReader r)
        {
            // Stats
            ItemStats stats = new ItemStats();
            foreach (StatType statType in ItemStats.DatabaseStats)
            {
                IStat stat = stats.GetStat(statType);
                stat.Read(r, r.GetOrdinal(statType.GetDatabaseField()));
            }

            // General
            int id = r.GetInt32("id");
            byte width = r.GetByte("width");
            byte height = r.GetByte("height");
            string name = r.GetString("name");
            string description = r.GetString("description");
            ushort graphicIndex = r.GetUInt16("graphic");
            byte amount = r.GetByte("amount");
            int value = r.GetInt32("value");
            ItemType type = r.GetItemType("type");

            // FUTURE: Recover from this error by just not creating an item
            if (!type.IsDefined())
                throw new InvalidCastException(string.Format("Invalid ItemType `{0}` for ItemEntity ID `{1}`", type, id));

            return new ItemValues(id, width, height, name, description, type, graphicIndex, amount, value, stats);
        }
    }
}