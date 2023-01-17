using Matrix;
using Matrix.Extensions.Client.Message;
using Matrix.Xmpp.Client;
using Matrix.Xmpp.XData;
using System.Threading.Tasks;
using UnityEngine;

public class XmppCommunicator
{
    public static async Task SendXmppCommand(XmppClient xmppClient, string username, string domain, string textMessage) {
        Jid jid = new Jid($"{username}@{domain}");
        await SendXmppCommand(xmppClient, jid, textMessage);
    }

    public static async Task SendXmppCommand(XmppClient xmppClient, Jid jid, string textMessage) {
        await SendMessageAsync(xmppClient, jid, textMessage, "command");
    }

    public static async Task SendXmppImage(XmppClient xmppClient, string username, string domain, ImageData imageData) {
        Jid jid = new Jid($"{username}@{domain}");
        await SendXmppImage(xmppClient, jid, imageData);
    }

    public static async Task SendXmppImage(XmppClient xmppClient, Jid jid, ImageData imageData) {
        string imageDataJson = JsonUtility.ToJson(imageData);
        await SendMessageAsync(xmppClient, jid, imageDataJson, "image");
    }

    private static async Task SendMessageAsync(XmppClient xmppClient, Jid jid, string textMessage, string fieldValue) {
        Message message = new Message(jid, textMessage) {
            XData = new Data(FormType.Form),
            Type = Matrix.Xmpp.MessageType.Chat
        };
        Field metadata = new Field("five", fieldValue) {
            Type = FieldType.TextSingle
        };
        message.XData.AddField(metadata);
        message.XData.Title = "spade:x:metadata";
        await xmppClient.SendMessageAsync(message);
    }
}
