using FpsGame.Common.ClientData;
using FpsGame.UiComponents;
using Newtonsoft.Json.Linq;
using SharpFont;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FpsGame.MessageProcessors
{
    public class PlayersInfoProcessor : IMessageProcessor
    {
        PlayersTable playersTable;
        public PlayersInfoProcessor(PlayersTable playersTable) 
        {
            this.playersTable = playersTable;
        }

        public void ProcessMessage(JObject data)
        {
            playersTable.Update(data.ToObject<PlayersInfo>().Players);
        }
    }
}
