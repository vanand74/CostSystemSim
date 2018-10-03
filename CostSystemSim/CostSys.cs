using System;
using System.Collections.Generic;
using Meta.Numerics.Matrices;
using System.Linq;

namespace CostSystemSim {

    /// <summary>
    /// Possible outcomes from iterating a cost system and a decision
    /// </summary>
    public enum CostSystemOutcomes {
        /// <summary>
        /// This value should never be attained as it represents an error condition.
        /// </summary>
        Unassigned = Int32.MinValue,
        /// <summary>
        /// Sometimes, costs come out to "not a number" and calculation must stop prematurely.
        /// </summary>
        NaN = -2,
        /// <summary>
        /// The starting decision either leads to, or is part of, a cycle of decisions.
        /// </summary>
        Cycle = -1,
        /// <summary>
        /// The starting decision leads to a death spiral.
        /// </summary>
        ZeroMix = 0,
        /// <summary>
        /// The starting decision either is, or leads to, an equilibrium decision.
        /// </summary>
        Equilibrium = 1
    }

    /// <summary>
    /// This class represents a cost system that is based on limited
    /// information. The class stores mappings of resources to cost
    /// pools (B), and driver choices for each cost pool (D).
    /// </summary>
    public class CostSys {
        #region Fields

        /// <summary>
        /// (Pointer to) the firm upon which this cost system is based.
        /// </summary>
        Firm firm;
        /// <summary>
        /// B[i] is the set of resources (indexes) that would go into 
        /// activity cost pool i, given the optimal mix of true system T0.
        /// </summary>
        List<int>[] B;
        /// <summary>
        /// D[i] is the set of resources (indexes) that constitute 
        /// drivers for activity cost pool i, given the optimal mix of
        /// true system T0.
        /// </summary>
        List<int>[] D;
        /// <summary>
        /// Local copy of simulation-wide parameter a, 
        /// number of activity cost pools
        /// </summary>
        readonly int a;
        /// <summary>
        /// Local copy of simulation-wide parameter p, 
        /// method for grouping resources into pools.
        /// See input file cheat sheet for details.
        /// </summary>
        readonly int p;
        /// <summary>
        /// Local copy of simulation-wide parameter r, 
        /// driver selection method.
        /// See input file cheat sheet for details.
        /// </summary>
        readonly int r;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a cost system. Assigns resources to pools and selects
        /// drivers for each pool.
        /// </summary>
        /// <param name="ip">An input parameters object.</param>
        /// <param name="firm">The firm upon which this cost system is based.</param>
        /// <param name="a">The number of activity cost pools to form.</param>
        /// <param name="p">A flag indicating method for assigning resources to cost pools.
        /// See input file cheat sheet for details.</param>
        /// <param name="r">A flag indicating which resources in the pools
        /// will be used to form drivers. See input file cheat sheet for details.</param>
        public CostSys(
            InputParameters ip,
            Firm firm,
            int a,
            int p,
            int r) 
        {
            this.firm = firm;
            RowVector RCC = firm.Initial_RCC;
            int[] RANK = firm.Initial_RANK;
            SymmetricMatrix CORR = firm.PEARSONCORR;

            this.a = a;
            this.p = p;
            this.r = r;

            if (a != 1) {
                #region Code shared in flowchart 6.1, 6.2, and 6.3

                // Segregate resources into big ones that will each
                // seed a pool, and miscellaneous resources.
                // The first (a-1) resources get their own pools.
                List<int> bigResources = RANK.Take(a - 1).ToList();
                List<int> miscResources = RANK.Skip(a - 1).ToList();

                // Create the set B and initialize the first
                // elements with the big pool resources.

                // Seeding big resources
                // Take each resource from bigPools, ane make it into a list
                // of length 1. Convert to an array of lists, and assign to B.
                B = bigResources.Select(elem => new List<int> { elem }).ToArray();

                // Increase the length by 1, to make room for the miscellaneous
                // pool.
                Array.Resize(ref B, B.Length + 1);
                B[B.Length - 1] = new List<int>();

                #endregion

                // p == 0:
                // Seed (a-1) pools with the largest (a-1) resources. 
                // All remaining resources assigned to miscellaneous pool
                if (p == 0) {
                    #region Flowchart 6.1

                    B[a - 1] = new List<int>(miscResources);

                    #endregion
                }
                // p == 1:
                // Seed acp-1 pools based on size. Check to see 
                // the highest correlation for the remaining resources. Assign the
                // unassigned resource with the highest correlation to 
                // the relevant ACP. Check to see if the value of remaining 
                // ACP > MISCPOOLSIZE. If so, continue to find the next highest 
                // correlation, assign and check. When remaining value < 20%, 
                // then pool everything into misc.
                else if (p == 1) {
                    #region Flowchart 6.2

                    // This query iterates over miscResources. For each one, it
                    // computes the correlation with every bigResource, and forms
                    // a record {smallResourceIndex, index of big pool (in B), correlation }.
                    // Order this list of records in descending order and keep the first one.
                    // This first one is the pool to which the small resources will be allocated
                    // if the correlation is sufficiently high. 
                    var query =
                        miscResources.Select(smallRes => bigResources.Select((bigRes, i) => new { smallRes, BigPoolNum = i, correl = CORR[bigRes, smallRes] }).OrderByDescending(x => x.correl).First());
                    // Order the small resources by correlation with big resources. Thus,
                    // if resource 7 is most correlated with big pool resource 0 (92%),
                    // and resource 12 is most correlated with big pool resource 1 (83%),
                    // 7 will be ahead of 12 in myArray.
                    var myArray = query.OrderByDescending(x => x.correl).ToArray();

                    // The following block makes sure that at least one nonzero
                    // resource is allocated to the last pool. The only time this
                    // fails is if all miscellaneous resources are zero.
                    int lastResourceToAllocate;
                    {
                        // Convert each record in myArray to the value of the resource
                        // cost pool represented by that resource
                        var moo = myArray.Select(x => RCC[x.smallRes]);
                        // Convert each element of moo to the value of the remaining
                        // resources in the array at this point.
                        var moo2 = moo.Select((_, i) => moo.Skip(i).Sum());

                        List<double> ld = moo2.ToList();
                        // If the list contains a 0, that means there are one or
                        // more zero resources. Find the index of the first one,
                        // or if there isn't one, use the end of the array.
                        if (ld.Contains(0.0))
                            lastResourceToAllocate = ld.IndexOf(0.0) - 1;
                        else
                            lastResourceToAllocate = myArray.Length;
                    }

                    double TR = RCC.Sum();
                    double notYetAllocated = miscResources.Aggregate(0.0, (acc, indx) => acc + RCC[indx]);
                    bool cutoffReached = (notYetAllocated / TR) < ip.MISCPOOLSIZE;

                    for (int k = 0; (k < lastResourceToAllocate) && !cutoffReached; ++k) {
                        var q = myArray[k];

                        if (q.correl >= ip.CC) {
                            B[q.BigPoolNum].Add(q.smallRes);
                            miscResources.Remove(q.smallRes);
                        }
                        else
                            break;

                        notYetAllocated = miscResources.Aggregate(0.0, (acc, indx) => acc + RCC[indx]);
                        cutoffReached = (notYetAllocated / TR) < ip.MISCPOOLSIZE;
                    }

                    // Check if there is anything left in miscResources
                    // If yes, throw it in the miscellaneous pool (B.Last()).
                    if (miscResources.Count > 0)
                        B.Last().AddRange(miscResources);
                    // If not, remove the last allocated resource (myArray.Last())
                    // from the pool to which it was allocated, and place it in the
                    // miscellaneous pool.
                    else {
                        var q = myArray.Last();
                        B[q.BigPoolNum].Remove(q.smallRes);
                        B.Last().Add(q.smallRes);
                    }

                    #endregion
                }
                // p == 2: 
                // Seed each of the (a-1) cost pools with the largest resources. 
                // Allocate the remaining resources to the (a-1) pools at random. 
                // However, ensure that enough resources are in the last pool. 
                // The fraction of resources in the last pool is MISCPOOLSIZE.
                else if (p == 2) {
                    #region Flowchart 6.3

                    double TR = RCC.Sum();
                    // Magnitude of resources not yet allocated
                    double notYetAllocated = miscResources.Aggregate(0.0, (acc, indx) => acc + RCC[indx]);
                    // Fraction of resources not yet allocated
                    double miscPoolPrct = notYetAllocated / TR;

                    // Logic: Check if the fraction of resources in 
                    // miscResources is greater than the cap (ip.MISCPOOLSIZE).
                    // If yes, take the first resource from miscResources
                    // and put it in one of the big pools, chosen at random.
                    // If the fraction of resources in miscResources is still
                    // greater than the cap, repeat the loop. Otherwise,
                    // stop and put the remaining resources in the last pool.
                    //
                    // Also stop under the following condition. Assume the head
                    // of the miscResources list is allocated. Is the value of the
                    // remaining resources in miscResources (the tail) greater than
                    // zero? If not, stop. There has to be at least one non-zero
                    // resource in the last pool.
                    while (
                        (miscPoolPrct > ip.MISCPOOLSIZE) && 
                        (miscResources.Skip(1).Aggregate(0.0, (acc, indx) => acc + RCC[indx]) > 0.0)
                    ) {
                        // Pick a pool at random to get the next resource
                        int poolIndx = (int) GenRandNumbers.GenUniformInt(0, a - 2);
                        B[poolIndx].Add(miscResources.First());
                        miscResources.RemoveAt(0);

                        notYetAllocated = miscResources.Aggregate(0.0, (acc, indx) => acc + RCC[indx]);
                        miscPoolPrct = notYetAllocated / TR;
                    }

                    B.Last().AddRange(miscResources);

                    #endregion
                }
                // p == 3:
                // Seed the first pool with the largest resource. 
                // Iterate over the other pools. For each pool, select a seed resource:
                // This is the largest of the remaining, unassigned resources, and
                // assign it to the pool.
                // Form a correlation vector (a list), which is the correlation
                // of each resource in remainingResources with the seed resource. 
                // If the highest correlation is greater than ip.CC, there are 
                // enough remaining resources to fill the remaining pools, and
                // satisfy the constraint about the miscellaneous pool size, 
                //assign resource with the highest correlation to the current pool.
                // Once there are just as many resources remaining as there are pools,
                // assign one resource to each remaining pool.
                else if (p == 3) {
                    #region Flowchart 6.4

                    // Initialize B
                    for (int i = 0; i < B.Length; ++i)
                        B[i] = new List<int>();

                    // Seed the first pool with the largest resource
                    B[0].Add(RANK[0]);
                    List<int> remainingResources = RANK.Skip(1).ToList();

                    // Assign all zero resources to the last (miscellaneous) pool.
                    // That way, each of the remaining pools is guaranteed to have
                    // a nonzero resource. 
                    // This only works if there are at least as many nonzero resources 
                    // as there are pools. If not, then skip this step so that each
                    // pool has at least one resource.
                    int numZeroResources = remainingResources.Count(res => RCC[res] == 0.0);
                    if (RCC.Dimension - numZeroResources >= B.Length) {
                        while (RCC[remainingResources.Last()] == 0.0) {
                            B.Last().Add(remainingResources.Last());
                            remainingResources.RemoveAt(remainingResources.Count - 1);
                        }
                    }

                    // Iterate over the pools. For each pool, select a seed resource,
                    // which is the first resource assigned to the pool.
                    // Form a correlation vector (a list), which is the correlation
                    // of each resource in remainingResources with the seed resource. 
                    // While max of the list is greater than ip.CC, and while 
                    // the other conditions are satisfied, assign resource with the 
                    // maximum correlation to the current pool.
                    // Once condition 2 is no longer true, there are just as many
                    // resources remaining as there are pools. The loop then assigns
                    // one resource to each remaining pool.
                    // Once condition 3 is no longer true, it assigns one resource
                    // to each pool, and all the remaining resources to the last pool.
                    for (int currentPool = 0; currentPool < B.Length - 1; ++currentPool) {
                        int seedResource = B[currentPool].First();
                        int poolsToBeFilled = B.Length - (currentPool + 1);

                        List<double> correlations = remainingResources.Select(res => CORR[res, seedResource]).ToList();
                        bool cond1 = correlations.Max() > ip.CC;
                        bool cond2 = remainingResources.Count > poolsToBeFilled;

                        // Magnitude of resources not yet allocated
                        double notYetAllocated = remainingResources.Aggregate(0.0, (acc, indx) => acc + RCC[indx]);
                        // Fraction of resources not yet allocated
                        double TR = RCC.Sum();
                        double miscPoolPrct = notYetAllocated / TR;
                        bool cond3 = miscPoolPrct > ip.MISCPOOLSIZE;

                        while (cond1 && cond2 && cond3) {
                            // Find the index of the resource with the maximum correlation
                            // with the seed resource
                            double maxCorr = correlations.Max();
                            int maxCorrIndx = remainingResources[correlations.IndexOf(maxCorr)];

                            // Add it to the current pool
                            B[currentPool].Add(maxCorrIndx);

                            // Remove it from the remainingResources list
                            remainingResources.RemoveAt(correlations.IndexOf(maxCorr));
                            correlations.Remove(maxCorr);

                            // Recompute loop termination conditions
                            cond1 = correlations.Max() > ip.CC;
                            cond2 = remainingResources.Count > poolsToBeFilled;
                            notYetAllocated = remainingResources.Aggregate(0.0, (acc, indx) => acc + RCC[indx]);
                            miscPoolPrct = notYetAllocated / TR;
                            cond3 = miscPoolPrct > ip.MISCPOOLSIZE;
                        }

                        B[currentPool + 1].Add(remainingResources[0]);
                        remainingResources.RemoveAt(0);
                    }

                    B.Last().AddRange(remainingResources);

                    #endregion
                }
                else {
                    throw new ApplicationException("Invalid value of p.");
                }
            }
            else {
                #region Flowchart 6.5

                B = new List<int>[] { new List<int>(RANK) };

                #endregion
            }

            // The fraction of RCC that is in the miscellaneous (last)
            // activity cost pool.
            double miscPoolSize = B.Last().Aggregate(0.0, (acc, i) => acc + RCC[i]) / RCC.Sum();

            #region Flowchart 6.5 -- Choosing drivers

            // For each element of B, which is a list of resource indexes,
            // sort it in descending order by pool size (RCC[element]).
            // Technically, this is unnecessary, since elements should have
            // been added to the lists in B in descending order. But instead
            // of assuming that, since that could change in the future,
            // I am going to re-sort. Heck, it's only one line of code,
            // plus this essay of a comment that I just wrote.
            {
                var query = B.Select(list => list.OrderByDescending(indx => RCC[indx]));

                int numToTake;
                if (r == 0)
                    numToTake = 1;
                else if (r == 1)
                    numToTake = ip.NUM;
                else
                    throw new ApplicationException("Invalid value of r in FalseSys.cs.");

                // This iterates over every list in query, and replaces that list
                // with a list containing only the first numToTake elements.
                var drivers = query.Select(list => list.Take(numToTake).ToList());
                D = drivers.ToArray();
            }
            #endregion
        }

