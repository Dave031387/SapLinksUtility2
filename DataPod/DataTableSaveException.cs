using System;

namespace Data
{
    /// <summary>
    /// Exception thrown when unable to save a Data Table to its associated file
    /// </summary>
    internal class DataTableSaveException : ApplicationException
    {
        #region Private Fields

        private const string _baseMessage = "Unable to save data table";

        #endregion Private Fields

        #region Public Constructors

        public DataTableSaveException() : base(_baseMessage)
        {
        }

        public DataTableSaveException(string msg) :
            base($"{_baseMessage}: {msg}")
        {
            Message = msg;
        }

        public DataTableSaveException(string msg, Exception inner) :
            base($"{_baseMessage}: {msg}", inner)
        {
            Message = msg;
        }

        #endregion Public Constructors

        #region Public Properties

        public string TableName { get; set; }
        public string FileName { get; set; }
        public string DirectoryPath { get; set; }
        public override string Message { get; }

        #endregion Public Properties
    }
}