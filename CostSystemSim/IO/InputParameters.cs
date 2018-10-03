using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Collections.ObjectModel;
using System.Data;
using System.Reflection;

namespace CostSystemSim {
    /// <summary>
    /// This class encapsulates all input parameters for the simulation. 
    /// Its constructor will read these from an input file.
    /// </summary>
    /// <remarks>
    /// To modify this file: See detailed instructions in the Input File Cheat Sheet
    /// </remarks>
    public class InputParameters {
        #region Fields and properties

        /// <summary>
        /// Total value of resources. This is only used to compute the initial RCC vector.
        /// </summary>
        private double tr;
        /// <summary>
        /// Total value of resources. This is only used to compute the initial RCC vector.
        /// </summary>
        public double TR {
            get { return tr; }
            protected set {
                if (value <= 0.0)
                    throw new ArgumentOutOfRangeException("Total resources must be positive.");
                else
                    tr = value;
            }
        }

        /// <summary>
        /// Number of products (cost objects)
        /// </summary>
        private int co;
        /// <summary>
        /// Number of products (cost objects)
        /// </summary>
        public int CO {
            get { return co; }
            protected set {
                if (value < 1)
                    throw new ArgumentOutOfRangeException("There must be at least one cost object.");
                else
                    co = value;
            }
        }

        /// <summary>
        /// Number of resources
        /// </summary>
        private int rcp;
        /// <summary>
        /// Number of resources
        /// </summary>
        public int RCP {
            get { return rcp; }
            protected set {
                if (value < 1)
                    throw new ArgumentOutOfRangeException("There must be at least one resource.");
                else
                    rcp = value;
            }
        }

        /// <summary>
        /// The number of simulated firms that will be created.
        /// This is the basis of the sample size.
        /// </summary>
        private int num_firms;
        /// <summary>
        /// The number of simulated firms that will be created.
        /// This is the basis of the sample size.
        /// </summary>
        public int NUM_FIRMS {
            get { return num_firms; }
            protected set {
                if (value < 1)
                    throw new ArgumentOutOfRangeException("There must be at least one firm in the sample.");
                else
                    num_firms = value;
            }
        }

        /// <summary>
        /// One of two parameters used to specify dispersion in resource costs. 
        /// The top DISP1 resources will account for DISP2 percent of the total 
        /// resources in the initial RCC vector created for each firm. 
        /// For example, 10 resources (DISP1) might account for 75% (DISP2) of total cost.
        /// </summary>
        private int disp1;
        /// <summary>
        /// One of two parameters used to specify dispersion in resource costs. 
        /// The top DISP1 resources will account for DISP2 percent of the total 
        /// resources in the initial RCC vector created for each firm. 
        /// For example, 10 resources (DISP1) might account for 75% (DISP2) of total cost.
        /// </summary>
        public int DISP1 {
            get { return disp1; }
            protected set {
                if (value < 1)
                    throw new ArgumentOutOfRangeException("DISP1 must be positive.");
                else
                    disp1 = value;
            }
        }

        /// <summary>
        /// One of two parameters used to specify dispersion in resource costs. 
        /// The top DISP1 resources will account for DISP2 percent of the total 
        /// resources in the initial RCC vector created for each firm. 
        /// For example, 10 resources (DISP1) might account for 75% (DISP2) of total cost.
        /// DISP2 must be between 0.0 and 1.0. Also, DISP2 must be greater than or
        /// equal to the ratio (DISP1 / RCP). DISP2 is sometimes referred to as g in the code.
        /// </summary>
        private double disp2_min;
        /// <summary>
        /// One of two parameters used to specify dispersion in resource costs. 
        /// The top DISP1 resources will account for DISP2 percent of the total 
        /// resources in the initial RCC vector created for each firm. 
        /// For example, 10 resources (DISP1) might account for 75% (DISP2) of total cost.
        /// DISP2 must be between 0.0 and 1.0. Also, DISP2 must be greater than or
        /// equal to the ratio (DISP1 / RCP). DISP2 is sometimes referred to as g in the code.
        /// </summary>
        public double DISP2_MIN {
            get { return disp2_min; }
            protected set {
                if (value <= 0.0 || value >= 1.0)
                    throw new ArgumentOutOfRangeException("DISP2 must be between 0.0 and 1.0, exclusive.");
                else
                    disp2_min = value;
            }
        }

