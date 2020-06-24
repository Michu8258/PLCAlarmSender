using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace SMSHandlerUI.GUIcontrolsHelper
{
    public static class ListViewBehavior
    {
        public static readonly DependencyProperty HideColumnsProperty = DependencyProperty.RegisterAttached("HideColumns", typeof(string), typeof(ListViewBehavior), new FrameworkPropertyMetadata(HideColumnsChangedCallback));

        public static string GetHideColumns(UIElement element)
        {
            return (string)element.GetValue(HideColumnsProperty);
        }

        public static void SetHideColumns(UIElement element, string value)
        {
            element.SetValue(HideColumnsProperty, value);
        }

        private static void HideColumnsChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            //create backup of the listview columns and save it into Tag property of ListView
            List<GridViewColumn> columnsCache = null;
            ListView lv = (ListView)sender;
            GridView gv = ((GridView)lv.View);
            if (lv.Tag == null)
            {
                columnsCache = new List<GridViewColumn>();
                foreach (GridViewColumn col in gv.Columns)
                {
                    columnsCache.Add(col);
                }
                lv.Tag = columnsCache;
            }
            else
            {
                columnsCache = ((List<GridViewColumn>)lv.Tag);
            }

            //clear all the columns and put ListView into their deafult columns.
            gv.Columns.Clear();
            foreach (GridViewColumn cl in columnsCache)
            {
                gv.Columns.Add(cl);
            }

            //make a list of delete columns
            string newColumnsToHide = (string)args.NewValue;
            List<GridViewColumn> columntoDeleteList = new List<GridViewColumn>();
            foreach (string s in newColumnsToHide.Split(','))
            {
                if (int.TryParse(s, out int columnToHide))
                {
                    columntoDeleteList.Add(gv.Columns[columnToHide]);
                }
            }

            //delete the columns
            columntoDeleteList.ForEach(w => gv.Columns.Remove(w));
        }
    }
}
