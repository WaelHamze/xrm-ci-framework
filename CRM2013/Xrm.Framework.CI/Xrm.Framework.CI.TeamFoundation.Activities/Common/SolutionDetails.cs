using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Linq;

namespace Xrm.Framework.CI.Common
{
    public class SolutionDetails
    {
        #region Properties

        public string SolutionFile { get; private set; }

        public string SolutionName
        {
            get
            {
                XElement name = SolutionNode.Descendants("UniqueName").First<XElement>();

                if (name != null)
                {
                    return name.Value;
                }
                else
                {
                    throw new Exception(string.Format("UniqueName element not found"));
                }
            }
        }

        public string SolutionVersion
        {
            get
            {
                XElement version = SolutionNode.Descendants("Version").First<XElement>();

                if (version != null)
                {
                    return version.Value;
                }
                else
                {
                    throw new Exception(string.Format("Version element not found"));
                }
            }
            set
            {
                XElement version = SolutionNode.Descendants("Version").First<XElement>();

                if (version != null)
                {
                    version.Value = value;
                }
                else
                {
                    throw new Exception(string.Format("Version element not found"));
                }
            }
        }

        private XElement SolutionNode { get; set; }

        #endregion

        #region Constructors

        private SolutionDetails(string solutionFile)
        {
            SolutionFile = solutionFile;
            Init();
        }
        
        #endregion

        #region Factories

        public static SolutionDetails Create(string solutionFile)
        {
            return new SolutionDetails(solutionFile);
        }

        #endregion

        #region Private Methods

        private void Init()
        {
            if (!File.Exists(SolutionFile))
            {
                throw new Exception(string.Format("{0} does not exist.", SolutionFile));
            }

            SolutionNode = XElement.Load(SolutionFile);
        }

        #endregion

        #region Public Methods

        public void Save()
        {
            SolutionNode.Save(SolutionFile);
        }

        #endregion
    }
}
