using ReceivingManagementSystem.Common;
using ReceivingManagementSystem.Common.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using static RMS_Pleasanter.Client;
using static RMS_Pleasanter.Custody;
using static RMS_Pleasanter.CustodyDetail;

namespace ReceivingManagementSystem.Android.ViewModels.Orders
{
    public class SendSettingParamViewModel : BaseModel
    {
        /// <summary>
        /// ユーザ名
        /// </summary>
        private string _userName;

        /// <summary>
        /// ユーザ名
        /// </summary>
        public string UserName
        {
            get { return _userName; }
            set { this.SetProperty(ref this._userName, value); }
        }

        /// <summary>
        /// パスワード
        /// </summary>
        private string _password;

        /// <summary>
        /// パスワード
        /// </summary>
        public string Password
        {
            get { return _password; }
            set { this.SetProperty(ref this._password, value); }
        }

        /// <summary>
        /// サーバー
        /// </summary>
        private string _server;

        /// <summary>
        /// サーバー
        /// </summary>
        public string Server
        {
            get { return _server; }
            set { this.SetProperty(ref this._server, value); }
        }

        /// <summary>
        /// ポート
        /// </summary>
        private string _port;

        /// <summary>
        /// ポート
        /// </summary>
        public string Port
        {
            get { return _port; }
            set { this.SetProperty(ref this._port, value); }
        }

        /// <summary>
        /// 暗号方法
        /// </summary>
        private string _encryptionMethod;

        /// <summary>
        /// 暗号方法
        /// </summary>
        public string EncryptionMethod
        {
            get { return _encryptionMethod; }
            set { this.SetProperty(ref this._encryptionMethod, value); }
        }

        /// <summary>
        /// 送信サーバには認証が必要です
        /// </summary>
        private bool _requiresAuthentication;

        /// <summary>
        /// 送信サーバには認証が必要です
        /// </summary>
        public bool RequiresAuthentication
        {
            get { return _requiresAuthentication; }
            set { this.SetProperty(ref this._requiresAuthentication, value); }
        }

        /// <summary>
        /// MailTo
        /// </summary>
        private string _mailTo;

        /// <summary>
        /// MailTo
        /// </summary>
        public string MailTo
        {
            get { return _mailTo; }
            set { this.SetProperty(ref this._mailTo, value); }
        }

        public SendSettingParamViewModel()
        {

        }
    }
}
