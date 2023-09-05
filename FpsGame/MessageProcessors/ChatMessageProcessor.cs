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
    public class ChatMessageProcessor : IMessageProcessor
    {
        List<ChatMessage> chatMessages;
        ChatBox chatBox;

        public ChatMessageProcessor(ChatBox chatBox) 
        {
            chatMessages = new List<ChatMessage>();
            this.chatBox = chatBox;
        }

        public void ProcessMessage(JObject data)
        {
            var message = data.ToObject<ChatMessage>();
            chatMessages.Add(message);
            chatBox.AddMessage(message);
        }
    }
}
