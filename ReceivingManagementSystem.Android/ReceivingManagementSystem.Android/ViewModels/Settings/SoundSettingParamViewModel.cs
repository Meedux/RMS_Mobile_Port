using ReceivingManagementSystem.Common;
using ReceivingManagementSystem.Common.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using static RMS_Pleasanter.Client;
using static RMS_Pleasanter.Custody;
using static RMS_Pleasanter.CustodyDetail;
using ReceivingManagementSystem.Android.Interfaces.Models;

namespace ReceivingManagementSystem.Android.ViewModels.Orders
{
    public class SoundSettingParamViewModel : BaseModel
    {
        /// <summary>
        /// Sound 1
        /// </summary>
        private string _track1;

        /// <summary>
        /// Sound 1
        /// </summary>
        public string Track1
        {
            get { return _track1; }
            set { this.SetProperty(ref this._track1, value); }
        }

        /// <summary>
        /// Sound 1
        /// </summary>
        private Interfaces.Models.FileModel _trackFile1;

        /// <summary>
        /// Sound 1
        /// </summary>
        public Interfaces.Models.FileModel TrackFile1
        {
            get { return _trackFile1; }
            set
            {
                Track1 = value != null ? value.FileName : string.Empty;
                this.SetProperty(ref this._trackFile1, value);
            }
        }

        /// <summary>
        /// Sound 2
        /// </summary>
        private string _track2;

        /// <summary>
        /// Sound 2
        /// </summary>
        public string Track2
        {
            get { return _track2; }
            set { this.SetProperty(ref this._track2, value); }
        }

        /// <summary>
        /// Sound 1
        /// </summary>
        private Interfaces.Models.FileModel _trackFile2;

        /// <summary>
        /// Sound 1
        /// </summary>
        public Interfaces.Models.FileModel TrackFile2
        {
            get { return _trackFile2; }
            set {

                Track2 = value != null ? value.FileName : string.Empty; 
                this.SetProperty(ref this._trackFile2, value); }
        }

        /// <summary>
        /// Sound 3
        /// </summary>
        private string _track3;

        /// <summary>
        /// Sound 3
        /// </summary>
        public string Track3
        {
            get { return _track3; }
            set { this.SetProperty(ref this._track3, value); }
        }

        /// <summary>
        /// Sound 1
        /// </summary>
        private Interfaces.Models.FileModel _trackFile3;

        /// <summary>
        /// Sound 1
        /// </summary>
        public Interfaces.Models.FileModel TrackFile3
        {
            get { return _trackFile3; }
            set {

                Track3 = value != null ? value.FileName : string.Empty; 
                this.SetProperty(ref this._trackFile3, value); }
        }

        /// <summary>
        /// Sound 4
        /// </summary>
        private string _track4;

        /// <summary>
        /// Sound 4
        /// </summary>
        public string Track4
        {
            get { return _track4; }
            set { this.SetProperty(ref this._track4, value); }
        }

        /// <summary>
        /// Sound 1
        /// </summary>
        private Interfaces.Models.FileModel _trackFile4;

        /// <summary>
        /// Sound 1
        /// </summary>
        public Interfaces.Models.FileModel TrackFile4
        {
            get { return _trackFile4; }
            set {

                Track4 = value != null ? value.FileName : string.Empty; 
                this.SetProperty(ref this._trackFile4, value); }
        }

        /// <summary>
        /// Sound 5
        /// </summary>
        private string _track5;

        /// <summary>
        /// Sound 5
        /// </summary>
        public string Track5
        {
            get { return _track5; }
            set { this.SetProperty(ref this._track5, value); }
        }

        /// <summary>
        /// Sound 1
        /// </summary>
        private Interfaces.Models.FileModel _trackFile5;

        /// <summary>
        /// Sound 1
        /// </summary>
        public Interfaces.Models.FileModel TrackFile5
        {
            get { return _trackFile5; }
            set {

                Track5 = value != null ? value.FileName : string.Empty;
                this.SetProperty(ref this._trackFile5, value); }
        }

        public SoundSettingParamViewModel()
        {

        }
    }
}
