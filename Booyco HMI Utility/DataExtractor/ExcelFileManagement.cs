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
        private System.Data.DataSet ds;
        string _LPDPath = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/Resources/Documents/LPD.xlsx";

        public void StoreLogProtocolInfo()
        {
            DataRowCollection _data = ReadExcelFile(_LPDPath, 0);
            DataRowCollection _lookup = ReadExcelFile(_LPDPath, 0);
         
            foreach (DataRow _row in _data)
            {
                string _tempData = "";

                for(int i = 2; i < 10; i++)
                {
                    string _tempByteName = Convert.ToString(_row.ItemArray[i]);

                    if (_tempByteName == "0xFF" || _tempByteName == "Reserved")
                    {
                        i = 10;
                    }
                    else
                    {
                                           }
                }
                


                LPDInfoList.Add(new LPDEntry

                {
                    EventID = Convert.ToUInt16(_row.ItemArray[0]),
                    EventName = Convert.ToString(_row.ItemArray[1]),
                    Data1 = _tempData                  

                });



            }
        
        }

        DataRowCollection ReadExcelFile(string _fileName, int TableNumber)
        {
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
