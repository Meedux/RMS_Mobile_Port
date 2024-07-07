using Syncfusion.SfDataGrid.XForms;
using Syncfusion.SfDataGrid;
using Xamarin.Forms;
using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace ReceivingManagementSystem.Android.Styles
{
    [Xamarin.Forms.Internals.Preserve(AllMembers = true)]
    public class RmsDataGridStyle : DataGridStyle
    {
        public RmsDataGridStyle()
        {
        }

        public override Color GetHeaderBackgroundColor()
        {
            return Color.FromRgb(153, 153, 153);
        }

        public override ImageSource GetHeaderSortIndicatorDown()
        {
            Assembly m_pAssembly = null;
            m_pAssembly = this.GetType().Assembly;
            return ImageSource.FromResource("ReceivingManagementSystem.Resource.sort-down.png", m_pAssembly);
        }

        public override ImageSource GetHeaderSortIndicatorUp()
        {
            Assembly m_pAssembly = null;
            m_pAssembly = this.GetType().Assembly;
            return ImageSource.FromResource("ReceivingManagementSystem.Resource.sort-up.png", m_pAssembly);
        }
    }
}
