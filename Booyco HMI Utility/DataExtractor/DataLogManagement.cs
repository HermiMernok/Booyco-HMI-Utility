using Booyco_HMI_Utility.CustomObservableCollection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Booyco_HMI_Utility
{
    class DataLogManagement    
    {
        public Action<int> ReportProgressDelegate { get; set; }

        private void ReportProgress(int percent)
        {
            if (ReportProgressDelegate != null)
            {
                ReportProgressDelegate(percent);
            }
        }

        #region Variables
        public RangeObservableCollection<LogEntry> TempList = new RangeObservableCollection<LogEntry>();
        public UInt32 TotalLogEntries = 0;
        private const byte TOTAL_ENTRY_BYTES = 16;
        public UInt32 LogErrorDateTimeCounter = 0;
        private ExcelFileManagement ExcelFilemanager = new ExcelFileManagement();
        #endregion

        public void ReadFile(string Log_Filename)

        {
            ExcelFilemanager.StoreLogProtocolInfo();
            byte[] _logBytes = { 0 };
            byte[] _logChuncks = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            byte[] _logTimeStamp = { 0, 0, 0, 0, 0, 0 };
            byte[] _logData = { 0, 0, 0, 0, 0, 0, 0, 0 };
            UInt16 _logID = 0;
            string _logGroup = "";
            string _logInfoRaw = System.IO.File.ReadAllText(Log_Filename, Encoding.Default);
            int _fileLength = (int)(new FileInfo(Log_Filename).Length);
            BinaryReader _breader = new BinaryReader(File.OpenRead(Log_Filename));

            _logBytes = _breader.ReadBytes(_fileLength);

            int PercentageComplete = 0;
            UInt32 UID = 0;
            UInt16 VID = 0;
            string Event_Information = " ";

            TotalLogEntries = (UInt32)((float)_fileLength / (float)TOTAL_ENTRY_BYTES);
            for (int i = 0; i < (int)TotalLogEntries - 1; i++)
            {
                for (int j = 0; j < TOTAL_ENTRY_BYTES; j++)
                {
                    _logChuncks[j] = _logBytes[i * TOTAL_ENTRY_BYTES + j];
                }

                // ---- Copy from the 16 byte logChunk the DateTimeStamp ----
                Buffer.BlockCopy(_logChuncks, 0, _logTimeStamp, 0, 6);
                // ---- Copy from the 16 byte logChunk the Data ----
                Buffer.BlockCopy(_logChuncks, 8, _logData, 0, 8);

                DateTime _eventDateTime;
                uint _dateTimeStatus = DateTimeCheck.CheckDateTimeStamp(_logTimeStamp, out _eventDateTime);

                if (_dateTimeStatus == (uint)DateTimeCheck.Status.Ok)
                {
                    UInt16 _tempEventID = BitConverter.ToUInt16(_logChuncks, 6);
                    string _tempEventInfo = "";
                    try
                    {
                        if (ExcelFilemanager.LPDInfoList.ElementAt(_tempEventID-1).Data[0].DataLink == "Empty")
                        {
                            _tempEventInfo = "No Information";
                        }
                        else
                        {
                            int _count = 0;
                            int _index = 0;
                            foreach (LPDDataLookupEntry _dataLookupEntry in ExcelFilemanager.LPDInfoList.ElementAt(_tempEventID-1).Data)
                            {

                                if (_dataLookupEntry.DataLink != "Empty")
                                {
                                    double _scale = Math.Pow(10, _dataLookupEntry.Scale);
                                    if (_count > 0)
                                    {
                                        _tempEventInfo += " , ";
                                    }

                                    if (_dataLookupEntry.NumberBytes == 4)
                                    {
                                        if (_dataLookupEntry.IsInt)
                                        {
                                            _tempEventInfo += _dataLookupEntry.DataName + ": " + (BitConverter.ToInt32(_logData, _index)*_scale).ToString();
                                            _index += 4;
                                        }
                                        else
                                        {
                                            _tempEventInfo += _dataLookupEntry.DataName + ": " + (BitConverter.ToUInt32(_logData, _index)*_scale).ToString();
                                            _index += 4;
                                        }
                                    }
                                    else if (_dataLookupEntry.NumberBytes == 2)
                                    {

                                        if (_dataLookupEntry.IsInt)
                                        {
                                            _tempEventInfo += _dataLookupEntry.DataName + ": " + (BitConverter.ToInt16(_logData, _index) * _scale).ToString();
                                            _index += 2;
                                        }
                                        else
                                        {
                                            _tempEventInfo += _dataLookupEntry.DataName + ": " + (BitConverter.ToUInt16(_logData, _index) * _scale).ToString();
                                            _index += 2;
                                        }
                                    }

                                    else if (_dataLookupEntry.NumberBytes == 1)
                                    {

                                        if (_dataLookupEntry.IsInt)
                                        {
                                            _tempEventInfo += _dataLookupEntry.DataName + ": " + ((ushort)_logData[_index]*_scale).ToString();
                                            _index += 1;
                                        }
                                        else
                                        {
                                            _tempEventInfo += _dataLookupEntry.DataName + ": " + (_logData[_index]*_scale).ToString();
                                            _index += 1;
                                        }
                                    }
                                }
                                _tempEventInfo += " " + _dataLookupEntry.Appendix;
                               _count++;

                            }

                        }
                    }
                    catch
                    {
                        _tempEventInfo = "No Information";
                    }


                    try
                    {
                        TempList.Add(new LogEntry
                        {
                            Number = i,
                            EventID = _tempEventID,
                            EventName = ExcelFilemanager.LPDInfoList.ElementAt(_tempEventID - 1).EventName,
                            RawEntry = _logChuncks,
                            RawData = _logData,
                            EventInfo = _tempEventInfo,
                            DateTime = _eventDateTime
                        });
                    }
                    catch
                    {

                    }
                }
                else if (_dateTimeStatus == (uint)DateTimeCheck.Status.Error_2)
                {
                    LogErrorDateTimeCounter++;
                }
                else if (_dateTimeStatus == (uint)DateTimeCheck.Status.Error_3)
                {
                    LogErrorDateTimeCounter++;
                }
                else if (_dateTimeStatus == (uint)DateTimeCheck.Status.Error_4)
                {
                    LogErrorDateTimeCounter++;
                }

                if ((i % (TotalLogEntries / 100)) == 0)
                {
                    PercentageComplete++;
                    ReportProgress(PercentageComplete);
                }
            }

            }

        }
}

 
