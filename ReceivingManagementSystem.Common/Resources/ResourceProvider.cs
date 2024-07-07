using ReceivingManagementSystem.Common.Resources;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace ReceivingManagementSystem.Common.Resources
{
    public class ResourceProvider : IResourceProvider
    {
        public string GetResourceByName(string name) =>
            TextResources.ResourceManager.GetString(name, new CultureInfo("ja")) ?? name;

        public string GetResource(string key, params object[] parameter)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException("The resource key input must be not null.");
            }

            try
            {
                if (parameter != null)
                {
                    return string.Format(TextResources.ResourceManager.GetString(key, new CultureInfo("ja")), GetResourceParameter(parameter));
                }
                else
                {
                    return TextResources.ResourceManager.GetString(key);
                }
            }
            catch
            {
                return key;
            }
        }

        private static object[] GetResourceParameter(params object[] parameter)
        {
            object[] paramR = new object[parameter.Length];
            string resource;
            for (int i = 0; i < parameter.Length; i++)
            {
                try
                {
                    //ResourceManager rm = TextResources.ResourceManager;
                    //ResourceSet rs = rm.GetResourceSet(new CultureInfo("ja"), true, true);
                    resource = TextResources.ResourceManager.GetString(parameter[i].ToString(), new CultureInfo("ja"));

                    paramR[i] = string.IsNullOrEmpty(resource) ? parameter[i] : resource;
                }
                catch
                {
                    paramR[i] = parameter[i];
                }
            }

            return paramR;
        }

        public string GetMesResource(string key, params object[] parameter)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException("The resource key input must be not null.");
            }

            try
            {
                //ResourceManager rm = MessageResources.ResourceManager;
                //ResourceSet rs = rm.GetResourceSet(new CultureInfo("ja"), true, true);
                string content = MessageResources.ResourceManager.GetString(key, new CultureInfo("ja")) ?? key;

                if (parameter != null)
                {
                    return string.Format(content, GetResourceParameter(parameter));
                }
                else
                {
                    return content;
                }
            }
            catch
            {
                return key;
            }
        }
    }
}
