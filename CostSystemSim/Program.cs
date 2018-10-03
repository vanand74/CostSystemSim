using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using Meta.Numerics.Matrices;

namespace CostSystemSim {
    class Program {

        static void Main(string[] args) {

            #region Console header

            DrawASCIIart();

            #endregion

            #region Read input file and create InputParameters object

            FileInfo inputFile = new FileInfo( Environment.CurrentDirectory + @"\input.txt" );

            if (!inputFile.Exists) {
                Console.WriteLine("Could not find input file: \n{0}", inputFile.FullName);
                Console.WriteLine( "Aborting. Press ENTER to end the program." );
                Console.ReadLine();
                return;
            }

            InputParameters ip = new InputParameters(inputFile);

            #endregion

            #region Make a copy of the input file

            // We found it helpful to make a copy of the input file every time we ran the
            // simulation. We stamp the copy's filename with the date and time so that
            // we know which results files correspond to which input file.
            DateTime dt = DateTime.Now;
            string inputFileCopyName = 
                String.Format( 
                    "input {0:D2}-{1:D2}-{2:D4} {3:D2}h {4:D2}m {5:D2}s, seed {6:G}.txt", 
                    dt.Month, 
                    dt.Day, 
                    dt.Year, 
                    dt.Hour, 
                    dt.Minute, 
                    dt.Second,
                    GenRandNumbers.GetSeed()
                );
            FileInfo inputFileCopy = new FileInfo( Environment.CurrentDirectory + @"\" + inputFileCopyName );
            inputFile.CopyTo( inputFileCopy.FullName, true );
            File.SetCreationTime( inputFileCopy.FullName, dt );
            File.SetLastWriteTime( inputFileCopy.FullName, dt );

            #endregion

            #region Create output files

            Output.CreateOutputFiles( ip );

            #endregion

            #region Generate Sample of Firms and their Cost Systems

            Firm[] sampleFirms = new Firm[ip.NUM_FIRMS];

            for (int firmID = 1; firmID <= ip.NUM_FIRMS; ++firmID) {
                Console.WriteLine(
                    "Starting firm {0:D3} of {1}",
                    firmID + 1, sampleFirms.Length
                );

                Firm f = new Firm(ip, firmID);
                sampleFirms[firmID - 1] = f;

                for (int a_indx = 0; a_indx < ip.ACP.Count; ++a_indx) {
                    int a = ip.ACP[a_indx];

                    for (int p_indx = 0; p_indx < ip.PACP.Count; ++p_indx) {
                        int p = ip.PACP[p_indx];

                        for (int r_indx = 0; r_indx < ip.PDR.Count; ++r_indx) {
                            int r = ip.PDR[r_indx];

                            // Create a cost system
                            CostSys costsys = new CostSys(ip, f, a, p, r);
                            f.costSystems.Add(costsys);
                            int costSysID = f.costSystems.Count;
                            Output.LogCostSys( costsys, firmID, costSysID );

                            // Generate a starting decision for the cost system.
                            RowVector startingDecision;
                            if (ip.STARTMIX == 0)
                                startingDecision = f.CalcOptimalDecision();
                            else {
                                var ones = Enumerable.Repeat( 1.0, ip.CO ).ToList();
                                startingDecision = new RowVector( ones );
                                for (int i = 0; i < startingDecision.Dimension; ++i) {
                                    if (GenRandNumbers.GenUniformDbl() < ip.EXCLUDE)
                                        startingDecision[i] = 0.0;
                                }
                            }

                            /* Examine error in cost from implementing this decision.
                             * Assume the firm implements the decision startingDecision. Upon
                             * doing so, it will observe total resource consumption. It will then
                             * allocate resources to cost pools, as per the B parameter of the cost
                             * system, choose drivers as per the D parameter of the cost system,
                             * and then allocate resources to cost objects and compute reported costs.
                             * The reported costs are returned as PC_R. The difference
                             * between these and the true benchmark costs (PC_B) is used to compute 
                             * the mean percent error in costs.
                            */
                            RowVector PC_R = costsys.CalcReportedCosts(ip, startingDecision);
                            RowVector PC_B = f.CalcTrueProductCosts();
                            double MPE = PC_B.Zip(PC_R, (pc_b, pc_r) => Math.Abs(pc_b - pc_r) / pc_b).Sum() / PC_B.Dimension;
                            Output.LogCostSysError( costsys, firmID, costSysID, startingDecision, PC_B, PC_R, MPE );

                            /* Assume the firm implements the decision startingDecision. Upon
                             * doing so, it will observe total resource consumption. It will then
                             * allocate resources to cost pools, as per the B parameter of the cost
                             * system, choose drivers as per the D parameter of the cost system,
                             * and then allocate resources to cost objects and compute reported costs.
                             * The reported costs are returned as PC_R. Upon observing the
                             * reported costs, the firm may wish to update its original decision. When
                             * it implements the updated decision, costs will change again. The outcome
                             * of this process will either be an equilibrium decision (fixed point), or
                             * a cycle of decisions.
                             */
                            (CostSystemOutcomes stopCode, RowVector endingDecision) = costsys.EquilibriumCheck(ip, startingDecision);
                            Output.LogCostSysLoop( costsys, firmID, costSysID, startingDecision, endingDecision, stopCode );
                        }
                    }
                }
            }

            #endregion

            Console.WriteLine("Writing output files...");
            Output.WriteOutput();
            Console.WriteLine( "Done!" );
        }

        /// <summary>
        /// Draws a randomly chosen picture in the Console window. For fun.
        /// </summary>
        static void DrawASCIIart() {
            // http://www.chris.com/ascii/

            System.Reflection.Assembly ass = 
                System.Reflection.Assembly.GetExecutingAssembly();
            string[] sa = ass.GetManifestResourceNames();

            Random rnd = new Random();

            int indexOfGraphic = rnd.Next(sa.Length);
            Stream strm = ass.GetManifestResourceStream(sa[indexOfGraphic]);

            using (StreamReader sr = new StreamReader(strm)) {
                Console.WriteLine(sr.ReadToEnd());
                sr.Close();
            }

            Console.WriteLine();

            return;
        }
    }
}