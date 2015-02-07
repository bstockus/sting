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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace QuickSting {

    public class GroupStyleSelector : StyleSelector {

        public Style DefaultStyle { get; set; }

        public Style GroupStyle { get; set; }

        public override Style SelectStyle(object item, DependencyObject container) {
            CollectionViewGroup groupItem = (CollectionViewGroup)item;
            if (groupItem.Name.Equals("")) {
                return DefaultStyle;
            } else {
                return GroupStyle;
            }
        }

    }
}