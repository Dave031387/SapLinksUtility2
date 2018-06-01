using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    /// <summary>
    /// The DataElement class represents a single piece of data. The class includes properties that
    /// define the type of data element (numeric, text, etc.) and domain (min/max values, disallowed values,
    /// etc.)
    /// </summary>
    public class DataElement
    {
        private string TableName => "DataElements";
    }
}
