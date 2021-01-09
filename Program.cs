using System;
using System.Collections.Generic;
using System.Linq;
using TeleSharp.TL;
using TeleSharp.TL.Messages;
using TLSharp.Core;

namespace TelegramConsoleApp
{
    public class MainClass
    {
        private const string hash = "7434fddacf9bd9ffff7389d36a899b0a";
        private const string userNumber = "+353833146370";
        public static async System.Threading.Tasks.Task Main(string[] args)
        {
            var client = new TelegramClient(2954623, hash);
            await client.ConnectAsync();

            var hashCode = await client.SendCodeRequestAsync(userNumber);
            Console.WriteLine("Type telegram code");
            var telegramCode = Console.ReadLine();
                
            var user = await client.MakeAuthAsync(userNumber, hashCode, telegramCode);

            if (client.IsUserAuthorized())
            {
                var dialogs = await client.GetUserDialogsAsync() as TLDialogs;

                foreach (var dia in dialogs.Dialogs.Where(x => x.Peer is TLPeerChannel && x.UnreadCount > 0))
                {
                    var peer = dia.Peer as TLPeerChannel;
                    var chat = dialogs.Chats.OfType<TLChannel>().FirstOrDefault(x => x.Id == peer.ChannelId
                                                                                && x.Title.Contains("ANGEL"));
                    var target = new TLInputPeerChannel() { ChannelId = chat.Id, AccessHash = (long)chat.AccessHash };
                    var hist = await client.GetHistoryAsync(target, 0, -1, dia.UnreadCount);

                    Console.WriteLine("=====================================================================");
                    Console.WriteLine("THIS IS:" + chat.Title + " WITH " + dia.UnreadCount + " UNREAD MESSAGES");
                    foreach (var m in (hist as TLChannelMessages).Messages)
                        Console.WriteLine((m as TLMessage).Message);
                }
                Console.ReadLine();

                //var dialogs = (TLDialogs)await client.GetUserDialogsAsync();
                //var chat = dialogs.Chats
                //    .OfType<TLChat>()
                //    .FirstOrDefault(c => c.Title.Contains("ANGEL"));

                //var tlAbsMessages =
                //        await client.GetHistoryAsync(
                //            new TLInputPeerChat { ChatId = chat.Id }, 0,
                //            0, -1, 1000);

                //var tlChannelMessages = (TLMessages)tlAbsMessages;

                //for (int i = 0; i < tlChannelMessages.Messages.Count - 1; i++)
                //{
                //    var tlAbsMessage = tlChannelMessages.Messages[i];

                //    var message = (TLMessage)tlAbsMessage;
                //}


                //var result = await client.GetContactsAsync();

                //var u = result.Users.OfType<TLUser>().ToList();

                //var dialogs = await client.GetUserDialogsAsync() as TLDialogs;

                //var dialogs = (TLDialogs)await client.GetUserDialogsAsync();
                //var chat = dialogs.Chats.OfType<TLChat>().ToList();


                //foreach (var dia in dialogs.dialogs.lists.Where(x => x.peer is TLPeerChannel && x.unread_count > 0))
                //{
                //    var peer = dia.peer as TLPeerChannel;
                //    var chat = dialogs.chats.lists.OfType<TLChannel>().First(x => x.id == peer.channel_id);
                //    var target = new TLInputPeerChannel() { channel_id = chat.id, access_hash = (long)chat.access_hash };
                //    var hist = await telegram.GetHistoryAsync(target, 0, -1, dia.unread_count);

                //    Console.WriteLine("=====================================================================");
                //    Console.WriteLine("THIS IS:" + chat.title + " WITH " + dia.unread_count + " UNREAD MESSAGES");
                //    foreach (var m in (hist as TLChannelMessages).messages.lists)
                //        Console.WriteLine((m as TLMessage).message);
                //}
            }
        }
    }
}
