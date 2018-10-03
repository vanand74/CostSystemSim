using System;
using Meta.Numerics.Statistics.Distributions;
using Meta.Numerics.Matrices;

namespace CostSystemSim {
    /// <summary>This static class generates random numbers
    /// using built-in functions from Meta Numerics.
    /// </summary>
    /// <remarks>
    /// The Meta Numerics library requires the use of a .NET random
    /// number generator. Each time a random number is returned
    /// by a Meta function, the .NET random number generator is
    /// advanced. This class encapsulates that .NET generator
    /// to simplify calls to Meta functions.
    /// </remarks>
    public static class GenRandNumbers {

        /// <summary>
        /// The MersenneTwister object that is used as the random
        /// number generator.
        /// </summary>
        private static MersenneTwister mt;

        private static readonly NormalDistribution normsDist = new NormalDistribution();

        public static long seed;

        /// <summary>
        /// This field keeps track of whether the seed has been
        /// set before. The seed can only be set once during
        /// execution of the code.
        /// </summary>
        private static bool seedHasBeenSet = false;

        /// <summary>
        /// THIS METHOD MUST BE CALLED BEFORE GenRandNumbers
        /// IS USED.
        /// 
        /// Sets the seed of the random number generator to the
        /// parameter seed. This method creates a new MersenneTwister
        /// object using seed as its seed. This method should only
        /// be called zero or one times. If it is called more than
        /// once, it will have no effect. 
        /// </summary>
        /// <param name="seed">The unsigned integer that will be
        /// used as a seed for the random number generator.</param>
        public static void SetSeed( long seed ) {
            if (!seedHasBeenSet) {
                mt = new MersenneTwister( (uint) seed );
                seedHasBeenSet = true;
                GenRandNumbers.seed = seed;
            }
        }

        /// <summary>
        /// Returns the seed used to initialize the random
        /// number generator
        /// </summary>
        /// <returns></returns>
        public static long GetSeed() {
            return seed;
        }

        /// <summary>
        /// Implements the Random.Next(maxValue) method.
        /// </summary>
        /// <param name="maxValue">The non-inclusive upper bound of 
        /// the range of numbers to be returned.</param>
        /// <returns>Returns a random integer in [0, maxValue - 1].</returns>
        public static int Next(int maxValue) {
            return mt.Next(maxValue);
        }

        /// <summary>
        /// Returns a vector with each element populated by a standard
        /// normal distribution.
        /// </summary>
        /// <param name="len">The length of the vector.</param>
        /// <returns>Returns a vector with each element populated by 
        /// a standard normal distribution.</returns>
        public static RowVector GenStdNormalVec(int len) {
            // This line of code is needed to remain consistent with version 1.5 of
            // Meta.Numerics. 
            return new RowVector( len ).Map( x => normsDist.InverseLeftProbability( mt.NextDouble() ) );
            // This line was what I used before upgrading to Meta.Numerics 2.2.
            //return new RowVector(len).Map(x => normsDist.GetRandomValue(mt));
        }

        /// <summary>
        /// Generates a random floating point number in [lb,ub].
        /// Requires: ub >= lb
        /// </summary>
        /// <param name="lb">Lower bound of the range.</param>
        /// <param name="ub">Upper bound of the range.</param>
        /// <returns>A random floating point number in [lb,ub].</returns>
        public static double GenUniformDbl(double lb, double ub) {
            return lb + (mt.NextDouble() * (ub - lb));
        }

        /// <summary>
        /// Returns a random double in the range [0.0, 1.0).
        /// </summary>
        /// <returns>Returns a random double in the range [0.0, 1.0).</returns>
        public static double GenUniformDbl() {
            return mt.NextDouble();
        }

        /// <summary>
        /// Generates an in integer from the discrete uniform distribution
        /// U[lb, ub].
        /// </summary>
        /// <param name="lb">The inclusive lower bound of the distribution.</param>
        /// <param name="ub">The inclusive upper bound of the distribution.</param>
        /// <returns>Returns an in integer from the discrete uniform distribution U[lb, ub]</returns>
        public static double GenUniformInt(int lb, int ub) {
            return mt.Next(lb, ub + 1);
        }

        /// <summary>
        /// Internal class that generates uniformly distributed random numbers.
        /// Inherits from System.Random.
        /// </summary>
        /// <remarks>
        /// C# Version Copyright (C) 2001-2004 Akihilo Kramot (Takel).
        /// C# porting from a C-program for MT19937, originaly coded by
        /// Takuji Nishimura, considering the suggestions by           
        /// Topher Cooper and Marc Rieffel in July-Aug. 1997.          
        /// This library is free software under the Artistic license:  
        ///                                                            
        /// You can find the original C-program at                     
        ///     http://www.math.keio.ac.jp/~matumoto/mt.html           
        ///                                                            
        /// Downloaded this from: http://takel.jp/mt/MersenneTwister.cs */
        /// Also see here: http://takel.jp/mt/MT19937.cs */
        /// And here: http://www.math.sci.hiroshima-u.ac.jp/~m-mat/MT/VERSIONS/C-LANG/c-lang.html */
        /// I found these by typing 'MT19937 c#' into Google. */
        ///
        /// For more info on random number generators, check out the
        /// GSL page on random number generator algorithms:
        /// http://www.gnu.org/software/gsl/manual/html_node/Random-number-generator-algorithms.html
        /// </remarks>
        private class MersenneTwister : System.Random {
            /* Period parameters */
            private const int N = 624;
            private const int M = 397;
            private const uint MATRIX_A = 0x9908b0df; /* constant vector a */
            private const uint UPPER_MASK = 0x80000000; /* most significant w-r bits */
            private const uint LOWER_MASK = 0x7fffffff; /* least significant r bits */

