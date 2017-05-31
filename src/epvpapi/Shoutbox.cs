﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using epvpapi.Connection;
using epvpapi.Evaluation;
using HtmlAgilityPack;

namespace epvpapi
{
    /// <summary>
    /// Represents the shoutbox accessable by premium users, level2 + level3 users and the staff
    /// </summary>
    public static class Shoutbox
    {
        /// <summary>
        /// Themed chat-channel of the shoutbox where messages can be stored, send and received. 
        /// </summary>
        public class Channel : UniqueObject
        {
            /// <summary>
            /// Single shout send by an user
            /// </summary>
            public class Shout : Message
            {
                public Shout(int id = 0)
                    : this(id, new Content())
                { }

                public Shout(Content content)
                    : this(0, content)
                { }

                public Shout(int id, Content content)
                    : base(id, content)
                { }
            }

            public string Name { get; set; }

            public Channel(int id, string name) :
                base(id)
            {
                Name = name;
            }

            /// <summary>
            /// Sends a message to the channel
            /// </summary>
            /// <param name="session"> Session used for sending the request </param>
            /// <param name="message"> The message text to send </param>
            public void Send<TUser>(AuthenticatedSession<TUser> session, string message) where TUser : User
            {
                session.ThrowIfInvalid();

                session.Post("https://www.elitepvpers.com/forum/mgc_cb_evo_ajax.php",
                            new List<KeyValuePair<string, string>>()
                            {
                                new KeyValuePair<string, string>("do", "ajax_chat"),
                                new KeyValuePair<string, string>("channel_id", ID.ToString()),
                                new KeyValuePair<string, string>("chat", message),
                                new KeyValuePair<string, string>("securitytoken", session.SecurityToken),
                                new KeyValuePair<string, string>("s", String.Empty)
                            });
            }

            /// <summary>
            /// Updates the most recent shouts usually displayed when loading the main page 
            /// </summary>
            /// <param name="session"> Session used for sending the request </param>
            public List<Shout> Shouts<TUser>(AuthenticatedSession<TUser> session) where TUser : User
            {
                session.ThrowIfInvalid();

                var res = session.Post("https://www.elitepvpers.com/forum/mgc_cb_evo_ajax.php",
                                            new List<KeyValuePair<string, string>>
                                            {
                                                new KeyValuePair<string, string>("do", "ajax_refresh_chat"),
                                                new KeyValuePair<string, string>("status", "open"),
                                                new KeyValuePair<string, string>("channel_id", ID.ToString()),
                                                new KeyValuePair<string, string>("location", "inc"),
                                                new KeyValuePair<string, string>("first_load", "0"),
                                                new KeyValuePair<string, string>("securitytoken", session.SecurityToken),
                                                new KeyValuePair<string, string>("securitytoken", session.SecurityToken), // for some reason, the security token is send twice
                                                new KeyValuePair<string, string>("s", String.Empty),
                                            });

                var shouts = new List<Shout>();

                try
                {
                    var doc = new HtmlDocument();
                    doc.LoadHtml(res);

                    // every shoutbox entry got 3 td nodes. One for the time, one for the username and one for the actual messages
                    // the target nodes are identified by their unique valign: top attribute
                    var tdNodes = new List<HtmlNode>(doc.DocumentNode.Descendants("td"));
                    var shoutboxNodes = new List<HtmlNode>(tdNodes.Where(node => node.Attributes.Any(attribute => attribute.Name == "valign" && attribute.Value == "top")));

                    var shoutboxNodeGroups = shoutboxNodes.Split(3);
                    foreach (var shoutboxNodeGroup in shoutboxNodeGroups)
                    {
                        if (shoutboxNodeGroup.Count != 3) continue; // every node group needs to have exactly 3 nodes in order to be valid
                        var parsedShout = new Shout();

                        var time = new DateTime();
                        var timeNode = shoutboxNodeGroup.ElementAt(0).SelectSingleNode(@"span[1]/span[1]");

                        if (timeNode != null)
                        {
                            Match match = new Regex(@"\s*(\S+)&nbsp;").Match(timeNode.InnerText);
                            string matchedTime = match.Groups.Count > 1 ? match.Groups[1].Value : String.Empty;
                            DateTime.TryParse(matchedTime, out time);
                        }

                        parsedShout.Date = time;

                        var userNameNode = shoutboxNodeGroup.ElementAt(1).SelectSingleNode(@"span[1]/a[1]/span[1]") ??
                                           shoutboxNodeGroup.ElementAt(1).SelectSingleNode(@"span[1]/a[1]"); // users with black names do not have the span element

                        if (userNameNode == null) continue;
                        parsedShout.Sender.Name = userNameNode.InnerText;
                        new UserParser.NamecolorParser(parsedShout.Sender).Execute(userNameNode);

                        var userLinkNode = shoutboxNodeGroup.ElementAt(1).SelectSingleNode(@"span[1]/a[1]");
                        parsedShout.Sender.ID = (userLinkNode != null) ? userLinkNode.Attributes.Contains("href") ? User.FromUrl(userLinkNode.Attributes["href"].Value) : 0 : 0;

                        var messageNode = shoutboxNodeGroup.ElementAt(2).SelectSingleNode(@"span[1]");
                        // strip the leading and trailing whitespaces of every shout
                        messageNode.InnerHtml = messageNode.InnerHtml.Strip();

                        new ContentParser(parsedShout.Content.Elements).Execute(messageNode);

                        shouts.Add(parsedShout);
                    }
                }
                catch (HtmlWebException exception)
                {
                    throw new ParsingFailedException("Parsing recent shouts from response content failed", exception);
                }

                return shouts;
            }