        #endregion

        #region Methods

        /// <summary>
        /// Assuming the firm implements decision DECF0, this method computes
        /// reported costs for this cost system. See remarks.
        /// </summary>
        /// <param name="ip">The current InputParameters object</param>
        /// <param name="DECF0">The starting decision. This is used to compute resource consumption.</param>
        /// <returns>The vector of reported costs that results from implementing DECF0. See remarks.</returns>
        /// <remarks>When the firm implements DECF0, it is able to observe total consumption
        /// of each resource needed to implement DECF0. It then uses this cost system to assign
        /// resources to cost pools, and then compute reported costs.</remarks>
        public RowVector CalcReportedCosts(InputParameters ip, RowVector DECF0) {

            #region Make local copies of firm-level variables

            RectangularMatrix res_cons_pat = this.firm.RES_CONS_PAT;

            #endregion

            #region Flowchart 7.3(a) - Compute total resource usage for this product mix

            // Production quantities, given DECF0
            ColumnVector q0 = this.firm.MXQ.ewMultiply(DECF0);

            // Calculate total unit resource consumption
            // required to produce q0
            ColumnVector TRU_F = res_cons_pat * q0;

            #endregion

            #region Flowchart 7.3(b)-(d) - Compute AC and drivers for this product mix

            // Resource usage (in dollars), given DECF0
            RowVector RCC_F = this.firm.RCU.ewMultiply(TRU_F);

            // For each set of resources in B, aggregate the total cost
            // of those resources and put them in ac, the activity cost
            // pools.
            double[] ac = new double[this.a];
            int ACPcount = ac.Length;
            double poolAmount;
            for (int i = 0; i < ACPcount; ++i) {
                poolAmount = 0.0;
                foreach (int resource in B[i]) {
                    poolAmount += RCC_F[resource];
                }
                ac[i] = poolAmount;
            }

            // Filter B, D, and AC to only contain lists for which the
            // corresponding activity cost pool is not empty.
            int ac2Count = ac.Length;
            // B_prime[i] is the set of resources (indexes) that go into 
            // activity cost pool i, given any product mix q. Note that
            // B_prime will be a subset of B.
            List<List<int>> B_prime = new List<List<int>>(ac2Count);
            // D_prime[i] is the index of the resource that constitutes 
            // the driver for activity cost pool i, given any product mix q.
            // Note that D_prime will be a subset of D. CURRENTLY ONLY WORKS
            // FOR BIGPOOL METHOD.
            List<List<int>> D_prime = new List<List<int>>(ac2Count);
            // Vector of activity cost pools that is filtered to remove
            // cost pools with zero resources.
            List<double> ac_prime = new List<double>(ac2Count);
            for (int i = 0; i < ac2Count; ++i) {
                if (ac[i] > 0.0) {
                    B_prime.Add(new List<int>(B[i]));
                    D_prime.Add(new List<int>(D[i]));
                    ac_prime.Add(ac[i]);
                }
            }

            // Choose one resource for each activity cost pool where the
            // resource usage is not zero. Use the First() function to get
            // the driver for the largest resource with non-zero usage.
            // NOTE: The First() function will throw an InvalidOperationException
            // if it does not find any resources that match the predicate condition.
            // However, if our method is programmed correctly, it should ALWAYS
            // find at least one resource per pool that has non-zero usage in TRU_F.
            // If it doesn't, that means the activity cost pool is empty.
            List<int> possibleDrivers2 = new List<int>(B_prime.Count);
            foreach (List<int> l in B_prime) {
                foreach (int resource in l) {
                    //possibleDrivers2.Add( l.First( resource => TRU_F[resource] > 0.0 ) );
                    if (TRU_F[resource] > 0.0) {
                        possibleDrivers2.Add(resource);
                        break;
                    }
                }
            }

            // Big pool method. If there is a driver for a cost pool and that
            // resource has zero usage, replace that driver with one from the set
            // B that has positive usage.
            if (this.r == 0) {
                // I had code here for determining if an alternate driver
                // was selected. I deleted all references to it because
                // I didn't use it anywhere.

                // Iterate over D_prime2, which should be a list of (lists of length 1).
                // If the resource usage for any element in D_prime2 is zero, replace it with 
                // the corresponding element in possibleDrivers.
                for (int i = 0; i < D_prime.Count; ++i) {
                    for (int j = 0; j < D_prime[i].Count; ++j) {
                        if (TRU_F[D_prime[i][j]] == 0.0)
                            D_prime[i][j] = possibleDrivers2[i];
                    }
                }
            }
            // Indexed drivers. 
            else if (this.r == 1) {
                // First, go through the lists in D and remove resources for which there
                // is zero usage
                D_prime =
                    D_prime.Select(list => list.Where(resource => TRU_F[resource] != 0.0).ToList()).ToList();

                // If any list in D_prime is empty, find a resource in the corresponding
                // list in B_prime that has non-zero resource usage, and add that one resource
                // to the empty list in D_prime.
                for (int i = 0; i < D_prime.Count; ++i) {
                    if (D_prime[i].Count == 0) {
                        D_prime[i].Add(possibleDrivers2[i]);
                    }
                }
            }
            else
                throw new ApplicationException("Invalid value of r (driver selection method) in consistency loop.");

            #endregion

            #region Flowchart 7.3(e)-(f) - Compute false system rates and product costs

            Dictionary<int, double> rates = new Dictionary<int, double>();
            for (int d = 0; d < D_prime.Count; ++d) {
                int dLength = D_prime[d].Count;
                double poolNumerator = ac_prime[d] / dLength;

                for (int d2 = 0; d2 < dLength; ++d2) {
                    int resIndx = D_prime[d][d2];
                    rates.Add(resIndx, poolNumerator / TRU_F[resIndx]);
                }
            }

            RowVector PC_R = new RowVector(ip.CO);
            foreach (KeyValuePair<int, double> kvp in rates) {
                for (int co = 0; co < PC_R.Dimension; ++co) {
                    //costs2[co] += res_cons_pat[kvp.Key, co] * kvp.Value;
                    PC_R[co] += res_cons_pat[kvp.Key, co] * kvp.Value;
                }
            }

            #endregion

            return PC_R;
        }

