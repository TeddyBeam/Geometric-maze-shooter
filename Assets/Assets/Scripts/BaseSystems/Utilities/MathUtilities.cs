using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace BaseSystems.Utilities
{
    public class MathUtilities
    {
        public T[] ShuffeArray<T>(T[] array, int seed = 1)
        {
            Debug.Assert(array != null, "Null array.");

            Random random = new Random(seed);
            for (int i = 0; i < array.Length - 1; i++)
            {
                int randomIndex = random.Next(i, array.Length);
                T tempItem = array[randomIndex];
                array[randomIndex] = array[i];
                array[i] = tempItem;
            }
            return array;
        }

        public List<T> ShuffeList<T>(List<T> list, int seed = 1)
        {
            Debug.Assert(list != null, "Null list.");

            Random random = new Random(seed);
            for (int i = 0; i < list.Count - 1; i++)
            {
                int randomIndex = random.Next(i, list.Count);
                T tempItem = list[randomIndex];
                list[randomIndex] = list[i];
                list[i] = tempItem;
            }
            return list;
        }
    }
}