            /// <summary>
            /// Fetches the history of the specified shoutbox channel and returns all shouts that have been stored
            /// </summary>
            /// <param name="firstPage"> Index of the first page to fetch </param>
            /// <param name="pageCount"> Amount of pages to get. The higher this count, the more data will be generated and received </param>
            /// <param name="session"> Session used for sending the request </param>
            /// <param name="updateShoutbox"> When set to true, additional shoutbox information will be updated on the fly. This does not cause any major
            /// resources to be used since the information can be parsed from the same <c>HtmlDocument</c> as the channel history </param>
            /// <returns> Shouts listed in the channel history that could be obtained and parsed </returns>
            public List<Shout> History<TUser>(AuthenticatedSession<TUser> session, uint pageCount = 1, uint firstPage = 1, bool updateShoutbox = true) where TUser : PremiumUser
            {
                session.ThrowIfInvalid();

                var shoutList = new List<Shout>();
                for (int i = 0; i < pageCount; ++i)
                {
                    var res = session.Get("https://www.elitepvpers.com/forum/mgc_cb_evo.php?do=view_archives&page=" + (firstPage + i));

                    var doc = new HtmlDocument();
                    doc.LoadHtml(res);

                    var messagesRootNode = doc.GetElementbyId("tickerwrapper").SelectSingleNode("../table[2]/tr[1]/td[3]/table[1]");
                    if (messagesRootNode == null) throw new ParsingFailedException("Parsing channel history failed, root node is invalid or was not found");

                    var messageNodes = new List<HtmlNode>(messagesRootNode.ChildNodes.GetElementsByTagName("tr"));
                    if (messageNodes.Count < 1) throw new ParsingFailedException("Parsing channel history failed, message nodes could not be retrieved");
                    messageNodes.RemoveAt(0); // remove the table header

                    foreach (var messageNode in messageNodes)
                    {
                        var parsedShout = new Shout();

                        var subNodes = new List<HtmlNode>(messageNode.ChildNodes.GetElementsByTagName("td"));
                        if (subNodes.Count != 4) continue; // every message node got exactly 4 subnodes where action, date, user and message are stored

                        var dateNode = messageNode.SelectSingleNode("td[2]/span[1]");
                        var time = new DateTime();
                        if (dateNode != null)
                            DateTime.TryParse(dateNode.InnerText, out time);

                        parsedShout.Date = time;

                        var userNode = messageNode.SelectSingleNode("td[3]/span[1]/a[1]");
                        if (userNode == null) continue;

                        var userNameNode = userNode.SelectSingleNode("span[1]") ??
                                           userNode; // users with black names do not have the span element

                        parsedShout.Sender.Name = userNameNode.InnerText;
                        parsedShout.Sender.ID = PremiumUser.FromUrl(userNode.Attributes["href"].Value);

                        var textNode = messageNode.SelectSingleNode("td[4]/span[1]");
                        new ContentParser(parsedShout.Content.Elements).Execute(textNode);

                        shoutList.Add(parsedShout);
                    }
                }

                if (updateShoutbox)
                    Update(session);

                return shoutList;
            }

        };

