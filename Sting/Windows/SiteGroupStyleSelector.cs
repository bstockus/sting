using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Sting.Windows {

    public class SiteGroupStyleSelector : StyleSelector {

        public Style DefaultStyle { get; set; }

        public Style SiteGroupStyle { get; set; }

        public override Style SelectStyle(object item, DependencyObject container) {
            CollectionViewGroup groupItem = (CollectionViewGroup)item;
            if (groupItem.Name == "") {
                return DefaultStyle;
            } else {
                return SiteGroupStyle;
            }
        }

    }
}
