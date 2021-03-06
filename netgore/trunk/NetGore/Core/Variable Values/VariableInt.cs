using System;
using System.ComponentModel;
using System.Linq;

namespace NetGore
{
    [TypeConverter(typeof(VariableIntConverter))]
    public struct VariableInt : IVariableValue<int>, IEquatable<VariableInt>
    {
        int _max;
        int _min;

        /// <summary>
        /// Initializes a new instance of the <see cref="VariableInt"/> struct.
        /// </summary>
        /// <param name="value">The value for both the <see cref="Min"/> and <see cref="Max"/>.</param>
        public VariableInt(int value)
        {
            _min = value;
            _max = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VariableInt"/> struct.
        /// </summary>
        /// <param name="min">The min.</param>
        /// <param name="max">The max.</param>
        public VariableInt(int min, int max)
        {
            if (min <= max)
            {
                _min = min;
                _max = max;
            }
            else
            {
                _min = max;
                _max = min;
            }
        }

        #region IVariableValue<int> Members

        /// <summary>
        /// Gets or sets the inclusive maximum possible value. If this value is set to less than <see cref="IVariableValue{T}.Min"/>,
        /// then <see cref="IVariableValue{T}.Min"/> will be lowered to equal this value.
        /// </summary>
        [Description("The inclusive maximum possible value.")]
        [Category("Variable Value")]
        [EditorBrowsable]
        public int Max
        {
            get { return _max; }
            set
            {
                _max = value;
                if (_min > _max)
                    _min = _max;
            }
        }

        /// <summary>
        /// Gets or sets the inclusive minimum possible value. If this value is set to greater than <see cref="IVariableValue{T}.Max"/>,
        /// then <see cref="IVariableValue{T}.Max"/> will be raised to equal this value.
        /// </summary>
        [Description("The inclusive minimum possible value.")]
        [Category("Variable Value")]
        [EditorBrowsable]
        public int Min
        {
            get { return _min; }
            set
            {
                _min = value;
                if (_max < _min)
                    _max = _min;
            }
        }

        /// <summary>
        /// Gets the next value, based off of the <see cref="IVariableValue{T}.Min"/> and <see cref="IVariableValue{T}.Max"/>.
        /// </summary>
        /// <returns>The next value, based off of the <see cref="IVariableValue{T}.Min"/> and <see cref="IVariableValue{T}.Max"/>.</returns>
        public int GetNext()
        {
            return RandomHelper.NextInt(Min, Max + 1);
        }

        #endregion

        /// <summary>
        /// Performs an implicit conversion from <see cref="System.Int32"/> to <see cref="NetGore.VariableInt"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator VariableInt(int value)
        {
            return new VariableInt(value);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        public bool Equals(VariableInt other)
        {
            return other._max == _max && other._min == _min;
        }

        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        /// <param name="obj">Another object to compare to.</param>
        /// <returns>
        /// true if <paramref name="obj"/> and this instance are the same type and represent the same value; otherwise, false.
        /// </returns>
        public override bool Equals(object obj)
        {
            return obj is VariableInt && this == (VariableInt)obj;
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer that is the hash code for this instance.
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                return (_max * 397) ^ _min;
            }
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="left">The left argument.</param>
        /// <param name="right">The right argument.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator ==(VariableInt left, VariableInt right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="left">The left argument.</param>
        /// <param name="right">The right argument.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator !=(VariableInt left, VariableInt right)
        {
            return !left.Equals(right);
        }
    }
}