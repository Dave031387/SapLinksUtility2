using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Settings
{
    public class Settings
    {
        private const string ParameterName = "ParameterName";
        private const string ParameterValue = "ParameterValue";
        private string ApplicationName => "SAP Links Utility";
        private string TableName => "settings";
        private string FileName => "settings.txt";
        private string DirectoryPath => @"%APPDATA%\SAP Links Utility";
        private readonly string[] FieldNames = { ParameterName, ParameterValue };
        private Data.DataTable _settings;
        public Settings()
        {
            _settings = new Data.DataTable(TableName, DirectoryPath, FileName, FieldNames);
        }
        public bool AddParameter(string parameter, string value)
        {
            Data.DataRow dr = new Data.DataRow(2);
            dr.AddField(ParameterName, parameter, true);
            dr.AddField(ParameterValue, value);
            _settings.AddRow(dr);
            if (dr.ReturnCode == Data.DataRow.STATUS.Success)
            {
                _settings.Save();
                return true;
            }
            else return false;
        }
        public bool ModifyParameter(string parameter, string value)
        {
            Data.DataRow dr = new Data.DataRow(2);
            dr.AddField(ParameterName, parameter, true);
            dr.AddField(ParameterValue, value);
            _settings.ModifyRow(dr);
            if (dr.ReturnCode == Data.DataRow.STATUS.Success)
            {
                _settings.Save();
                return true;
            }
            else return false;
        }
        public bool DeleteParameter(string parameter)
        {
            Data.DataRow dr = new Data.DataRow(1);
            dr.AddField(ParameterName, parameter, true);
            _settings.DeleteRow(dr);
            if (dr.ReturnCode == Data.DataRow.STATUS.Success)
            {
                _settings.Save();
                return true;
            }
            else return false;
        }
        public bool RenameParameter(string oldName, string newName)
        {
            Data.DataRow dr = new Data.DataRow(2);
            return false;
        }
    }
}
