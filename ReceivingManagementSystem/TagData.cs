using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace ReceivingManagementSystem
{
    /**
     * タグに関するデータ
     * Data on tags
     */
    public class TagData : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly int MAX_LINE_CHAR_NUM = 24;

        private string text = "";  // テキスト text
        public int lineNumber = 1; // 行数  Number of lines

        private bool m_bIsStore = false;

        public bool isStore { get { return m_bIsStore; } }   // 格納されたかどうか Whether it was stored

        public string ShowText
        {
            get { return text; }
            set
            {
                text = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(ShowText)));
                }
            }
        }

        string m_strBackgroundColor = "";
        string m_strForegroundColor = "";

        public string BackgroundColor
        {
            get { return m_strBackgroundColor; }

            set
            {
                m_strBackgroundColor = value;

                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(BackgroundColor)));
                }
            }
        }
        public string ForegroundColor
        {
            get { return m_strForegroundColor; }

            set
            {
                m_strForegroundColor = value;

                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(ForegroundColor)));
                }

            }
        }

        /**
         * 空のデータ作成
         * Create empty data
         */
        public TagData()
        {
            SetEmptyData();
        }

        /**
         * テキストをもとにデータ作成
         * Data creation based on text
         * @param sourceText もとになるテキスト The original text
         */
        public TagData(String sourceText)
        {
            SetDataFromText(sourceText);
        }

        /**
         * データを空にする
         * Empty data
         */
        void SetEmptyData()
        {
            ShowText = "";
            lineNumber = 1;
            m_bIsStore = false;
        }

        /**
         * テキストをもとにデータ設定
         * Data setting based on text
         * @param sourceText もとになるテキスト The original text
         */
        public void SetDataFromText(String sourceText)
        {
            // 一定の文字数で折り返す
            // Wrap with a certain number of characters
            StringBuilder textBuilder = new StringBuilder(sourceText);
            int currentIndex = 0;
            lineNumber = 1;
            while (currentIndex + MAX_LINE_CHAR_NUM < textBuilder.Length)
            {
                textBuilder.Insert(currentIndex + MAX_LINE_CHAR_NUM, "\n");
                ++lineNumber;
                currentIndex += MAX_LINE_CHAR_NUM + 1 /* \n == 1 */;
            }
            ShowText = textBuilder.ToString();
            m_bIsStore = true;
        }

        /**
         * テキストを取得する
         * Retrieve text
         * @return テキスト text
         */
        String GetText()
        {
            return text;
        }

        /**
         * 行数を取得する
         * Retrieve  number of lines
         * @return 行数  Number of lines
         */
        int GetLineNumber()
        {
            return lineNumber;
        }

    }
}
