using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace Sting.Windows {
    public class ShowToolTipOnOverflowTextBox : TextBlock {
        /// <summary>
        /// handles whether tooltip should be displayed or not
        /// </summary>
        /// <param name="e"></param>
        protected override void OnToolTipOpening(ToolTipEventArgs e) {
            // checks whether text trimming is set or not
            // if text trimming is not set then tooltip will display as normal
            if (this.TextTrimming != System.Windows.TextTrimming.None)
                e.Handled = !IsTextTrimmed();
        }
 
        /// <summary>
        /// this method determines whether text has been trimmed or not
        /// </summary>
        /// <returns>if text is truncated then returns true else false</returns>
        private bool IsTextTrimmed() {
            Typeface typeface = new Typeface(this.FontFamily,
                this.FontStyle,
                this.FontWeight,
                this.FontStretch);
            
            // FormattedText is used to measure the whole width of the text held up by this container.
            FormattedText formmatedText = new FormattedText(
                this.Text,
                System.Threading.Thread.CurrentThread.CurrentCulture,
                this.FlowDirection,
                typeface,
                this.FontSize,
                this.Foreground);
 
            return formmatedText.Width > this.ActualWidth;
        }
    }

}
