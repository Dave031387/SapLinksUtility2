using FileIO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Data
{
    public class DataTable : IEnumerator<string[]>
    {
        #region Private Fields

        private const string BeginDataList = ControlFieldPrefix + "BDL"; // Control - beginning of data list
        private const string BeginFieldList = ControlFieldPrefix + "BFL"; // Control - beginning of field list
        private const string ControlFieldPrefix = "##"; // String that appears at the start of every control field
        private const string DataRowPrefix = "->"; // String that appears at the start of every data row in the file
        private const char DelimChar = '\t'; // Delimiter character
        private const string Delimiter = "\t"; // Delimiter string
        private const string EndDataList = ControlFieldPrefix + "EDL"; // Control - end of data list
        private const string EndFieldList = ControlFieldPrefix + "EFL"; // Control - end of field list
        private const int MaxFields = 100; // Maximum number of fields that can appear in a single data table
        private const string TableNameHeader = ControlFieldPrefix + "TNH"; // Control - table name header
        private List<string[]> _dataRows; // List of data rows in this data table
        private List<string> _fieldNames; // List of field names contained in each data row
        private EncodedTextFile _file; // Physical file where the data table is stored
        private int _position = -1; // Position used for the implementing the IEnumerator interface
        private int[] _sortFieldIndex = null; // Index value of sort field names
        private SortOrder[] _sortFieldOrder = null; // Sort order of sort field names
        private string[] _sortFields = null; // Current list of sort field names

        #endregion Private Fields

        #region Public Constructors

        /// <summary>
        /// Public constructor for the DataTable
        /// </summary>
        /// <param name="tableName">
        /// The identifying name of this DataTable
        /// </param>
        /// <param name="directoryPath">
        /// The directory path to the associated text file
        /// </param>
        /// <param name="fileName">
        /// The text file that is associated with this DataTable
        /// </param>
        /// <param name="fieldNames">
        /// The list of file names for each data row in the table
        /// </param>
        public DataTable(string tableName, string directoryPath, string fileName, string[] fieldNames)
        {
            Name = tableName;
            _file = new EncodedTextFile();
            // Create the associated text file for the DataTable if it doesn't already exist. The
            // created file will be empty
            _file.CreateIfFileDoesNotExist(directoryPath, fileName);
            FullFilePath = _file.FilePath; // Save the full file path (including the file name)
            FileName = _file.FileName; // Save the file name
            _fieldNames = new List<string>();
            _dataRows = new List<string[]>();
            // Open the associated text file for reading
            _file.OpenForRead(FullFilePath);
            if (FileLineCount == 0)
            {
                // If the associated text file is empty, then initialize the file by writing the list
                // of field names to the file.
                Initialize(fieldNames);
            }
            else
            {
                // If the associated text file contains data, then load that data into the DataTable
                Load();
                // Verify that the list of field names returned from the text file matches the
                // expected list of fields names passed to this constructor.
                ValidateTableFieldNameList(fieldNames);
            }
        }

        #endregion Public Constructors

        #region Public Enums

        public enum Operation { ADD, MODIFY, DELETE, REPLACE, READ } // Possible operations on a data row
        public enum SortOrder { ASCENDING, DESCENDING } // Possible sort order

        #endregion Public Enums

        #region Public Properties

        /// <summary>
        /// IEnumerator implementation of Current method
        /// </summary>
        public string[] Current
        {
            get
            {
                try
                {
                    return _dataRows[_position];
                }
                catch (IndexOutOfRangeException)
                {
                    throw new InvalidOperationException();
                }
            }
        }

        /// <summary>
        /// Property that returns the number of field names for this data table
        /// </summary>
        public int FieldCount
        {
            get
            {
                return _fieldNames.Count;
            }
        }

        /// <summary>
        /// Property that returns the list of field names in the data table
        /// </summary>
        public string[] FieldNames
        {
            get
            {
                return _fieldNames.ToArray();
            }
        }

        /// <summary>
        /// Property that returns the directory path for the text file
        /// </summary>
        public string FileDirectoryPath
        {
            get
            {
                return _file.DirectoryPath;
            }
        }

        /// <summary>
        /// Property that returns the text file name associated with this data table
        /// </summary>
        public string FileName { get; private set; }

        /// <summary>
        /// Property that returns the full file path of the associated text file for this DataTable
        /// </summary>
        public string FullFilePath { get; private set; }

        /// <summary>
        /// Property that returns the name of this DataTable
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Property that returns the number of data rows for this data table
        /// </summary>
        public int RowCount
        {
            get
            {
                return _dataRows.Count;
            }
        }

        /// <summary>
        /// IEnumerator implementation of base Current method
        /// </summary>
        object IEnumerator.Current
        {
            get
            {
                try
                {
                    return _dataRows[_position];
                }
                catch (IndexOutOfRangeException)
                {
                    throw new InvalidOperationException();
                }
            }
        }

        #endregion Public Properties

        #region Private Properties

        /// <summary>
        /// Private property that returns "true" if we have reached the end of the text file
        /// </summary>
        private bool EndOfFile
        {
            get
            {
                return _file.EndOfFile;
            }
        }

        /// <summary>
        /// Private property that returns the number of lines in the text file
        /// </summary>
        private int FileLineCount
        {
            get
            {
                return _file.Count;
            }
        }

        /// <summary>
        /// Private property that returns the mode of the text file (READ, WRITE, or INITIAL)
        /// </summary>
        private FileMode FileMode
        {
            get
            {
                return _file.Mode;
            }
        }

        /// <summary>
        /// Private property that returns the current position within the text file
        /// </summary>
        private int FilePosition
        {
            get
            {
                return _file.Position;
            }
        }

        /// <summary>
        /// Private property that returns the state of the text file (INITIAL, OPEN, or CLOSED)
        /// </summary>
        private FileState FileState
        {
            get
            {
                return _file.State;
            }
        }

        #endregion Private Properties

        #region Public Methods

        /// <summary>
        /// Add a new data row to the table. Returns "true" if successful. Returns "false" if there
        /// is already a row in the table with a matching unique key.
        /// </summary>
        /// <param name="dataRow">
        /// A DataRow object representing the data row to be added to the table
        /// </param>
        public void AddRow(DataRow dataRow)
        {
            // Get the list of field names from the data row
            string[] dataFieldNames = ValidateDataRowFieldNames(dataRow, Operation.ADD);
            // If key fields are specified, verify that a row having the same key field values
            // doesn't already exist in the data table.
            if (dataRow.KeyCount > 0)
            {
                string[] keyFields = dataRow.KeyFields; // List of key field names
                string[] keyValues = dataRow.KeyValues; // Values of the key fields
                int[] tableKeyFieldIndex = new int[keyFields.Length]; // Array of key field index values
                try
                {
                    tableKeyFieldIndex = GetFieldIndexValues(keyFields);
                }
                // Throw an exception if we are unable to get the list of key field index values
                catch (Exception e)
                {
                    string msg = $"Table \"{Name}\" - Unable to add data row to table\nKey fields for row:";
                    for (int i = 0; i < keyFields.Length; i++)
                    {
                        msg += $"\n[{i + 1}] {keyFields[i]} = {keyValues[i]}";
                    }
                    throw new DataTableMaintenanceException(msg, e)
                    {
                        TableName = Name,
                        Operation = Operation.ADD,
                        DataRow = dataRow
                    };
                }
                // Return "DuplicateRow" if a data row already exists in the table having the same key field values
                if (RowWithKeyExists(tableKeyFieldIndex, keyValues))
                {
                    dataRow.ReturnCode = DataRow.STATUS.DuplicateRow;
                    dataRow.RowNumber = -1;
                    return;
                }
            }
            int[] tableDataFieldIndex = new int[dataFieldNames.Length]; // Array of field name index values
            // Get a list of index values corresponding to the data field names in the DataRow object.
            try
            {
                tableDataFieldIndex = GetFieldIndexValues(dataFieldNames);
            }
            // Throw an exception if we are unable to get the list of field name index values
            catch (Exception e)
            {
                string msg = $"Table \"{Name}\" - Unable to add data row to table\nData fields for row:";
                for (int i = 0; i < dataFieldNames.Length; i++)
                {
                    msg += $"\n[{i + 1}] {dataFieldNames[i]} = {dataRow[dataFieldNames[i]]}";
                }
                throw new DataTableMaintenanceException(msg, e)
                {
                    TableName = Name,
                    Operation = Operation.ADD,
                    DataRow = dataRow
                };
            }
            string[] dataFieldValues = new string[dataFieldNames.Length]; // Array of data field values
            // Copy the values from the data row to the correct positions in the table data row
            for (int i = 0; i < dataFieldNames.Length; i++)
            {
                dataFieldValues[tableDataFieldIndex[i]] = dataRow[dataFieldNames[i]];
            }
            // Insert the new data row into the table
            try
            {
                dataRow.RowNumber = InsertRow(dataFieldValues);
            }
            catch (Exception e)
            {
                string msg = $"Table \"{Name}\" - Insertion failure when adding new data row to table";
                throw new DataTableMaintenanceException(msg, e)
                {
                    TableName = Name,
                    Operation = Operation.ADD,
                    DataRow = dataRow
                };
            }
            // Return "Success" to let the caller know the row was successfully added
            dataRow.ReturnCode = DataRow.STATUS.Success;
        }

        /// <summary>
        /// Delete the specified data row from the table. Only the key fields are used to identify
        /// the row to be deleted from the table. The values in the data fields are ignored.
        /// </summary>
        /// <param name="dataRow">
        /// DataRow object representing the data row to be deleted from the table
        /// </param>
        public void DeleteRow(DataRow dataRow)
        {
            // Get the list of data field names from the DataRow object
            string[] dataFieldNames = ValidateDataRowFieldNames(dataRow, Operation.DELETE);
            // We require at least one key field in order to uniquely identify the data table row to
            // be deleted. Return "KeyFieldMissing" if there aren't any key fields.
            if (dataRow.KeyCount < 1)
            {
                dataRow.ReturnCode = DataRow.STATUS.KeyFieldMissing;
                dataRow.RowNumber = -1;
                return;
            }
            string[] keyFieldNames = dataRow.KeyFields; // Array of key field names
            string[] keyFieldValues = dataRow.KeyValues; // Array of key field values to match on
            int[] keyFieldIndex = new int[keyFieldNames.Length]; // Array of data row field index values
            // Get the list of table field index values corresponding to the list of key field names
            try
            {
                keyFieldIndex = GetFieldIndexValues(keyFieldNames);
            }
            // Throw an exception if for any reason we aren't able to get the list of table field
            // index values
            catch (Exception e)
            {
                string msg = $"Table \"{Name}\" - Unable to delete data row from table\nKey fields for row:";
                for (int i = 0; i < keyFieldNames.Length; i++)
                {
                    msg += $"\n[{i + 1}] {keyFieldNames[i]} = {keyFieldValues[i]}";
                }
                throw new DataTableMaintenanceException(msg, e)
                {
                    TableName = Name,
                    Operation = Operation.DELETE,
                    DataRow = dataRow
                };
            }
            try
            {
                // Get the index value of the table row that matches the key field values.
                int rowIndex = GetRowIndex(keyFieldIndex, keyFieldValues);
                // If the index value is less than zero, then no matching row was found. Return "RowNotFound".
                if (rowIndex < 0)
                {
                    dataRow.ReturnCode = DataRow.STATUS.RowNotFound;
                    dataRow.RowNumber = -1;
                    return;
                }
                // Remove the matching data row from the table
                _dataRows.RemoveAt(rowIndex);
                dataRow.RowNumber = rowIndex;
            }
            // Throw an exception if we are unable to delete the data row for any reason.
            catch (Exception e)
            {
                string msg = $"Table \"{Name}\" - Unable to delete data row from table\nKey fields for row:";
                for (int i = 0; i < keyFieldNames.Length; i++)
                {
                    msg += $"\n[{i + 1}] {keyFieldNames[i]} = {keyFieldValues[i]}";
                }
                throw new DataTableMaintenanceException(msg, e)
                {
                    TableName = Name,
                    Operation = Operation.DELETE,
                    DataRow = dataRow
                };
            }
            // If we get to here then the data row was successfully removed from the table. Return "Success".
            dataRow.ReturnCode = DataRow.STATUS.Success;
        }

        /// <summary>
        /// IEnumerator implementation of Dispose method (do nothing)
        /// </summary>
        public void Dispose()
        {
        }

        /// <summary>
        /// Get the first row in the data table whose key field values match the values in the DataRow
        /// object that is passed into the routine.
        /// </summary>
        /// <param name="dataRow">
        /// DataRow object that specifies the key field values to search on
        /// </param>
        public void GetFirstMatchingRow(DataRow dataRow)
        {
            // Return status "EmptyTable" if the table is empty
            if (RowCount < 1)
            {
                dataRow.ReturnCode = DataRow.STATUS.EmptyTable;
                dataRow.RowNumber = -1;
                return;
            }
            // Get the list of data field names from the DataRow object
            string[] dataFieldNames = ValidateDataRowFieldNames(dataRow, Operation.READ);
            // Retrieve the data field values from the data row
            string[] dataFieldValues = dataRow.DataValues;
            // We require at least one key field in order to identify the data table rows to be retrieved.
            // Return "KeyFieldMissing" if there aren't any key fields.
            if (dataRow.KeyCount < 1)
            {
                dataRow.ReturnCode = DataRow.STATUS.KeyFieldMissing;
                dataRow.RowNumber = -1;
                return;
            }
            // Retrieve the key field names and values from the Data Row object
            string[] keyFieldNames = dataRow.KeyFields;
            string[] keyFieldValues = dataRow.KeyValues;
            int[] keyFieldIndex = new int[keyFieldNames.Length]; // Array of data row field index values
            // Get the list of table field index values corresponding to the list of key field names
            try
            {
                keyFieldIndex = GetFieldIndexValues(keyFieldNames);
            }
            // Throw an exception if for any reason we aren't able to get the list of table field
            // index values
            catch (Exception e)
            {
                string msg = $"Table \"{Name}\" - Unable to read data row from table\nKey fields for row:";
                for (int i = 0; i < keyFieldNames.Length; i++)
                {
                    msg += $"\n[{i + 1}] {keyFieldNames[i]} = {keyFieldValues[i]}";
                }
                throw new DataTableMaintenanceException(msg, e)
                {
                    TableName = Name,
                    Operation = Operation.READ,
                    DataRow = dataRow
                };
            }
            try
            {
                // Get the index value of the table row that matches the key field values.
                int rowIndex = GetRowIndex(keyFieldIndex, keyFieldValues);
                // If the index value is less than zero, then no matching row was found. Return "RowNotFound".
                if (rowIndex < 0)
                {
                    dataRow.ReturnCode = DataRow.STATUS.RowNotFound;
                    dataRow.RowNumber = -1;
                    return;
                }
                // Save the row number in the DataRow object so that it can be used as the starting point of
                // the GetNextRow method.
                dataRow.RowNumber = rowIndex;
                // Get the list of table row field index values for the data row fields
                int[] dataFieldIndex = GetFieldIndexValues(dataFieldNames);
                // Get the data values for the matching data row
                string[] dataValues = _dataRows[rowIndex];
                // Copy the data values to the corresponding fields in the DataRow object
                for (int i= 0; i < dataValues.Length; i++)
                {
                    // Only copy the value if the DataRow object contains the field
                    if (dataRow.ContainsDataField(_fieldNames[i]))
                    {
                        // Only copy the value if the field isn't a key field
                        if (!dataRow.ContainsKeyField(_fieldNames[i]))
                        {
                            dataRow[_fieldNames[i]] = dataValues[i];
                        }
                    }
                }
                // Set the status to "Success" and return
                dataRow.ReturnCode = DataRow.STATUS.Success;
                return;
            }
            catch (Exception e)
            {
                string msg = $"Table \"{Name}\" - Unable to read data row in table\nKey fields for row:";
                for (int i = 0; i < keyFieldNames.Length; i++)
                {
                    msg += $"\n[{i + 1}] {keyFieldNames[i]} = {keyFieldValues[i]}";
                }
                throw new DataTableMaintenanceException(msg, e)
                {
                    TableName = Name,
                    Operation = Operation.READ,
                    DataRow = dataRow
                };
            }
        }

        /// <summary>
        /// Get the next row in the data table whose key field values match the values specified in
        /// the DataRow object that is passed into the routine.
        /// </summary>
        /// <param name="dataRow">
        /// DataRow object that specifies the key field values to search for and the table row number to
        /// start searching at.
        /// </param>
        public void GetNextMatchingRow(DataRow dataRow)
        {
            // Return status "EmptyTable" if the table is empty
            if (RowCount < 1)
            {
                dataRow.ReturnCode = DataRow.STATUS.EmptyTable;
                dataRow.RowNumber = -1;
                return;
            }
            // Return status "NoMoreMatches" if the data row index is at the end of the table
            // or if it is less than zero
            if (dataRow.RowNumber >= RowCount - 1 || dataRow.RowNumber < 0)
            {
                dataRow.ReturnCode = DataRow.STATUS.NoMoreMatches;
                dataRow.RowNumber = -1;
                return;
            }
            // Get the list of data field names from the DataRow object
            string[] dataFieldNames = ValidateDataRowFieldNames(dataRow, Operation.READ);
            // Retrieve the data field values from the data row
            string[] dataFieldValues = dataRow.DataValues;
            // We require at least one key field in order to identify the data table rows to be retrieved.
            // Return "KeyFieldMissing" if there aren't any key fields.
            if (dataRow.KeyCount < 1)
            {
                dataRow.ReturnCode = DataRow.STATUS.KeyFieldMissing;
                dataRow.RowNumber = -1;
                return;
            }
            // Retrieve the key field names and values from the Data Row object
            string[] keyFieldNames = dataRow.KeyFields;
            string[] keyFieldValues = dataRow.KeyValues;
            int[] keyFieldIndex = new int[keyFieldNames.Length]; // Array of data row field index values
            // Get the list of table field index values corresponding to the list of key field names
            try
            {
                keyFieldIndex = GetFieldIndexValues(keyFieldNames);
            }
            // Throw an exception if for any reason we aren't able to get the list of table field
            // index values
            catch (Exception e)
            {
                string msg = $"Table \"{Name}\" - Unable to read data row from table\nKey fields for row:";
                for (int i = 0; i < keyFieldNames.Length; i++)
                {
                    msg += $"\n[{i + 1}] {keyFieldNames[i]} = {keyFieldValues[i]}";
                }
                throw new DataTableMaintenanceException(msg, e)
                {
                    TableName = Name,
                    Operation = Operation.READ,
                    DataRow = dataRow
                };
            }
            try
            {
                // Get the index value of the table row that matches the key field values.
                int rowIndex = GetRowIndex(keyFieldIndex, keyFieldValues, dataRow.RowNumber + 1);
                // If the index value is less than zero, then no matching row was found. Return "NoMoreMatches".
                if (rowIndex < 0)
                {
                    dataRow.ReturnCode = DataRow.STATUS.NoMoreMatches;
                    dataRow.RowNumber = -1;
                    return;
                }
                // Save the row number in the DataRow object so that it can be used as the starting point of
                // the GetNextRow method.
                dataRow.RowNumber = rowIndex;
                // Get the list of table row field index values for the data row fields
                int[] dataFieldIndex = GetFieldIndexValues(dataFieldNames);
                // Get the data values for the matching data row
                string[] dataValues = _dataRows[rowIndex];
                // Copy the data values to the corresponding fields in the DataRow object
                for (int i = 0; i < dataValues.Length; i++)
                {
                    // Only copy the value if the DataRow object contains the field
                    if (dataRow.ContainsDataField(_fieldNames[i]))
                    {
                        // Only copy the value if the field isn't a key field
                        if (!dataRow.ContainsKeyField(_fieldNames[i]))
                        {
                            dataRow[_fieldNames[i]] = dataValues[i];
                        }
                    }
                }
                // Set the status to "Success" and return
                dataRow.ReturnCode = DataRow.STATUS.Success;
                return;
            }
            catch (Exception e)
            {
                string msg = $"Table \"{Name}\" - Unable to read data row in table\nKey fields for row:";
                for (int i = 0; i < keyFieldNames.Length; i++)
                {
                    msg += $"\n[{i + 1}] {keyFieldNames[i]} = {keyFieldValues[i]}";
                }
                throw new DataTableMaintenanceException(msg, e)
                {
                    TableName = Name,
                    Operation = Operation.READ,
                    DataRow = dataRow
                };
            }
        }

        /// <summary>
        /// Modify an existing row in the data table. The key fields of the Data Row object are used
        /// to locate the data table row whose values are being modified. The existing data table row
        /// is removed and the new row with updated values is inserted in its place.
        /// </summary>
        /// <param name="dataRow">
        /// DataRow object representing the data row to be modified in the table
        /// </param>
        public void ModifyRow(DataRow dataRow)
        {
            // Get the list of data field names from the DataRow object
            string[] dataFieldNames = ValidateDataRowFieldNames(dataRow, Operation.MODIFY);
            // Retrieve the data field values from the data row
            string[] dataFieldValues = dataRow.DataValues;
            // We require at least one key field in order to uniquely identify the data table row to
            // be modified. Return "KeyFieldMissing" if there aren't any key fields.
            if (dataRow.KeyCount < 1)
            {
                dataRow.ReturnCode = DataRow.STATUS.KeyFieldMissing;
                dataRow.RowNumber = -1;
                return;
            }
            // Retrieve the key field names and values from the Data Row object
            string[] keyFieldNames = dataRow.KeyFields;
            string[] keyFieldValues = dataRow.KeyValues;
            int[] keyFieldIndex = new int[keyFieldNames.Length]; // Array of data row field index values
            // Get the list of table field index values corresponding to the list of key field names
            try
            {
                keyFieldIndex = GetFieldIndexValues(keyFieldNames);
            }
            // Throw an exception if for any reason we aren't able to get the list of table field
            // index values
            catch (Exception e)
            {
                string msg = $"Table \"{Name}\" - Unable to modify data row in table\nKey fields for row:";
                for (int i = 0; i < keyFieldNames.Length; i++)
                {
                    msg += $"\n[{i + 1}] {keyFieldNames[i]} = {keyFieldValues[i]}";
                }
                throw new DataTableMaintenanceException(msg, e)
                {
                    TableName = Name,
                    Operation = Operation.MODIFY,
                    DataRow = dataRow
                };
            }
            try
            {
                // Get the index value of the table row that matches the key field values.
                int rowIndex = GetRowIndex(keyFieldIndex, keyFieldValues);
                // If the index value is less than zero, then no matching row was found. Return "RowNotFound".
                if (rowIndex < 0)
                {
                    dataRow.ReturnCode = DataRow.STATUS.RowNotFound;
                    dataRow.RowNumber = -1;
                    return;
                }
                // Get the list of table row field index values for the data row fields
                int[] dataFieldIndex = GetFieldIndexValues(dataFieldNames);
                // Arrange the new data field values in the order required by the table
                string[] newDataTableRow = new string[dataFieldValues.Length];
                for (int i = 0; i < dataFieldValues.Length; i++)
                {
                    newDataTableRow[i] = dataFieldValues[dataFieldIndex[i]];
                }
                // Remove the old data row from the table
                _dataRows.RemoveAt(rowIndex);
                // Insert the new data row into the table
                try
                {
                    dataRow.RowNumber = InsertRow(newDataTableRow);
                }
                catch (Exception e)
                {
                    string msg = $"Table \"{Name}\" - Insertion failure when modifying data row in table";
                    throw new DataTableMaintenanceException(msg, e)
                    {
                        TableName = Name,
                        Operation = Operation.MODIFY,
                        DataRow = dataRow
                    };
                }
            }
            // Throw an exception if we are unable to modify the data row for any reason.
            catch (Exception e)
            {
                string msg = $"Table \"{Name}\" - Unable to modify data row in table\nKey fields for row:";
                for (int i = 0; i < keyFieldNames.Length; i++)
                {
                    msg += $"\n[{i + 1}] {keyFieldNames[i]} = {keyFieldValues[i]}";
                }
                throw new DataTableMaintenanceException(msg, e)
                {
                    TableName = Name,
                    Operation = Operation.MODIFY,
                    DataRow = dataRow
                };
            }
            // If we get to here then the data row was successfully modified in the table. Return "Success".
            dataRow.ReturnCode = DataRow.STATUS.Success;
        }

        /// <summary>
        /// IEnumerator implementation of MoveNext method. Increment the position within the data
        /// table by one if we have not yet reached the end of the data table.
        /// </summary>
        /// <returns>
        /// Return "false" if we have reached the end of the data table. Otherwise, return "true"
        /// </returns>
        public bool MoveNext()
        {
            if (_position >= _dataRows.Count) return false;
            _position++;
            return (_position < _dataRows.Count);
        }

        /// <summary>
        /// Replace the values of one or more fields with new values in every matching data row
        /// </summary>
        /// <param name="dataRow">
        /// DataRow object containing the old values and new values
        /// </param>
        public void ReplaceRows(DataRow dataRow)
        {
            // Initialize the "replaced" indicator to "false"
            bool replaced = false;
            // Get the list of data field names from the DataRow object
            string[] dataFieldNames = ValidateDataRowFieldNames(dataRow, Operation.REPLACE);
            // Retrieve the data field values from the data row
            string[] dataFieldValues = dataRow.DataValues;
            // We require at least one key field to search for matching rows. Return "KeyFieldMissing"
            // if no key fields are given,
            if (dataRow.KeyCount < 1)
            {
                dataRow.ReturnCode = DataRow.STATUS.KeyFieldMissing;
                dataRow.RowNumber = -1;
                return;
            }
            int[] dataFieldIndex = new int[dataFieldNames.Length];
            // Get the list of table field index values corresponding to the list of data field names
            try
            {
                dataFieldIndex = GetFieldIndexValues(dataFieldNames);
            }
            // Throw an exception if for any reason we aren't able to get the list of table field
            // index values
            catch (Exception e)
            {
                string msg = $"Table \"{Name}\" - Unable to replace data rows in table\nData fields for row:";
                for (int i = 0; i < dataFieldNames.Length; i++)
                {
                    msg += $"\n[{i + 1}] {dataFieldNames[i]} = {dataFieldValues[i]}";
                }
                throw new DataTableMaintenanceException(msg, e)
                {
                    TableName = Name,
                    Operation = Operation.REPLACE,
                    DataRow = dataRow
                };
            }
            // Retrieve the key field names and values from the Data Row object
            string[] keyFieldNames = dataRow.KeyFields;
            string[] keyFieldValues = dataRow.KeyValues;
            int[] keyFieldIndex = new int[keyFieldNames.Length]; // Array of data row field index values
            // Get the list of table field index values corresponding to the list of key field names
            try
            {
                keyFieldIndex = GetFieldIndexValues(keyFieldNames);
            }
            // Throw an exception if for any reason we aren't able to get the list of table field
            // index values
            catch (Exception e)
            {
                string msg = $"Table \"{Name}\" - Unable to replace data rows in table\nKey fields for row:";
                for (int i = 0; i < keyFieldNames.Length; i++)
                {
                    msg += $"\n[{i + 1}] {keyFieldNames[i]} = {keyFieldValues[i]}";
                }
                throw new DataTableMaintenanceException(msg, e)
                {
                    TableName = Name,
                    Operation = Operation.REPLACE,
                    DataRow = dataRow
                };
            }
            // Find all matching rows and replace the data values with the new data values
            try
            {
                // Iterate through all data rows in the data table
                for (int i = 0; i < _dataRows.Count; i++)
                {
                    // Reference the current data row
                    string[] currentRow = _dataRows[i];
                    // Start out assuming the data row is a match
                    bool match = true;
                    // Iterate through all of the key fields to see if they match the data row
                    for (int j = 0; j < keyFieldIndex.Length; j++)
                    {
                        // If the key field doesn't match the data row, then break out of the loop
                        // and go get the next data row
                        if (currentRow[keyFieldIndex[j]] != keyFieldValues[j])
                        {
                            match = false;
                            break;
                        }
                    }
                    // If all of the key fields match the current data row, then set the data fields
                    // to their new values
                    if (match)
                    {
                        // Remove the row from the data table
                        _dataRows.RemoveAt(i);
                        // Modify the fields that are being replaced
                        for (int k = 0; k < dataFieldIndex.Length; k++)
                        {
                            currentRow[dataFieldIndex[k]] = dataFieldValues[k];
                        }
                        // Insert the modified row back into the table
                        try
                        {
                            dataRow.RowNumber = InsertRow(currentRow);
                        }
                        catch (Exception e)
                        {
                            string msg = $"Table \"{Name}\" - Insertion failure when replacing data rows in table";
                            throw new DataTableMaintenanceException(msg, e)
                            {
                                TableName = Name,
                                Operation = Operation.REPLACE,
                                DataRow = dataRow
                            };
                        }
                        // Set the flag indicating we have modified at least one data row
                        replaced = true;
                        // If the new position of the modified data row comes after its current position, then
                        // we need to decrement the index by 1
                        if (i < dataRow.RowNumber) i--;
                    }
                }
            }
            // Throw an exception if we are unable to replace the data row for any reason.
            catch (Exception e)
            {
                string msg = $"Table \"{Name}\" - Unable to replace data rows in table\nKey fields for row:";
                for (int i = 0; i < keyFieldNames.Length; i++)
                {
                    msg += $"\n[{i + 1}] {keyFieldNames[i]} = {keyFieldValues[i]}";
                }
                throw new DataTableMaintenanceException(msg, e)
                {
                    TableName = Name,
                    Operation = Operation.REPLACE,
                    DataRow = dataRow
                };
            }
            // Return"Success" if at least one data row has been updated
            if (replaced) dataRow.ReturnCode = DataRow.STATUS.Success;
            // Return "RowNotFound" if no data rows were updated
            else
            {
                dataRow.ReturnCode = DataRow.STATUS.RowNotFound;
                dataRow.RowNumber = -1;
            }
        }

        /// <summary>
        /// IEnumerator implementation of the Reset method
        /// </summary>
        public void Reset() => _position = -1;

        /// <summary>
        /// Save the table contents to the text file
        /// </summary>
        public void Save()
        {
            // Open the text file for writing
            PrepareFileForSave();
            // Write the table name header to the text file
            WriteTableNameHeader();
            // Write the list of fields to the text file
            WriteFieldList();
            // Write the data rows to the text file
            WriteDataRows();
            // Close the file to save the changes
            _file.Close();
        }

        /// <summary>
        /// Sort the data rows in the data table by the specified sort fields and sort order
        /// </summary>
        /// <param name="sortFields">
        /// List of field names to sort on
        /// </param>
        /// <param name="sortOrder">
        /// Sort order for each sort field (ascending or descending)
        /// </param>
        public void Sort(string[] sortFields, SortOrder[] sortOrder)
        {
            // Throw an exception if there isn't at least one sort field
            if (sortFields == null || sortFields.Length < 1)
            {
                string msg = $"Table \"{Name}\" - There must be at least one sort field.";
                throw new DataTableSortException
                {
                    TableName = Name,
                    SortFields = null
                };
            }
            // Throw an exception if there are more sort fields than there are fields in the data table
            if (sortFields.Length > FieldCount)
            {
                string msg = $"Table \"{Name}\" - Number of sort fields must not exceed the number of fields " +
                    "in the data table.\nSort fields:";
                for (int i = 0; i < sortFields.Length; i++)
                {
                    msg += $"\n[{i + 1}] {sortFields[i]}";
                }
                throw new DataTableSortException
                {
                    TableName = Name,
                    SortFields = sortFields
                };
            }
            // Throw an exception if the size of the sort order array doesn't match the size of the
            // sort field array
            if (sortOrder == null || sortOrder.Length != sortFields.Length)
            {
                string msg = $"Table \"{Name}\" - Number of sort fields and number of sort orders must match\n" +
                    $"Number of sort fields: {sortFields.Length}\n" +
                    $"Number of sort orders: {sortOrder.Length}";
                throw new DataTableSortException
                {
                    TableName = Name,
                    SortFields = sortFields
                };
            }
            // Throw an exception if any of the sort fields is not a valid field for this data table
            for (int i = 0; i < sortFields.Length; i++)
            {
                if (!_fieldNames.Contains(sortFields[i]))
                {
                    string msg = $"Table \"{Name}\" - Sort field \"{sortFields[i]}\" doesn't exist in this data table";
                    throw new DataTableSortException
                    {
                        TableName = Name,
                        SortFields = sortFields
                    };
                }
            }
            _sortFields = sortFields;
            _sortFieldOrder = sortOrder;
            // Get the index values for the sort field names
            _sortFieldIndex = GetFieldIndexValues(sortFields);
            // Sort the data rows according to the specified sort order
            try
            {
                DataComparer dc = new DataComparer(_sortFieldIndex, _sortFieldOrder);
                _dataRows.Sort(dc);
            }
            catch (Exception e)
            {
                string msg = $"Table \"{Name}\" - Unexpected exception while sorting the table";
                throw new DataTableSortException(msg, e)
                {
                    TableName = Name,
                    SortFields = _sortFields
                };
            }
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Read the Begin Field List control line from the text file and retrieve the expected
        /// number of field names
        /// </summary>
        /// <returns>
        /// Returns an integer value equal to the expected number of fields
        /// </returns>
        private int GetFieldCount()
        {
            // Read the first line from the text file
            string fileData = _file.ReadLine();
            // Throw an exception if the first line is null (this shouldn't happen)
            if (fileData == null)
            {
                string msg = $"Table \"{Name}\" - First line of file \"{FileName}\" is null";
                throw new DataTableLoadException(msg)
                {
                    TableName = Name,
                    FileName = FileName,
                    DirectoryPath = FileDirectoryPath
                };
            }
            // Split the line into individual fields that are delimited by the delimiter character
            string[] beginFieldListLine = fileData.Split(DelimChar);
            // The first line in the file should be the Begin Field List control. Throw an exception
            // if it is not.
            if (beginFieldListLine[0] != BeginFieldList)
            {
                string msg = $"Table \"{Name}\" - Missing the Begin Field List control in file \"{FileName}\"" +
                    $"\nThe first line in the file contains this ->\n{fileData}";
                throw new DataTableLoadException(msg)
                {
                    TableName = Name,
                    FileName = FileName,
                    DirectoryPath = FileDirectoryPath
                };
            }
            // The Begin Field List control should be followed by a value which equals the number of
            // field names
            if (beginFieldListLine.Length != 2)
            {
                string msg = $"Table \"{Name}\" - Missing the field count value of the Begin Field List control " +
                    $"in file \"{FileName}\"\nThe first line in the file contains this ->\n{fileData}";
                throw new DataTableLoadException(msg)
                {
                    TableName = Name,
                    FileName = FileName,
                    DirectoryPath = FileDirectoryPath
                };
            }
            int totalFields = 0; // Total number of fields we expect to find in the text file
            // Convert the field count value from a string value to an integer value
            try
            {
                totalFields = Convert.ToInt32(beginFieldListLine[1]);
            }
            // Throw an exception if we are unable to convert the string value to an integer
            catch (Exception e)
            {
                string msg = $"Table \"{Name}\" - Unable to convert the field count string value to an integer " +
                    $"in file \"{FileName}\"\nThe first line in the file contains this ->\n{fileData}";
                throw new DataTableLoadException(msg, e)
                {
                    TableName = Name,
                    FileName = FileName,
                    DirectoryPath = FileDirectoryPath
                };
            }
            return totalFields;
        }

        /// <summary>
        /// Return an array of index values corresponding to the array of field names passed to this
        /// method. Throw an exception if a field name is passed that doesn't exist in this data table.
        /// </summary>
        /// <param name="fieldNames">
        /// An array of field names whose index values we want to look up
        /// </param>
        /// <returns>
        /// Returns an array of index values corresponding to an array of field names
        /// </returns>
        private int[] GetFieldIndexValues(string[] fieldNames)
        {
            int[] fieldIndexList = new int[fieldNames.Length];
            int i = 0;
            // Process each of the field names in the list
            foreach (string lookupFieldName in fieldNames)
            {
                // Verify that the field name exists in the list of table field names
                if (_fieldNames.Contains(lookupFieldName))
                {
                    // Get the index value that corresponds to the given field name
                    fieldIndexList[i] = _fieldNames.FindIndex(tableFieldName => lookupFieldName == tableFieldName);
                    i++;
                }
                // Throw an exception if the requested field name doesn't exist in the data table
                else
                {
                    string msg = $"Requested field \"{lookupFieldName}\" doesn't exist in data table \"{Name}\"";
                    throw new DataRowException(msg);
                }
            }
            // Return the array of key field index values
            return fieldIndexList;
        }

        /// <summary>
        /// Retrieve the expected number of data rows from the Begin Data List control line in the
        /// text file
        /// </summary>
        /// <returns>
        /// Returns an integer value equal to the number of data rows in the text file
        /// </returns>
        private int GetRowCount()
        {
            // Throw an exception if we reach the end of the file before reading any data rows
            if (EndOfFile)
            {
                string msg = $"Table \"{Name}\" - End of file \"{FileName}\" reached before any data rows " +
                        $"were read";
                throw new DataTableLoadException(msg)
                {
                    TableName = Name,
                    FileName = FileName,
                    DirectoryPath = FileDirectoryPath
                };
            }
            // Read the next line from the file. Should be the Begin Data Rows control line.
            string fileData = _file.ReadLine();
            // Throw an exception if the line is null (this shouldn't happen)
            if (fileData == null)
            {
                string msg = $"Table \"{Name}\" - Null line found in file \"{FileName}\" when " +
                    $"Begin Data List was expected";
                throw new DataTableLoadException(msg)
                {
                    TableName = Name,
                    FileName = FileName,
                    DirectoryPath = FileDirectoryPath
                };
            }
            // Split the line into individual fields that are delimited by the delimiter character
            string[] begindDataListLine = fileData.Split(DelimChar);
            // The next line in the file should be the Begin Data List control. Throw an exception if
            // it is not.
            if (begindDataListLine[0] != BeginDataList)
            {
                string msg = $"Table \"{Name}\" - Missing the Begin Data List control in file \"{FileName}\"" +
                    $"\nLine {FilePosition} in the file contains this ->\n{fileData}";
                throw new DataTableLoadException(msg)
                {
                    TableName = Name,
                    FileName = FileName,
                    DirectoryPath = FileDirectoryPath
                };
            }
            // The Begin Data List control should be followed by a value which equals the number of
            // data rows
            if (begindDataListLine.Length != 2)
            {
                string msg = $"Table \"{Name}\" - Invalid number of parameters in the Begin Field List control " +
                    $"in file \"{FileName}\"\nLine {FilePosition} in the file contains this ->\n{fileData}";
                throw new DataTableLoadException(msg)
                {
                    TableName = Name,
                    FileName = FileName,
                    DirectoryPath = FileDirectoryPath
                };
            }
            int totalRows = 0; // Total number of data rows we expect to find in the text file
            // Convert the row count value from a string value to an integer value
            try
            {
                totalRows = Convert.ToInt32(begindDataListLine[1]);
            }
            // Throw an exception if we are unable to convert the string value to an integer
            catch (Exception e)
            {
                string msg = $"Table \"{Name}\" - Unable to convert the data row count string value to an integer " +
                    $"in file \"{FileName}\"\nLine {FilePosition} in the file contains this ->\n{fileData}";
                throw new DataTableLoadException(msg, e)
                {
                    TableName = Name,
                    FileName = FileName,
                    DirectoryPath = FileDirectoryPath
                };
            }
            return totalRows;
        }

        /// <summary>
        /// Get the index value of the data row that matches the given key field values. Return -1 if
        /// no match is found.
        /// </summary>
        /// <param name="fieldIndex">
        /// Array of data table field index values
        /// </param>
        /// <param name="fieldValue">
        /// Array of field values
        /// </param>
        /// <param name="startIndex">
        /// The row index from which the search begins (defaults to zero)
        /// </param>
        /// <returns>
        /// Returns the index value of the matching row in the data table. Returns -1 if no match.
        /// </returns>
        private int GetRowIndex(int[] fieldIndex, string[] fieldValue, int startIndex = 0)
        {
            // Return -1 if the data table doesn't contain any data rows
            if (RowCount < 1) return -1;
            // Throw an exception if the starting row index is out of range
            if (startIndex < 0 || startIndex > RowCount - 1)
            {
                string msg = $"GetRowIndex: Start index value {startIndex} is out of range.\n" +
                    $"Must be between 0 and {RowCount - 1}.";
                throw new ArgumentOutOfRangeException("startIndex", msg);
            }
            // Iterate through the rows in the data table
            for (int i = startIndex; i < RowCount; i++)
            {
                // Get the row located at index value "i"
                string[] dataRow = _dataRows[i];
                // Start out assuming the row matches
                bool match = true;
                // Iterate through all of the fields
                for (int j = 0; j < fieldIndex.Length; j++)
                {
                    // If any field value doesn't match the data row, set "match" to "false" and go
                    // check the next row.
                    if (dataRow[fieldIndex[j]] != fieldValue[j])
                    {
                        match = false;
                        break;
                    }
                }
                // If we get to here, then the current data row is a match. Return the index value.
                if (match) return i;
            }
            // If we get to here, then there were no matching rows in the data table. Return -1 to
            // indicate that.
            return -1;
        }

        /// <summary>
        /// Initialize a new file by writing the list of field names to the file
        /// </summary>
        /// <param name="fieldNames">
        /// </param>
        private void Initialize(string[] fieldNames)
        {
            // Add the list of field names to the field name list
            foreach (string fieldName in fieldNames)
            {
                _fieldNames.Add(fieldName);
            }
            // Save the table to the text file
            Save();
        }

        /// <summary>
        /// Insert a new row into the data table. If sort fields have been set, then the row is
        /// inserted in the appropriate position in the table. Otherwise, the row is added to the end
        /// of the table.
        /// </summary>
        /// <param name="dataRow">
        /// </param>
        /// <returns>
        /// Returns the row index of the inserted data row
        /// </returns>
        private int InsertRow(string[] dataRow)
        {
            // If no sort fields have been set, then insert the row at the end of the data table
            if (_sortFieldIndex == null)
            {
                _dataRows.Add(dataRow);
                return RowCount - 1;
            }
            // Insert the row into the table at the position that preserves the current sort order
            else
            {
                // Instantiate a new IComparer object. Pass in the sort field index and sort order.
                DataComparer dc = new DataComparer(_sortFieldIndex, _sortFieldOrder);
                // Get the index position where the new data row should be inserted
                int index = _dataRows.BinarySearch(dataRow, dc);
                // If the index is positive, then there exists another data row with the same sort
                // key fields. We will insert the new row in front of the matching row. If the index
                // is negative, then there aren't any matching rows. Take the complement of the index
                // value to get the index of the data row that has the next highest collating
                // sequence. The new row will be inserted ahead of this row.
                if (index < 0) index = ~index;
                // Insert the new row at the appropriate position to maintain the sort order
                _dataRows.Insert(index, dataRow);
                return index;
            }
        }

        /// <summary>
        /// Load the contents of the associated text file into the DataTable
        /// </summary>
        private void Load()
        {
            // Prepare the text file for loading. Close it if it is open for writing. Open it for reading.
            PrepareFileForLoad();
            // Clear the field name list and data row list
            _fieldNames.Clear();
            _dataRows.Clear();
            // Throw an exception if the text file is empty. At a minimum it should contain a list of
            // field names.
            if (FileLineCount == 0)
            {
                string msg = $"File \"{FileName}\" of table \"{Name}\" is empty";
                throw new DataTableLoadException(msg)
                {
                    TableName = Name,
                    FileName = FileName,
                    DirectoryPath = FileDirectoryPath
                };
            }
            // Load the table name from the text file
            LoadTableName();
            // Load the field names from the text file
            LoadFieldNames();
            // Load the data rows from the text file
            LoadDataRows();
            // We should be at the end of the text file now. Throw an exception if there are more
            // lines in the file.
            if (!EndOfFile)
            {
                string fileData = _file.ReadLine();
                string msg = $"Table \"{Name}\" - End of file \"{FileName}\" not reached after the End Data " +
                    $"List control. Line {FilePosition} in the file contains this ->\n{fileData}";
                throw new DataTableLoadException(msg)
                {
                    TableName = Name,
                    FileName = FileName,
                    DirectoryPath = FileDirectoryPath
                };
            }
        }

        /// <summary>
        /// Load all of the data rows from the text file into the data table
        /// </summary>
        private void LoadDataRows()
        {
            int totalRows = GetRowCount(); // Get the total number of expected data rows
            // Read all of the data rows from the text file and add the row data to the data list
            while (!LoadNextDataRow(totalRows)) { }
            // Verify that we have read the expected number of data rows. Throw an exception if we haven't.
            if (RowCount != totalRows)
            {
                string rowDiff = (RowCount < totalRows) ? "Fewer" : "More";
                string msg = $"Table \"{Name}\" - {rowDiff} rows were found than were expected.\n " +
                    $"Found {RowCount} of {totalRows} data rows.";
                throw new DataTableLoadException(msg)
                {
                    TableName = Name,
                    FileName = FileName,
                    DirectoryPath = FileDirectoryPath
                };
            }
        }

        /// <summary>
        /// Load all of the field names from the text file into the data table
        /// </summary>
        private void LoadFieldNames()
        {
            int totalFields = GetFieldCount(); // Get the expected number of field names from the text file
            // Read all of the field name lines from the text file and add the field names to the
            // field list
            while (!LoadNextFieldName(totalFields)) { }
            // We expect the number of field names read from the text file to match the number that
            // was specified on the Begin Field List control line
            if (FieldCount == totalFields)
            {
                // Throw an exception if there are an invalid number of fields. The number of fields
                // must be in the range from 1 to MaxFields.
                if (FieldCount <= 0 || FieldCount > MaxFields)
                {
                    string msg = $"Table \"{Name}\" - Invalid number of fields in file \"{FileName}\".\n" +
                        $"Found {FieldCount} of {totalFields} fields. The number found should be between " +
                        $"1 and {MaxFields}.";
                    throw new DataTableLoadException(msg)
                    {
                        TableName = Name,
                        FileName = FileName,
                        DirectoryPath = FileDirectoryPath
                    };
                }
            }
            // Throw an exception if the number of fields found doesn't match the number specified on
            // the Begin Field List control line.
            else
            {
                string fieldMsg = (FieldCount < totalFields) ? "Too few" : "Too many";
                string msg = $"Table \"{Name}\" - {fieldMsg} fields found in file \"{FileName}\".\n" +
                        $"Found {FieldCount} of {totalFields} fields.";
                throw new DataTableLoadException(msg)
                {
                    TableName = Name,
                    FileName = FileName,
                    DirectoryPath = FileDirectoryPath
                };
            }
        }

        /// <summary>
        /// Load the next data row from the text file. Return "true" if there are no more rows to
        /// load. Otherwise, return "false".
        /// </summary>
        /// <param name="totalRows">
        /// The expected number of data rows in the text file
        /// </param>
        /// <returns>
        /// Returns "true" if there are no more rows to load
        /// </returns>
        private bool LoadNextDataRow(int totalRows)
        {
            // Throw an exception if we reach the end of the file too soon
            if (EndOfFile)
            {
                string msg = $"Table \"{Name}\" - Reached the end of file \"{FileName}\" before finding " +
                    $"the End Data List control.\nFound {RowCount} of {totalRows} data rows.";
                throw new DataTableLoadException(msg)
                {
                    TableName = Name,
                    FileName = FileName,
                    DirectoryPath = FileDirectoryPath
                };
            }
            string fileData = _file.ReadLine();
            // Throw an exception if a null line is returned from the file (this should never happen)
            if (fileData == null)
            {
                string msg = $"Table \"{Name}\" - Null line returned from file \"{FileName}\" when " +
                    $"reading the data rows.\nFound {RowCount} of {totalRows} data rows.";
                throw new DataTableLoadException(msg)
                {
                    TableName = Name,
                    FileName = FileName,
                    DirectoryPath = FileDirectoryPath
                };
            }
            // If we have reached the End Data List control, then we have processed all of the data
            // rows. Return "true" to let the caller know we are done.
            if (fileData == EndDataList) return true;
            // There shouldn't be any other control lines in the file other than the End Data List
            // control. Throw an exception if we come across any other control field.
            else if (fileData.Substring(0, ControlFieldPrefix.Length) == ControlFieldPrefix)
            {
                string msg = $"Table \"{Name}\" - Unexpected control field in file \"{FileName}\".\n" +
                    $"Line {FilePosition} from file ->\n{fileData}\nFound {RowCount} of {totalRows} data rows.";
                throw new DataTableLoadException(msg)
                {
                    TableName = Name,
                    FileName = FileName,
                    DirectoryPath = FileDirectoryPath
                };
            }
            // The first two characters on the line should be the Data Row Prefix. Discard the prefix
            // and separate the rest of the line into individual fields. Keep a count of total data
            // rows processed thus far.
            else if (fileData.Substring(0, DataRowPrefix.Length) == DataRowPrefix)
            {
                string[] dataFields = (fileData.Substring(DataRowPrefix.Length)).Split(DelimChar);
                // The number of fields should match the number of field names. Throw an exception if
                // it doesn't.
                if (dataFields.Length != FieldCount)
                {
                    string msg = $"Table \"{Name}\" - Data row {RowCount + 1} in file \"{FileName}\" " +
                        $"contains an incorrect number of fields.\nExpected {FieldCount} fields, but found " +
                        $"{dataFields.Length}.\nLine {FilePosition} from file contains this ->\n{fileData}";
                    throw new DataTableLoadException(msg)
                    {
                        TableName = Name,
                        FileName = FileName,
                        DirectoryPath = FileDirectoryPath
                    };
                }
                _dataRows.Add(dataFields);
            }
            // If the first two characters on the line aren't a Control Field Prefix or a Data Row
            // Prefix, then the file has been corrupted. Throw an exception.
            else
            {
                string msg = $"Table \"{Name}\" - Data Row Prefix missing in file \"{FileName}\".\n" +
                    $"Line {FilePosition} from file ->\n{fileData}\nFound {RowCount} of {totalRows} data rows.";
                throw new DataTableLoadException(msg)
                {
                    TableName = Name,
                    FileName = FileName,
                    DirectoryPath = FileDirectoryPath
                };
            }
            return false; // Return "false" to let the caller know we're not done yet
        }

        /// <summary>
        /// Load the next field name from the text file. Return "true" if we have reached the End
        /// Field List control. Otherwise, return "false".
        /// </summary>
        /// <param name="totalFields">
        /// The total number of field names expected to be found in the text file
        /// </param>
        /// <returns>
        /// Returns "true" when we have reached the end of the list of field names
        /// </returns>
        private bool LoadNextFieldName(int totalFields)
        {
            // Throw an exception if we reach the end of the file too soon
            if (EndOfFile)
            {
                string msg = $"Table \"{Name}\" - Reached the end of file \"{FileName}\" before finding " +
                    $"the End Field List control.\nFound {FieldCount} of {totalFields} fields.";
                throw new DataTableLoadException(msg)
                {
                    TableName = Name,
                    FileName = FileName,
                    DirectoryPath = FileDirectoryPath
                };
            }
            string fileData = _file.ReadLine();
            // Throw an exception if a null line is returned from the file (this should never happen)
            if (fileData == null)
            {
                string msg = $"Table \"{Name}\" - Null line returned from file \"{FileName}\" when " +
                    $"reading the list of fields.\nFound {FieldCount} of {totalFields} fields.";
                throw new DataTableLoadException(msg)
                {
                    TableName = Name,
                    FileName = FileName,
                    DirectoryPath = FileDirectoryPath
                };
            }
            // Throw an exception if there are any field delimiter characters on the current line.
            // The field name lines shouldn't contain any delimiter characters.
            if (fileData.Contains(Delimiter))
            {
                string msg = $"Table \"{Name}\" - Delimiter found in field list of file \"{FileName}\".\n" +
                    $"Line {FilePosition} from file ->\n{fileData}\nFound {FieldCount} of {totalFields} fields.";
                throw new DataTableLoadException(msg)
                {
                    TableName = Name,
                    FileName = FileName,
                    DirectoryPath = FileDirectoryPath
                };
            }
            // If we have reached the End Field List control, then we have processed all of the field
            // names. Return "true" to let the caller know we are done.
            if (fileData == EndFieldList) return true;
            // The next control field in the file should be the End Field List control which we just
            // checked for above. Throw an exception if we come across any other control field.
            else if (fileData.Substring(0, ControlFieldPrefix.Length) == ControlFieldPrefix)
            {
                string msg = $"Table \"{Name}\" - Unexpected control field in file \"{FileName}\".\n" +
                    $"Line {FilePosition} from file ->\n{fileData}\nFound {FieldCount} of {totalFields} fields.";
                throw new DataTableLoadException(msg)
                {
                    TableName = Name,
                    FileName = FileName,
                    DirectoryPath = FileDirectoryPath
                };
            }
            // The first two characters on the line should be the Data Row Prefix. Discard the prefix
            // and add the field name to the field name list. Keep a tally of how many fields we've
            // processed thus far.
            else if (fileData.Substring(0, DataRowPrefix.Length) == DataRowPrefix)
            {
                _fieldNames.Add(fileData.Substring(DataRowPrefix.Length));
            }
            // If the first two characters on the line aren't a Control Field Prefix or a Data Row
            // Prefix, then the file has been corrupted. Throw an exception.
            else
            {
                string msg = $"Table \"{Name}\" - Data Row Prefix missing in file \"{FileName}\".\n" +
                    $"Line {FilePosition} from file ->\n{fileData}\nFound {FieldCount} of {totalFields} fields.";
                throw new DataTableLoadException(msg)
                {
                    TableName = Name,
                    FileName = FileName,
                    DirectoryPath = FileDirectoryPath
                };
            }
            return false;
        }

        /// <summary>
        /// Read the table name header from the text file. Throw an exception if the name found in
        /// the file doesn't match the name of this table.
        /// </summary>
        private void LoadTableName()
        {
            string fileData = _file.ReadLine();
            // Throw an exception if the line is null (this shouldn't happen)
            if (fileData == null)
            {
                string msg = $"Table \"{Name}\" - Null line found in file \"{FileName}\" when " +
                    $"Table Name Header was expected";
                throw new DataTableLoadException(msg)
                {
                    TableName = Name,
                    FileName = FileName,
                    DirectoryPath = FileDirectoryPath
                };
            }
            // Split the line into individual fields that are delimited by the delimiter character
            string[] tableNameHeader = fileData.Split(DelimChar);
            // Throw an exception if the line doesn't contain the Table Name Header control
            if (tableNameHeader[0] != TableNameHeader)
            {
                string msg = $"Table \"{Name}\" - Missing the Table Name Header control in file \"{FileName}\"" +
                    $"\nLine {FilePosition} in the file contains this ->\n{fileData}";
                throw new DataTableLoadException(msg)
                {
                    TableName = Name,
                    FileName = FileName,
                    DirectoryPath = FileDirectoryPath
                };
            }
            // Throw an exception if there isn't exactly one parameter following the Table Name
            // Header control
            if (tableNameHeader.Length != 2)
            {
                string msg = $"Table \"{Name}\" - Invalid number of parameters in the Table Name Header control " +
                    $"in file \"{FileName}\"\nLine {FilePosition} in the file contains this ->\n{fileData}";
                throw new DataTableLoadException(msg)
                {
                    TableName = Name,
                    FileName = FileName,
                    DirectoryPath = FileDirectoryPath
                };
            }
            string tableName = tableNameHeader[1];
            // Throw an exception if the table name returned from the text file doesn't match the
            // name of this table
            if (tableName != Name)
            {
                string msg = $"Incorrect table name found in file \"{FileName}\".\nExpected \"{Name}\" " +
                    $"but found \"{tableName}\".";
                throw new DataTableLoadException(msg)
                {
                    TableName = Name,
                    FileName = FileName,
                    DirectoryPath = FileDirectoryPath
                };
            }
            // Throw an exception if we have reached the end of the text file
            if (EndOfFile)
            {
                string msg = $"Table \"{Name}\" - End of file \"{FileName}\" reached immediately after the " +
                    "Table Name Header line";
                throw new DataTableLoadException(msg)
                {
                    TableName = Name,
                    FileName = FileName,
                    DirectoryPath = FileDirectoryPath
                };
            }
        }

        /// <summary>
        /// Prepare the text file for loading into the data table. If the file is open, close it. If
        /// the file is in the initial state or closed, open it for reading. If the file is already
        /// opened for reading, reset the file pointer back to the beginning of the file.
        /// </summary>
        private void PrepareFileForLoad()
        {
            if (FileState == FileState.OPEN)
            {
                // Close the text file if it is currently open for writing
                if (FileMode == FileMode.WRITE)
                {
                    _file.Close();
                }
                // Reset the position to the start of the file if it is open for reading
                else
                {
                    try
                    {
                        _file.Reset();
                    }
                    // Throw an exception if we are unable to reset the position pointer for the file
                    catch (Exception e)
                    {
                        string msg = $"Unable to reset file \"{FileName}\" for data table \"{Name}\"";
                        throw new DataTableLoadException(msg, e)
                        {
                            TableName = Name,
                            FileName = FileName,
                            DirectoryPath = FileDirectoryPath
                        };
                    }
                }
            }
            // Open the text file for reading if it is currently closed or in an initial state
            if (FileState == FileState.INITIAL || FileState == FileState.CLOSED)
            {
                try
                {
                    _file.OpenForRead(FullFilePath);
                }
                // Throw an exception if we can't open the file for reading
                catch (Exception e)
                {
                    string msg = $"Unable to open file \"{FileName}\" to load table \"{Name}\"";
                    throw new DataTableLoadException(msg, e)
                    {
                        TableName = Name,
                        FileName = FileName,
                        DirectoryPath = FileDirectoryPath
                    };
                }
            }
            // Throw an exception if the file isn't opened for reading.
            // Note: This exception shouldn't occur.
            if (FileState != FileState.OPEN || FileMode != FileMode.READ)
            {
                string msg = $"Expected file state {FileState.OPEN.ToString()} and " +
                    $"file mode {FileMode.READ.ToString()}\nbut found file state {FileState.ToString()} and " +
                    $"file mode {FileMode.ToString()}";
                throw new UnexpectedFileStateException(msg)
                {
                    ExpectedState = FileState.OPEN.ToString(),
                    ExpectedMode = FileMode.READ.ToString(),
                    ActualState = FileState.ToString(),
                    ActualMode = FileMode.ToString()
                };
            }
        }

        /// <summary>
        /// Open the text file for writing. Throw an exception if the file is already open for
        /// writing. Close the file if it is open for reading and then open it for writing.
        /// </summary>
        private void PrepareFileForSave()
        {
            // Close the file if it is open
            if (FileState == FileState.OPEN)
            {
                // Throw an exception if the file is currently open for writing
                if (FileMode == FileMode.WRITE)
                {
                    // Close the file first to save any changes
                    _file.Close();
                    string msg = $"Table \"{Name}\" - Unable to save table to file \"{FileName}\" because the file " +
                        $"was already open for writing.";
                    throw new DataTableSaveException(msg)
                    {
                        TableName = Name,
                        FileName = FileName,
                        DirectoryPath = FileDirectoryPath
                    };
                }
                // Close the file
                _file.Close();
            }
            // Open the file for writing
            try
            {
                _file.OpenForWrite(FullFilePath);
            }
            catch (Exception e)
            {
                // Throw an exception if we are unable to open the text file for writing
                string msg = $"Table \"{Name}\" - Unable to open file \"{FileName}\" for saving the table.";
                throw new DataTableSaveException(msg, e)
                {
                    TableName = Name,
                    FileName = FileName,
                    DirectoryPath = FileDirectoryPath
                };
            }
        }

        /// <summary>
        /// Scan the data rows in the table to see if any of them contain a specified set of key
        /// field values. Return "true" if any data row matches all of the key field values.
        /// Otherwise return "false".
        /// </summary>
        /// <param name="fieldIndex">
        /// Array contains a list of index values corresponding to the key fields
        /// </param>
        /// <param name="fieldValue">
        /// Array contains the key field values to search for
        /// </param>
        /// <returns>
        /// Returns "true" if any data row matches all of the key field values
        /// </returns>
        private bool RowWithKeyExists(int[] fieldIndex, string[] fieldValue)
        {
            // Search all of the data rows in the table
            foreach (string[] dataRow in _dataRows)
            {
                // Start out by assuming the current data row is a match
                bool match = true;
                // Check all of the key fields
                for (int i = 0; i < fieldIndex.Length; i++)
                {
                    // If the key field value in the current data row doesn't match the value being
                    // searched on, set the match indicator to "false" and go check the next data row.
                    if (dataRow[fieldIndex[i]] != fieldValue[i])
                    {
                        match = false;
                        break;
                    }
                }
                // If we reach this point, then all of the key field values in the data row match.
                // Return "true".
                if (match) return true;
            }
            // If we reach this point then none of the data rows were a match. Return "false".
            return false;
        }

        /// <summary>
        /// Validate that the list of field names in a data row exactly matches the list of field
        /// names for this table. Throw an exception if there is any discrepancy. Otherwise, return
        /// the list of field names to the caller.
        /// </summary>
        /// <param name="dataRow">
        /// The DataRow object whose field names are to be validated
        /// </param>
        /// <returns>
        /// Returns an array of field names
        /// </returns>
        private string[] ValidateDataRowFieldNames(DataRow dataRow, Operation action)
        {
            string[] dataRowFieldNames = dataRow.DataFields;
            if (dataRowFieldNames.Length == 0)
            {
                throw new DataRowException("Data row contains no fields");
            }
            // Verify that each of the fields in the data row are also fields in the table
            foreach (string fieldName in dataRowFieldNames)
            {
                if (_fieldNames.Contains(fieldName)) continue;
                // Throw an exception if the data row field name doesn't exist in the table
                else
                {
                    string msg = $"Data row field name \"{fieldName}\" doesn't exist in table \"{Name}\"";
                    throw new DataRowException(msg);
                }
            }
            // For ADD and MODIFY actions we need to verify that all data table fields are accounted
            // for in the data row
            if (action == Operation.ADD || action == Operation.MODIFY)
            {
                // Verify that each of the table field names also appear in the data row
                foreach (string fieldName in _fieldNames)
                {
                    if (dataRowFieldNames.Contains(fieldName)) continue;
                    // Throw an exception if the data row doesn't contain all of the fields that are
                    // in the table
                    else
                    {
                        string msg = $"Table \"{Name}\" field \"{fieldName}\" is missing in the data row";
                        throw new DataRowException(msg);
                    }
                }
            }
            return dataRowFieldNames;
        }

        /// <summary>
        /// Verify that the list of field names that were read in from the text file exactly matches
        /// the expected list of field names that was passed to the constructor.
        /// </summary>
        /// <param name="fieldNames">
        /// </param>
        private void ValidateTableFieldNameList(string[] fieldNames)
        {
            // Check to see that each of the field names that were read in from the text file are in
            // the list of expected field names
            foreach (string fieldName in _fieldNames)
            {
                if (fieldNames.Contains(fieldName)) continue;
                // Throw an exception if the text file contains any field names that aren't in the
                // list of expected field names
                else
                {
                    string msg = $"Table \"{Name}\" - Unexpected field name \"{fieldName}\" found in file " +
                        $"\"{FileName}\"";
                    throw new DataTableLoadException(msg)
                    {
                        TableName = Name,
                        FileName = FileName,
                        DirectoryPath = FileDirectoryPath
                    };
                }
            }
            // Verify that each of the names in the expected list of field names was found in the
            // text file
            foreach (string fieldName in fieldNames)
            {
                if (_fieldNames.Contains(fieldName)) continue;
                // Throw an exception if we didn't find all of the expected field names in the text file
                else
                {
                    string msg = $"Table \"{Name}\" - Expected field name \"{fieldName}\" is missing in file " +
                        $"\"{FileName}\"";
                    throw new DataTableLoadException(msg)
                    {
                        TableName = Name,
                        FileName = FileName,
                        DirectoryPath = FileDirectoryPath
                    };
                }
            }
        }

        /// <summary>
        /// Write all of the data rows to the text file
        /// </summary>
        private void WriteDataRows()
        {
            // Write the Begin Data List line to the text file
            string beginDataList = BeginDataList + Delimiter + Convert.ToString(RowCount);
            _file.WriteLine(beginDataList);
            // Write the data rows to the text file
            if (RowCount > 0)
            {
                foreach (string[] dataRow in _dataRows)
                {
                    string line = dataRow[0];
                    if (dataRow.Length > 1)
                    {
                        for (int i = 1; i < dataRow.Length; i++)
                        {
                            line += Delimiter + dataRow[i];
                        }
                    }
                    _file.WriteLine(DataRowPrefix + line);
                }
            }
            // Write the End Data List line to the text file
            _file.WriteLine(EndDataList);
        }

        /// <summary>
        /// Write the list of field names to the text file
        /// </summary>
        private void WriteFieldList()
        {
            // Write the Begin Field List line to the text file
            string beginFieldList = BeginFieldList + Delimiter + Convert.ToString(FieldCount);
            _file.WriteLine(beginFieldList);
            // Write the list of field names to the text file
            if (FieldCount > 0)
            {
                foreach (string fieldName in _fieldNames)
                {
                    _file.WriteLine(DataRowPrefix + fieldName);
                }
            }
            // Write the End Field List line to the text file
            _file.WriteLine(EndFieldList);
        }

        /// <summary>
        /// Write the Table Name Header to the text file
        /// </summary>
        private void WriteTableNameHeader()
        {
            _file.WriteLine(TableNameHeader + Delimiter + Name);
        }

        #endregion Private Methods
    }
}