        /// <summary>
        /// One of two parameters used to specify dispersion in resource costs. 
        /// The top DISP1 resources will account for DISP2 percent of the total 
        /// resources in the initial RCC vector created for each firm. 
        /// For example, 10 resources (DISP1) might account for 75% (DISP2) of total cost.
        /// DISP2 must be between 0.0 and 1.0. Also, DISP2 must be greater than or
        /// equal to the ratio (DISP1 / RCP). DISP2 is sometimes referred to as g in the code.
        /// </summary>
        private double disp2_max;
        /// <summary>
        /// One of two parameters used to specify dispersion in resource costs. 
        /// The top DISP1 resources will account for DISP2 percent of the total 
        /// resources in the initial RCC vector created for each firm. 
        /// For example, 10 resources (DISP1) might account for 75% (DISP2) of total cost.
        /// DISP2 must be between 0.0 and 1.0. Also, DISP2 must be greater than or
        /// equal to the ratio (DISP1 / RCP). DISP2 is sometimes referred to as g in the code.
        /// </summary>
        public double DISP2_MAX {
            get { return disp2_max; }
            protected set {
                if (value <= 0.0 || value >= 1.0)
                    throw new ArgumentOutOfRangeException("DISP2 must be between 0.0 and 1.0, exclusive.");
                else
                    disp2_max = value;
            }
        }

        /// <summary>
        /// Minimum density of the resource consumption pattern matrix (RES_CONS_PAT). 
        /// A density of 0.8 means that approximately 20% of the elements of the 
        /// matrix are zero. This parameter is sometimes referred to as d in the code.
        /// </summary>
        private double dns_min;
        /// <summary>
        /// Minimum density of the resource consumption pattern matrix (RES_CONS_PAT). 
        /// A density of 0.8 means that approximately 20% of the elements of the 
        /// matrix are zero. This parameter is sometimes referred to as d in the code.
        /// </summary>
        public double DNS_MIN {
            get { return dns_min; }
            protected set {
                if (value <= 0.0 || value >= 1.0)
                    throw new ArgumentOutOfRangeException("DNS_MIN not valid: 0.0 < DNS_MIN < 1.0");
                else
                    dns_min = value;
            }
        }

        /// <summary>
        /// Maximum density of the resource consumption pattern matrix (RES_CONS_PAT). 
        /// A density of 0.8 means that approximately 20% of the elements of the 
        /// matrix are zero. This parameter is sometimes referred to as d in the code.
        /// </summary>
        private double dns_max;
        /// <summary>
        /// Maximum density of the resource consumption pattern matrix (RES_CONS_PAT). 
        /// A density of 0.8 means that approximately 20% of the elements of the 
        /// matrix are zero. This parameter is sometimes referred to as d in the code.
        /// </summary>
        public double DNS_MAX {
            get { return dns_max; }
            protected set {
                if (value <= 0.0 || value >= 1.0)
                    throw new ArgumentOutOfRangeException("DNS_MAX not valid: 0.0 < DNS_MAX < 1.0");
                else
                    dns_max = value;
            }
        }

        /// <summary>
        /// The number of activity cost pools in the cost systems.
        /// </summary>
        public ReadOnlyCollection<int> ACP;
        /// <summary>
        /// The number of activity cost pools in the cost systems.
        /// </summary>
        /// <param name="acp">The different values of ACP to be used</param>
        protected void SetACP(IList<int> acp) {
            if (acp.Count == 0)
                throw new ArgumentOutOfRangeException("You must specify at least one value of ACP.");
            if (acp.Any(a => a < 1))
                throw new ArgumentOutOfRangeException("Number of cost pools must be positive.");

            ACP = new ReadOnlyCollection<int>(acp);
        }

