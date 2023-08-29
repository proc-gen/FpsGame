using FpsGame.Common.ClientData;
using FpsGame.Ui.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Myra.Graphics2D.Brushes;

namespace FpsGame.UiComponents
{
    public class PlayersTable
    {
        public Grid Table { get; private set; }
        public List<PlayerInfo> Players { get; private set; }

        List<Label> TableHeaders;


        public PlayersTable() 
        {
            Init(new List<PlayerInfo>());
        }

        public PlayersTable(List<PlayerInfo> players)
        {
            Init(players);
        }

        private void Init(List<PlayerInfo> players)
        {
            Table = new Grid("players-table");
            Table.UiWidget.HorizontalAlignment = Myra.Graphics2D.UI.HorizontalAlignment.Center;
            Table.UiWidget.VerticalAlignment = Myra.Graphics2D.UI.VerticalAlignment.Center;
            Table.UiWidget.Background = new SolidBrush(new Color(.1f, .1f, .1f));
            Table.UiWidget.Margin = new Myra.Graphics2D.Thickness(4);
            Table.UiWidget.Padding = new Myra.Graphics2D.Thickness(4);
            Table.UiWidget.Opacity = .75f;

            Players = players;

            TableHeaders = new List<Label>()
            {
                new Label("players-table-player-name", "Player Name"),
                new Label("players-table-player-color", "Color"),
                new Label("players-table-player-ping", "Ping"),
            };
        }

        public void Update(List<PlayerInfo> players)
        {
            Players = players;
            Table.UiWidget.Widgets.Clear();
            for(int i = 0; i < TableHeaders.Count; i++)
            {
                TableHeaders[i].UiWidget.GridRow = 0;
                TableHeaders[i].UiWidget.GridColumn = i;
                Table.UiWidget.Widgets.Add(TableHeaders[i].UiWidget);
            }

            if (Players.Any())
            {
                for (int i = 0; i < Players.Count; i++) 
                {
                    var nameLabel = new Label(string.Format("players-table-player-name-{0}", i), Players[i].Name);
                    nameLabel.UiWidget.GridRow = i + 1;
                    nameLabel.UiWidget.GridColumn = 0;
                    
                    var colorLabel = new Label(string.Format("players-table-player-color-{0}", i), "          ");
                    colorLabel.UiWidget.Background = new SolidBrush(new Color(Players[i].Color));
                    colorLabel.UiWidget.GridRow = i + 1;
                    colorLabel.UiWidget.GridColumn = 1;

                    var pingLabel = new Label(string.Format("players-table-player-ping-{0}", i), string.Format("{0}ms", Players[i].Ping));
                    pingLabel.UiWidget.GridRow = i + 1;
                    pingLabel.UiWidget.GridColumn = 2;

                    Table.UiWidget.Widgets.Add(nameLabel.UiWidget);
                    Table.UiWidget.Widgets.Add(colorLabel.UiWidget);
                    Table.UiWidget.Widgets.Add(pingLabel.UiWidget);
                }
            }
        }
    }
}
