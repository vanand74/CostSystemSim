using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Meta.Numerics.Matrices;
using System.Linq;

namespace CostSystemSim {
    /// <summary>
    /// This class handles output for the simulation.
    /// Output files are in CSV format.
    /// </summary>
    public static class Output {

        #region Output file names

        /// <summary>
        /// Name of file containing summary information about firms
        /// </summary>
        private static readonly string file_Firm_SUM = "Firm_SUM.csv";
        /// <summary>
        /// Name of file containing RCC (resource consumption) and RCU (unit resource price)
        /// for firms.
        /// </summary>
        private static readonly string file_Firm_RESCON = "Firm_RESCON.csv";
        /// <summary>
        /// Name of file containing information about each firm's products
        /// </summary>
        private static readonly string file_Firm_PRODUCT = "Firm_PRODUCT.csv";
        /// <summary>
        /// Name of file containing summary information about cost systems
        /// </summary>
        private static readonly string file_CostSys_SUM = "CostSys_SUM.csv";
        /// <summary>
        /// Name of file containing information about error in reported costs
        /// </summary>
        private static readonly string file_CostSys_ERROR = "CostSys_ERROR.csv";
        /// <summary>
        /// Name of file containing the results of looping from a starting decision
        /// </summary>
        private static readonly string file_CostSys_LOOP = "CostSys_LOOP.csv";

        #endregion

        #region StringBuilders for in-memory storage of output

        /// <summary>
        /// Temporary, in-memory storage for output file Firm_SUM
        /// </summary>
        private static StringBuilder sb_Firm_SUM = new StringBuilder();
        /// <summary>
        /// Temporary, in-memory storage for output file Firm_RESCON
        /// </summary>
        private static StringBuilder sb_Firm_RESCON = new StringBuilder();
        /// <summary>
        /// Temporary, in-memory storage for output file Firm_PRODUCT
        /// </summary>
        private static StringBuilder sb_Firm_PRODUCT = new StringBuilder();
        /// <summary>
        /// Temporary, in-memory storage for output file CostSys_SUM
        /// </summary>
        private static StringBuilder sb_CostSys_SUM = new StringBuilder();
        /// <summary>
        /// Temporary, in-memory storage for output file Costsys_ERROR
        /// </summary>
        private static StringBuilder sb_CostSys_ERROR = new StringBuilder();
        /// <summary>
        /// Temporary, in-memory storage for output file CostSys_LOOP
        /// </summary>
        private static StringBuilder sb_CostSys_LOOP = new StringBuilder();

        #endregion

        #region Method to create and initialize output files 

        /// <summary>
        /// Creates the output files and writes their headers.
        /// This should be called exactly once during the simulation,
        /// before any calls to the logging functions in this class.
        /// </summary>
        /// <param name="ip">An input parameters object</param>
        public static void CreateOutputFiles( InputParameters ip) {

            // Create firm summary file
            string Hdr_Firm_SUM = "FirmID,g,d,numRCP,numCO,BenchmarkRevenue,BenchmarkTotCost,BenchmarkProfit,NumProdInBenchmarkMix" + Environment.NewLine;
            File.WriteAllText( file_Firm_SUM, Hdr_Firm_SUM );

            // Create firm resource consumption file
            StringBuilder Hdr_Firm_RESCON = new StringBuilder( "FirmID,g,d" );
            Hdr_Firm_RESCON.Append( GenIndexedStringList( "RCC_", ip.RCP ) );
            Hdr_Firm_RESCON.Append( GenIndexedStringList( "RCU_", ip.RCP ) );
            Hdr_Firm_RESCON.Append( Environment.NewLine );
            File.WriteAllText( file_Firm_RESCON, Hdr_Firm_RESCON.ToString() );

            // Create firm product information file
            StringBuilder Hdr_Firm_PRODUCT = new StringBuilder( "FirmID,g,d" );
            Hdr_Firm_PRODUCT.Append( GenIndexedStringList( "MAR_", ip.CO ) );
            Hdr_Firm_PRODUCT.Append( GenIndexedStringList( "MXQ_", ip.CO ) );
            Hdr_Firm_PRODUCT.Append( GenIndexedStringList( "SP_", ip.CO ) );
            Hdr_Firm_PRODUCT.Append( GenIndexedStringList( "DECT0_", ip.CO ) );
            Hdr_Firm_PRODUCT.Append( GenIndexedStringList( "Rank_by_val_", ip.CO ) );
            Hdr_Firm_PRODUCT.Append( GenIndexedStringList( "Rank_by_mar_", ip.CO ) );
            Hdr_Firm_PRODUCT.Append( Environment.NewLine );
            File.WriteAllText( file_Firm_PRODUCT, Hdr_Firm_PRODUCT.ToString() );

            // Create cost system summary file
            string Hdr_CostSys_SUM = "FirmID,CostSysID,PACP,ACP,PDR,B,D" + Environment.NewLine;
            File.WriteAllText( file_CostSys_SUM, Hdr_CostSys_SUM );

            // Create cost system product information file
            StringBuilder Hdr_CostSys_ERROR = new StringBuilder("FirmID,CostSysID,PACP,ACP,PDR");
            Hdr_CostSys_ERROR.Append( GenIndexedStringList( "startDecision_", ip.CO ) );
            Hdr_CostSys_ERROR.Append( GenIndexedStringList( "PC_B_", ip.CO ) );
            Hdr_CostSys_ERROR.Append( GenIndexedStringList( "PC_R_", ip.CO ) );
            Hdr_CostSys_ERROR.AppendLine( ",MPE" );
            File.WriteAllText( file_CostSys_ERROR, Hdr_CostSys_ERROR.ToString() );

            // Create cost system looping results file
            StringBuilder Hdr_CostSys_LOOP = new StringBuilder( "FirmID,CostSysID,PACP,ACP,PDR" );
            Hdr_CostSys_LOOP.Append( GenIndexedStringList( "startDecision_", ip.CO ) );
            Hdr_CostSys_LOOP.Append( GenIndexedStringList( "endingDecision_", ip.CO ) );
            Hdr_CostSys_LOOP.AppendLine( ",outcome" );
            File.WriteAllText( file_CostSys_LOOP, Hdr_CostSys_LOOP.ToString() );
        }

