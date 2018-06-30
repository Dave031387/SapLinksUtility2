using System.Collections.Generic;
using System.Linq;

namespace Data
{
    /// <summary>
    /// The DataRow object can be used for transferring a single data row between a data table and
    /// another object
    /// </summary>
    public class DataRow
    {
        #region Public Fields

        public enum STATUS // Status codes for data row operations
        {
            Success = 0,
            DuplicateRow,
            RowNotFound,
            EmptyTable,
            NoMoreMatches,
            KeyFieldMissing = 50,
            Initial = 99
        };

        #endregion Public Fields

        #region Private Fields

        private Dictionary<string, string> _data; // Internally a data row is represented as a Dictionary object
        private Dictionary<string, string> _keys; // Key fields whose values uniquely identify a single data row

        #endregion Private Fields

        #region Public Constructors

        /// <summary>
        /// Default constructor that creates a DataRow with an unspecified number of fields
        /// </summary>
        public DataRow()
        {
            _data = new Dictionary<string, string>();
            _keys = new Dictionary<string, string>();
            ReturnCode = STATUS.Initial;
            RowNumber = -1;
        }

        /// <summary>
        /// Constructor that creates a DataRow with the specified number of fields
        /// </summary>
        /// <param name="size">
        /// Number of fields contained in this DataRow
        /// </param>
        public DataRow(int size)
        {
            _data = new Dictionary<string, string>(size);
            _keys = new Dictionary<string, string>(size);
            ReturnCode = STATUS.Initial;
            RowNumber = -1;
        }

        #endregion Public Constructors

        #region Public Properties

        /// <summary>
        /// Property that returns a list of data field names as an array of strings
        /// </summary>
        public string[] DataFields => _data.Keys.ToArray();

        /// <summary>
        /// Property that returns the list of data field values as an array of strings
        /// </summary>
        public string[] DataValues => _data.Values.ToArray();

        /// <summary>
        /// Property that returns the number of fields in the DataRow
        /// </summary>
        public int FieldCount => _data.Count();

        /// <summary>
        /// Property that returns the number of key fields for this DataRow
        /// </summary>
        public int KeyCount => _keys.Count();

        /// <summary>
        /// Property that returns a list of key field names as an array of strings
        /// </summary>
        public string[] KeyFields => _keys.Keys.ToArray();

        /// <summary>
        /// Property that returns a list of key field values as an array of strings
        /// </summary>
        public string[] KeyValues => _keys.Values.ToArray();

        /// <summary>
        /// Public property for getting or setting the return code
        /// </summary>
        public STATUS ReturnCode { get; set; }

        public int RowNumber { get; set; }

        #endregion Public Properties

        #region Public Indexers

        /// <summary>
        /// Indexer that returns or sets the field contents for the specified field name
        /// </summary>
        /// <param name="fieldName">
        /// This is the field name we wish to retrieve
        /// </param>
        /// <returns>
        /// Returns the field contents for the specified field name
        /// </returns>
        public string this[string fieldName]
        {
            get
            {
                // Throw an exception if an invalid field name is specified
                if (!ContainsDataField(fieldName))
                {
                    string msg = $"Unable to return field value. Field name \"{fieldName}\" doesn't exist in " +
                        $"this data row.";
                    throw new DataRowException(msg);
                }
                return _data[fieldName];
            }
            set
            {
                // Throw an exception if an invalid field name is specified
                if (!ContainsDataField(fieldName))
                {
                    string msg = $"Unable to set field value. Field name \"{fieldName}\" doesn't exist in " +
                        $"this data row.";
                    throw new DataRowException(msg);
                }
                _data[fieldName] = value;
            }
        }

        #endregion Public Indexers

        #region Public Methods

        /// <summary>
        /// Add a new field to the DataRow
        /// </summary>
        /// <param name="fieldName">
        /// The key is the field name
        /// </param>
        /// <param name="fieldValue">
        /// This is the field contents
        /// </param>
        /// <param name="isKeyField">
        /// Key field indicator. Defaults to "false". Set "true" for key fields.
        /// </param>
        /// <param name="keyFieldValue">
        /// If a key field is being changed, then the original value is supplied here. Otherwise
        /// default to null.
        /// </param>
        public void AddField(string fieldName, string fieldValue, bool isKeyField = false, string keyFieldValue = null)
        {
            // Throw an exception if the DataRow already contains the specified field name
            if (ContainsDataField(fieldName))
            {
                string msg = $"Field name \"{fieldName}\" has already been added to this data row";
                throw new DataRowException(msg);
            }
            // Add the new field to the data row
            _data.Add(fieldName, fieldValue);
            // If this is a key field, then add the field name and value to the list of key fields
            if (isKeyField)
            {
                // If no key field value was supplied, then use the data field value instead
                if (keyFieldValue == null)
                {
                    _keys.Add(fieldName, fieldValue);
                }
                // If a key field value was supplied, then use it
                else
                {
                    _keys.Add(fieldName, keyFieldValue);
                }
            }
        }

        /// <summary>
        /// Method used for determining if the specified data field name exists in the DataRow object.
        /// </summary>
        /// <param name="fieldName">Name of the field whose existence we want to check</param>
        /// <returns>Returns true if the DataRow contains the specified field. Otherwise returns false.</returns>
        public bool ContainsDataField(string fieldName) => _data.ContainsKey(fieldName);

        /// <summary>
        /// Method used for determining if the specified key field name exists in the DataRow object.
        /// </summary>
        /// <param name="fieldName">Name of the field whose existence we want to check</param>
        /// <returns>Returns true if the DataRow contains the specified field. Otherwise returns false.</returns>
        public bool ContainsKeyField(string fieldName) => _keys.ContainsKey(fieldName);

        /// <summary>
        /// Method that returns the key field value for the specified key field name
        /// </summary>
        /// <param name="keyField">
        /// Name of the key field whose value we wish to return
        /// </param>
        /// <returns>
        /// Returns the key field value
        /// </returns>
        public string GetKeyValue(string keyField)
        {
            // Verify that the requested key field exists in the data row
            if (ContainsKeyField(keyField))
            {
                return _keys[keyField];
            }
            // Throw an exception if the requested key field doesn't exist in the data row
            else
            {
                string msg = $"Requested key field \"{keyField}\" doesn't exist in this data row";
                throw new DataRowException(msg);
            }
        }

        #endregion Public Methods
    }
}