using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReceivingManagementSystem.Common.Resources
{
    public interface IResourceProvider
    {
        string GetResourceByName(string name);
        string GetResource(string key, params object[] parameter);
        string GetMesResource(string key, params object[] parameter);
    }
}