        /// <summary>
        /// A flag (0, 1, 2, 3) indicating how resources will be pooled into
        /// activity cost pools in the false system. See the input file cheat
        /// sheet for descriptions of the flags.
        /// </summary>
        public ReadOnlyCollection<int> PACP;
        /// <summary>
        /// A flag (0, 1, 2, 3) indicating how resources will be pooled into
        /// activity cost pools in the false system. See the input file cheat
        /// sheet for descriptions of the flags.
        /// </summary>
        protected void SetPACP(IList<int> pacp) {
            if (pacp.Any(p => (p < 0) || (p > 3)))
                throw new ArgumentOutOfRangeException("Valid values for PACP are 0, 1, 2, 3.");

            PACP = new ReadOnlyCollection<int>(pacp);
        }

        /// <summary>
        /// A flag indicating driver selection in the false system. PDR==0 
        /// indicates that the driver is bigpool. PDR==1 indicates that the driver 
        /// is indexed with NUM resources included in the index.
        /// </summary>
        public ReadOnlyCollection<int> PDR;
        /// <summary>
        /// A flag indicating driver selection in the false system. PDR==0 
        /// indicates that the driver is bigpool. PDR==1 indicates that the driver 
        /// is indexed with NUM resources included in the index.
        /// </summary>
        protected void SetPDR(IList<int> pdr) {
            if (pdr.Any(a => (a < 0) || (a > 1)))
                throw new ArgumentOutOfRangeException("Valid values for PDR are 0 and 1.");

            PDR = new ReadOnlyCollection<int>(pdr);
        }

        /// <summary>
        /// The number of resources to be included in indexed drivers.
        /// </summary>
        private int num;
        /// <summary>
        /// The number of resources to be included in indexed drivers.
        /// </summary>
        public int NUM {
            get { return num; }
            protected set {
                if (value < 1)
                    throw new ArgumentOutOfRangeException("NUM must be positive.");
                else
                    num = value;
            }
        }

        /// <summary>
        /// The fraction of resources that should be in the last
        /// activity cost pool.
        /// </summary>
        private double miscpoolsize;
        /// <summary>
        /// The fraction of resources that should be in the last
        /// activity cost pool.
        /// </summary>
        public double MISCPOOLSIZE {
            get { return miscpoolsize; }
            protected set {
                if (value <= 0.0 || value >= 1.0)
                    throw new ArgumentOutOfRangeException("MISCPOOLSIZE must be between 0.0 and 1.0, exclusive.");
                else
                    miscpoolsize = value;
            }
        }

        /// <summary>
        /// Lower bound on COR1, the correlation between the largest resources
        /// </summary>
        private double cor1lb;
        /// <summary>
        /// Lower bound on COR1, the correlation between the largest resources
        /// </summary>
        public double COR1LB {
            get { return cor1lb; }
            protected set {
                if (value < -1.0 || value > 1.0)
                    throw new ArgumentOutOfRangeException("COR1LB must be between 0.0 and 1.0, inclusive.");
                else
                    cor1lb = value;
            }
        }

        /// <summary>
        /// Upper bound on COR1, the correlation between the large resources
        /// </summary>
        private double cor1ub;
        /// <summary>
        /// Upper bound on COR1, the correlation between the large resources
        /// </summary>
        public double COR1UB {
            get { return cor1ub; }
            protected set {
                if (value < -1.0 || value > 1.0)
                    throw new ArgumentOutOfRangeException("COR1UB must be between 0.0 and 1.0, inclusive.");
                else
                    cor1ub = value;
            }
        }

        /// <summary>
        /// Lower bound on COR2, the correlation between the small resources
        /// </summary>
        private double cor2lb;
        /// <summary>
        /// Lower bound on COR2, the correlation between the small resources
        /// </summary>
        public double COR2LB {
            get { return cor2lb; }
            protected set {
                if (value < -1.0 || value > 1.0)
                    throw new ArgumentOutOfRangeException("COR2LB must be between 0.0 and 1.0, inclusive.");
                else
                    cor2lb = value;
            }
        }