        #endregion

        #region Methods for logging information

        /// <summary>
        /// Writes summary information about a firm. Writes RCC and RCU vectors for
        /// the firm in the RESCON file. Writes info about the firm's 
        /// products (margins, capacities, selling prices).
        /// </summary>
        /// <param name="ip">An InputParameters object.</param>
        /// <param name="firm">The firm object whose data will be logged.</param>
        /// <param name="firmID">A unique identifier for this firm</param>
        /// <param name="MAR">Vector of product margins. Products with
        /// margins >= 1 are produced.</param>
        /// <param name="DECT0">Vector of 0 and 1's indicating which products
        /// are produced.</param>
        /// <param name="revenue">Revenue realized when producing the benchmark product mix</param>
        /// <param name="totalCost">Total cost incurred when producing the benchmark product mix</param>
        /// <param name="benchProfit">Profit realized when producing the benchmark product mix</param>
        /// <param name="RCC">Vector of resource costs</param>
        public static void LogFirm(
            InputParameters ip, Firm firm,
            int firmID,
            RowVector MAR, RowVector DECT0,
            double revenue, double totalCost, double benchProfit,
            RowVector RCC )
        {
            #region Log summary information for the firm

            int numProdInMix = DECT0.Count(x => x == 1.0);

            sb_Firm_SUM.AppendFormat(
                "{0},{1},{2},{3},{4},{5:F2},{6:F2},{7:F2},{8},{9}",
                firmID, firm.G, firm.D,
                ip.RCP, ip.CO,
                revenue, totalCost, benchProfit,
                numProdInMix,
                Environment.NewLine
            );

            #endregion

            #region Log resource consumption information for the firm

            sb_Firm_RESCON.AppendFormat(
                "{0},{1},{2},{3},{4},{5}",
                firmID, firm.G, firm.D,
                RCC.ToCSVString(false),
                firm.RCU.ToCSVString(false),
                Environment.NewLine
            );

            #endregion

            #region Log product information for the firm

            #region Create rank vectors
            // Rank the products by value (by total profit)
            RowVector RANK_BY_VAL;
            {
                // Some algebra: the profit of a product is
                // (SP - PC_B) x QT
                // = (SP - SP/MAR) x (MXQ x DECT0)
                // = SP (1 - 1/MAR) x (MXQ x DECT0)
                // where all operations are element-wise
                var unitProfit = firm.SP.Zip(MAR, (sp, mar) => sp * (1.0 - (1.0 / mar)));
                var productionQty = firm.MXQ.Zip(DECT0, (mxq, dect0) => mxq * dect0);
                var totProfit = unitProfit.Zip(productionQty, (pi, q) => pi * q);
                double[] PROFIT = totProfit.ToArray();

                var rank = Enumerable.Range(0, ip.CO).Select(x => (double) x);
                // If the product is not produced, set its rank value
                // to ip.CO
                var rank2 = rank.Zip(DECT0, (r, dect0) => (dect0 == 1.0) ? r : (double) ip.CO);

                double[] rank_by_val = rank2.ToArray();
                Array.Sort(PROFIT, rank_by_val);
                Array.Reverse(rank_by_val);
                RANK_BY_VAL = new RowVector(rank_by_val);
            }

            // Rank the products by margin
            RowVector RANK_BY_MAR;
            {
                double[] rank_by_mar =
                    Enumerable.Range(0, ip.CO).Select(x => (double) x).ToArray();
                double[] mar = MAR.ToArray();
                Array.Sort(mar, rank_by_mar);
                Array.Reverse(rank_by_mar);
                RANK_BY_MAR = new RowVector(rank_by_mar);
            }
            #endregion

            sb_Firm_PRODUCT.AppendFormat(
                "{0},{1},{2},{3},{4},{5},{6},{7},{8},{9}",
                firmID, firm.G, firm.D,
                MAR.ToCSVString(false),
                firm.MXQ.ToCSVString(true),
                firm.SP.ToCSVString(false),
                DECT0.ToCSVString(true),
                RANK_BY_VAL.ToCSVString(true),
                RANK_BY_MAR.ToCSVString(true),
                Environment.NewLine
            );

            #endregion
        }

