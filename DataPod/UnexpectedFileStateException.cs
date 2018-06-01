using System;

namespace Data
{
    /// <summary>
    /// Exception thrown when an unexpected file stat is detected
    /// </summary>
    internal class UnexpectedFileStateException : ApplicationException
    {
        #region Private Fields

        private const string _baseMessage = "Unexpected file state exception";

        #endregion Private Fields

        #region Public Constructors

        public UnexpectedFileStateException() : base(_baseMessage)
        {
        }

        public UnexpectedFileStateException(string msg) :
            base($"{_baseMessage}: {msg}")
        {
            Message = msg;
        }

        public UnexpectedFileStateException(string msg, Exception inner) :
            base($"{_baseMessage}: {msg}", inner)
        {
            Message = msg;
        }

        #endregion Public Constructors

        #region Public Properties

        public string ActualMode { get; set; }
        public string ActualState { get; set; }
        public string ExpectedMode { get; set; }
        public string ExpectedState { get; set; }
        public override string Message { get; }

        #endregion Public Properties
    }
}