        /// <summary>
        /// Assuming the firm implements decision DECF0, this method computes
        /// reported costs for this cost system, which are used to compute the firm's updated decision. 
        /// It iterates using the updated decision as the new starting decision until a terminal outcome
        /// (e.g. equilibrium, cycle) is reached.
        /// </summary>
        /// <param name="ip">The current InputParameters object</param>
        /// <param name="DECF0">The starting decision.</param>
        /// <returns>Returns the outcome of iterations and the final decision.</returns>
        public (CostSystemOutcomes stopCode, RowVector DECF1) EquilibriumCheck(InputParameters ip, RowVector DECF0) {

            #region Make local copies of firm-level variables

            ColumnVector MXQ = this.firm.MXQ;
            RowVector SP = this.firm.SP;

            #endregion

            // The initial vector of production quantities, given 
            // starting decision DECF0
            ColumnVector q0 = MXQ.ewMultiply(DECF0);
            // A list of past decisions made during the iteration process.
            // If a decision appears twice on this list, then a cycle exists.
            List<RowVector> pastDecisions = new List<RowVector> { DECF0 };
            // The "next" decision that the firm would make. Assume the firm
            // starts with DECF0, computes resulting resource consumption, 
            // and reported costs (through the cost system). Given reported
            // costs, it updates its decision to DECF1.
            RowVector DECF1;

            bool foundThisDecisionBefore;
            double MAR_DROP = 1.0 - ip.HYSTERESIS;
            double MAR_MAKE = 1.0 + ip.HYSTERESIS;
            CostSystemOutcomes stopCode = CostSystemOutcomes.Unassigned;

            bool done;
            do {
                RowVector PC_R = CalcReportedCosts(ip, DECF0);

                if (PC_R.Contains(double.NaN)) {
                    DECF1 = PC_R.Map(x => double.NaN);
                }
                else {
                    double[] MAR = PC_R.Zip(SP, (pc_r, sp) => sp / pc_r).ToArray();
                    var decf1 = MAR.Zip(q0, (mar, q) =>
                       (q > 0.0) ? ((mar <= MAR_DROP) ? 0.0 : 1.0) : ((mar > MAR_MAKE) ? 1.0 : 0.0));
                    DECF1 = new RowVector(decf1.ToList());
                }

                ColumnVector q1 = MXQ.ewMultiply(DECF1);
                if (!(foundThisDecisionBefore = pastDecisions.Contains(DECF1)))
                    pastDecisions.Add(DECF1);

                //double ExpectedCosts = PC_R * q1;
                //double TCF0 = this.firm.CalcTotCosts(q1);

                done = true;
                //if (q1 == q0) {
                if (DECF1 == DECF0) {
                    stopCode = CostSystemOutcomes.Equilibrium;
                }
                else if (q1.TrueForAll(qty => qty == 0.0)) {
                    stopCode = CostSystemOutcomes.ZeroMix;
                }
                else if (foundThisDecisionBefore) {
                    stopCode = CostSystemOutcomes.Cycle;
                }
                else if (DECF1.Contains(double.NaN)) {
                    stopCode = CostSystemOutcomes.NaN;
                }
                else
                    done = false;

                if (!done) {
                    DECF0 = DECF1;
                    q0 = q1;
                }

            } while (!done);

            return (stopCode, DECF1);
        }

