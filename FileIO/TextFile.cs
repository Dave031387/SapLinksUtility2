using System;
using System.Collections.Generic;
using System.IO;

namespace FileIO
{
    /// <summary>
    /// The FileMode enumeration tells which mode a file has been opened in - INITIAL - The file
    /// hasn't been opened yet. READ - The file has been opened for reading. WRITE - The file has
    /// been opened for writing.
    /// </summary>
    public enum FileMode { INITIAL, READ, WRITE }

    /// <summary>
    /// The FileState enumeration indicates the state of the current file - INITIAL - The file object
    /// has been created but not opened yet. OPEN - Then file object has been opened. CLOSED - The
    /// file object has been closed.
    /// </summary>
    public enum FileState { INITIAL, OPEN, CLOSED }

    /// <summary>
    /// Object represents a plain text flat file. Implements IDataFile interface.
    /// </summary>
    public class TextFile : IDataFile
    {
        #region Protected Fields

        protected const string EMPTY = "EMPTY";
        protected const string NA = "n/a";
        protected const string NULL = "NULL";

        /// <summary>
        /// A collection of strings. These are the lines that are either read from or written to the file.
        /// </summary>
        protected List<string> _fileData = new List<string>();

        #endregion Protected Fields

        #region Private Fields

        /// <summary>
        /// The absolute directory path where the file is located
        /// </summary>
        private string _directoryPath = null;

        /// <summary>
        /// The file mode (INITIAL, READ, or WRITE)
        /// </summary>
        private FileMode _fileMode = FileMode.INITIAL;

        /// <summary>
        /// The name of the file to be read from or written to.
        /// </summary>
        private string _fileName = null;

        /// <summary>
        /// For files opened for reading, this gives the index of the next line to be read from the
        /// _fileData collection. For files opened for writing, this gives the index of the next line
        /// to be written to the _fileData collection. A value of -1 means the file is empty.
        /// </summary>
        private int _filePosition = -1;

        /// <summary>
        /// The current state of the file (INITIAL, OPEN, or CLOSED)
        /// </summary>
        private FileState _fileState = FileState.INITIAL;

        #endregion Private Fields

        #region Public Constructors

        /// <summary>
        /// The default constructor doesn't do anything other than to instantiate a TextFile object.
        /// </summary>
        public TextFile() { }

        #endregion Public Constructors

        #region Private Destructors

        /// <summary>
        /// This is a finalizer that closes the text file if it hasn't been closed already before
        /// handing things over to the garbage collector.
        /// </summary>
        ~TextFile()
        {
            if (State == FileState.OPEN) Close();
        }

        #endregion Private Destructors

        #region Public Properties

        /// <summary>
        /// Public property that returns the number of lines in the file
        /// </summary>
        public virtual int Count
        {
            get
            {
                return _fileData.Count;
            }
        }

        /// <summary>
        /// Public property that returns the file path of the TextFile object
        /// </summary>
        public virtual string DirectoryPath
        {
            get
            {
                if (_directoryPath == null) return NULL;
                return _directoryPath;
            }
            protected set
            {
                _directoryPath = value;
            }
        }

        /// <summary>
        /// Public property that returns true if we are positioned at the end of the file
        /// </summary>
        public virtual bool EndOfFile
        {
            get
            {
                return (Position < 0 || Position >= Count);
            }
        }

        /// <summary>
        /// Public property that returns the file name of the TextFile object
        /// </summary>
        public virtual string FileName
        {
            get
            {
                if (_fileName == null) return NULL;
                return _fileName;
            }
            protected set
            {
                _fileName = value;
            }
        }

        /// <summary>
        /// Public property that returns the full file path and file name of the TextFile object
        /// </summary>
        public virtual string FilePath
        {
            get
            {
                if (_fileName == null || _directoryPath == null) return NULL;
                return Path.Combine(_directoryPath, _fileName);
            }
        }

        /// <summary>
        /// Protected property for getting and setting the file mode
        /// </summary>
        public virtual FileMode Mode
        {
            get
            {
                return _fileMode;
            }
            protected set
            {
                _fileMode = value;
            }
        }

