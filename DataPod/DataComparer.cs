using System.Collections.Generic;

namespace Data
{
    /// <summary>
    /// This class can be used for performing culture-sensitive comparison between two string arrays
    /// </summary>
    public class DataComparer : IComparer<string[]>
    {
        #region Public Constructors

        /// <summary>
        /// Public constructor
        /// </summary>
        /// <param name="sortFieldIndex">
        /// Array of sort field index values
        /// </param>
        /// <param name="sortFieldOrder">
        /// Array of sort field orders
        /// </param>
        public DataComparer(int[] sortFieldIndex, DataTable.SortOrder[] sortFieldOrder)
        {
            SortFieldIndex = sortFieldIndex;
            SortFieldOrder = sortFieldOrder;
        }

        #endregion Public Constructors

        #region Public Properties

        public int[] SortFieldIndex { get; set; } // Array of sort field index values
        public DataTable.SortOrder[] SortFieldOrder { get; set; } // Array of sort field orders

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Method for comparing two string arrays
        /// </summary>
        /// <param name="x">
        /// First string array
        /// </param>
        /// <param name="y">
        /// Second string array
        /// </param>
        /// <returns>
        /// Returns -1 (x LT y), 0 (x EQ y), or 1 (x GT y)
        /// </returns>
        public int Compare(string[] x, string[] y)
        {
            if (x == null)
            {
                if (y == null)
                {
                    // If x and y are both null, then they are equal
                    return 0;
                }
                else
                {
                    // If x is null and y is not null, then y is greater
                    return -1;
                }
            }
            else
            {
                if (y == null)
                {
                    // If y is null and x is not null, then x is greater
                    return 1;
                }
                // If neither x nor y are null, we must compare individual fields in each data row
                else
                {
                    // Iterate through all of the sort fields
                    for (int i = 0; i < SortFieldIndex.Length; i++)
                    {
                        if (x[SortFieldIndex[i]] == null)
                        {
                            if (y[SortFieldIndex[i]] == null)
                            {
                                // If x and y both contain null values for this field, then the
                                // fields are equal. Go check the next field.
                                continue;
                            }
                            else
                            {
                                // If the field in x is null but the field in y is not null, then y
                                // is greater. Return -1 if the sort order is ascending for this
                                // field. Otherwise, return 1;
                                return (SortFieldOrder[i] == DataTable.SortOrder.ASCENDING) ? -1 : 1;
                            }
                        }
                        else
                        {
                            if (y[SortFieldIndex[i]] == null)
                            {
                                // If the field in x is not null but the field in y is null, then x
                                // is greater. Return 1 if the sort order is ascending for this
                                // field. Otherwise, return -1;
                                return (SortFieldOrder[i] == DataTable.SortOrder.ASCENDING) ? 1 : -1;
                            }
                            else
                            {
                                // If the field in x and in y are both not null, then do a
                                // culture-sensitive comparison of the two fields.
                                int compareResult = (x[SortFieldIndex[i]]).CompareTo(y[SortFieldIndex[i]]);
                                // If the fields are equal, go check the next field.
                                if (compareResult == 0) continue;
                                // Return the comparison result as-is if the sort order is ascending.
                                // Otherwise, negate the result before returning it.
                                return (SortFieldOrder[i] == DataTable.SortOrder.ASCENDING) ? compareResult : -compareResult;
                            }
                        }
                    }
                }
            }
            // If we reach this point, then all sort fields match between rows x and y, so return 0;
            return 0;
        }

        #endregion Public Methods
    }
}