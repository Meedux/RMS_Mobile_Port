using ReceivingManagementSystem.Wrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[assembly: Xamarin.Forms.Dependency(typeof(ReceivingManagementSystem.UWP.SaveSettingsWrapper))]
namespace ReceivingManagementSystem.UWP
{

    class SaveSettingsWrapper : ReceivingManagementSystem.Wrapper.ISaveSettingsWrapper
    {
        Windows.Storage.ApplicationDataContainer m_pPreferences = Windows.Storage.ApplicationData.Current.LocalSettings;

        public void InitSharedPreferences()
        {
            //いらない?
        }

        public bool SaveSettings(params SaveSettingsParam[] IN_varrpParams)
        {
            if (m_pPreferences != null)
            {
                foreach (SaveSettingsParam pParam in IN_varrpParams)
                {
                    switch (pParam.m_SaveTypes)
                    {
                        case SaveSettingsParam.SaveTypes.STRING:
                            m_pPreferences.Values[pParam.m_strKey] = (string)(pParam.m_pObj);
                            break;
                        case SaveSettingsParam.SaveTypes.LONG:
                            m_pPreferences.Values[pParam.m_strKey] = (long?)(pParam.m_pObj);
                            break;
                        case SaveSettingsParam.SaveTypes.FLOAT:
                            m_pPreferences.Values[pParam.m_strKey] = (float)(pParam.m_pObj);
                            break;
                        case SaveSettingsParam.SaveTypes.BOOL:
                            m_pPreferences.Values[pParam.m_strKey] = (bool)(pParam.m_pObj);
                            break;
                        default:
                            break;
                    }
                }
                return true;
            }

            return false;
        }

        public string GetString(string IN_strKey, string IN_strDefValue)
        {
            try
            {
                return (string)m_pPreferences.Values[IN_strKey];
            }
            catch (Exception /* e */)
            {
                return IN_strDefValue;
            }
        }

        public bool GetBool(string IN_strKey, bool IN_boDefValue)
        {
            try
            {
                return m_pPreferences.Values[IN_strKey] == null ? false : (bool)m_pPreferences.Values[IN_strKey];
            }
            catch (Exception /* e */)
            {
                return IN_boDefValue;
            }
        }

        public long? GetLong(string IN_strKey, long? IN_lDefValue)
        {
            try
            {
                long? value = (long?)m_pPreferences.Values[IN_strKey];
                return value.HasValue ? value : IN_lDefValue;
            }
            catch (Exception /* e */)
            {
                return IN_lDefValue;
            }
        }

        public float GetFloat(string IN_strKey, float IN_fDefValue)
        {
            try
            {
                return (float)m_pPreferences.Values[IN_strKey];
            }
            catch (Exception /* e */)
            {
                return IN_fDefValue;
            }
        }
    }
}
