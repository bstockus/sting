using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace QuickSting {
    public static class ImageButtonWithHoverProperties {

        public static ImageSource GetNormalImage(DependencyObject obj) {
            return (ImageSource)obj.GetValue(NormalImageProperty);
        }

        public static void SetNormalImage(DependencyObject obj, ImageSource value) {
            obj.SetValue(NormalImageProperty, value);
        }

        public static ImageSource GetHoverImage(DependencyObject obj) {
            return (ImageSource)obj.GetValue(HoverImageProperty);
        }

        public static void SetHoverImage(DependencyObject obj, ImageSource value) {
            obj.SetValue(HoverImageProperty, value);
        }

        public static readonly DependencyProperty NormalImageProperty = DependencyProperty.RegisterAttached("NormalImage", 
            typeof(ImageSource), 
            typeof(ImageButtonWithHoverProperties));

        public static readonly DependencyProperty HoverImageProperty = DependencyProperty.RegisterAttached("HoverImage",
            typeof(ImageSource),
            typeof(ImageButtonWithHoverProperties));

    }
}
