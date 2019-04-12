using Booyco_HMI_Utility.CustomObservableCollection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProximityDetectionSystemInfo;

namespace Booyco_HMI_Utility
{
    class DataLogManagement    
    {
        public Action<int> ReportProgressDelegate { get; set; }
        public const int PDSThreatEventID = 150;
        public const int PDSThreatEventIDLast = 158;
        public const int PDSThreatEventEndID = 159;
        public const int PDSThreatEventEndIDLast = 168;
        public const int PDSThreatEventLength = 9;

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
        public ExcelFileManagement ExcelFilemanager = new ExcelFileManagement();
        public bool AbortRequest = false;

        #endregion

      
        public bool ReadFile(string Log_Filename)
        {
            ExcelFilemanager.StoreLogProtocolInfo();
            byte[] _logBytes = { 0 };
      
            byte[] _logTimeStamp = { 0, 0, 0, 0, };
          
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
            for (uint i = 0; i < (int)TotalLogEntries - 1; i++)
            {

                if ((i % (TotalLogEntries / 100)) == 0)
                {
                    PercentageComplete++;
                    ReportProgress(PercentageComplete);
                }

                if (AbortRequest)
                {
                    _breader.Close();
                    _breader.Dispose();
                    PercentageComplete=0;
                    ReportProgress(PercentageComplete);
                    return false;
                }
                byte[] _logChuncks = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                byte[] _logData = { 0, 0, 0, 0, 0, 0, 0, 0 };
                for (int j = 0; j < TOTAL_ENTRY_BYTES; j++)
                {
                    _logChuncks[j] = _logBytes[i * TOTAL_ENTRY_BYTES + j];
                }

                // ---- Copy from the 16 byte logChunk the DateTimeStamp ----
                Buffer.BlockCopy(_logChuncks, 0, _logTimeStamp, 0, 4);
                DateTime _eventDateTime;
                uint _dateTimeStatus = DateTimeCheck.CheckDateTimeStampUnix(BitConverter.ToUInt32(_logTimeStamp, 0), out _eventDateTime);
               
                if (_dateTimeStatus == (uint)DateTimeCheck.Status.Ok)
                {
                  //  Buffer.BlockCopy(_logChuncks, 4, _logMiliseconds, 0, 1);
                    // ---- Copy from the 16 byte logChunk the Data ----
                    Buffer.BlockCopy(_logChuncks, 8, _logData, 0, 8);



                    //byte[] unixvaluebyte = { 0x0B, 0x62, 0x7F, 0x5C };
                    if (BitConverter.ToUInt16(_logChuncks, 6) == 1)
                    {
                        int testing = 0;
                    }

                    if (_logChuncks[4] < 100)
                    {
                        _eventDateTime = _eventDateTime.AddMilliseconds((double)_logChuncks[4] * 10);
                    }
                    else
                    {
                        _eventDateTime = _eventDateTime.AddMilliseconds(990);
                    }
                    UInt16 _tempEventID = BitConverter.ToUInt16(_logChuncks, 6);
                    if (_tempEventID > 0)
                    {

                        string _tempEventInfo = "";
                        List<string> _tempDataListString = new List<string>();
                        List<string> _tempEventInfoList = new List<string>();
                        List<double> _tempDataList = new List<double>();
                        try
                        {

                            if (ExcelFilemanager.LPDInfoList.ElementAt(_tempEventID - 1).Data[0].DataLink == "Empty")
                            {
                                _tempEventInfo = "No Information";
                                _tempEventInfoList.Add("No Information");
                            }
                            else
                            {
                                int _count = 0;
                                int _index = 0;
                                foreach (LPDDataLookupEntry _dataLookupEntry in ExcelFilemanager.LPDInfoList.ElementAt(_tempEventID - 1).Data)
                                {

                                    if (_dataLookupEntry.DataLink != "Empty")
                                    {
                                        double _scale = Math.Pow(10, _dataLookupEntry.Scale);
                                        if (_count > 0)
                                        {
                                            _tempEventInfo += " , ";
                                        }

                                        //if(_Data

                                        if (_dataLookupEntry.NumberBytes == 4)
                                        {
                                            if (_dataLookupEntry.IsInt == 2)
                                            {
                                                byte[] _tempByteArray = { 0, 0, 0, 0 };

                                                Array.Copy(_logData, _index, _tempByteArray, 0, 4);

                                                UInt32 HexValue = BitConverter.ToUInt32(_tempByteArray, 0);

                                                _tempDataListString.Add("0x" + (Convert.ToUInt32(HexValue * _scale)).ToString("X8"));
                                                _tempDataList.Add(Convert.ToDouble(HexValue));


                                            }
                                            else if (_dataLookupEntry.IsInt == 1)
                                            {
                                                _tempDataListString.Add((BitConverter.ToInt32(_logData, _index) * _scale).ToString());
                                                _tempDataList.Add(double.Parse(_tempDataListString.Last()));

                                            }
                                            else
                                            {
                                                _tempDataListString.Add((BitConverter.ToUInt32(_logData, _index) * _scale).ToString());
                                                _tempDataList.Add(double.Parse(_tempDataListString.Last()));

                                            }


                                            _tempEventInfo += _dataLookupEntry.DataName + ": " + _tempDataListString.Last();
                                            _tempEventInfoList.Add(_dataLookupEntry.DataName + ": " + _tempDataListString.Last() + _dataLookupEntry.Appendix);
                                            _index += 4;

                                        }
                                        else if (_dataLookupEntry.NumberBytes == 2)
                                        {

                                            if (_dataLookupEntry.IsInt == 1)
                                            {
                                                _tempDataListString.Add((BitConverter.ToInt16(_logData, _index) * _scale).ToString());
                                            }
                                            else
                                            {
                                                _tempDataListString.Add((BitConverter.ToUInt16(_logData, _index) * _scale).ToString());
                                            }
                                            _tempDataList.Add(double.Parse(_tempDataListString.Last()));
                                            _tempEventInfo += _dataLookupEntry.DataName + ": " + _tempDataListString.Last() ;
                                            _tempEventInfoList.Add(_dataLookupEntry.DataName + ": " + _tempDataListString.Last() + _dataLookupEntry.Appendix);
                                            _index += 2;
                                        }

                                        else if (_dataLookupEntry.NumberBytes == 1)
                                        {
                                          
                                            if (_dataLookupEntry.EnumLink == 0 || ExcelFilemanager.LPDInfoEnumList[_dataLookupEntry.EnumLink].Count < _logData[_index])
                                            {

                                                if (_dataLookupEntry.IsInt == 1)
                                                {
                                                    _tempDataListString.Add(((ushort)_logData[_index] * _scale).ToString());
                                                }
                                                else
                                                {
                                                    _tempDataListString.Add((_logData[_index] * _scale).ToString());
                                                }
                                                _tempDataList.Add(double.Parse(_tempDataListString.Last()));

                                            }

                                            else if (_dataLookupEntry.EnumLink != 0)
                                            {
                                                _tempDataList.Add((_logData[_index] * _scale));
                                                _tempDataListString.Add((ExcelFilemanager.LPDInfoEnumList[_dataLookupEntry.EnumLink].ElementAt(_logData[_index])));
                                            }
                                       
                                            _tempEventInfo += _dataLookupEntry.DataName + ": " + _tempDataListString.Last() ;
                                            _tempEventInfoList.Add(_dataLookupEntry.DataName + ": " + _tempDataListString.Last() + _dataLookupEntry.Appendix);


                                            _index += 1;
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
                            _tempEventInfoList.Add("No Information");
                        }


                        try
                        {

                            if (_tempEventID > PDSThreatEventID && _tempEventID < PDSThreatEventIDLast)
                            {
                               if (TempList.Last().EventID == PDSThreatEventID && _tempDataList.Count() > 0)
                                {
                                    TempList.Last().EventInfo += Environment.NewLine + _tempEventInfo;
                                    TempList.Last().DataList.AddRange( _tempDataList);
                                    TempList.Last().DataListString.AddRange(_tempDataListString);
                                    TempList.Last().EventInfoList.AddRange(_tempEventInfoList);
                                    Buffer.BlockCopy(_logData, 0, TempList.Last().RawData, (_tempEventID - PDSThreatEventID) * 8, 8);

                                }

                            }
                            else if (_tempEventID > PDSThreatEventEndID && _tempEventID < PDSThreatEventEndIDLast)
                            {
                                if (TempList.Last().EventID == PDSThreatEventEndID && _tempDataList.Count() > 0)
                                {
                                    TempList.Last().EventInfo += Environment.NewLine + _tempEventInfo;
                                    TempList.Last().DataList.AddRange(_tempDataList);
                                    TempList.Last().DataListString.AddRange(_tempDataListString);
                                    TempList.Last().EventInfoList.AddRange(_tempEventInfoList);
                                    Buffer.BlockCopy(_logData, 0, TempList.Last().RawData, (_tempEventID - PDSThreatEventEndID) * 8, 8);
                                }
                                
                            }
                            else if(_tempEventID == 501)
                            {
                                if (TempList.Last().EventID == 500 && _tempDataList.Count() > 0)
                                    {
                                    TempList.Last().EventInfo += Environment.NewLine + _tempEventInfo;
                                    TempList.Last().DataList.AddRange(_tempDataList);
                                    TempList.Last().DataListString.AddRange(_tempDataListString);
                                    TempList.Last().EventInfoList.AddRange(_tempEventInfoList);
                                    Buffer.BlockCopy(_logData, 0, TempList.Last().RawData, 8, 8);
                                }
                                
                            }
                     
                            else if (_tempEventID == PDSThreatEventID || _tempEventID == PDSThreatEventEndID || _tempEventID == 500)
                            {
                                byte[] byteArray = new byte[PDSThreatEventLength*8];
                                byteArray[0] = _logData[0];
                                byteArray[1] = _logData[1];
                                byteArray[2] = _logData[2];
                                byteArray[3] = _logData[3];
                                byteArray[4] = _logData[4];
                                byteArray[5] = _logData[5];
                                byteArray[6] = _logData[6];
                                byteArray[7] = _logData[7];

                                TempList.Add(new LogEntry
                                {
                                    Number = i,
                                    EventID = _tempEventID,
                                    EventName = ExcelFilemanager.LPDInfoList.ElementAt(_tempEventID - 1).EventName,
                                    RawEntry = _logChuncks,
                                    RawData = byteArray,
                                    EventInfo = _tempEventInfo,
                                    DateTime = _eventDateTime,
                                    DataListString = _tempDataListString,
                                    DataList =_tempDataList,
                                    EventInfoList = _tempEventInfoList

                                });
                            }
                            else
                            {
                                TempList.Add(new LogEntry
                                {
                                    Number = i,
                                    EventID = _tempEventID,
                                    EventName = ExcelFilemanager.LPDInfoList.ElementAt(_tempEventID - 1).EventName,
                                    RawEntry = _logChuncks,
                                    RawData = _logData,
                                    EventInfo = _tempEventInfo,
                                    DateTime = _eventDateTime,
                                    DataListString = _tempDataListString,
                                    DataList = _tempDataList,
                                    EventInfoList = _tempEventInfoList

                                });
                            }

                        }
                        catch(Exception e)
                        {
                            Console.WriteLine("unable to add event");
                        }
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

            }

            try
            {
                _breader.Close();
                _breader.Dispose();
            }
            catch
            {
                Console.WriteLine("failed to close Binary reader.");
            }
            return true;
            }

        }
}

 
