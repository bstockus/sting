// Copyright 2014-2015, Bryan Stockus
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace Sting.Windows {
    public class ShowToolTipOnOverflowTextBox : TextBlock {
        protected override void OnToolTipOpening(ToolTipEventArgs e) {
            if (this.TextTrimming != System.Windows.TextTrimming.None)
                e.Handled = !IsTextTrimmed();
        }

        private bool IsTextTrimmed() {
            Typeface typeface = new Typeface(this.FontFamily,
                this.FontStyle,
                this.FontWeight,
                this.FontStretch);
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
