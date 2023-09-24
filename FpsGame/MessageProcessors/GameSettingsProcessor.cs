using FpsGame.Common.ClientData;
using FpsGame.Common.Constants;
using FpsGame.Common.Level;
using FpsGame.Common.Utils;
using FpsGame.Ui.Components;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
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
        Dictionary<string, Model> Models;
        AssetImporter importer;

        public GameSettingsProcessor(GameSettings gameSettings, Label gameNameLabel, Label levelNameLabel, Dictionary<string, Model> Models, AssetImporter importer) 
        {
            this.gameSettings = gameSettings;
            this.gameNameLabel = gameNameLabel;
            this.levelNameLabel = levelNameLabel;
            this.Models = Models;
            this.importer = importer;
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

            string levelFile = data["LevelFile"].ToString();
            var level = new LevelFromFile(null, null, Path.Combine("Levels", levelFile));
            level.GetLevelInfo();
            foreach(var model in level.LevelData.Models)
            {
                if (!Models.ContainsKey(model.ModelName))
                {
                    Models.Add(model.ModelName, importer.LoadModel(Path.Combine("Content", model.ModelName + ".fbx")));
                }
            }
        }
    }
}