            /* Tempering parameters */
            private const uint TEMPERING_MASK_B = 0x9d2c5680;
            private const uint TEMPERING_MASK_C = 0xefc60000;

            private static uint TEMPERING_SHIFT_U(uint y) { return (y >> 11); }
            private static uint TEMPERING_SHIFT_S(uint y) { return (y << 7); }
            private static uint TEMPERING_SHIFT_T(uint y) { return (y << 15); }
            private static uint TEMPERING_SHIFT_L(uint y) { return (y >> 18); }

            private uint[] mt = new uint[N]; /* the array for the state vector  */

            private short mti;

            private static uint[] mag01 = { 0x0, MATRIX_A };

            /* initializing the array with a NONZERO seed */
            public MersenneTwister(uint seed) {
                /* setting initial seeds to mt[N] using         */
                /* the generator Line 25 of Table 1 in          */
                /* [KNUTH 1981, The Art of Computer Programming */
                /*    Vol. 2 (2nd Ed.), pp102]                  */
                mt[0] = seed & 0xffffffffU;
                for (mti = 1; mti < N; ++mti) {
                    mt[mti] = (69069 * mt[mti - 1]) & 0xffffffffU;
                }
            }

            public MersenneTwister()
                : this(4357) /* a default initial seed is used   */
            {
            }

            protected uint GenerateUInt() {
                uint y;

                /* mag01[x] = x * MATRIX_A  for x=0,1 */
                if (mti >= N) /* generate N words at one time */ {
                    short kk = 0;

                    for (; kk < N - M; ++kk) {
                        y = (mt[kk] & UPPER_MASK) | (mt[kk + 1] & LOWER_MASK);
                        mt[kk] = mt[kk + M] ^ (y >> 1) ^ mag01[y & 0x1];
                    }

                    for (; kk < N - 1; ++kk) {
                        y = (mt[kk] & UPPER_MASK) | (mt[kk + 1] & LOWER_MASK);
                        mt[kk] = mt[kk + (M - N)] ^ (y >> 1) ^ mag01[y & 0x1];
                    }

                    y = (mt[N - 1] & UPPER_MASK) | (mt[0] & LOWER_MASK);
                    mt[N - 1] = mt[M - 1] ^ (y >> 1) ^ mag01[y & 0x1];

                    mti = 0;
                }

                y = mt[mti++];
                y ^= TEMPERING_SHIFT_U(y);
                y ^= TEMPERING_SHIFT_S(y) & TEMPERING_MASK_B;
                y ^= TEMPERING_SHIFT_T(y) & TEMPERING_MASK_C;
                y ^= TEMPERING_SHIFT_L(y);

                return y;
            }

            public virtual uint NextUInt() {
                return this.GenerateUInt();
            }

            public virtual uint NextUInt(uint maxValue) {
                return (uint)(this.GenerateUInt() / ((double)uint.MaxValue / maxValue));
            }

            public virtual uint NextUInt(uint minValue, uint maxValue) /* throws ArgumentOutOfRangeException */
            {
                if (minValue >= maxValue) {
                    throw new ArgumentOutOfRangeException();
                }

                return (uint)(this.GenerateUInt() / ((double)uint.MaxValue / (maxValue - minValue)) + minValue);
            }

            public override int Next() {
                return this.Next(int.MaxValue);
            }

            public override int Next(int maxValue) /* throws ArgumentOutOfRangeException */
            {
                if (maxValue <= 1) {
                    if (maxValue < 0) {
                        throw new ArgumentOutOfRangeException();
                    }

                    return 0;
                }

                return (int)(this.NextDouble() * maxValue);
            }

            public override int Next(int minValue, int maxValue) {
                if (maxValue < minValue) {
                    throw new ArgumentOutOfRangeException();
                }
                else if (maxValue == minValue) {
                    return minValue;
                }
                else {
                    return this.Next(maxValue - minValue) + minValue;
                }
            }

            public override void NextBytes(byte[] buffer) /* throws ArgumentNullException*/
            {
                int bufLen = buffer.Length;

                if (buffer == null) {
                    throw new ArgumentNullException();
                }

                for (int idx = 0; idx < bufLen; ++idx) {
                    buffer[idx] = (byte)this.Next(256);
                }
            }

            public override double NextDouble() {
                return (double)this.GenerateUInt() / ((ulong)uint.MaxValue + 1);
            }
        }
    }
}