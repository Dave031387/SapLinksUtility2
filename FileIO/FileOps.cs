using System;
using System.IO;
using System.Text;

namespace FileIO
{
    /// <summary>
    /// Static class containing several basic file-based methods
    /// </summary>
    public static class FileOps
    {
        #region Public Methods

        /// <summary>
        /// Combines two parts of a file path into one file path
        /// </summary>
        /// <param name="filePath1">
        /// The first portion of the file path
        /// </param>
        /// <param name="filePath2">
        /// The second portion of the file path
        /// </param>
        /// <returns>
        /// Returns the combined file path as a string object
        /// </returns>
        public static string CombinePath(string filePath1, string filePath2)
        {
            string combinedPath = null;
            try
            {
                combinedPath = Path.Combine(filePath1, filePath2);
            }
            catch (Exception e)
            {
                if (filePath1 == null) filePath1 = "NULL";
                if (filePath2 == null) filePath2 = "NULL";
                throw new FilePathException("Unable to construct full file path", e)
                {
                    FilePath = filePath1,
                    FileName = filePath2
                };
            }
            return combinedPath;
        }

        /// <summary>
        /// Create the specified directory path unless it already exists
        /// </summary>
        /// <param name="directoryPath">
        /// The directory path to be created
        /// </param>
        public static void CreateDirectory(string directoryPath)
        {
            try
            {
                Directory.CreateDirectory(directoryPath);
            }
            catch (Exception e)
            {
                if (directoryPath == null) directoryPath = "NULL";
                throw new FileOperationException("Unable to create directory", e)
                {
                    TargetPath = directoryPath,
                    SourcePath = "n/a"
                };
            }
        }

        /// <summary>
        /// Create a new file if it doesn't already exist
        /// </summary>
        /// <param name="filePath">
        /// The file path of the file to be created
        /// </param>
        public static void CreateFile(string filePath)
        {
            try
            {
                using (FileStream fs = File.Create(filePath)) { };
            }
            catch (Exception e)
            {
                if (filePath == null) filePath = "NULL";
                throw new FileOperationException("Unable to create file", e)
                {
                    TargetPath = filePath,
                    SourcePath = "n/a"
                };
            }
        }

        /// <summary>
        /// Check to verify that the specified file exists. Throw an exception if it doesn't.
        /// </summary>
        /// <param name="filePath">
        /// The path to the file to be checked
        /// </param>
        public static void FileMustExist(string filePath)
        {
            if (!File.Exists(filePath))
            {
                if (filePath == null) filePath = "NULL";
                throw new FileOpenException("File doesn't exist")
                {
                    FilePath = filePath,
                    FileName = "n/a"
                };
            }
        }

        /// <summary>
        /// Check to verify that the specified file doesn't exist. Throw an exception if it does.
        /// </summary>
        /// <param name="filePath">
        /// The path to the file to be checked
        /// </param>
        public static void FileMustNotExist(string filePath)
        {
            if (File.Exists(filePath))
            {
                throw new FileOpenException("File already exists")
                {
                    FilePath = filePath,
                    FileName = "n/a"
                };
            }
        }

        /// <summary>
        /// Returns the absolute file path for the given path string
        /// </summary>
        /// <param name="filePath">
        /// File path string
        /// </param>
        /// <returns>
        /// Returns the absolute file path as a string object
        /// </returns>
        public static string GetAbsoluteFilePath(string filePath)
        {
            string absoluteFilePath = null;
            try
            {
                absoluteFilePath = Path.GetFullPath(filePath);
            }
            catch (Exception e)
            {
                if (filePath == null) filePath = "NULL";
                throw new FilePathException("Unable to parse file path", e)
                {
                    FilePath = filePath,
                    FileName = "n/a"
                };
            }
            return absoluteFilePath;
        }

        /// <summary>
        /// Return the directory path portion of a file path string (excluding the final directory
        /// separator character)
        /// </summary>
        /// <param name="filePath">
        /// The file path (directory path plus file name)
        /// </param>
        /// <returns>
        /// Returns the directory path as a string object
        /// </returns>
        public static string GetDirectoryPath(string filePath)
        {
            if (filePath == null) return null;
            int i = FindEndOfDirectoryPath(filePath);
            if (i <= 0) return null;
            else return filePath.Substring(0, i);
        }

        /// <summary>
        /// Return the file name portion of a file path string
        /// </summary>
        /// <param name="filePath">
        /// The file path (directory path plus file name)
        /// </param>
        /// <returns>
        /// Returns the file name as a string object
        /// </returns>
        public static string GetFileName(string filePath)
        {
            if (filePath == null) return null;
            int i = FindEndOfDirectoryPath(filePath);
            if (i >= filePath.Length - 1) return null;
            else if (i < 0) return filePath;
            else return filePath.Substring(i + 1);
        }

        /// <summary>
        /// Truncates an existing text file to remove all lines
        /// </summary>
        /// <param name="filePath">
        /// File path of the file to be truncated
        /// </param>
        public static void TruncateFile(string filePath)
        {
            try
            {
                Byte[] text = new UTF8Encoding(true).GetBytes("");
                File.WriteAllBytes(filePath, text);
            }
            catch (Exception e)
            {
                if (filePath == null) filePath = "NULL";
                throw new FileOpenException("Unable to truncate file", e)
                {
                    FilePath = filePath,
                    FileName = "n/a"
                };
            }
        }

        /// <summary>
        /// Verify that the given directory path doesn't contain any invalid characters
        /// </summary>
        /// <param name="directoryPath">
        /// The directory path to be verified
        /// </param>
        /// <returns>
        /// Returns "true" if the directory path is valid. Otherwise returns "false".
        /// </returns>
        public static bool ValidDirectoryPath(string directoryPath)
        {
            foreach (char x in Path.GetInvalidPathChars())
            {
                if (directoryPath.IndexOf(x) >= 0) return false;
            }
            return true;
        }

        /// <summary>
        /// Verify that the given file name doesn't contain any invalid characters
        /// </summary>
        /// <param name="fileName">
        /// The file name to be verified
        /// </param>
        /// <returns>
        /// Returns "true" if the file name is valid. Otherwise returns "false".
        /// </returns>
        public static bool ValidFileName(string fileName)
        {
            foreach (char x in Path.GetInvalidFileNameChars())
            {
                if (fileName.IndexOf(x) >= 0) return false;
            }
            return true;
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Parse the given file path string and locate the last directory separator character
        /// </summary>
        /// <param name="filePath">
        /// File path string to be parsed (directory path plus file name)
        /// </param>
        /// <returns>
        /// Returns an integer representing the location of the last directory separator character
        /// </returns>
        private static int FindEndOfDirectoryPath(string filePath)
        {
            if (filePath == null) return -1;
            int start = 0;
            int index = 0;
            while (start < filePath.Length && index > -1)
            {
                index = filePath.IndexOf(Path.DirectorySeparatorChar, start);
                if (index < 0) break;
                start = index + 1;
            }
            // The only way the start variable can be equal to zero at this point is if there weren't
            // any directory separate characters found in the path string. In this case, return zero.
            // Otherwise, return the zero-based index of the last directory separator character.
            if (start == 0) return -1;
            else return start - 1;
        }

        #endregion Private Methods
    }
}