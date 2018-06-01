using System;

namespace Data
{
    /// <summary>
    /// Exception thrown when unable to load a Data Table from its associated file
    /// </summary>
    internal class DataTableLoadException : ApplicationException
    {
        #region Private Fields

        private const string _baseMessage = "Unable to load data table";

        #endregion Private Fields

        #region Public Constructors

        public DataTableLoadException() : base(_baseMessage)
        {
        }

        public DataTableLoadException(string msg) :
            base($"{_baseMessage}: {msg}")
        {
            Message = msg;
        }

        public DataTableLoadException(string msg, Exception inner) :
            base($"{_baseMessage}: {msg}", inner)
        {
            Message = msg;
        }

        #endregion Public Constructors

        #region Public Properties

        public string DirectoryPath { get; set; }
        public string FileName { get; set; }
        public override string Message { get; }
        public string TableName { get; set; }

        #endregion Public Properties
    }
}