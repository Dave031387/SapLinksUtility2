using System;

namespace FileIO
{
    /// <summary>
    /// Exception thrown when an encoded text file is found to be corrupted
    /// </summary>
    public class CorruptedFileException : ApplicationException
    {
        #region Private Fields

        private const string _fileSystemMessage = "Corrupted file exception";
        private string _fileName = null;
        private string _filePath = null;
        private readonly string _msg = null;

        #endregion Private Fields

        #region Public Constructors

        public CorruptedFileException() : base(_fileSystemMessage)
        {
        }

        public CorruptedFileException(string msg) :
            base($"{_fileSystemMessage}: {msg}")
        {
            _msg = msg;
        }

        public CorruptedFileException(string msg, Exception inner) :
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