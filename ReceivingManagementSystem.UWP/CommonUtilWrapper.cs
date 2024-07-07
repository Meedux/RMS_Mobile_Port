using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Notifications;

[assembly: Xamarin.Forms.Dependency(typeof(ReceivingManagementSystem.UWP.CommonUtilWrapper))]
namespace ReceivingManagementSystem.UWP
{
    class CommonUtilWrapper : ReceivingManagementSystem.Wrapper.ICommonUtilWrapper
    {
        public void CloseApplication()
        {
            //Do nothing (Not Required in UWP)
        }

        /**
         * トースト表示
         * Toast display
         * @param msg
         */

        ToastNotification m_pToast = null;

        private readonly object showMessageLock = new object();
        public void ShowMessage(string msg)
        {
            lock (showMessageLock)
            {
                ToastNotifier ToastNotifier = ToastNotificationManager.CreateToastNotifier();
                Windows.Data.Xml.Dom.XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText02);
                Windows.Data.Xml.Dom.XmlNodeList toastNodeList = toastXml.GetElementsByTagName("text");
                toastNodeList.Item(0).AppendChild(toastXml.CreateTextNode("Receiving Management System"));
                toastNodeList.Item(1).AppendChild(toastXml.CreateTextNode(msg));
                Windows.Data.Xml.Dom.IXmlNode toastNode = toastXml.SelectSingleNode("/toast");
                Windows.Data.Xml.Dom.XmlElement audio = toastXml.CreateElement("audio");
                audio.SetAttribute("src", "ms-winsoundevent:Notification.SMS");

                if (m_pToast != null)
                {
                    ToastNotifier.Hide(m_pToast);
                }

                m_pToast = new ToastNotification(toastXml);
                m_pToast.ExpirationTime = DateTime.Now.AddSeconds(3);
                ToastNotifier.Show(m_pToast);
            }

        }

        /**
         * トースト表示
         * Toast display
         * @param msg
         */
        private readonly object showMessage2Lock = new object();
        public void ShowMessage2(string msg)
        {
            Task thread = new Task(() =>
            {
                //作成中
                //Looper.Prepare();
                ShowMessage(msg);
                //Looper.Loop();

            });

            thread.Start();
        }


        /**
            * バックグラウンドでサービス起動
            * Service start up in the background
            */
        public void StartService()
        {
            //Do nothing (Not Required in UWP)
        }
    }
}
