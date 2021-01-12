using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TelegramConsoleApp.Model;
using TeleSharp.TL;
using TeleSharp.TL.Messages;
using TeleSharp.TL.Updates;
using TLSharp.Core;
using TLSharp.Core.Utils;

namespace TelegramConsoleApp
{
    public class MainClass
    {
        private const string hash = "7434fddacf9bd9ffff7389d36a899b0a";
        private const string userNumber = "+353833146370";
        public static async System.Threading.Tasks.Task Main(string[] args)
        {
            var client = new TelegramClient(2954623, hash);
            var signalsList = new List<Signal>();

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
                                                                                && x.Title.Contains("24 HORAS"));
                    if (chat != null)
                    {
                        var target = new TLInputPeerChannel() { ChannelId = chat.Id, AccessHash = (long)chat.AccessHash };
                        var hist = await client.GetHistoryAsync(target, 0, -1, dia.UnreadCount);

                        Console.WriteLine("=====================================================================");
                        Console.WriteLine("THIS IS:" + chat.Title + " WITH " + dia.UnreadCount + " UNREAD MESSAGES");
                        var messagesLists = (hist as TLChannelMessages).Messages.ToList();
                        foreach (var m in messagesLists)
                        {
                            var me = (m as TLMessage);
                            var result = me.Message.Split(new char[0], StringSplitOptions.RemoveEmptyEntries);
                            for (int i = 0; i < result.Length; i++)
                            {
                                var regxTimeFormat = new Regex(@"^(?=\d)(?:(?:31(?!.(?:0?[2469]|11))|(?:30|29)(?!.0?2)|
                                        29(?=.0?2.(?:(?:(?:1[6-9]|[2-9]\d)?(?:0[48]|[2468][048]|[13579][26])|(?:(?:16|[2468][048]|[3579]
                                        [26])00)))(?:\x20|$))|(?:2[0-8]|1\d|0?[1-9]))([-./])(?:1[012]|0?[1-9])\1(?:1[6-9]|[2-9]\d)?\d\d
                                        (?:(?=\x20\d)\x20|$))?(((0?[1-9]|1[012])(:[0-5]\d){0,2}(\x20[AP]M))|([01]\d|2[0-3])(:[0-5]\d){1,2})?$");
                                
                                if (!regxTimeFormat.IsMatch(result[i]))
                                {
                                    var regxSpecialCharac = new Regex("[^a-zA-Z0-9_.]+");
                                    var regxEmoticons = new Regex(@"/(\u00a9|\u00ae|[\u2000-\u3300]|\ud83c[\ud000-\udfff]|\ud83d[\ud000-\udfff]|\ud83e
                                                    [\ud000-\udfff])/g");
                                    if (regxSpecialCharac.IsMatch(result[i]))
                                    {
                                        result = result.Where((source, index) => index != i).ToArray();
                                    }
                                    else if (regxEmoticons.IsMatch(result[i]))
                                    {
                                        result = result.Where((source, index) => index != i).ToArray();
                                    }
                                }
                            }

                            if (result.Length > 20)
                            {
                                var signal = new Signal();
                                signal.MessageId = Convert.ToInt64(me.Id);
                                signal.Currency = result[0].ToString();
                                signal.CurrencyTime = result[1].ToString();
                                signal.Time = result[2].ToString();
                                signal.CurrencySignal = result[3].ToString();
                                signal.Date = result[4].ToString();
                                signal.CurrencyAssertPercentage1 = result[12].ToString();
                                signal.CurrencyAssertPercentage2 = result[15].ToString();
                                signal.CurrencyAssertPercentage3 = result[16].ToString().Equals("BACKTEST") ? result[15].ToString() : result[19].ToString();

                                signalsList.Add(signal); 
                            }
                            
                            
                            //ForwardMessage(client, 1079068893, chat.Id, -4463481739700017704, me.Id);
                            Console.WriteLine((m as TLMessage).Message);
                        } 
                    }
                }
                Console.ReadLine();

                //while (true)
                //{
                //    var state = await client.SendRequestAsync<TLState>(new TLRequestGetState());
                //    var req = new TLRequestGetDifference() { Date = state.Date, Pts = state.Pts, Qts = state.Qts };
                //    var diff = await client.SendRequestAsync<TLAbsDifference>(req) as TLDifference;
                //    if(diff != null)
                //    {
                //        var channel = diff.Chats.OfType<TLChannel>().FirstOrDefault(x => x.Title.ToUpper().Contains("ANGEL"));

                //        if (channel != null)
                //        {
                //            foreach (var upd in diff.OtherUpdates.OfType<TLUpdateNewChannelMessage>())
                //            {
                //                Console.WriteLine((upd.Message as TLMessage).Message);
                //                Console.WriteLine("\n --------------------------------------------- \n");
                //            }
                                

                //            foreach (var ch in diff.Chats.OfType<TLChannel>().Where(x => !x.Left))
                //            {
                //                var ich = new TLInputChannel() { ChannelId = ch.Id, AccessHash = (long)ch.AccessHash };
                //                var readed = new TeleSharp.TL.Channels.TLRequestReadHistory() { Channel = ich, MaxId = -1 };
                //                await client.SendRequestAsync<bool>(readed);
                //            } 
                //        }
                //    }
                //    //await Task.Delay(200);
                //}
            }
        }

        public async static void ForwardMessage(TelegramClient client, int userId, int chatId, long hashCode ,int messageIdInSourceContactToForward)
        {
            // normal Group
            var sourcePeer = new TLInputPeerChat { ChatId = chatId };
            var targetPeer = new TLInputPeerUser { UserId = userId, AccessHash = hashCode };

            // random Ids to prevent bombinggg
            var randomIds = new TLVector<long>
            {
                TLSharp.Core.Utils.Helpers.GenerateRandomLong()
            };

            // source messages
            var sourceMessageIds = new TLVector<int>
            {
                messageIdInSourceContactToForward	// this ID should be in the SourcePeer's Messages
            };

            var forwardRequest = new TLRequestForwardMessages()
            {
                FromPeer = sourcePeer,
                Id = sourceMessageIds,
                ToPeer = targetPeer,
                RandomId = randomIds,
                //Silent = false
            };

            var result = await client.SendRequestAsync<TLUpdates>(forwardRequest);

            //var req = new TLRequestForwardMessage()
            //{
            //    //Id = message.Id,
            //    Peer = new TLInputPeerUser() { UserId = userId, AccessHash = (long)hashCode },
            //    RandomId = Helpers.GenerateRandomLong(),
            //    MessageId = messageIdInSourceContactToForward
            //};

            //var r = await client.SendRequestAsync<TLAbsUpdates>(req);

            //var result = await client.SendRequestAsync<TLAbsUpdates>(new TLRequestForwardMessage() 
            //{ 
            //    Id = messageIdInSourceContactToForward, 
            //    Peer = new TLInputPeerChat() { ChatId = chatId }, 
            //    RandomId = Helpers.GenerateRandomLong(), 
            //});

            #region full code
            ///* e.g you can use TLInputPeerUser, TLInputPeerChat, TLInputPeerChannel here as an SourcePeer */
            //// a Person
            //var sourcePeer = new TLInputPeerUser { UserId = <<< USER.ID >>>, AccessHash = <<< USER.AccessHash >>> };

            //// normal Group
            ////var sourcePeer = new TLInputPeerChat { ChatId = <<<USER.ID>>> };

            //// SuperGroup or Channel
            ////var sourcePeer = new TLInputPeerChannel { ChatId = <<<USER.ID>>> , AccessHash = <<<USER.AccessHash>>> };


            ///* e.g you can use TLInputPeerUser, TLInputPeerChat, TLInputPeerChannel here as an SourcePeer */
            //// a Person
            ////var targetPeer = new TLInputPeerUser { UserId = <<<USER.ID>>>, AccessHash = <<<USER.AccessHash>>> };

            //// normal Group
            //var targetPeer = new TLInputPeerChat { ChatId = <<< USER.ID >>> };

            //// SuperGroup or Channel
            ////var targetPeer = new TLInputPeerChannel { ChatId = <<<USER.ID>>> , AccessHash = <<<USER.AccessHash>>> };

            //// random Ids to prevent bombinggg
            //var randomIds = new TLVector<long>
            //{
            //    TLSharp.Core.Utils.Helpers.GenerateRandomLong()
            //};

            //// source messages
            //var sourceMessageIds = new TLVector<int>
            //{
            //    messageIdInSourceContactToForward	// this ID should be in the SourcePeer's Messages
            //};

            //var forwardRequest = new TLRequestForwardMessages()
            //{
            //    FromPeer = sourcePeer,
            //    Id = sourceMessageIds,
            //    ToPeer = targetPeer,
            //    RandomId = randomIds,
            //    //Silent = false
            //};

            //var result = await myTelegramClient.SendRequestAsync<TLUpdates>(forwardRequest); 
            #endregion
        }
    }
}
