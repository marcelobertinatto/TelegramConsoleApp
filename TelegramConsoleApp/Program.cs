using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TelegramConsoleApp.Model;
using TLSchema;
using TLSchema.Messages;
using TLSharp;


namespace TelegramConsoleApp
{
    public class MainClass
    {
        private const string hash = "7434fddacf9bd9ffff7389d36a899b0a";
        private const string userNumber = "+353833146370";
        private static string fullPathToDat = Directory.GetCurrentDirectory() + "\\session.dat";
        public static async System.Threading.Tasks.Task Main(string[] args)
        {
            //var client = new TelegramClient(2954623, hash);
            var client = new TelegramClient(2954623, hash, new FileSessionStore(), fullPathToDat);
            var signalsList = new List<Signal>();
            var sentSignalsList = new List<Signal>();
            //int seconds = 0;
            
            await client.ConnectAsync();

            if (!client.IsUserAuthorized())
            {
                var hashCode = await client.SendCodeRequestAsync(userNumber);
                Console.WriteLine("Type telegram code");
                var telegramCode = Console.ReadLine();

                var user = await client.MakeAuthAsync(userNumber, hashCode, telegramCode); 
            }
            //else
            //{
                var store = new FileSessionStore();
                TelegramClient clientAccess = new TelegramClient(2954623, hash, store, fullPathToDat);
                await clientAccess.ConnectAsync();

                if (clientAccess.IsUserAuthorized())
                {
                    while (true)
                    {
                        //while (seconds < 20 && seconds != 0)
                        //{
                        //    await Task.Delay(2000);
                        //    seconds += 2;
                        //}
                        var dialogs = await clientAccess.GetUserDialogsAsync() as TLDialogs;
                        //seconds = 0;

                        foreach (var dia in dialogs.Dialogs.Where(x => x.Peer is TLPeerChannel && x.UnreadCount > 0))
                        {
                            var peer = dia.Peer as TLPeerChannel;
                            //var userPeer = dia.Peer as TLPeerUser;
                            //var userChat = dialogs.Users.OfType<TLUser>().First(x => x.Id == userPeer.UserId);
                            var chat = dialogs.Chats.OfType<TLChannel>().FirstOrDefault(x => x.Id == peer.ChannelId
                                                                                        && x.Title.ToUpper().Contains("BOT MISTER X 24H"));
                            if (chat != null)
                            {
                                var target = new TLInputPeerChannel() { ChannelId = chat.Id, AccessHash = (long)chat.AccessHash };
                                var hist = await clientAccess.GetHistoryAsync(target, 0, -1, dia.UnreadCount);

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
                                            var regxSpecialCharac = new Regex(@"[^0-9azA-Z:,%\/]");
                                            if (regxSpecialCharac.IsMatch(result[i]))
                                            {
                                                result = result.Where((source, index) => index != i).ToArray();
                                            }
                                        }
                                    }

                                    if (result.Length > 20 && !result[3].Contains("PARCIAL"))
                                    {
                                        var signal = new Signal();
                                        signal.MessageId = Convert.ToInt64(me.Id);
                                        signal.Currency = result[0].ToString().Equals("|") ? result[6].ToString() : result[0].ToString();
                                        signal.CurrencyTime = result[1].ToString();
                                        signal.Time = result[2].ToString();
                                        signal.CurrencySignal = result[3].ToString();
                                        signal.Date = result[4].ToString();
                                        signal.CurrencyAssertPercentage1 = result[12].ToString();
                                        signal.CurrencyAssertPercentage2 = result[14].ToString();
                                        signal.CurrencyAssertPercentage3 = result[16].ToString();
                                        signal.BackTest = GetBackTest(result);

                                        if (signalsList.FirstOrDefault(x => x.MessageId == signal.MessageId) == null)
                                        {
                                            signalsList.Add(signal);
                                        }
                                    }
                                    //mark message as read
                                    //else
                                    //{
                                    //    MarkMessageAsRead(clientAccess, chat.Id, (long)chat.AccessHash, (int)me.Id, dia.UnreadCount);
                                    //}
                                    //ForwardMessage(clientAccess, 1079068893, chat.Id, -4463481739700017704, me.Id);
                                    //Console.WriteLine((m as TLMessage).Message);
                                }
                            }

                            var chat2 = dialogs.Chats.OfType<TLChannel>().FirstOrDefault(x => x.Title.ToUpper().Contains("TESTE"));
                            if (chat2 != null)
                            {
                                var list = signalsList.Where(x => !sentSignalsList.Any(y => y.MessageId == x.MessageId)).OrderBy(x => x.MessageId).ToList();
                                foreach (var item in list)
                                {
                                    var currentDate = DateTime.Parse(item.Date).Date;
                                    var time = DateTime.Now.AddHours(-3);//.AddMinutes(5);
                                    var signalTime = new DateTime(Convert.ToInt32(item.Date.Substring(6, 4)),
                                                        Convert.ToInt32(item.Date.Substring(3, 2)),
                                                        Convert.ToInt32(item.Date.Substring(0, 2)),
                                                        Convert.ToInt32(item.CurrencyTime.Substring(0, 2)),
                                                        Convert.ToInt32(item.CurrencyTime.Substring(3, 2)), 00);
                                    var percentage = Decimal.Round(decimal.Parse(item.CurrencyAssertPercentage1.Replace("%", string.Empty).Replace(",", ".")), 2);
                                    if (percentage >= 60.0m && time.Date == currentDate)
                                    {
                                        if (signalTime >= time && item.BackTest.Values.ElementAt(0).ToUpper().Contains("WIN S/ GALE")
                                                               && item.BackTest.Values.ElementAt(1).ToUpper().Contains("WIN S/ GALE"))
                                        {
                                            var squareColor = item.CurrencySignal.Equals("CALL") ? "🟩 " + item.CurrencySignal + "\n\n"
                                                                            : "🟥 " + item.CurrencySignal + "\n\n";

                                            await clientAccess.SendMessageAsync(new TLInputPeerChannel()
                                            {
                                                ChannelId = chat2.Id,
                                                AccessHash = chat2.AccessHash.Value
                                            },
                                                            string.Format("--- {0} ---\n" +
                                                                "🇧🇷 ANGEL SIGNALS 🇧🇷\n" +
                                                                "   🇨🇮 TRADER X 🇨🇮\n" +
                                                                "================\n" +
                                                                "💰 {1}\n" +
                                                                "⏰ {2}\n" +
                                                                "⏳ {3}\n" +
                                                                "{4}" +
                                                                "Sinal até gale 1."
                                                                , item.Date, item.Currency
                                                                , item.Time, item.CurrencyTime.Replace(",", ""), squareColor));

                                            Console.WriteLine("=====================================================================");
                                            Console.WriteLine(string.Format("--- {0} ---\n" +
                                                                "🇧🇷 ANGEL SIGNALS 🇧🇷\n" +
                                                                "   🇨🇮 TRADER X 🇨🇮\n" +
                                                                "================\n" +
                                                                "💰 {1}\n" +
                                                                "⏰ {2}\n" +
                                                                "⏳ {3}\n" +
                                                                "{4}" +
                                                                "Sinal até gale 1."
                                                                , item.Date, item.Currency
                                                                , item.Time, item.CurrencyTime.Replace(",", ""), squareColor));

                                            //mark message as read
                                            //MarkMessageAsRead(clientAccess, chat.Id, (long)chat.AccessHash, (int)item.MessageId, dia.UnreadCount);
                                        }
                                        //mark message as read
                                        //else
                                        //{
                                        //    MarkMessageAsRead(clientAccess, chat.Id, (long)chat.AccessHash, (int)item.MessageId, dia.UnreadCount);
                                        //}
                                    }
                                    //mark message as read
                                    //else
                                    //{
                                    //    MarkMessageAsRead(clientAccess, chat.Id, (long)chat.AccessHash, (int)item.MessageId, dia.UnreadCount);
                                    //}
                                    sentSignalsList.Add(item);

                                    //await Task.Delay(500);
                                    //seconds += 1;                                
                                }
                            }
                        }
                        await Task.Delay(30000);
                    }
                }
            //}
        }

        public async static void MarkMessageAsRead(TelegramClient client, int chatId, long hashCode, int messageId, int unreadCount)
        {
            //var ch = new TLInputChannel() { ChannelId = chatId, AccessHash = hashCode };
            //var targetPeer = new TLInputPeerUser { UserId = chatId, AccessHash = hashCode };

            //var markAsRead = new TeleSharp.TL.Channels.TLRequestReadHistory()
            //{
            //    Channel = ch,
            //    MaxId = -1,
            //    Dirty = true,
            //    MessageId = messageId,
            //    ConfirmReceived = true,
            //    Sequence = unreadCount
            //};
            //var readed = await client.SendRequestAsync<bool>(markAsRead);

            //var a = new TeleSharp.TL.Channels.TLRequestReadHistory()
            //{
            //    Channel = ch
            //};
            //var affectedMessages = await client.SendRequestAsync<TLAffectedMessages>(a);
        }

        public static Dictionary<DateTime, string> GetBackTest(string[] result)
        {
            if (result[19].Equals("HIT"))
            {
                if (result[21].Equals("HIT"))
                {
                    if (result[23].Equals("HIT"))
                    {
                        if (result[25].Equals("HIT"))
                        {
                            if (result[27].Equals("HIT"))
                            {
                                return new Dictionary<DateTime, string>
                                        {
                                            { new DateTime(2021, int.Parse(result[18].Substring(3, 2)), int.Parse(result[18].Substring(0, 2))), string.Format("{0}", result[19]) },
                                            { new DateTime(2021, int.Parse(result[20].Substring(3, 2)), int.Parse(result[20].Substring(0, 2))), string.Format("{0}", result[21]) },
                                            { new DateTime(2021, int.Parse(result[22].Substring(3, 2)), int.Parse(result[22].Substring(0, 2))), string.Format("{0}", result[23]) },
                                            { new DateTime(2021, int.Parse(result[24].Substring(3, 2)), int.Parse(result[24].Substring(0, 2))), string.Format("{0}", result[25]) },
                                            { new DateTime(2021, int.Parse(result[26].Substring(3, 2)), int.Parse(result[26].Substring(0, 2))), string.Format("{0}", result[27]) }
                                        };

                            }
                            else
                            {
                                return new Dictionary<DateTime, string>
                                        {
                                            { new DateTime(2021, int.Parse(result[18].Substring(3, 2)), int.Parse(result[18].Substring(0, 2))), string.Format("{0}", result[19]) },
                                            { new DateTime(2021, int.Parse(result[20].Substring(3, 2)), int.Parse(result[20].Substring(0, 2))), string.Format("{0}", result[21]) },
                                            { new DateTime(2021, int.Parse(result[22].Substring(3, 2)), int.Parse(result[22].Substring(0, 2))), string.Format("{0}", result[23]) },
                                            { new DateTime(2021, int.Parse(result[24].Substring(3, 2)), int.Parse(result[24].Substring(0, 2))), string.Format("{0}", result[25]) },
                                            { new DateTime(2021, int.Parse(result[26].Substring(3, 2)), int.Parse(result[26].Substring(0, 2))), string.Format("{0} {1} {2}", result[27], result[28], result[29]) }
                                        };
                            }
                        }
                        else
                        {
                            return new Dictionary<DateTime, string>
                                    {
                                        { new DateTime(2021, int.Parse(result[18].Substring(3, 2)), int.Parse(result[18].Substring(0, 2))), string.Format("{0}", result[19]) },
                                        { new DateTime(2021, int.Parse(result[20].Substring(3, 2)), int.Parse(result[20].Substring(0, 2))), string.Format("{0}", result[21]) },
                                        { new DateTime(2021, int.Parse(result[22].Substring(3, 2)), int.Parse(result[22].Substring(0, 2))), string.Format("{0}", result[23]) },
                                        { new DateTime(2021, int.Parse(result[24].Substring(3, 2)), int.Parse(result[24].Substring(0, 2))), string.Format("{0} {1} {2}", result[25], result[26], result[27]) },
                                        { new DateTime(2021, int.Parse(result[28].Substring(3, 2)), int.Parse(result[28].Substring(0, 2))), string.Format("{0} {1} {2}", result[29], result[30], result[31]) }
                                    };
                        }
                    }
                    else
                    {
                        return new Dictionary<DateTime, string>
                                {
                                    { new DateTime(2021, int.Parse(result[18].Substring(3, 2)), int.Parse(result[18].Substring(0, 2))), string.Format("{0}", result[19]) },
                                    { new DateTime(2021, int.Parse(result[20].Substring(3, 2)), int.Parse(result[20].Substring(0, 2))), string.Format("{0}", result[21]) },
                                    { new DateTime(2021, int.Parse(result[22].Substring(3, 2)), int.Parse(result[22].Substring(0, 2))), string.Format("{0} {1} {2}", result[23], result[24], result[25]) },
                                    { new DateTime(2021, int.Parse(result[26].Substring(3, 2)), int.Parse(result[26].Substring(0, 2))), string.Format("{0} {1} {2}", result[27], result[28], result[29]) },
                                    { new DateTime(2021, int.Parse(result[30].Substring(3, 2)), int.Parse(result[30].Substring(0, 2))), string.Format("{0} {1} {2}", result[31], result[32], result[33]) }
                                };
                    }
                }
                else
                {
                    if (result[25].Equals("HIT"))
                    {
                        if (result[27].Equals("HIT"))
                        {
                            if (result[29].Equals("HIT"))
                            {
                                return new Dictionary<DateTime, string>
                                {
                                    { new DateTime(2021, int.Parse(result[18].Substring(3, 2)), int.Parse(result[18].Substring(0, 2))), string.Format("{0}", result[19]) },
                                    { new DateTime(2021, int.Parse(result[20].Substring(3, 2)), int.Parse(result[20].Substring(0, 2))), string.Format("{0} {1} {2}", result[21], result[22], result[23]) },
                                    { new DateTime(2021, int.Parse(result[24].Substring(3, 2)), int.Parse(result[24].Substring(0, 2))), string.Format("{0}", result[25]) },
                                    { new DateTime(2021, int.Parse(result[26].Substring(3, 2)), int.Parse(result[26].Substring(0, 2))), string.Format("{0}", result[27]) },
                                    { new DateTime(2021, int.Parse(result[28].Substring(3, 2)), int.Parse(result[28].Substring(0, 2))), string.Format("{0}", result[29]) }
                                };
                            }
                            else
                            {
                                return new Dictionary<DateTime, string>
                                {
                                    { new DateTime(2021, int.Parse(result[18].Substring(3, 2)), int.Parse(result[18].Substring(0, 2))), string.Format("{0}", result[19]) },
                                    { new DateTime(2021, int.Parse(result[20].Substring(3, 2)), int.Parse(result[20].Substring(0, 2))), string.Format("{0} {1} {2}", result[21], result[22], result[23]) },
                                    { new DateTime(2021, int.Parse(result[24].Substring(3, 2)), int.Parse(result[24].Substring(0, 2))), string.Format("{0}", result[25]) },
                                    { new DateTime(2021, int.Parse(result[26].Substring(3, 2)), int.Parse(result[26].Substring(0, 2))), string.Format("{0}", result[27]) },
                                    { new DateTime(2021, int.Parse(result[28].Substring(3, 2)), int.Parse(result[28].Substring(0, 2))), string.Format("{0} {1} {2}", result[29], result[30], result[31]) }
                                };
                            }
                        }
                        else
                        {
                            if (result[31].Equals("HIT"))
                            {
                                return new Dictionary<DateTime, string>
                                {
                                    { new DateTime(2021, int.Parse(result[18].Substring(3, 2)), int.Parse(result[18].Substring(0, 2))), string.Format("{0}", result[19]) },
                                    { new DateTime(2021, int.Parse(result[20].Substring(3, 2)), int.Parse(result[20].Substring(0, 2))), string.Format("{0} {1} {2}", result[21], result[22], result[23]) },
                                    { new DateTime(2021, int.Parse(result[24].Substring(3, 2)), int.Parse(result[24].Substring(0, 2))), string.Format("{0}", result[25]) },
                                    { new DateTime(2021, int.Parse(result[26].Substring(3, 2)), int.Parse(result[26].Substring(0, 2))), string.Format("{0} {1} {2}", result[27], result[28], result[29]) },
                                    { new DateTime(2021, int.Parse(result[30].Substring(3, 2)), int.Parse(result[30].Substring(0, 2))), string.Format("{0}", result[31]) }
                                };
                            }
                            else
                            {
                                return new Dictionary<DateTime, string>
                                {
                                    { new DateTime(2021, int.Parse(result[18].Substring(3, 2)), int.Parse(result[18].Substring(0, 2))), string.Format("{0}", result[19]) },
                                    { new DateTime(2021, int.Parse(result[20].Substring(3, 2)), int.Parse(result[20].Substring(0, 2))), string.Format("{0} {1} {2}", result[21], result[22], result[23]) },
                                    { new DateTime(2021, int.Parse(result[24].Substring(3, 2)), int.Parse(result[24].Substring(0, 2))), string.Format("{0}", result[25]) },
                                    { new DateTime(2021, int.Parse(result[26].Substring(3, 2)), int.Parse(result[26].Substring(0, 2))), string.Format("{0} {1} {2}", result[27], result[28], result[29]) },
                                    { new DateTime(2021, int.Parse(result[30].Substring(3, 2)), int.Parse(result[30].Substring(0, 2))), string.Format("{0} {1} {2}", result[31], result[32], result[33]) }
                                };
                            }
                        }
                    }
                    else
                    {
                        if (result[29].Equals("HIT"))
                        {
                            if (result[31].Equals("HIT"))
                            {
                                return new Dictionary<DateTime, string>
                                {
                                    { new DateTime(2021, int.Parse(result[18].Substring(3, 2)), int.Parse(result[18].Substring(0, 2))), string.Format("{0}", result[19]) },
                                    { new DateTime(2021, int.Parse(result[20].Substring(3, 2)), int.Parse(result[20].Substring(0, 2))), string.Format("{0} {1} {2}", result[21], result[22], result[23]) },
                                    { new DateTime(2021, int.Parse(result[24].Substring(3, 2)), int.Parse(result[24].Substring(0, 2))), string.Format("{0} {1} {2}", result[25], result[26], result[27]) },
                                    { new DateTime(2021, int.Parse(result[28].Substring(3, 2)), int.Parse(result[28].Substring(0, 2))), string.Format("{0}", result[29]) },
                                    { new DateTime(2021, int.Parse(result[30].Substring(3, 2)), int.Parse(result[30].Substring(0, 2))), string.Format("{0}", result[31]) }
                                };
                            }
                            else
                            {
                                return new Dictionary<DateTime, string>
                                {
                                    { new DateTime(2021, int.Parse(result[18].Substring(3, 2)), int.Parse(result[18].Substring(0, 2))), string.Format("{0}", result[19]) },
                                    { new DateTime(2021, int.Parse(result[20].Substring(3, 2)), int.Parse(result[20].Substring(0, 2))), string.Format("{0} {1} {2}", result[21], result[22], result[23]) },
                                    { new DateTime(2021, int.Parse(result[24].Substring(3, 2)), int.Parse(result[24].Substring(0, 2))), string.Format("{0} {1} {2}", result[25], result[26], result[27]) },
                                    { new DateTime(2021, int.Parse(result[28].Substring(3, 2)), int.Parse(result[28].Substring(0, 2))), string.Format("{0}", result[29]) },
                                    { new DateTime(2021, int.Parse(result[30].Substring(3, 2)), int.Parse(result[30].Substring(0, 2))), string.Format("{0} {1} {2}", result[31], result[32], result[33]) }
                                };
                            }
                        }
                        else
                        {
                            if (result[33].Equals("HIT"))
                            {
                                return new Dictionary<DateTime, string>
                                {
                                    { new DateTime(2021, int.Parse(result[18].Substring(3, 2)), int.Parse(result[18].Substring(0, 2))), string.Format("{0}", result[19]) },
                                    { new DateTime(2021, int.Parse(result[20].Substring(3, 2)), int.Parse(result[20].Substring(0, 2))), string.Format("{0} {1} {2}", result[21], result[22], result[23]) },
                                    { new DateTime(2021, int.Parse(result[24].Substring(3, 2)), int.Parse(result[24].Substring(0, 2))), string.Format("{0} {1} {2}", result[25], result[26], result[27]) },
                                    { new DateTime(2021, int.Parse(result[28].Substring(3, 2)), int.Parse(result[28].Substring(0, 2))), string.Format("{0} {1} {2}", result[29], result[30], result[31]) },
                                    { new DateTime(2021, int.Parse(result[32].Substring(3, 2)), int.Parse(result[32].Substring(0, 2))), string.Format("{0}", result[33]) }
                                };
                            }
                            else
                            {
                                return new Dictionary<DateTime, string>
                                {
                                    { new DateTime(2021, int.Parse(result[18].Substring(3, 2)), int.Parse(result[18].Substring(0, 2))), string.Format("{0}", result[19]) },
                                    { new DateTime(2021, int.Parse(result[20].Substring(3, 2)), int.Parse(result[20].Substring(0, 2))), string.Format("{0} {1} {2}", result[21], result[22], result[23]) },
                                    { new DateTime(2021, int.Parse(result[24].Substring(3, 2)), int.Parse(result[24].Substring(0, 2))), string.Format("{0} {1} {2}", result[25], result[26], result[27]) },
                                    { new DateTime(2021, int.Parse(result[28].Substring(3, 2)), int.Parse(result[28].Substring(0, 2))), string.Format("{0} {1} {2}", result[29], result[30], result[31]) },
                                    { new DateTime(2021, int.Parse(result[32].Substring(3, 2)), int.Parse(result[32].Substring(0, 2))), string.Format("{0} {1} {2}", result[33], result[34], result[35]) }
                                };
                            }
                        }
                    }
                }
            }
            else
            {
                if (result[23].Equals("HIT"))
                {
                    if (result[25].Equals("HIT"))
                    {
                        if (result[27].Equals("HIT"))
                        {
                            if (result[29].Equals("HIT"))
                            {
                                return new Dictionary<DateTime, string>
                                {
                                    { new DateTime(2021, int.Parse(result[18].Substring(3, 2)), int.Parse(result[18].Substring(0, 2))), string.Format("{0} {1} {2}",result[19], result[20], result[21])},
                                    { new DateTime(2021, int.Parse(result[22].Substring(3, 2)), int.Parse(result[22].Substring(0, 2))), string.Format("{0}",result[23])},
                                    { new DateTime(2021, int.Parse(result[24].Substring(3, 2)), int.Parse(result[24].Substring(0, 2))), string.Format("{0}",result[25])},
                                    { new DateTime(2021, int.Parse(result[26].Substring(3, 2)), int.Parse(result[26].Substring(0, 2))), string.Format("{0}",result[27])},
                                    { new DateTime(2021, int.Parse(result[28].Substring(3, 2)), int.Parse(result[28].Substring(0, 2))), string.Format("{0}",result[29])},
                                };
                            }
                            else
                            {
                                return new Dictionary<DateTime, string>
                                {
                                    { new DateTime(2021, int.Parse(result[18].Substring(3, 2)), int.Parse(result[18].Substring(0, 2))), string.Format("{0} {1} {2}",result[19], result[20], result[21])},
                                    { new DateTime(2021, int.Parse(result[22].Substring(3, 2)), int.Parse(result[22].Substring(0, 2))), string.Format("{0}",result[23])},
                                    { new DateTime(2021, int.Parse(result[24].Substring(3, 2)), int.Parse(result[24].Substring(0, 2))), string.Format("{0}",result[25])},
                                    { new DateTime(2021, int.Parse(result[26].Substring(3, 2)), int.Parse(result[26].Substring(0, 2))), string.Format("{0}",result[27])},
                                    { new DateTime(2021, int.Parse(result[28].Substring(3, 2)), int.Parse(result[28].Substring(0, 2))), string.Format("{0} {1} {2}",result[29], result[30], result[31])},
                                };
                            }
                        }
                        else
                        {
                            return new Dictionary<DateTime, string>
                            {
                                { new DateTime(2021, int.Parse(result[18].Substring(3, 2)), int.Parse(result[18].Substring(0, 2))), string.Format("{0} {1} {2}",result[19], result[20], result[21])},
                                { new DateTime(2021, int.Parse(result[22].Substring(3, 2)), int.Parse(result[22].Substring(0, 2))), string.Format("{0}",result[23])},
                                { new DateTime(2021, int.Parse(result[24].Substring(3, 2)), int.Parse(result[24].Substring(0, 2))), string.Format("{0}",result[25])},
                                { new DateTime(2021, int.Parse(result[26].Substring(3, 2)), int.Parse(result[26].Substring(0, 2))), string.Format("{0} {1} {2}",result[27], result[28], result[29])},
                                { new DateTime(2021, int.Parse(result[30].Substring(3, 2)), int.Parse(result[30].Substring(0, 2))), string.Format("{0} {1} {2}",result[31], result[32], result[33])},
                            };
                        }
                    }
                    else
                    {
                        if (result[29].Equals("HIT"))
                        {
                            if (result[31].Equals("HIT"))
                            {
                                return new Dictionary<DateTime, string>
                                {
                                    { new DateTime(2021, int.Parse(result[18].Substring(3, 2)), int.Parse(result[18].Substring(0, 2))), string.Format("{0} {1} {2}",result[19], result[20], result[21])},
                                    { new DateTime(2021, int.Parse(result[22].Substring(3, 2)), int.Parse(result[22].Substring(0, 2))), string.Format("{0}",result[23])},
                                    { new DateTime(2021, int.Parse(result[24].Substring(3, 2)), int.Parse(result[24].Substring(0, 2))), string.Format("{0} {1} {2}",result[25], result[26], result[27])},
                                    { new DateTime(2021, int.Parse(result[28].Substring(3, 2)), int.Parse(result[28].Substring(0, 2))), string.Format("{0}",result[29])},
                                    { new DateTime(2021, int.Parse(result[30].Substring(3, 2)), int.Parse(result[30].Substring(0, 2))), string.Format("{0}",result[31])},
                                };
                            }
                            else
                            {
                                return new Dictionary<DateTime, string>
                                {
                                    { new DateTime(2021, int.Parse(result[18].Substring(3, 2)), int.Parse(result[18].Substring(0, 2))), string.Format("{0} {1} {2}",result[19], result[20], result[21])},
                                    { new DateTime(2021, int.Parse(result[22].Substring(3, 2)), int.Parse(result[22].Substring(0, 2))), string.Format("{0}",result[23])},
                                    { new DateTime(2021, int.Parse(result[24].Substring(3, 2)), int.Parse(result[24].Substring(0, 2))), string.Format("{0} {1} {2}",result[25], result[26], result[27])},
                                    { new DateTime(2021, int.Parse(result[28].Substring(3, 2)), int.Parse(result[28].Substring(0, 2))), string.Format("{0}",result[29])},
                                    { new DateTime(2021, int.Parse(result[30].Substring(3, 2)), int.Parse(result[30].Substring(0, 2))), string.Format("{0} {1} {2}",result[31], result[32], result[33])},
                                };
                            }
                        }
                        else
                        {
                            if (result[33].Equals("HIT"))
                            {
                                return new Dictionary<DateTime, string>
                                {
                                    { new DateTime(2021, int.Parse(result[18].Substring(3, 2)), int.Parse(result[18].Substring(0, 2))), string.Format("{0} {1} {2}",result[19], result[20], result[21])},
                                    { new DateTime(2021, int.Parse(result[22].Substring(3, 2)), int.Parse(result[22].Substring(0, 2))), string.Format("{0}",result[23])},
                                    { new DateTime(2021, int.Parse(result[24].Substring(3, 2)), int.Parse(result[24].Substring(0, 2))), string.Format("{0} {1} {2}",result[25], result[26], result[27])},
                                    { new DateTime(2021, int.Parse(result[28].Substring(3, 2)), int.Parse(result[28].Substring(0, 2))), string.Format("{0} {1} {2}",result[29], result[30], result[31])},
                                    { new DateTime(2021, int.Parse(result[32].Substring(3, 2)), int.Parse(result[32].Substring(0, 2))), string.Format("{0}",result[33])},
                                };
                            }
                            else
                            {
                                return new Dictionary<DateTime, string>
                                {
                                    { new DateTime(2021, int.Parse(result[18].Substring(3, 2)), int.Parse(result[18].Substring(0, 2))), string.Format("{0} {1} {2}",result[19], result[20], result[21])},
                                    { new DateTime(2021, int.Parse(result[22].Substring(3, 2)), int.Parse(result[22].Substring(0, 2))), string.Format("{0}",result[23])},
                                    { new DateTime(2021, int.Parse(result[24].Substring(3, 2)), int.Parse(result[24].Substring(0, 2))), string.Format("{0} {1} {2}",result[25], result[26], result[27])},
                                    { new DateTime(2021, int.Parse(result[28].Substring(3, 2)), int.Parse(result[28].Substring(0, 2))), string.Format("{0} {1} {2}",result[29], result[30], result[31])},
                                    { new DateTime(2021, int.Parse(result[32].Substring(3, 2)), int.Parse(result[32].Substring(0, 2))), string.Format("{0} {1} {2}",result[33], result[34], result[35])},
                                };
                            }
                        }
                    }
                }
                else
                {
                    if (result[27].Equals("HIT"))
                    {
                        if (result[29].Equals("HIT"))
                        {
                            if (result[31].Equals("HIT"))
                            {
                                return new Dictionary<DateTime, string>
                                {
                                    { new DateTime(2021, int.Parse(result[18].Substring(3, 2)), int.Parse(result[18].Substring(0, 2))), string.Format("{0} {1} {2}",result[19], result[20], result[21])},
                                    { new DateTime(2021, int.Parse(result[22].Substring(3, 2)), int.Parse(result[22].Substring(0, 2))), string.Format("{0} {1} {2}",result[23], result[24], result[25])},
                                    { new DateTime(2021, int.Parse(result[26].Substring(3, 2)), int.Parse(result[26].Substring(0, 2))), string.Format("{0}",result[27])},
                                    { new DateTime(2021, int.Parse(result[28].Substring(3, 2)), int.Parse(result[28].Substring(0, 2))), string.Format("{0}",result[29])},
                                    { new DateTime(2021, int.Parse(result[30].Substring(3, 2)), int.Parse(result[30].Substring(0, 2))), string.Format("{0}",result[31])},
                                };
                            }
                            else
                            {
                                return new Dictionary<DateTime, string>
                                {
                                    { new DateTime(2021, int.Parse(result[18].Substring(3, 2)), int.Parse(result[18].Substring(0, 2))), string.Format("{0} {1} {2}",result[19], result[20], result[21])},
                                    { new DateTime(2021, int.Parse(result[22].Substring(3, 2)), int.Parse(result[22].Substring(0, 2))), string.Format("{0} {1} {2}",result[23], result[24], result[25])},
                                    { new DateTime(2021, int.Parse(result[26].Substring(3, 2)), int.Parse(result[26].Substring(0, 2))), string.Format("{0}",result[27])},
                                    { new DateTime(2021, int.Parse(result[28].Substring(3, 2)), int.Parse(result[28].Substring(0, 2))), string.Format("{0}",result[29])},
                                    { new DateTime(2021, int.Parse(result[30].Substring(3, 2)), int.Parse(result[30].Substring(0, 2))), string.Format("{0} {1} {2}",result[31], result[32], result[33])},
                                };
                            }
                        }
                        else
                        {
                            if (result[33].Equals("HIT"))
                            {
                                return new Dictionary<DateTime, string>
                                {
                                    { new DateTime(2021, int.Parse(result[18].Substring(3, 2)), int.Parse(result[18].Substring(0, 2))), string.Format("{0} {1} {2}",result[19], result[20], result[21])},
                                    { new DateTime(2021, int.Parse(result[22].Substring(3, 2)), int.Parse(result[22].Substring(0, 2))), string.Format("{0} {1} {2}",result[23], result[24], result[25])},
                                    { new DateTime(2021, int.Parse(result[26].Substring(3, 2)), int.Parse(result[26].Substring(0, 2))), string.Format("{0}",result[27])},
                                    { new DateTime(2021, int.Parse(result[28].Substring(3, 2)), int.Parse(result[28].Substring(0, 2))), string.Format("{0} {1} {2}",result[29], result[30], result[31])},
                                    { new DateTime(2021, int.Parse(result[32].Substring(3, 2)), int.Parse(result[32].Substring(0, 2))), string.Format("{0}",result[33])},
                                };
                            }
                            else
                            {
                                return new Dictionary<DateTime, string>
                                {
                                    { new DateTime(2021, int.Parse(result[18].Substring(3, 2)), int.Parse(result[18].Substring(0, 2))), string.Format("{0} {1} {2}",result[19], result[20], result[21])},
                                    { new DateTime(2021, int.Parse(result[22].Substring(3, 2)), int.Parse(result[22].Substring(0, 2))), string.Format("{0} {1} {2}",result[23], result[24], result[25])},
                                    { new DateTime(2021, int.Parse(result[26].Substring(3, 2)), int.Parse(result[26].Substring(0, 2))), string.Format("{0}",result[27])},
                                    { new DateTime(2021, int.Parse(result[28].Substring(3, 2)), int.Parse(result[28].Substring(0, 2))), string.Format("{0} {1} {2}",result[29], result[30], result[31])},
                                    { new DateTime(2021, int.Parse(result[32].Substring(3, 2)), int.Parse(result[32].Substring(0, 2))), string.Format("{0} {1} {2}",result[33], result[34], result[35])},
                                };
                            }
                        }
                    }
                    else
                    {
                        if (result[31].Equals("HIT"))
                        {
                            if (result[33].Equals("HIT"))
                            {
                                return new Dictionary<DateTime, string>
                                {
                                    { new DateTime(2021, int.Parse(result[18].Substring(3, 2)), int.Parse(result[18].Substring(0, 2))), string.Format("{0} {1} {2}",result[19], result[20], result[21])},
                                    { new DateTime(2021, int.Parse(result[22].Substring(3, 2)), int.Parse(result[22].Substring(0, 2))), string.Format("{0} {1} {2}",result[23], result[24], result[25])},
                                    { new DateTime(2021, int.Parse(result[26].Substring(3, 2)), int.Parse(result[26].Substring(0, 2))), string.Format("{0} {1} {2}",result[27], result[28], result[29])},
                                    { new DateTime(2021, int.Parse(result[30].Substring(3, 2)), int.Parse(result[30].Substring(0, 2))), string.Format("{0}",result[31])},
                                    { new DateTime(2021, int.Parse(result[32].Substring(3, 2)), int.Parse(result[32].Substring(0, 2))), string.Format("{0}",result[33])},
                                };
                            }
                            else
                            {
                                return new Dictionary<DateTime, string>
                                {
                                    { new DateTime(2021, int.Parse(result[18].Substring(3, 2)), int.Parse(result[18].Substring(0, 2))), string.Format("{0} {1} {2}",result[19], result[20], result[21])},
                                    { new DateTime(2021, int.Parse(result[22].Substring(3, 2)), int.Parse(result[22].Substring(0, 2))), string.Format("{0} {1} {2}",result[23], result[24], result[25])},
                                    { new DateTime(2021, int.Parse(result[26].Substring(3, 2)), int.Parse(result[26].Substring(0, 2))), string.Format("{0} {1} {2}",result[27], result[28], result[29])},
                                    { new DateTime(2021, int.Parse(result[30].Substring(3, 2)), int.Parse(result[30].Substring(0, 2))), string.Format("{0}",result[31])},
                                    { new DateTime(2021, int.Parse(result[32].Substring(3, 2)), int.Parse(result[32].Substring(0, 2))), string.Format("{0} {1} {2}",result[33], result[34], result[35])},
                                };
                            }
                        }
                        else
                        {
                            if (result[35].Equals("HIT"))
                            {
                                return new Dictionary<DateTime, string>
                                {
                                    { new DateTime(2021, int.Parse(result[18].Substring(3, 2)), int.Parse(result[18].Substring(0, 2))), string.Format("{0} {1} {2}",result[19], result[20], result[21])},
                                    { new DateTime(2021, int.Parse(result[22].Substring(3, 2)), int.Parse(result[22].Substring(0, 2))), string.Format("{0} {1} {2}",result[23], result[24], result[25])},
                                    { new DateTime(2021, int.Parse(result[26].Substring(3, 2)), int.Parse(result[26].Substring(0, 2))), string.Format("{0} {1} {2}",result[27], result[28], result[29])},
                                    { new DateTime(2021, int.Parse(result[30].Substring(3, 2)), int.Parse(result[30].Substring(0, 2))), string.Format("{0} {1} {2}",result[31], result[32], result[33])},
                                    { new DateTime(2021, int.Parse(result[34].Substring(3, 2)), int.Parse(result[34].Substring(0, 2))), string.Format("{0}",result[35])},
                                };
                            }
                            else
                            {
                                return new Dictionary<DateTime, string>
                                {
                                    { new DateTime(2021, int.Parse(result[18].Substring(3, 2)), int.Parse(result[18].Substring(0, 2))), string.Format("{0} {1} {2}",result[19], result[20], result[21])},
                                    { new DateTime(2021, int.Parse(result[22].Substring(3, 2)), int.Parse(result[22].Substring(0, 2))), string.Format("{0} {1} {2}",result[23], result[24], result[25])},
                                    { new DateTime(2021, int.Parse(result[26].Substring(3, 2)), int.Parse(result[26].Substring(0, 2))), string.Format("{0} {1} {2}",result[27], result[28], result[29])},
                                    { new DateTime(2021, int.Parse(result[30].Substring(3, 2)), int.Parse(result[30].Substring(0, 2))), string.Format("{0} {1} {2}",result[31], result[32], result[33])},
                                    { new DateTime(2021, int.Parse(result[34].Substring(3, 2)), int.Parse(result[34].Substring(0, 2))), string.Format("{0} {1} {2}",result[35], result[36], result[37])},
                                };
                            }
                        }
                    }
                }
            }
        }

        public static DateTime RoundUp(DateTime dt, TimeSpan d)
        {
            var modTicks = dt.Ticks % d.Ticks;
            var delta = modTicks != 0 ? d.Ticks - modTicks : 0;
            return new DateTime(dt.Ticks + delta, dt.Kind);
        }

        public async static void ForwardMessage(TelegramClient client, int userId, int chatId, long hashCode, int messageIdInSourceContactToForward)
        {
            //// normal Group
            //var sourcePeer = new TLInputPeerChat { ChatId = chatId };
            //var targetPeer = new TLInputPeerUser { UserId = userId, AccessHash = hashCode };

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

            //var result = await client.SendRequestAsync<TLUpdates>(forwardRequest);

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
