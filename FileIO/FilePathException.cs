using System;

namespace FileIO
{
    /// <summary>
    /// Exception thrown for various file path issues
    /// </summary>
    public class FilePathException : ApplicationException
    {
        #region Private Fields

        private const string _filePathMessage = "File path exception";
        private string _fileName = null;
        private string _filePath = null;
        private readonly string _msg = null;

        #endregion Private Fields

        #region Public Constructors

        public FilePathException() : base(_filePathMessage)
        {
        }

        public FilePathException(string msg) :
            base($"{_filePathMessage}: {msg}")
        {
            _msg = msg;
        }

        public FilePathException(string msg, Exception inner) :
            base($"{_filePathMessage}: {msg}", inner)
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