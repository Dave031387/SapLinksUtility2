using System;

namespace FileIO
{
    /// <summary>
    /// Exception thrown when plain text character value is larger then 0x00ff
    /// </summary>
    public class InvalidCharacterException : ApplicationException
    {
        #region Private Fields

        private const string _fileSystemMessage = "Invalid character exception";
        private string _fileName = null;
        private string _filePath = null;
        private readonly string _msg = null;

        #endregion Private Fields

        #region Public Constructors

        public InvalidCharacterException() : base(_fileSystemMessage)
        {
        }

        public InvalidCharacterException(string msg) :
            base($"{_fileSystemMessage}: {msg}")
        {
            _msg = msg;
        }

        public InvalidCharacterException(string msg, Exception inner) :
            base($"{_fileSystemMessage}: {msg}", inner)
        {
            _msg = msg;
        }

        #endregion Public Constructors

        #region Public Properties

        public string FileName
        {
            get
            {
                if (_fileName == null) return "NULL";
                else return _fileName;
            }
            set
            {
                _fileName = value;
            }
        }

        public string FilePath
        {
            get
            {
                if (_filePath == null) return "NULL";
                else return _filePath;
            }
            set
            {
                _filePath = value;
            }
        }

        public override string Message
        {
            get
            {
                return _msg;
            }
        }

        #endregion Public Properties
    }
}