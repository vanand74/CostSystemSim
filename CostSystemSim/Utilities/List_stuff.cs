using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CostSystemSim {
    /// <summary>
    /// This static class provides a set of routines that will
    /// be useful for list processing in the simulation.
    /// </summary>
    public static class ListUtil {
        /// <summary>
        /// Converts the numbers in the list to proportions.
        /// Sums all elements in the list, and then divides
        /// each element by the sum. The list is normalized in
        /// place.
        /// Requires: the sum of the elements in the list is not 0.
        /// </summary>
        /// <param name="theList">The list that will be normalized.</param>
        public static void Normalize(this IList<double> theList) {
            double mySum = theList.Sum();
            for (int i = 0; i < theList.Count; ++i)
                theList[i] /= mySum;
        }

        /// <summary>
        /// Multiplies every element in the list by the parameter
        /// multiplier. The list is multiplied in place.
        /// </summary>
        /// <param name="theList">The list that will be multiplied.</param>
        /// <param name="multiplier">A factor that will by multiplied
        /// with every element of the list.</param>
        public static void MultiplyBy(this IList<double> theList, double multiplier) {
            for (int i = 0; i < theList.Count; ++i)
                theList[i] *= multiplier;
        }

        /// <summary>
        /// Shuffles a list based on the Fisher-Yates shuffle.
        /// </summary>
        /// <typeparam name="T">A generic type.</typeparam>
        /// <param name="list">The list to be shuffled.</param>
        public static void Shuffle<T>(this IList<T> list) {
            int n = list.Count;
            while (n > 1) {
                n--;
                int k = GenRandNumbers.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        /// <summary>
        /// Returns a string representation of a list of lists:
        /// "{{o11, ..., o1n}, ..., {om1, ..., omn}}"
        /// </summary>
        /// <param name="list">A list of lists of integers.</param>
        /// <returns>"{{o11, ..., o1n}, ..., {om1, ..., omn}}"</returns>
        public static string PrintList(List<List<int>> list) {
            StringBuilder sb = new StringBuilder("{");
            if (list.Count > 0) {
                list.ForEach(elem => sb.AppendFormat("{0}; ", PrintList(elem)));
                sb.Replace("; ", "}", sb.Length - 2, 2);
            }
            else
                sb.Append("}");

            return sb.ToString();
        }

        /// <summary>
        /// Returns a string representation of a list: "{o1, o2, ..., on}"
        /// </summary>
        /// <param name="list">A list of integers.</param>
        /// <returns>"{o1, o2, ..., on}"</returns>
        public static string PrintList(List<int> list) {
            StringBuilder sb = new StringBuilder("{");
            if (list.Count > 0) {
                list.ForEach(elem => sb.AppendFormat("{0}; ", elem));
                sb.Replace("; ", "}", sb.Length - 2, 2);
            }
            else
                sb.Append("}");

            return sb.ToString();
        }

        /// <summary>
        /// Converts a list to a CSV string "e1,e2,...,en"
        /// </summary>
        /// <typeparam name="T">A type</typeparam>
        /// <param name="myList">A list</param>
        /// <returns>A string: "e1,e2,...,en"</returns>
        public static string ToCSVstring<T>(List<T> myList) {
            StringBuilder sb = new StringBuilder();

            if (typeof(T).FullName == "System.Double")
                myList.ForEach(x => sb.AppendFormat("{0:F4},", x));
            else
                myList.ForEach(x => sb.Append(x.ToString() + ","));

            sb.Remove(sb.Length - 1, 1);

            return sb.ToString();
        }

    }
}