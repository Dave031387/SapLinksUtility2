using System;

namespace Data
{
    /// <summary>
    /// Exception thrown when there is an error trying to sort the rows in a DataTable
    /// </summary>
    internal class DataTableSortException : ApplicationException
    {
        #region Private Fields

        private const string _baseMessage = "Data table sort exception";

        #endregion Private Fields

        #region Public Constructors

        public DataTableSortException() : base(_baseMessage)
        {
        }

        public DataTableSortException(string msg) :
            base($"{_baseMessage}: {msg}")
        {
            Message = msg;
        }

        public DataTableSortException(string msg, Exception inner) :
            base($"{_baseMessage}: {msg}", inner)
        {
            Message = msg;
        }

        #endregion Public Constructors

        #region Public Properties

        public override string Message { get; }
        public string[] SortFields { get; set; }
        public string TableName { get; set; }

        #endregion Public Properties
    }
}