        #endregion

        #region Helper methods

        /// <summary>
        /// Treats a vector of 1's and 0's as a binary number and returns
        /// the base 10 equivalent.
        /// </summary>
        /// <param name="BinaryNum">Vector of 1's and 0's</param>
        /// <returns>Base 10 equivalent, as an integer</returns>
        public static int BIN2Int( double[] BinaryNum ) {
            double retval = 0;

            for (int i = 0; i < BinaryNum.Length; ++i)
                retval += BinaryNum[i] * (1 << i);

            return (int)retval;
        }

        /// <summary>
        /// Converts an integer to an array of 1's and 0's.
        /// Returns the result in the in-place array BinaryNum, little endian.
        /// Requires: mixNumber < 2^CO
        /// </summary>
        /// <param name="BinaryNum">The array that will contain the result.</param>
        /// <param name="DecimalNum">The number to be converted to binary</param>
        public static void Int2DECT( double[] BinaryNum, int DecimalNum ) {
            int CO = BinaryNum.Length;
            Array.Clear( BinaryNum, 0, CO );
            int pos = 0;

            while (DecimalNum > 0) {
                BinaryNum[pos++] = DecimalNum % 2;
                DecimalNum /= 2;
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Returns the set of sets, B, that shows which
        /// resources are assigned to which cost pools
        /// in AC. This property is the unadulterated
        /// set B, based on the true system.
        /// </summary>
        public string B_as_String {
            get { return ListUtil.PrintList(B.ToList()); }
        }

        /// <summary>
        /// Returns the set of sets, D, that shows the 
        /// indexes of the resources that serve as drivers 
        /// for the cost pools in AC. This property is the 
        /// unadulterated set D, based on the true system.
        /// </summary>
        public string D_as_String {
            get { return ListUtil.PrintList(D.ToList()); }
        }

        /// <summary>
        /// The number of activity cost pools (ACP)
        /// </summary>
        public int A {
            get { return this.a; }
        }

        /// <summary>
        /// Flag indicating resource pooling method (PACP)
        /// </summary>
        public int P {
            get { return this.p; }
        }

        /// <summary>
        /// Flag indicating driver type (PDR)
        /// </summary>
        public int R {
            get { return this.r; }
        }

        #endregion
    }
}