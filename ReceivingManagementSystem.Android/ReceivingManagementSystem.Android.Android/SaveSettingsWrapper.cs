using Android.Content;
using ReceivingManagementSystem.Android.Interfaces;
using Xamarin.Forms;
using ReceivingManagementSystem.Android.Droid;

[assembly: Dependency(typeof(ReceivingManagementSystem.Android.Wrapper.SaveSettingsWrapper))]
namespace ReceivingManagementSystem.Android.Wrapper
{
    public class SaveSettingsWrapper : ISaveSettingsWrapper
    {
        ISharedPreferences mPreferences = MainActivity.AppContext.GetSharedPreferences("AppPreferences", FileCreationMode.Private);

        public void InitSharedPreferences()
        {
            // No initialization required for Android SharedPreferences
        }

        public bool SaveSettings(params SaveSettingsParam[] IN_varrpParams)
        {
            var editor = mPreferences.Edit();

            foreach (SaveSettingsParam pParam in IN_varrpParams)
            {
                switch (pParam.m_SaveTypes)
                {
                    case SaveSettingsParam.SaveTypes.STRING:
                        editor.PutString(pParam.m_strKey, (string)pParam.m_pObj);
                        break;
                    case SaveSettingsParam.SaveTypes.LONG:
                        editor.PutLong(pParam.m_strKey, (long)pParam.m_pObj);
                        break;
                    case SaveSettingsParam.SaveTypes.FLOAT:
                        editor.PutFloat(pParam.m_strKey, (float)pParam.m_pObj);
                        break;
                    case SaveSettingsParam.SaveTypes.BOOL:
                        editor.PutBoolean(pParam.m_strKey, (bool)pParam.m_pObj);
                        break;
                    default:
                        break;
                }
            }

            return editor.Commit();
        }

        public string GetString(string IN_strKey, string IN_strDefValue)
        {
            return mPreferences.GetString(IN_strKey, IN_strDefValue);
        }

        public bool GetBool(string IN_strKey, bool IN_boDefValue)
        {
            return mPreferences.GetBoolean(IN_strKey, IN_boDefValue);
        }

        public long? GetLong(string IN_strKey, long? IN_lDefValue)
        {
            if (mPreferences.Contains(IN_strKey))
            {
                return mPreferences.GetLong(IN_strKey, IN_lDefValue ?? 0);
            }
            return IN_lDefValue;
        }

        public float GetFloat(string IN_strKey, float IN_fDefValue)
        {
            return mPreferences.GetFloat(IN_strKey, IN_fDefValue);
        }
    }
}
