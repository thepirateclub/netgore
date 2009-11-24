﻿using System.ComponentModel;
using System.Linq;
using Microsoft.Xna.Framework;
using NetGore.IO;

namespace NetGore.Graphics.ParticleEngine
{
    /// <summary>
    /// A <see cref="ConeEmitter"/> that emits particles in the form of a cone.
    /// </summary>
    public class ConeEmitter : ParticleEmitter
    {
        const float _defaultConeAngle = MathHelper.PiOver2;
        const float _defaultDirection = 0;
        const string _emitterCategoryName = "Cone Emitter";

        float _coneAngle = _defaultConeAngle;
        float _direction = _defaultDirection;

        const string _coneAngleKeyName = "ConeAngle";
        const string _directionKeyName = "Direction";

        /// <summary>
        /// When overridden in the derived class, writes all custom state values to the <paramref name="writer"/>.
        /// </summary>
        /// <param name="writer">The <see cref="IValueWriter"/> to write the state values to.</param>
        protected override void WriteCustomValues(NetGore.IO.IValueWriter writer)
        {
            writer.Write(_coneAngleKeyName, ConeAngle);
            writer.Write(_directionKeyName, Direction);

            base.WriteCustomValues(writer);
        }

        /// <summary>
        /// Gets or sets the angle (in radians) from edge to edge of the emitter beam.
        /// </summary>
        [Category(_emitterCategoryName)]
        [Description("The angle (in radians) from edge to edge of the emitter beam.")]
        [DisplayName("Angle")]
        [DefaultValue(_defaultConeAngle)]
        public float ConeAngle
        {
            get { return _coneAngle; }
            set { _coneAngle = value; }
        }

        /// <summary>
        /// Gets or sets the angle (in radians) that the emitter is facing.
        /// </summary>
        [Category(_emitterCategoryName)]
        [Description("The angle (in radians) that the emitter is facing.")]
        [DisplayName("Direction")]
        [DefaultValue(_defaultDirection)]
        public float Direction
        {
            get { return _direction; }
            set { _direction = value; }
        }

        /// <summary>
        /// Generates the offset and normalized force vectors to release the <see cref="Particle"/> at.
        /// </summary>
        /// <param name="particle">The <see cref="Particle"/> that the values are being generated for.</param>
        /// <param name="offset">The offset vector.</param>
        /// <param name="force">The normalized force vector.</param>
        protected override void GenerateParticleOffsetAndForce(Particle particle, out Vector2 offset, out Vector2 force)
        {
            offset = Vector2.Zero;

            float f = ConeAngle * 0.5f;
            float radians = RandomHelper.NextFloat(Direction - f, Direction + f);

            GetForce(radians, out force);
        }
    }
}