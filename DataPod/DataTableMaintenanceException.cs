using System;

namespace Data
{
    /// <summary>
    /// Exception thrown when we are unable to maintain a data table row
    /// </summary>
    internal class DataTableMaintenanceException : ApplicationException
    {
        #region Private Fields

        private const string _baseMessage = "Data table maintenance exception";

        #endregion Private Fields

        #region Public Constructors

        public DataTableMaintenanceException() : base(_baseMessage)
        {
        }

        public DataTableMaintenanceException(string msg) :
            base($"{_baseMessage}: {msg}")
        {
            Message = msg;
        }

        public DataTableMaintenanceException(string msg, Exception inner) :
            base($"{_baseMessage}: {msg}", inner)
        {
            Message = msg;
        }

        #endregion Public Constructors

        #region Public Properties

        public string TableName { get; set; }
        public DataTable.Operation Operation { get; set; }
        public DataRow DataRow { get; set; }
        public override string Message { get; }

        #endregion Public Properties
    }
}