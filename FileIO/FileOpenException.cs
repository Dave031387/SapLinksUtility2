using System;

namespace FileIO
{
    /// <summary>
    /// Exception thrown when issues are detected opening a file
    /// </summary>
    public class FileOpenException : ApplicationException
    {
        #region Private Fields

        private const string _fileOpenMessage = "File open exception";
        private string _fileName = null;
        private string _filePath = null;
        private readonly string _msg = null;

        #endregion Private Fields

        #region Public Constructors

        public FileOpenException() : base(_fileOpenMessage)
        {
        }

        public FileOpenException(string msg) :
            base($"{_fileOpenMessage}: {msg}")
        {
            _msg = msg;
        }

        public FileOpenException(string msg, Exception inner) :
            base($"{_fileOpenMessage}: {msg}", inner)
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