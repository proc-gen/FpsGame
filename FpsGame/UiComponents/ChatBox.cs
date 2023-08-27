using FpsGame.Common.ClientData;
using FpsGame.Ui.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FpsGame.UiComponents
{
    public class ChatBox
    {
        List<ChatMessage> messages;
        public Label MessagesLabel { get; private set; }


        public ChatBox()
        {
            messages = new List<ChatMessage>();
            MessagesLabel = new Label("chat-messages", string.Empty);
        }

        public void AddMessage(ChatMessage message)
        {
            messages.Add(message);

            if(messages.Count > 6)
            {
                messages.RemoveAt(0);
            }

            string text = string.Empty;
            foreach(var msg in  messages)
            {
                text += string.Format("{0} ({1}): {2} \n", msg.SenderName, msg.Time.ToLongTimeString(), msg.Message);
            }

            MessagesLabel.UpdateText(text);
        }
    }
}
