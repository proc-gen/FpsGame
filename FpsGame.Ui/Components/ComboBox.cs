using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FpsGame.Ui.Styles;
using MyraComboBox = Myra.Graphics2D.UI.ComboBox;
using MyraListItem = Myra.Graphics2D.UI.ListItem;


namespace FpsGame.Ui.Components
{
    public class ComboBox : Component<MyraComboBox>
    {
        private static Style baseStyle = new Style()
        {
            Margin = new Styles.Thickness(4),
        };

        public List<ListItem> ComboBoxItems { get; private set; }

        public ComboBox(string id)
            : base(id)
        {
            Init(new List<ListItem>(), baseStyle);
        }

        public ComboBox(string id, Style style)
            : base(id)
        {
            Init(new List<ListItem>(), style);
        }

        public ComboBox(string id, List<ListItem> comboBoxItems)
            : base(id)
        {
            Init(comboBoxItems, baseStyle);
        }

        public ComboBox(string id, List<ListItem> comboBoxItems, Style style)
            : base(id)
        {
            Init(comboBoxItems, style);
        }

        private void Init(List<ListItem> comboBoxItems, Style style)
        {
            ComboBoxItems = comboBoxItems;

            UiWidget = new MyraComboBox()
            {
                Id = Id,
            };

            foreach (ListItem item in comboBoxItems)
            {
                UiWidget.Items.Add(item.MyraListItem);
            }

            UpdateStyle(style);
        }

        public void AddComboBoxItem(ListItem item)
        {
            ComboBoxItems.Add(item);
            UiWidget.Items.Add(item.MyraListItem);
        }

        public void RemoveComboBoxItem(ListItem item)
        {
            UiWidget.Items.Remove(item.MyraListItem);
            ComboBoxItems.Remove(item);
        }
    }

    public class ListItem
    {
        public MyraListItem MyraListItem { get; private set; }
        public string Value;

        public ListItem(string text, string value = "")
        {
            MyraListItem = new MyraListItem(text);
            if(string.IsNullOrEmpty(value))
            {
                Value = text;
            }
            else
            {
                Value = value;
            }
        }
    }
}
