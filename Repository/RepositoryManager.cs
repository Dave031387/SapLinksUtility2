using Data;
using FileIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    /// <summary>
    /// The RepositoryManager class manages the meta-data repository.
    /// </summary>
    public class RepositoryManager
    {
        private const string RepositoryFileSuffix = ".rep";
        private static string RepositoryDirectroyPath =
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\SAPLinks2\Repository";
        private SortedList<string, DataTable> repositoryList = new SortedList<string, DataTable>();
        private class RepositoryInfo
        {
            public string TableName { get; set; }
            public string FileName { get; set; }
            public string[] FieldNames { get; set; }
        }
        #region Repository Information
        private readonly RepositoryInfo[] repositoryInfo =
        {
            #region Data Elements
            new RepositoryInfo
            {
                TableName = "Data Elements",
                FileName = "data_elements",
                FieldNames = new string[]
                {
                    "ElementName",
                    "Type",
                    "MinLength",
                    "MaxLength",
                    "HasMinValue",
                    "MinValue",
                    "HasMaxValue",
                    "MaxValue",
                    "LetterCase",
                    "Description"
                }
            },
            #endregion
            #region Data Files
            new RepositoryInfo
            {
                TableName = "Data Files",
                FileName = "data_files",
                FieldNames = new string[]
                {
                    "FileName",
                    "PhysicalFileName",
                    "Description"
                }
            },
            #endregion
            #region Data File Elements
            new RepositoryInfo
            {
                TableName = "Data File Elements",
                FileName = "data_file_elements",
                FieldNames = new string[]
                {
                    "FileName",
                    "ElementName",
                    "FieldName",
                    "HasDefaultValue",
                    "DefaultValue",
                    "IsRequiredField",
                    "HasValueList",
                    "IsExclusionList",
                    "EnforceValueList",
                    "Description",
                    "AllowLetters",
                    "AllowDigits",
                    "AllowSpecials",
                    "AllowSpaces"
                }
            },
            #endregion
            #region Foreign Keys
            new RepositoryInfo
            {
                TableName = "Foreign Keys",
                FileName = "foreign_keys",
                FieldNames = new string[]
                {
                    "FileName",
                    "ForeignKeyName",
                    "ValidationMessage"
                }
            },
            #endregion
            #region Foreign Key Fields
            new RepositoryInfo
            {
                TableName = "Foreign Key Fields",
                FileName = "foreign_key_fields",
                FieldNames = new string[]
                {
                    "ForeignKeyName",
                    "FileName",
                    "FieldName",
                    "FKFileName",
                    "FKFieldName"
                }
            },
            #endregion
            #region Unique Keys
            new RepositoryInfo
            {
                TableName = "Unique Keys",
                FileName = "unique_keys",
                FieldNames = new string[]
                {
                    "FileName",
                    "UniqueKeyName",
                    "ValidationMessage"
                }
            },
            #endregion
            #region Unique Key Fields
            new RepositoryInfo
            {
                TableName = "Unique Key Fields",
                FileName = "unique_key_fields",
                FieldNames = new string[]
                {
                    "UniqueKeyName",
                    "FileName",
                    "FieldName"
                }
            },
            #endregion
            #region Sort Keys
            new RepositoryInfo
            {
                TableName = "Sort Keys",
                FileName = "sort_keys",
                FieldNames = new string[]
                {
                    "FileName",
                    "SortKeyName",
                    "IsDefaultSortKey",
                    "Description"
                }
            },
            #endregion
            #region Sort Key Fields
            new RepositoryInfo
            {
                TableName = "Sort Key Fields",
                FileName = "sort_key_fields",
                FieldNames = new string[]
                {
                    "SortKeyName",
                    "FileName",
                    "FieldName",
                    "SortOrder",
                    "SortSequence"
                }
            },
            #endregion
            #region Values List
            new RepositoryInfo
            {
                TableName = "Values List",
                FileName = "values_list",
                FieldNames = new string[]
                {
                    "FileName",
                    "FieldName",
                    "Value"
                }
            },
            #endregion
            #region Unique Field Groups
            new RepositoryInfo
            {
                TableName = "Unique Field Groups",
                FileName = "unique_field_groups",
                FieldNames = new string[]
                {
                    "FileName",
                    "GroupName",
                    "ValidationMessage"
                }
            },
            #endregion
            #region Unique Fields
            new RepositoryInfo
            {
                TableName = "Unique Fields",
                FileName = "unique_fields",
                FieldNames = new string[]
                {
                    "GroupName",
                    "FileName",
                    "FieldName"
                }
            },
            #endregion
            #region Maintenance Tabs
            new RepositoryInfo
            {
                TableName = "Maintenance Tabs",
                FileName = "maintenance_tabs",
                FieldNames = new string[]
                {
                    "TabName",
                    "TabOrder",
                    "Description"
                }
            },
            #endregion
            #region Maintenance Screens
            new RepositoryInfo
            {
                TableName = "Maintenance Screens",
                FileName = "maintenance_screens",
                FieldNames = new string[]
                {
                    "ScreenName",
                    "TabName",
                    "ButtonText",
                    "ButtonOrder",
                    "WindowTitle",
                    "FileName"
                }
            },
            #endregion
            #region Screen Fields
            new RepositoryInfo
            {
                TableName = "Screen Fields",
                FileName = "screen_fields",
                FieldNames = new string[]
                {
                    "ScreenName",
                    "ScreenFieldName",
                    "ScreenFieldSequence",
                    "ScreenFieldType",
                    "FileName",
                    "FieldName",
                    "DataGridColumnName",
                    "DetailFieldName",
                    "DataSourceType",
                    "DataSourceFileName",
                    "DataSourceFieldName"
                }
            }
            #endregion
        };
        #endregion
        /// <summary>
        /// Default constructor for the RepositoryManager class
        /// </summary>
        public RepositoryManager()
        {
            FileOps.CreateDirectory(RepositoryDirectroyPath);
            LoadRepository();
        }
        private void LoadRepository()
        {
            foreach (RepositoryInfo ri in repositoryInfo)
            {
                repositoryList.Add(ri.TableName, new DataTable(ri.TableName, RepositoryDirectroyPath,
                    ri.FileName + RepositoryFileSuffix, ri.FieldNames));
            }
        }
    }
}
