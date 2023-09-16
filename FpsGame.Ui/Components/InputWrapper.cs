using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FpsGame.Ui.Components
{
    public class InputWrapper<T, U>
        where T : Component<U>
        where U : Myra.Graphics2D.UI.Widget
    {
        public Grid Grid { get; set; }
        public Label InputLabel { get; set; }
        public T InputComponent { get; set; }
        private string Id;

        public InputWrapper(string id, string labelText, T inputComponent) 
        {
            Id = id;
            InputLabel = new Label(id + "-label", labelText);
            InputComponent = inputComponent;
        
            Init();
        }

        public InputWrapper(string id, Label label, T inputComponent)
        {
            Id = id;
            InputLabel = label;
            InputComponent = inputComponent;

            Init();
        }

        private void Init()
        {
            Grid = new Grid(Id + "-grid");
            Grid.UiWidget.MinWidth = 200;
            InputLabel.UiWidget.GridRow = 0;
            InputLabel.UiWidget.GridColumn = 0;
            InputLabel.UiWidget.MinWidth = 100;
            InputComponent.UiWidget.GridRow = 0;
            InputComponent.UiWidget.GridColumn = 1;
            InputComponent.UiWidget.MinWidth = 100;
            Grid.UiWidget.Widgets.Add(InputLabel.UiWidget);
            Grid.UiWidget.Widgets.Add(InputComponent.UiWidget);
        }
    }
}
