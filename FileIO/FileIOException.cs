using System;

namespace FileIO
{
    /// <summary>
    /// Exception thrown for file read/write errors
    /// </summary>
    public class FileIOException : ApplicationException
    {
        #region Private Fields

        private const string _fileSystemMessage = "File I/O exception";
        private string _fileName = null;
        private string _filePath = null;
        private readonly string _msg = null;

        #endregion Private Fields

        #region Public Constructors

        public FileIOException() : base(_fileSystemMessage)
        {
        }

        public FileIOException(string msg) :
            base($"{_fileSystemMessage}: {msg}")
        {
            _msg = msg;
        }

        public FileIOException(string msg, Exception inner) :
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