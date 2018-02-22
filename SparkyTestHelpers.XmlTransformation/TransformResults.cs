﻿using System.Xml.Linq;

namespace SparkyTestHelpers.XmlTransformation
{
    internal class TransformResults
    {
        public bool AreSuccessful { get; set; }
        public XDocument XDocument { get; set; }
        public string TransformedXml { get; set; }
        public string FailureMessage { get; set; }
    }
}