        /// <summary>
        /// Public property that returns a zero-based index to the current line in the file
        /// </summary>
        public virtual int Position
        {
            get
            {
                return _filePosition;
            }
            protected set
            {
                _filePosition = value;
            }
        }

        /// <summary>
        /// Protected property for getting and setting the file state
        /// </summary>
        public virtual FileState State
        {
            get
            {
                return _fileState;
            }
            protected set
            {
                _fileState = value;
            }
        }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Close the text file. If the file was opened for writing, then first write the lines of
        /// text to the physical file.
        /// </summary>
        public virtual void Close()
        {
            if (State == FileState.OPEN)
            {
                if (Mode == FileMode.WRITE && Count > 0) Save();
                _fileData.Clear();
                Position = -1;
                DirectoryPath = null;
                FileName = null;
                Mode = FileMode.INITIAL;
                State = FileState.CLOSED;
            }
        }

        /// <summary>
        /// Copy the file to a different directory
        /// </summary>
        /// <param name="directoryPath">
        /// The target directory path
        /// </param>
        /// <returns>
        /// Returns the full file path of the target file
        /// </returns>
        public virtual string Copy(string directoryPath)
        {
            // Get the absolute directory path
            string targetPath = ValidateTargetPath(directoryPath);
            // Verify that the file is open
            if (State != FileState.OPEN)
            {
                throw new FileOpenException("File not open")
                {
                    FileName = FileName,
                    FilePath = DirectoryPath
                };
            }
            // Determine the full target file path
            string fullFilePath;
            try
            {
                fullFilePath = Path.Combine(targetPath, FileName);
            }
            catch (Exception e)
            {
                throw new FilePathException("Unable to determine full target file path", e)
                {
                    FilePath = targetPath,
                    FileName = FileName
                };
            }
            // The target file path must not match the source file path
            if (fullFilePath == FilePath)
            {
                throw new FileOperationException("Source and target are the same")
                {
                    SourcePath = FilePath,
                    TargetPath = fullFilePath
                };
            }
            // The target file must not already exist
            try
            {
                FileOps.FileMustNotExist(fullFilePath);
            }
            catch (Exception e)
            {
                throw new FileOpenException("Target file already exists", e)
                {
                    FileName = FileName,
                    FilePath = targetPath
                };
            }

            // If the source file is open for write, then save it first
            if (Mode == FileMode.WRITE)
            {
                Save();
            }
            // Copy the file to the target path
            try
            {
                File.Copy(FilePath, fullFilePath);
            }
            catch (Exception e)
            {
                throw new FileOperationException("Error copying file to target location", e)
                {
                    SourcePath = FilePath,
                    TargetPath = fullFilePath
                };
            }
            return fullFilePath;
        }

        /// <summary>
        /// Creates a new text file for writing
        /// </summary>
        /// <param name="filePath">
        /// The file path of the text file to be opened
        /// </param>
        public virtual void CreateForWrite(string filePath)
        {
            // Check to see if the file is already open. Throw an exception if it is.
            if (State == FileState.OPEN)
            {
                throw new FileOpenException("File already open")
                {
                    FilePath = DirectoryPath,
                    FileName = FileName
                };
            }
            // Obtain the absolute file path.
            ParseFilePath(filePath);
            // Throw an exception if the file already exists.
            FileOps.FileMustNotExist(FilePath);
            // Create the new text file
            FileOps.CreateFile(FilePath);
            // Finish opening the text file for writing
            State = FileState.OPEN;
            Mode = FileMode.WRITE;
            Position = 0;
        }

        /// <summary>
        /// Creates a new text file for writing
        /// </summary>
        /// <param name="directoryPath">
        /// The directory path where the file is located
        /// </param>
        /// <param name="fileName">
        /// The file name of the text file to be opened
        /// </param>
        public virtual void CreateForWrite(string directoryPath, string fileName)
        {
            string filePath = FileOps.CombinePath(directoryPath, fileName);
            CreateForWrite(filePath);
        }

