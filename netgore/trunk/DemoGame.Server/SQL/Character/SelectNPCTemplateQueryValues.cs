using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;


namespace DemoGame.Server
{
    public class SelectNPCTemplateQueryValues
    {
        public readonly string AIName;
        public readonly string Alliance;
        public readonly ushort BodyIndex;
        public readonly ushort GiveCash;
        public readonly ushort GiveExp;
        public readonly int ID;
        public readonly string Name;
        public readonly ushort Respawn;
        public readonly IEnumerable<IStat> Stats;

        public SelectNPCTemplateQueryValues(int id, string name, ushort bodyIndex, string aiName, string alliance,
                                            ushort respawn, ushort giveExp, ushort giveCash,
                                            IEnumerable<IStat> stats)
        {
            ID = id;
            Name = name;
            BodyIndex = bodyIndex;
            AIName = aiName;
            Alliance = alliance;
            Respawn = respawn;
            GiveExp = giveExp;
            GiveCash = giveCash;
            Stats = stats;
        }
    }
}