        /// <summary>
        /// Upper bound on COR2, the correlation between the small resources
        /// </summary>
        private double cor2ub;
        /// <summary>
        /// Upper bound on COR2, the correlation between the small resources
        /// </summary>
        public double COR2UB {
            get { return cor2ub; }
            protected set {
                if (value < -1.0 || value > 1.0)
                    throw new ArgumentOutOfRangeException("COR2UB must be between 0.0 and 1.0, inclusive.");
                else
                    cor2ub = value;
            }
        }

        /// <summary>
        /// Correlation cutoff for correlation-based methods of assigning
        /// resources to pools.
        /// </summary>
        private double cc;
        /// <summary>
        /// Correlation cutoff for correlation-based methods of assigning
        /// resources to pools.
        /// </summary>
        public double CC {
            get { return cc; }
            protected set {
                if (value < 0.0 || value > 1.0)
                    throw new ArgumentOutOfRangeException("CC must be between 0.0 and 1.0, inclusive.");
                else
                    cc = value;
            }
        }

        /// <summary>
        /// Lower bound on MAR, the (random) margin on products.
        /// Margin is price per unit divided by total cost per unit,
        /// so a product that earns zero profit will have a margin of 1.0.
        /// </summary>
        private double marlb;
        /// <summary>
        /// Lower bound on MAR, the (random) margin on products. 
        /// Margin is price per unit divided by total cost per unit, 
        /// so a product that earns zero profit will have a margin of 1.0.
        /// </summary>
        public double MARLB {
            get { return marlb; }
            protected set {
                if (value <= 0.0)
                    throw new ArgumentOutOfRangeException("MARLB must be positive.");
                else
                    marlb = value;
            }
        }

        /// <summary>
        /// Upper bound on MAR, the (random) margin on products. 
        /// Margin is price per unit divided by total cost per unit, 
        /// so a product that earns zero profit will have a margin of 1.0.
        /// </summary>
        private double marub;
        /// <summary>
        /// Lower bound on MAR, the (random) margin on products. 
        /// Margin is price per unit divided by total cost per unit, 
        /// so a product that earns zero profit will have a margin of 1.0.
        /// </summary>
        public double MARUB {
            get { return marub; }
            protected set {
                if (value <= 0.0)
                    throw new ArgumentOutOfRangeException("MARUB must be positive.");
                else
                    marub = value;
            }
        }

        /// <summary>
        /// A value of zero means that the starting mix for the consistency loop is 
        /// the benchmark mix. A value of 1 means that the starting mix can be 
        /// adjusted using the EXCLUDE parameter.
        /// </summary>
        private int startmix;
        /// <summary>
        /// A value of zero means that the starting mix for the consistency loop is 
        /// the benchmark mix. A value of 1 means that the starting mix can be 
        /// adjusted using the EXCLUDE parameter.
        /// </summary>
        public int STARTMIX {
            get { return startmix; }
            protected set {
                if ((value < 0) || (value > 1))
                    throw new ArgumentOutOfRangeException("STARTMIX must be 0 or 1.");
                else
                    startmix = value;
            }
        }

        /// <summary>
        /// EXCLUDE is used to adjust the starting mix. EXCLUDE is ignored if
        /// STARTMIX == 0. If the value is 0.1, then 10% of the products, 
        /// on average from the benchmark mix will be excluded
        /// from the starting mix of the convergence loop.
        /// </summary>
        private double exclude;
        /// <summary>
        /// EXCLUDE is used to adjust the starting mix. EXCLUDE is ignored if
        /// STARTMIX == 0. If the value is 0.1, then 10% of the products, 
        /// on average from the benchmark mix will be excluded
        /// from the starting mix of the convergence loop.
        /// </summary>
        public double EXCLUDE {
            get { return exclude; }
            protected set {
                if ((value < 0.0) || (value > 1.0))
                    throw new ArgumentOutOfRangeException("EXCLUDE must be between 0 or 1, inclusive.");
                else
                    exclude = value;
            }
        }