        /// <summary>
        /// Creates a new text file if the specified file doesn't already exist
        /// </summary>
        /// <param name="filePath">The full file path of the file to be created</param>
        public virtual void CreateIfFileDoesNotExist(string filePath)
        {
            // This method should be called only if the TextFile object is in an INITIAL state.
            if (State != FileState.INITIAL)
            {
                throw new FileOpenException("File state is not INITIAL")
                {
                    FilePath = DirectoryPath,
                    FileName = FileName
                };
            }
            // Obtain the absolute file path.
            ParseFilePath(filePath);
            // Create an empty file if the specified file does not exist.
            if (!File.Exists(filePath))
            {
                FileOps.CreateFile(filePath);
            }
        }

        /// <summary>
        /// Creates a new text file if the specified file doesn't already exist
        /// </summary>
        /// <param name="directoryPath">The directory path where the file is located</param>
        /// <param name="fileName">The file name of the text file to be created</param>
        public virtual void CreateIfFileDoesNotExist(string directoryPath, string fileName)
        {
            string filePath = FileOps.CombinePath(directoryPath, fileName);
            CreateIfFileDoesNotExist(filePath);
        }

        /// <summary>
        /// Move the file to a different directory
        /// </summary>
        /// <param name="directoryPath">
        /// The target directory path
        /// </param>
        /// <returns>
        /// Returns the full file path of the target file
        /// </returns>
        public virtual string Move(string directoryPath)
        {
            // First copy the current file to the target directory
            string filePath = Copy(directoryPath);
            string targetPath = FileOps.GetDirectoryPath(filePath);
            // Delete the file from the source location if the copy operation was successful
            try
            {
                File.Delete(FilePath);
            }
            catch (Exception e)
            {
                throw new FileOperationException("Error removing file from source location", e)
                {
                    SourcePath = FilePath,
                    TargetPath = targetPath
                };
            }
            DirectoryPath = targetPath;
            return filePath;
        }

        /// <summary>
        /// Open an existing text file for reading
        /// </summary>
        /// <param name="filePath">
        /// The file path and file name of the text file to be opened
        /// </param>
        public virtual void OpenForRead(string filePath)
        {
            // Check to see if the file is already open. Throw an exception if it is.
            if (State == FileState.OPEN)
            {
                throw new FileOpenException("File already open")
                {
                    FilePath = DirectoryPath,
                    FileName = FileName
                };
            }
            // Obtain the absolute file path. Throw an exception if the path is invalid.
            ParseFilePath(filePath);
            // Throw an exception if the file doesn't exist.
            FileOps.FileMustExist(FilePath);
            State = FileState.OPEN;
            Mode = FileMode.READ;
            // Read the contents of the file into the _fileData collection
            try
            {
                using (StreamReader sr = new StreamReader(FilePath))
                {
                    while (sr.Peek() >= 0) // Check to see if we've reached the end of the file
                    {
                        _fileData.Add(sr.ReadLine()); // Read the next line from the file
                    }
                    if (Count > 0) Position = 0; // Set the position index to the first line
                    else Position = -1; // Set position index to a negative number if the file is empty
                }
            }
            catch (Exception e)
            {
                string msg = $"Error reading line {Count + 1} from file";
                throw new FileIOException(msg, e)
                {
                    FilePath = DirectoryPath,
                    FileName = FileName
                };
            }
        }

        /// <summary>
        /// Open an existing text file for reading
        /// </summary>
        /// <param name="directoryPath">
        /// The directory path where the file is located
        /// </param>
        /// <param name="fileName">
        /// The file name of the text file to be opened
        /// </param>
        public virtual void OpenForRead(string directoryPath, string fileName)
        {
            string filePath = FileOps.CombinePath(directoryPath, fileName);
            OpenForRead(filePath);
        }

        /// <summary>
        /// Open an existing text file for writing
        /// </summary>
        /// <param name="filePath">
        /// The file path and file name of the text file to be opened
        /// </param>
        public virtual void OpenForWrite(string filePath)
        {
            // Check to see if the file is already open. Throw an exception if it is.
            if (State == FileState.OPEN)
            {
                throw new FileOpenException("File already open")
                {
                    FilePath = DirectoryPath,
                    FileName = FileName
                };
            }
            // Obtain the absolute file path.
            ParseFilePath(filePath);
            // Throw an exception if the file doesn't exist.
            FileOps.FileMustExist(FilePath);
            // Finish opening the text file for writing
            State = FileState.OPEN;
            Mode = FileMode.WRITE;
            Position = 0;
        }

