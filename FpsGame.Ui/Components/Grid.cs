using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Myra.Graphics2D.UI;
using MyraGrid = Myra.Graphics2D.UI.Grid;

namespace FpsGame.Ui.Components
{
    public class Grid : Component<MyraGrid>
    {
        public Grid(string id)
            : base(id)
        { 
            UiWidget = new MyraGrid()
            {
                Id = id,
                RowSpacing = 8,
                ColumnSpacing = 8
            };

            UiWidget.ColumnsProportions.Add(new Proportion(ProportionType.Auto));
            UiWidget.ColumnsProportions.Add(new Proportion(ProportionType.Auto));
            UiWidget.RowsProportions.Add(new Proportion(ProportionType.Auto));
            UiWidget.RowsProportions.Add(new Proportion(ProportionType.Auto));
        }

        public void AddWidget<T>(Component<T> component)
            where T : Widget
        {
            UiWidget.Widgets.Add(component.UiWidget);
        }
    }
}
