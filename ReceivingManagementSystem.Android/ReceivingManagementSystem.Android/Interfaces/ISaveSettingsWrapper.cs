using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReceivingManagementSystem.Android.Interfaces
{
   public class SaveSettingsParam
    {
        public enum SaveTypes
        {
            STRING = 0,
            LONG,
            FLOAT,
            BOOL
        }

        public SaveTypes m_SaveTypes;
        public string m_strKey;
        public object m_pObj;

        public SaveSettingsParam ( SaveTypes IN_SaveTypes, string IN_strKey, object IN_pObj )
        {
            m_SaveTypes = IN_SaveTypes;
            m_strKey = IN_strKey;
            m_pObj = IN_pObj;
        }
    }

    public interface ISaveSettingsWrapper
    {
        void InitSharedPreferences();
        bool SaveSettings(params SaveSettingsParam[] IN_varrpParams);
        string GetString(string IN_strKey, string IN_strDefValue);
        bool GetBool(string IN_strKey, bool IN_boDefValue);
        long? GetLong(string IN_strKey, long? IN_lDefValue);
        float GetFloat(string IN_strKey, float IN_fDefValue);
    }
}