        /// <summary>
        /// Open an existing text file for writing
        /// </summary>
        /// <param name="directoryPath">
        /// The directory path where the file is located
        /// </param>
        /// <param name="fileName">
        /// The file name of the text file to be opened
        /// </param>
        public virtual void OpenForWrite(string directoryPath, string fileName)
        {
            string filePath = FileOps.CombinePath(directoryPath, fileName);
            OpenForWrite(filePath);
        }

        /// <summary>
        /// Return the next line from the text file
        /// </summary>
        /// <returns>
        /// Returns a text string, or null if the end of file is reached
        /// </returns>
        public virtual string ReadLine()
        {
            if (EndOfFile) return null;
            else
            {
                string line = _fileData[Position];
                _filePosition++;
                return line;
            }
        }

        /// <summary>
        /// Rename the file
        /// </summary>
        /// <param name="fileName">
        /// New file name
        /// </param>
        public virtual void Rename(string fileName)
        {
            // Verify that the target file name is not empty or null
            if (fileName == null || fileName.Length == 0)
            {
                string eFileName = EMPTY;
                if (fileName == null) eFileName = NULL;
                throw new FilePathException("Missing file name")
                {
                    FilePath = DirectoryPath,
                    FileName = eFileName
                };
            }
            // Verify that the target file name is valid
            if (fileName.IndexOf(Path.DirectorySeparatorChar) >= 0 ||
                !FileOps.ValidFileName(fileName))
            {
                throw new FilePathException("Target file name contains invalid characters")
                {
                    FilePath = DirectoryPath,
                    FileName = fileName
                };
            }
            // Verify that the target file name doesn't already exist
            string targetPath = Path.Combine(DirectoryPath, fileName);
            try
            {
                FileOps.FileMustNotExist(targetPath);
            }
            catch (Exception e)
            {
                throw new FileOperationException("Target file already exists", e)
                {
                    SourcePath = FilePath,
                    TargetPath = targetPath
                };
            }
            // Rename the file
            try
            {
                File.Move(FilePath, targetPath);
            }
            catch (Exception e)
            {
                throw new FileOperationException("Unable to rename file", e)
                {
                    SourcePath = FilePath,
                    TargetPath = targetPath
                };
            }
            FileName = fileName;
        }

        /// <summary>
        /// Rest the file position back to the beginning of the file if it is opened for reading.
        /// Throw an exception if the file is open for writing. Otherwise just return without doing anything.
        /// </summary>
        public void Reset()
        {
            if (State == FileState.OPEN)
            {
                if (Mode == FileMode.READ) Position = 0;
                else
                {
                    throw new FileIOException("Can't reset file opened for write access")
                    {
                        FilePath = DirectoryPath,
                        FileName = FileName
                    };
                }
            }
        }

        /// <summary>
        /// Save the contents of the text file to disk
        /// </summary>
        public virtual void Save()
        {
            if (State != FileState.OPEN)
            {
                throw new FileIOException("Can't save unopened file")
                {
                    FilePath = DirectoryPath,
                    FileName = FileName
                };
            }
            if (Mode != FileMode.WRITE)
            {
                FileIOException fie = new FileIOException("Can't save file that isn't opened for write")
                {
                    FilePath = DirectoryPath,
                    FileName = FileName
                };
            }
            try
            {
                using (StreamWriter sw = new StreamWriter(FilePath))
                {
                    foreach (string line in _fileData)
                    {
                        sw.WriteLine(line);
                    }
                }
            }
            catch (Exception e)
            {
                throw new FileIOException("Unable to write file to disk", e)
                {
                    FilePath = DirectoryPath,
                    FileName = FileName
                };
            }
        }

