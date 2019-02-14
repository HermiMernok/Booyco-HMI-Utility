using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using ExcelDataReader;
using System.IO;
using System.Data;
using System.Reflection;

namespace Booyco_HMI_Utility
{
    class ExcelFileManagement
    {
        public List<LPDEntry> LPDInfoList = new List<LPDEntry>();
        public List<LPDDataLookupEntry> LPDLookupList = new List<LPDDataLookupEntry>();
        string _LPDPath = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/Resources/Documents/LPD.xlsx";
        public void StoreLogProtocolInfo()
        {
            DataRowCollection _data = ReadExcelFile(_LPDPath, 0);
            DataRowCollection _lookup = ReadExcelFile(_LPDPath, 1);

            foreach (DataRow _row in _lookup)
            {
                try
                {


                    LPDLookupList.Add(new LPDDataLookupEntry
                    {
                        DataLink = Convert.ToString(_row.ItemArray[1]),
                        DataName = Convert.ToString(_row.ItemArray[2]),
                        NumberBytes = Convert.ToInt32(_row.ItemArray[3]),
                        Scale = Convert.ToInt32(_row.ItemArray[4]),
                        IsInt = Convert.ToBoolean(_row.ItemArray[5]),
                        Appendix = Convert.ToString(_row.ItemArray[6])

                    });


                }
                catch
                {

                }

            }

            foreach (DataRow _row in _data)
            {
                List<LPDDataLookupEntry> _tempData = new List<LPDDataLookupEntry>();

                if (_row.ItemArray.Count() == 10)
                {
                    for (int i = 2; i < 10; i++)
                    {
                        string _tempByteName = Convert.ToString(_row.ItemArray[i]);


                        if (LPDLookupList.Any(p => p.DataLink == _tempByteName))
                        {
                            _tempData.Add(LPDLookupList.FirstOrDefault(p => p.DataLink == _tempByteName));

                            //Buffer.BlockCopy(_logChuncks, 0, _logTimeStamp, 0, 6);     

                        }
                        else
                        {

                            if (_tempByteName == "0xFF" || _tempByteName == "Reserved" || _tempByteName == "")
                            {
                                i = 10;
                                _tempData.Add(new LPDDataLookupEntry
                                {
                                    DataLink = "Empty"
                                });
                            }
                        }

                    }




                    LPDInfoList.Add(new LPDEntry

                    {
                        EventID = Convert.ToUInt16(_row.ItemArray[0]),
                        EventName = Convert.ToString(_row.ItemArray[1]),
                        Data = _tempData

                    });

                }



            }

        }


        
        public List<Parameters> ParametersfromFile(string fileName)
        {
            List<Parameters> parameters = new List<Parameters>();
            List<ParameterEnum> enums = new List<ParameterEnum>();
            DataRowCollection _data = ReadExcelFile(fileName, 0);

            foreach (DataRow _row in _data)
            {
                try
                {


                    parameters.Add(new Parameters
                    {
                        Name = Convert.ToString(_row.ItemArray[1]),
                        CurrentValue = Convert.ToInt32(_row.ItemArray[2]),
                        MaximumValue = Convert.ToInt32(_row.ItemArray[3]),
                        MinimumValue = Convert.ToInt32(_row.ItemArray[4]),
                        DefaultValue = Convert.ToInt32(_row.ItemArray[5]),
                        type = Convert.ToInt16(_row.ItemArray[6]),
                        enumVal = Convert.ToInt16(_row.ItemArray[7]),
                    });

                    if(_row.ItemArray[12] != null)
                    {
                        enums.Add(new ParameterEnum
                        {
                            enumVal = Convert.ToInt16(_row.ItemArray[12]),
                            enumName = Convert.ToString(_row.ItemArray[13])
                        });
                    }
                    

                }
                catch
                {

                }

            }

            foreach (var item in parameters)
            {
                item.parameterEnums = enums.Where(t => t.enumVal == item.enumVal).ToList();
            }

            return parameters;
        }


        public 

        DataRowCollection ReadExcelFile(string _fileName, int TableNumber)
        {
            System.Data.DataSet ds;
            var extension = System.IO.Path.GetExtension(_fileName).ToLower();
            using (var stream = new FileStream(_fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                IExcelDataReader reader = null;
                if (extension == ".xls")
                {
                    reader = ExcelReaderFactory.CreateBinaryReader(stream);
                }
                else if (extension == ".xlsx")
                {
                    reader = ExcelReaderFactory.CreateOpenXmlReader(stream);
                }
                else if (extension == ".csv")
                {
                    reader = ExcelReaderFactory.CreateCsvReader(stream);
                }

                // if (reader == null)
                //     return;

                //reader.IsFirstRowAsColumnNames = firstRowNamesCheckBox.Checked;
                using (reader)
                {
                    ds = reader.AsDataSet(new ExcelDataSetConfiguration()
                    {
                        UseColumnDataType = false,
                        ConfigureDataTable = (tableReader) => new ExcelDataTableConfiguration()
                        {
                            UseHeaderRow = true
                        }
                    });
                }

                return ds.Tables[TableNumber].DefaultView.ToTable().Rows; 

            }

        }
        
    }
}
