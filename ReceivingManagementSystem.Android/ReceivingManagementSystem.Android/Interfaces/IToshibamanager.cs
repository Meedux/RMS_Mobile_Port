using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JP.CO.Toshibatec;
using JP.CO.Toshibatec.Callback;
using JP.CO.Toshibatec.Model;

namespace ReceivingManagementSystem.Android.Interfaces
{
    public interface IToshibamanager
    {
        int OpenDevice(string deviceName);
        int SetOptions(Dictionary<string, Java.Lang.Integer> options);
        int ClaimDevice(string macAddress, ConnectionEventHandler handler);
        int SetDeviceEnabled(bool enabled);
        void OnEvent(int p0);
        int ReadTags(int cmd, string tagID, string filterID, string filtermask, int start, int length, int timeout, ResultTagCallback callback);
        void OnCallback(int resultCode, int resultCodeExtended, IDictionary<string, TagPack> tagList);
        int StartReadTags(string arg1, string arg2, int arg3, DataEventHandler dataHandler, ErrorEventHandler errorHandler);
        void OnDataEvent(IDictionary<string, TagPack> tagList);
        void OnErrorEvent(int resultCode, int resultCodeExtended);
        ErrorEventHandler ErrorEventHandler { get; set; }
        DataEventHandler DataEventHandler { get; set; }
        int StopReadTags(ResultCallback callback);
        void OnStopReadTagsCallback(int p0, int p1);
        int SetQValue(int value, ResultCallback callback);
        int StartReadTagsEx(int rfidRtId, string arg2, string arg3, int arg4, int arg5, int arg6, string arg7, DataEventHandler dataHandler, ErrorEventHandler errorHandler);
        int StopReadTagsEx(ResultCallback callback);

        // Method to connect to the device
        bool ConnectToDeviceAsync(string deviceName);

        // Method to scan for RFID tags
        Task<List<TagPack>> ScanForTagsAsync();

        // Method to calculate the distance based on the radio wave strength
        double CalculateDistanceFromSignalStrength(int signalStrength);
        Task<string> FindDirectionAsync(TagPack tag);
    }
}