        /// <summary>
        /// ToString displays the characteristics of the text file
        /// </summary>
        /// <returns>
        /// Returns a string containing information about the file
        /// </returns>
        public override string ToString()
        {
            string directoryPath = DirectoryPath;
            if (directoryPath == null) directoryPath = NULL;
            string fileName = FileName;
            if (fileName == null) fileName = NULL;
            string fileState = State.ToString();
            string fileMode = Mode.ToString();
            return String.Format(
                "Plain text file =========/n" +
                "File path: {0}\n" +
                "File name: {1}\n" +
                "File state: {2}\n" +
                "File mode: {3}\n" +
                "Number of lines: {4}" +
                "Current line: {5}",
                directoryPath, fileName, fileState, fileMode, Count, Position);
        }

        /// <summary>
        /// Write a line to the text file. The line is saved in the _fileData collection until the
        /// file is closed, at which point all lines are flushed to the file.
        /// </summary>
        /// <param name="line">
        /// </param>
        public virtual void WriteLine(string line)
        {
            _fileData.Add(line);
            _filePosition++;
        }

        #endregion Public Methods

        #region Protected Methods

        /// <summary>
        /// Parse the given file path string and extract the directory path and file name.
        /// </summary>
        /// <param name="filePath">
        /// The file path string to be parsed
        /// </param>
        protected void ParseFilePath(string filePath)
        {
            // Obtain the absolute file path. Throw an exception if the path is invalid.
            string absoluteFilePath = FileOps.GetAbsoluteFilePath(filePath);
            // Parse the file path and extract the directory path
            DirectoryPath = FileOps.GetDirectoryPath(absoluteFilePath);
            // Throw an exception if the directory path is null
            if (DirectoryPath == NULL)
            {
                throw new FilePathException("Directory path can't be null")
                {
                    FileName = FileName,
                    FilePath = NULL
                };
            }
            // Parse the file path and extract the file name
            FileName = FileOps.GetFileName(absoluteFilePath);
            // Throw an exception if the file name is null
            if (FileName == NULL)
            {
                throw new FilePathException("File name can't be null")
                {
                    FilePath = DirectoryPath,
                    FileName = FileName
                };
            }
        }

        #endregion Protected Methods

        #region Private Methods

        /// <summary>
        /// Verify that the given directory path is valid. Create the directory if it doesn't already
        /// exist. Return the absolute directory path.
        /// </summary>
        /// <param name="directoryPath">
        /// The directory path to be validated
        /// </param>
        /// <returns>
        /// Returns the absolute directory path
        /// </returns>
        private string ValidateTargetPath(string directoryPath)
        {
            // The target path can't be null or zero length
            if (directoryPath == null || directoryPath.Length == 0)
            {
                string eFileName = NA;
                if (State == FileState.OPEN) eFileName = FileName;
                string eFilePath = EMPTY;
                if (directoryPath == null) eFilePath = NULL;
                throw new FilePathException("Target path is missing")
                {
                    FileName = eFileName,
                    FilePath = eFilePath
                };
            }
            // Verify that the target file path doesn't contain any invalid characters
            if (!FileOps.ValidDirectoryPath(directoryPath))
            {
                throw new FilePathException("Target file path contains invalid characters")
                {
                    FilePath = directoryPath,
                    FileName = NA
                };
            }
            // Get the full target file path
            string targetPath;
            try
            {
                targetPath = FileOps.GetAbsoluteFilePath(directoryPath);
            }
            catch (Exception e)
            {
                string eFileName = NA;
                if (State == FileState.OPEN) eFileName = FileName;
                throw new FilePathException("Invalid target file path", e)
                {
                    FileName = eFileName,
                    FilePath = directoryPath
                };
            }
            // Verify that the target directory exists. Create the directory if it doesn't exist.
            if (!Directory.Exists(targetPath))
            {
                try
                {
                    Directory.CreateDirectory(targetPath);
                }
                catch (Exception e)
                {
                    string eTargetPath = targetPath;
                    if (targetPath == null) eTargetPath = NULL;
                    throw new FileOperationException("Unable to create target directory", e)
                    {
                        TargetPath = eTargetPath,
                        SourcePath = NA
                    };
                }
            }
            return targetPath;
        }

        #endregion Private Methods
    }
}