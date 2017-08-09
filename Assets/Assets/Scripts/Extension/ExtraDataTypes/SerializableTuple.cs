using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Extension.ExtraTypes
{
    [Serializable]
    public class SerializableTuple<T, U> : MonoBehaviour
    {
        public T first;
        public U second;

        private static readonly IEqualityComparer Item1Comparer = EqualityComparer<T>.Default;
        private static readonly IEqualityComparer Item2Comparer = EqualityComparer<U>.Default;

        public SerializableTuple(T first, U second)
        {
            this.first = first;
            this.second = second;
        }

        public override string ToString()
        {
            return string.Format("<{0}, {1}>", first, second);
        }

        public static bool operator ==(SerializableTuple<T, U> a, SerializableTuple<T, U> b)
        {
            if (IsNull(a) && !IsNull(b))
                return false;

            if (!IsNull(a) && IsNull(b))
                return false;

            if (IsNull(a) && IsNull(b))
                return true;

            return
                a.first.Equals(b.first) &&
                a.second.Equals(b.second);
        }

        public static bool operator !=(SerializableTuple<T, U> a, SerializableTuple<T, U> b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            int hash = 17;
            hash = hash * 23 + first.GetHashCode();
            hash = hash * 23 + second.GetHashCode();
            return hash;
        }

        public override bool Equals(object obj)
        {
            var other = obj as SerializableTuple<T, U>;
            if (object.ReferenceEquals(other, null))
                return false;
            else
                return Item1Comparer.Equals(first, other.first) && Item2Comparer.Equals(second, other.second);
        }

        private static bool IsNull(object obj)
        {
            return object.ReferenceEquals(obj, null);
        }
    }
}