        /// <summary>
        /// Contains the Top 10 chatters of all channels
        /// </summary>
        public static List<PremiumUser> TopChatter { get; set; }

        /// <summary>
        /// Amount of messages stored in all shoutbox channels
        /// </summary>
        public static uint MessageCount { get; set; }

        /// <summary>
        /// Amount of messages stored within the last 24 hours in all shoutbox channels
        /// </summary>
        public static uint MessageCountCurrentDay { get; set; }

        private static Channel _Global = new Channel(0, "General");
        public static Channel Global
        {
            get { return _Global; }
            set { _Global = value; }
        }

        private static Channel _EnglishOnly = new Channel(1, "EnglishOnly");
        public static Channel EnglishOnly
        {
            get { return _EnglishOnly; }
            set { _EnglishOnly = value; }
        }

        /// <summary>
        /// Updates statistics and information about the shoutbox
        /// </summary>
        /// <param name="session"> Session used for storing personal shoutbox data into the session user field </param>
        public static void Update<TUser>(AuthenticatedSession<TUser> session) where TUser : PremiumUser
        {
            session.ThrowIfInvalid();

            var res = session.Get("https://www.elitepvpers.com/forum/mgc_cb_evo.php?do=view_archives&page=1");
            var document = new HtmlDocument();
            document.LoadHtml(res);

            var statsBodyNode = document.GetElementbyId("tickerwrapper").SelectSingleNode("../table[2]/tr[1]/td[1]/table[1]");
            if (statsBodyNode == null) throw new ParsingFailedException("Updating the shoutbox information failed, root node is invalid or was not found");

            var chatStatsNodes = new List<HtmlNode>(statsBodyNode.Descendants("tr"));
            if (chatStatsNodes.Count < 1) throw new ParsingFailedException("Updating the shoutbox information failed, no chat nodes have been found");
            chatStatsNodes.RemoveAt(0);

            TopChatter = new List<PremiumUser>();
            var topChatterNodes = chatStatsNodes.GetRange(0, 10); // always 10 nodes
            foreach (var node in topChatterNodes)
            {
                var userNode = node.SelectSingleNode("td[1]/a[1]");
                if (userNode == null) continue;
                var userID = userNode.Attributes.Contains("href") ? User.FromUrl(userNode.Attributes["href"].Value) : 0;

                var userNameNode = userNode.SelectSingleNode("span[1]") ?? userNode; // black user names do not have a span element
                var userName = userNameNode.InnerText;

                var chatCountNode = node.SelectSingleNode("td[2]");
                var chatCount = (chatCountNode != null) ? chatCountNode.InnerText.To<uint>() : 0;

                TopChatter.Add(new PremiumUser(userName, userID) { ShoutboxMessages = chatCount });
            }

            var additionalInfoNodes = chatStatsNodes.GetRange(11, 3); // 11 because we omit the drawing "Additional information"
            if (additionalInfoNodes.Count != 3) return; // return on mismatch, no exception

            var totalMessagesValueNode = additionalInfoNodes.ElementAt(0).SelectSingleNode("td[2]");
            MessageCount = (totalMessagesValueNode != null) ? totalMessagesValueNode.InnerText.To<uint>() : 0;

            var totalMessages24HoursValueNode = additionalInfoNodes.ElementAt(1).SelectSingleNode("td[2]");
            MessageCountCurrentDay = (totalMessages24HoursValueNode != null) ? totalMessages24HoursValueNode.InnerText.To<uint>() : 0;

            var ownMessagesValueNode = additionalInfoNodes.ElementAt(2).SelectSingleNode("td[2]");
            session.User.ShoutboxMessages = (ownMessagesValueNode != null) ? ownMessagesValueNode.InnerText.To<uint>() : 0;
        }
    }
}