        /// <summary>
        /// Writes summary information about a cost system.
        /// </summary>
        /// <param name="costsys">Pointer to the CostSys object being logged</param>
        /// <param name="firmID">Unique identifier of the containing firm</param>
        /// <param name="costSysID">Identifier of the cost system</param>
        public static void LogCostSys( CostSys costsys, int firmID, int costSysID ) {
            sb_CostSys_SUM.AppendFormat( 
                "{0},{1},{2},{3},{4},{5},{6},{7}",
                firmID, costSysID,
                costsys.P, costsys.A, costsys.R,
                costsys.B_as_String, costsys.D_as_String,
                Environment.NewLine
            );
        }

        /// <summary>
        /// Records the error in reported costs that results from implementing a decision.
        /// </summary>
        /// <param name="costsys">Pointer to the CostSys object being logged</param>
        /// <param name="firmID">Unique identifier of the containing firm</param>
        /// <param name="costSysID">Identifier of the cost system</param>
        /// <param name="startingDecision">A binary vector indicating which products were produced</param>
        /// <param name="PC_B">A vector of true product costs</param>
        /// <param name="PC_R">A vector of reported product costs</param>
        /// <param name="MPE">The mean percent error between PC_B and PC_R</param>
        public static void LogCostSysError(
            CostSys costsys, int firmID, int costSysID, 
            RowVector startingDecision, 
            RowVector PC_B, RowVector PC_R, double MPE ) 
        {
            sb_CostSys_ERROR.AppendFormat(
                "{0},{1},{2},{3},{4},{5},{6},{7},{8},{9}",
                firmID, costSysID,
                costsys.P, costsys.A, costsys.R,
                startingDecision.ToCSVString(false),
                PC_B.ToCSVString(false), PC_R.ToCSVString(false),
                MPE,
                Environment.NewLine
            );
        }

        /// <summary>
        /// Records the outcome of implementing a decision, observing reported costs, and
        /// updating the original decision. Repeats this process until a terminal outcome
        /// results. See remarks.
        /// </summary>
        /// <param name="costsys"></param>
        /// <param name="firmID"></param>
        /// <param name="costSysID"></param>
        /// <param name="startingDecision"></param>
        /// <param name="endingDecision"></param>
        /// <param name="stopCode"></param>
        /// <remarks>Assume the firm implements the decision startingDecision. Upon
        /// doing so, it will observe total resource consumption.It will then
        /// allocate resources to cost pools, as per the B parameter of the cost
        /// system, choose drivers as per the D parameter of the cost system,
        /// and then allocate resources to cost objects and compute reported costs.
        /// The reported costs are returned as PC_R. Upon observing the
        /// reported costs, the firm may wish to update its original decision.When
        /// it implements the updated decision, costs will change again.The outcome
        /// of this process will either be an equilibrium decision (fixed point), or
        /// a cycle of decisions.</remarks>
        public static void LogCostSysLoop( 
            CostSys costsys, int firmID, int costSysID, 
            RowVector startingDecision, RowVector endingDecision, 
            CostSystemOutcomes stopCode ) 
        {
            sb_CostSys_LOOP.AppendFormat(
                "{0},{1},{2},{3},{4},{5},{6},{7},{8}",
                firmID, costSysID,
                costsys.P, costsys.A, costsys.R,
                startingDecision.ToCSVString( false ),
                endingDecision.ToCSVString(false),
                stopCode,
                Environment.NewLine
            );
        }

        #endregion

        #region Method to write StringBuilders to output files

        /// <summary>
        /// Writes the contents of the StringBuilders to the output files.
        /// This should be called once at the end of the simulation.
        /// </summary>
        public static void WriteOutput() {
            File.AppendAllText( file_Firm_SUM, sb_Firm_SUM.ToString() );
            File.AppendAllText( file_Firm_RESCON, sb_Firm_RESCON.ToString() );
            File.AppendAllText( file_Firm_PRODUCT, sb_Firm_PRODUCT.ToString() );
            File.AppendAllText( file_CostSys_SUM, sb_CostSys_SUM.ToString() );
            File.AppendAllText( file_CostSys_ERROR, sb_CostSys_ERROR.ToString() );
            File.AppendAllText( file_CostSys_LOOP, sb_CostSys_LOOP.ToString() );
        }

        #endregion

        #region Utility method

        /// <summary>
        /// Given s and maxVal, returns ",s0,s1,...,s(maxVal-1)"
        /// </summary>
        /// <param name="s">The string to repeat</param>
        /// <param name="maxVal">The non-inclusive upper bound of the range.</param>
        /// <returns>",s0,s1,...,s(maxVal-1)"</returns>
        private static string GenIndexedStringList(string s, int maxVal) {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < maxVal; ++i) {
                sb.AppendFormat(",{0}{1}", s, i);
            }

            return sb.ToString();
        }

        #endregion
    }
}