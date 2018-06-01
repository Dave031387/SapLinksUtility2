using System;

namespace Data
{
    /// <summary>
    /// Exception thrown when there is an error in a data row
    /// </summary>
    internal class DataRowException : ApplicationException
    {
        #region Private Fields

        private const string _baseMessage = "Error in data row";

        #endregion Private Fields

        #region Public Constructors

        public DataRowException() : base(_baseMessage)
        {
        }

        public DataRowException(string msg) :
            base($"{_baseMessage}: {msg}")
        {
            Message = msg;
        }

        public DataRowException(string msg, Exception inner) :
            base($"{_baseMessage}: {msg}", inner)
        {
            Message = msg;
        }

        #endregion Public Constructors

        #region Public Properties

        public override string Message { get; }

        #endregion Public Properties
    }
}