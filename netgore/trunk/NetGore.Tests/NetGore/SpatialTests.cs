using System.Collections.Generic;
using System.Linq;
using NetGore.World;
using NUnit.Framework;
using SFML.Graphics;

namespace NetGore.Tests.NetGore
{
    [TestFixture]
    public class SpatialTests
    {
        static readonly Vector2 SpatialSize = new Vector2(1024, 512);

        static IEnumerable<Entity> CreateEntities(int amount, Vector2 minPos, Vector2 maxPos)
        {
            var ret = new Entity[amount];
            for (var i = 0; i < amount; i++)
            {
                ret[i] = new TestEntity { Position = RandomHelper.NextVector2(minPos, maxPos) };
            }

            return ret;
        }

        static IEnumerable<ISpatialCollection> GetSpatials()
        {
            var a = new LinearSpatialCollection();
            a.SetAreaSize(SpatialSize);

            var b = new DynamicGridSpatialCollection();
            b.SetAreaSize(SpatialSize);

            var c = new StaticGridSpatialCollection();
            c.SetAreaSize(SpatialSize);

            return new ISpatialCollection[] { a, b };
        }

        #region Unit tests

        [Test]
        public void AddTest()
        {
            foreach (var spatial in GetSpatials())
            {
                var entity = new TestEntity();
                spatial.Add(entity);
                Assert.IsTrue(spatial.CollectionContains(entity), "Current spatial: " + spatial);
            }
        }

        [Test]
        public void GetEntitiesTest()
        {
            const int count = 25;
            var min = new Vector2(32, 64);
            var max = new Vector2(256, 128);
            var diff = max - min;

            foreach (var spatial in GetSpatials())
            {
                var entities = CreateEntities(count, min, max);
                spatial.Add(entities);

                foreach (var entity in entities)
                {
                    Assert.IsTrue(spatial.CollectionContains(entity), "Current spatial: " + spatial);
                }

                var found = spatial.GetMany(new Rectangle((int)min.X, (int)min.Y, (int)diff.X, (int)diff.Y));

                Assert.AreEqual(count, found.Count());
            }
        }

        [Test]
        public void MoveTest()
        {
            foreach (var spatial in GetSpatials())
            {
                var entity = new TestEntity();
                spatial.Add(entity);
                Assert.IsTrue(spatial.CollectionContains(entity), "Current spatial: " + spatial);

                entity.Teleport(new Vector2(128, 128));
                Assert.IsTrue(spatial.Contains(new Vector2(128, 128)), "Current spatial: " + spatial);
                Assert.IsFalse(spatial.Contains(new Vector2(256, 128)), "Current spatial: " + spatial);
                Assert.IsFalse(spatial.Contains(new Vector2(128, 256)), "Current spatial: " + spatial);
            }
        }

        [Test]
        public void RemoveTest()
        {
            foreach (var spatial in GetSpatials())
            {
                var entity = new TestEntity();
                spatial.Add(entity);
                Assert.IsTrue(spatial.CollectionContains(entity), "Current spatial: " + spatial);

                spatial.Remove(entity);
                Assert.IsFalse(spatial.CollectionContains(entity), "Current spatial: " + spatial);
            }
        }

        #endregion

        class TestEntity : Entity
        {
            /// <summary>
            /// When overridden in the derived class, gets if this <see cref="Entity"/> will collide against
            /// walls. If false, this <see cref="Entity"/> will pass through walls and completely ignore them.
            /// </summary>
            public override bool CollidesAgainstWalls
            {
                get { return true; }
            }
        }
    }
}