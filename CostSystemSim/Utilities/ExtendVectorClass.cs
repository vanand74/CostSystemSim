using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Meta.Numerics.Matrices;

namespace CostSystemSim {
    public static class ExtendVectorClass {

        /// <summary>
        /// Element-wise multiplication of two vectors.
        /// Identical to MATLAB's '.*' operator.
        /// </summary>
        /// <param name="v1">The first operand.</param>
        /// <param name="v2">The second operand.</param>
        /// <returns>A vector in which each element is the product
        /// of the corresponding elements in v1, v2.</returns>
        private static List<double> elemWiseMultiply( VectorBase v1, VectorBase v2 ) {
            
            if (v1.Dimension != v2.Dimension)
                throw new ApplicationException("Vectors not of same length.");

            return v1.Zip(v2, (x1, x2) => x1 * x2).ToList();
        }

        /// <summary>
        /// Element-wise multiplication of two vectors.
        /// Identical to MATLAB's '.*' operator.
        /// Requires: rv1.Dimension == v2.Dimension
        /// </summary>
        /// <param name="rv1">The first operand.</param>
        /// <param name="v2">The second operand.</param>
        /// <returns>A vector in which each element is the product
        /// of the corresponding elements in rv1, rv2.</returns>
        public static RowVector ewMultiply( this RowVector rv1, VectorBase v2 ) {
            return new RowVector(elemWiseMultiply(rv1, v2));
        }

        /// <summary>
        /// Element-wise multiplication of two vectors.
        /// Identical to MATLAB's '.*' operator.
        /// Requires: rv1.Dimension == v2.Dimension
        /// </summary>
        /// <param name="cv1">The first operand.</param>
        /// <param name="v2">The second operand.</param>
        /// <returns>A vector in which each element is the product
        /// of the corresponding elements in cv1, cv2.</returns>
        public static ColumnVector ewMultiply( this ColumnVector cv1, VectorBase v2 ) {
            return new ColumnVector(elemWiseMultiply(cv1, v2));
        }

        /// <summary>
        /// Implements the map function from functional programming. This 'maps' the 
        /// function lambda onto the vector rv and returns the result.
        /// </summary>
        /// <param name="rv">A RowVector object.</param>
        /// <param name="lambda">A function that takes a double and returns a double.</param>
        /// <returns>A vector with the function lambda mapped onto rv.</returns>
        public static RowVector Map(this RowVector rv, Func<double,double> lambda) {
            // Note: the Select method refers to Enumerable.Select in the Linq
            // namespace.
            return new RowVector(rv.Select(lambda).ToList());
        }

        /// <summary>
        /// Implements the map function from functional programming. This 'maps' the 
        /// function lambda onto the vector rv and returns the result.
        /// </summary>
        /// <param name="rv">A RowVector object.</param>
        /// <param name="lambda">A function that takes a double and an index and 
        /// returns a double.</param>
        /// <returns>A vector with the function lambda mapped onto rv.</returns>
        public static RowVector Map(this RowVector rv, Func<double, int, double> lambda) {
            return new RowVector(rv.Select(lambda).ToList());
        }

        /// <summary>
        /// Implements the map function from functional programming. This 'maps' the 
        /// function lambda onto the vector cv and returns the result.
        /// </summary>
        /// <param name="cv">A ColumnVector object.</param>
        /// <param name="lambda">A function that takes a double and returns a double.</param>
        /// <returns>A vector with the function lambda mapped onto cv.</returns>
        public static ColumnVector Map(this ColumnVector cv, Func<double, double> lambda) {
            // Note: the Select method refers to Enumerable.Select in the Linq
            // namespace.
            return new ColumnVector(cv.Select(lambda).ToList());
        }

        /// <summary>
        /// Formats the vector as a comma-separated string.
        /// </summary>
        /// <param name="v">A vector to be converted to a CSV string.</param>
        /// <param name="writeAsInts">True if the elements of the vector should
        /// be formatted as integers. False if formatted as doubles.</param>
        /// <returns>A string containing each element of the vector, 
        /// separated by commas.</returns>
        public static string ToCSVString( this VectorBase v, bool writeAsInts ) {
            string formatString = 
                "{0:F" + (writeAsInts ? "0" : "4") + "},";
            StringBuilder sb = new StringBuilder();

            foreach (double x in v)
                sb.AppendFormat(formatString, x);

            sb.Remove(sb.Length - 1, 1);

            return sb.ToString();
        }

        /// <summary>
        /// Implements Array.TrueForAll for vectors.
        /// </summary>
        /// <param name="v">A vector of arbitrary dimension.</param>
        /// <param name="match">A delegate specifying a function that will be
        /// checked against all elements of the vector.</param>
        /// <returns>Returns true if match evaluates to true for every element
        /// in the vector.</returns>
        public static bool TrueForAll( this VectorBase v, Predicate<double> match ) {
            return v.ToList().TrueForAll(match);
        }

        /// <summary>
        /// Sets the r'th row of the matrix m to x.
        /// Requires: x.Dimension == m.ColumnCount
        /// </summary>
        /// <param name="m">A matrix whose r'th row will be modified.</param>
        /// <param name="x">A row vector to copy into m.</param>
        /// <param name="r">The zero-based index of the row of m to be modified.</param>
        public static void CopyRowInto(this RectangularMatrix m, RowVector x, int r) {
            for (int i = 0; i < x.Dimension; ++i)
                m[r, i] = x[i];
        }
    }
}