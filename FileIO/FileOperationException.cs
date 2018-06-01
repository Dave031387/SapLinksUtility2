using System;

namespace FileIO
{
    /// <summary>
    /// Exception thrown when errors occur coying, moving, or renaming a file
    /// </summary>
    public class FileOperationException : ApplicationException
    {
        #region Private Fields

        private const string _fileMovementMessage = "File operation exception";
        private readonly string _msg = null;

        #endregion Private Fields

        #region Public Constructors

        public FileOperationException() : base(_fileMovementMessage)
        {
        }

        public FileOperationException(string msg) :
            base($"{_fileMovementMessage}: {msg}")
        {
            _msg = msg;
        }

        public FileOperationException(string msg, Exception inner) :
            base($"{_fileMovementMessage}: {msg}", inner)
        {
            _msg = msg;
        }

        #endregion Public Constructors

        #region Public Properties

        public override string Message
        {
            get
            {
                return _msg;
            }
        }

        public string SourcePath
        {
            get; set; } = null;

        public string TargetPath
        {
            get; set; } = null;

        #endregion Public Properties
    }
}