        /// <summary>
        /// If true, the random number generator will be initialized
        /// with the seed provided by the SEED parameter of the input
        /// file. If false, the random number generator will be initialized
        /// using the system clock.
        /// </summary>
        public bool USESEED { get; protected set; }

        /// <summary>
        /// The seed value used to initialize the random number generator.
        /// </summary>
        public long SEED { get; protected set; }

        /// <summary>
        /// With hysteresis, to add a product to the current product mix, its reported margin 
        /// must exceed (1.0 + hysteresis). To drop a product from the mix,
        /// its margin must be less than (1.0 - hysteresis).
        /// Setting this parameter to zero means no hysteresis.
        /// </summary>
        private double hysteresis;
        /// <summary>
        /// With hysteresis, to add a product to the current product mix, its reported margin 
        /// must exceed (1.0 + hysteresis). To drop a product from the mix,
        /// its margin must be less than (1.0 - hysteresis).
        /// Setting this parameter to zero means no hysteresis.
        /// </summary>
        public double HYSTERESIS {
            get { return hysteresis; }
            protected set {
                if (value < 0)
                    throw new ArgumentOutOfRangeException("HYSTERESIS must be greater than or equal to 0.");
                else
                    hysteresis = value;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Reads the input parameters from a text file. 
        /// Verifies that all input values are valid, and runs the
        /// method EnforceConstraints().
        /// </summary>
        /// <param name="inputFileName">An object that encapsulates the input file.</param>
        public InputParameters(FileInfo inputFileName) {

            // The data structure dictMembers keeps track of the public properties 
            // and fields that have been assigned to. The constructor checks that all
            // parameters are assigned to. 
            Dictionary<string, bool> dictMembers = new Dictionary<string, bool>();
            // The following code uses Reflection to obtain a list of all public
            // properties and fields, and adds their names to the dictionary.
            MemberInfo[] members = this.GetType().GetProperties();
            foreach (MemberInfo mi in members)
                dictMembers.Add(mi.Name, false);
            members = this.GetType().GetFields();
            foreach (MemberInfo mi in members)
                dictMembers.Add(mi.Name, false);

            // The remaining code in this constructor reads input file, line by line,
            // and sets the fields of this class.
            string[] mySplit;
            char[] separator = { ',' };
            string[] fileLines = File.ReadAllLines(inputFileName.FullName);

            // Each line of the file should have the format:
            // Parameter_name,value1,value2,...
            foreach (string line in fileLines) {
                // Skip comment lines
                if (line.StartsWith("//")) continue;

                mySplit = line.Trim().Split(separator);
                if (mySplit.Length < 2) {
                    string s =
                        String.Format(
                            "The input line:{0}{1}{0} is invalid. Enter parameter name, a comma, and then a value or comma-separated list of values",
                            Environment.NewLine, line
                        );
                    throw new InvalidDataException(s);
                }

                string paramName = mySplit[0].Trim().ToUpper();

                // Make sure only one data value was given for the parameter, unless
                // a list is allowed. At present, only ACP, PACP, and PDR accept lists.
                int numParams = mySplit.Length - 1;
                if ((paramName != "ACP") && (paramName != "PACP") && (paramName != "PDR")) {
                    if (numParams != 1) {
                        string msg = String.Format("Looks like you entered multiple values for parameter {0}. Only 1 is allowed.", paramName);
                        throw new InvalidDataException(msg);
                    }
                }

                switch (paramName) {
                    case "TR":
                        TR = ParseDouble(mySplit[1]);
                        dictMembers["TR"] = true;
                        break;
                    case "CO":
                        CO = ParseInt(mySplit[1]);
                        dictMembers["CO"] = true;
                        break;
                    case "RCP":
                        RCP = ParseInt(mySplit[1]);
                        dictMembers["RCP"] = true;
                        break;
                    case "NUM_FIRMS":
                        NUM_FIRMS = ParseInt(mySplit[1]);
                        dictMembers["NUM_FIRMS"] = true;
                        break;
                    case "DISP1":
                        DISP1 = ParseInt(mySplit[1]);
                        dictMembers["DISP1"] = true;
                        break;
                    case "DISP2_MIN":
                        DISP2_MIN = ParseDouble(mySplit[1]);
                        dictMembers["DISP2_MIN"] = true;
                        break;
                    case "DISP2_MAX":
                        DISP2_MAX = ParseDouble(mySplit[1]);
                        dictMembers["DISP2_MAX"] = true;
                        break;
                    case "DNS_MIN":
                        DNS_MIN = ParseDouble(mySplit[1]);
                        dictMembers["DNS_MIN"] = true;
                        break;
                    case "DNS_MAX":
                        DNS_MAX = ParseDouble(mySplit[1]);
                        dictMembers["DNS_MAX"] = true;
                        break;
                    case "ACP":
                        SetACP(ParseIntList(mySplit.Skip(1)));
                        dictMembers["ACP"] = true;
                        break;
                    case "PACP":
                        SetPACP(ParseIntList(mySplit.Skip(1)));
                        dictMembers["PACP"] = true;
                        break;
                    case "PDR":
                        SetPDR(ParseIntList(mySplit.Skip(1)));
                        dictMembers["PDR"] = true;
                        break;
                    case "NUM":
                        NUM = ParseInt(mySplit[1]);
                        dictMembers["NUM"] = true;
                        break;
                    case "MISCPOOLSIZE":
                        MISCPOOLSIZE = ParseDouble(mySplit[1]);
                        dictMembers["MISCPOOLSIZE"] = true;
                        break;
                    case "COR1LB":
                        COR1LB = ParseDouble(mySplit[1]);
                        dictMembers["COR1LB"] = true;
                        break;
                    case "COR1UB":
                        COR1UB = ParseDouble(mySplit[1]);
                        dictMembers["COR1UB"] = true;
                        break;
                    case "COR2LB":
                        COR2LB = ParseDouble(mySplit[1]);
                        dictMembers["COR2LB"] = true;
                        break;
                    case "COR2UB":
                        COR2UB = ParseDouble(mySplit[1]);
                        dictMembers["COR2UB"] = true;
                        break;
                    case "CC":
                        CC = ParseDouble(mySplit[1]);
                        dictMembers["CC"] = true;
                        break;
                    case "MARLB":
                        MARLB = ParseDouble(mySplit[1]);
                        dictMembers["MARLB"] = true;
                        break;
                    case "MARUB":
                        MARUB = ParseDouble(mySplit[1]);
                        dictMembers["MARUB"] = true;
                        break;
                    case "STARTMIX":
                        STARTMIX = ParseInt(mySplit[1]);
                        dictMembers["STARTMIX"] = true;
                        break;
                    case "EXCLUDE":
                        EXCLUDE = ParseDouble(mySplit[1]);
                        dictMembers["EXCLUDE"] = true;
                        break;
                    case "USESEED":
                        if (mySplit[1].Equals("TRUE", StringComparison.OrdinalIgnoreCase))
                            USESEED = true;
                        else if (mySplit[1].Equals("FALSE", StringComparison.OrdinalIgnoreCase))
                            USESEED = false;
                        else
                            throw new InvalidDataException("Invalid value for USESEED in input file.");
                        dictMembers["USESEED"] = true;
                        break;
                    case "SEED":
                        SEED = ParseLong(mySplit[1]);
                        dictMembers["SEED"] = true;
                        break;
                    case "HYSTERESIS":
                        HYSTERESIS = ParseDouble(mySplit[1]);
                        dictMembers["HYSTERESIS"] = true;
                        break;
                    default:
                        string msg = String.Format("The parameter name {0} is invalid. Aborting.", paramName);
                        throw new InvalidDataException(msg);
                }
            }

            // Make sure all parameters have been initialized (i.e. input file is complete)
            var q = dictMembers.Where(kvp => kvp.Value == false);
            if (q.Count() > 0) {
                Console.WriteLine("The following parameters did not appear in your input file.");
                Console.WriteLine("Please add them and run the simulation again.");
                Console.WriteLine("Aborting.");

                foreach (KeyValuePair<string, bool> kvp in q) {
                    Console.WriteLine(kvp.Key);
                }

                throw new MissingFieldException();
            }

            EnforceConstraints();

            if (this.USESEED)
                GenRandNumbers.SetSeed(this.SEED);
            else
                GenRandNumbers.SetSeed(DateTime.Now.Ticks);
        }

        #endregion

        #region Other constraints on parameters

        /// <summary>
        /// This method specifies additional constraints on parameters.
        /// Use this method to create constraints that rely on runtime values
        /// of parameters. This method should be called after all parameter
        /// values have been assigned.
        /// </summary>
        /// <remarks>Since the order of assignment of values of these parameters 
        /// is unknown at compile time, this method should be used to check for 
        /// constraints that rely on the runtime values.
        /// For example, MARUB >= MARLB.</remarks>
        private void EnforceConstraints() {
            if (rcp < co)
                throw new ArgumentException("RCP must be greater than or equal to CO.");
            if (disp1 > rcp)
                throw new ArgumentException("DISP1 must be less than or equal to RCP.");
            if (!(disp2_min >= (disp1 / rcp)))
                throw new ArgumentException("DISP2_MIN must be greater than or equal to (DISP1 / RCP).");
            if (disp2_max < disp2_min)
                throw new ArgumentException("DISP2_MAX must be greater than or equal to DISP2_MIN.");
            if (dns_max < dns_min)
                throw new ArgumentException("DNS_MAX must be greater than or equal to DNS_MIN.");
            if (ACP.Any(a => a > RCP))
                throw new ArgumentException("Number of activity cost pools must be less than or equal to the number of resources.");
            if (num * ACP.Max() > rcp)
                throw new ArgumentException("Number of resources used in indexed drivers is greater than number of available resources.");
            if (cor1ub < cor1lb)
                throw new ArgumentException("COR1UB must be greater than or equal to COR1LB.");
            if (cor2ub < cor2lb)
                throw new ArgumentException("COR2UB must be greater than or equal to COR2LB.");
            if (marub < marlb)
                throw new ArgumentException("MARUB must be greater than or equal to MARLB.");
        }

        #endregion

        #region Helper methods

        /// <summary>
        /// Converts an input string to a double using Double.TryParse().
        /// Throws an InvalidDataException if the conversion fails.
        /// </summary>
        /// <param name="s">A string to be converted to a double</param>
        /// <returns>Double representation of s, or throws an InvalidDataException</returns>
        double ParseDouble(string s) {
            if (!Double.TryParse(s, out double d)) {
                string msg = String.Format("The string {0} cannot be converted to double", s);
                throw new InvalidDataException(msg);
            }

            return d;
        }

        /// <summary>
        /// Converts an input string to a double using Int32.TryParse().
        /// Throws an InvalidDataException if the conversion fails.
        /// </summary>
        /// <param name="s">A string to be converted to an int</param>
        /// <returns>Integer representation of s, or throws an InvalidDataException</returns>
        int ParseInt(string s) {
            if (!Int32.TryParse(s, out int i)) {
                string msg = String.Format("The string {0} cannot be converted to int", s);
                throw new InvalidDataException(msg);
            }

            return i;
        }

        /// <summary>
        /// Converts an input string to a long using Int64.TryParse().
        /// Throws an InvalidDataException if the conversion fails.
        /// </summary>
        /// <param name="s">A string to be converted to a long</param>
        /// <returns>Long representation of s, or throws an InvalidDataException</returns>
        long ParseLong(string s) {
            if (!Int64.TryParse(s, out long i)) {
                string msg = String.Format("The string {0} cannot be converted to long", s);
                throw new InvalidDataException(msg);
            }

            return i;
        }

        /// <summary>
        /// Converts a list of strings to a list of integers.
        /// Throws a FormatException or OverflowException if any conversion fails.
        /// </summary>
        /// <param name="sa">Enumerable list of strings</param>
        /// <returns>List of integers, or throws an exception</returns>
        List<int> ParseIntList(IEnumerable<string> sa) {
            return sa.Select(s => Convert.ToInt32(s)).ToList();
        }

        #endregion
    }
}
