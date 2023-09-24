using FpsGame.Common.ClientData;
using FpsGame.Common.Constants;
using FpsGame.Ui.Components;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FpsGame.MessageProcessors
{
    public class GameSettingsProcessor : IMessageProcessor
    {
        GameSettings gameSettings;
        Label gameNameLabel;
        Label levelNameLabel;

        public GameSettingsProcessor(GameSettings gameSettings, Label gameNameLabel, Label levelNameLabel) 
        {
            this.gameSettings = gameSettings;
            this.gameNameLabel = gameNameLabel;
            this.levelNameLabel = levelNameLabel;
        }

        public void ProcessMessage(JObject data)
        {
            if (gameSettings.GameMode != GameMode.SinglePlayer)
            {
                gameSettings.GameName = data["GameName"].ToString();
                gameNameLabel.UpdateText(gameSettings.GameName);
            }

            gameSettings.LevelName = data["LevelName"].ToString();
            levelNameLabel.UpdateText(gameSettings.LevelName);
        }
    }
}
