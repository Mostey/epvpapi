﻿using System;
using System.Collections.Generic;
using System.Linq;
using epvpapi.Connection;
using epvpapi.Evaluation;
using HtmlAgilityPack;

namespace epvpapi
{
    public class PrivateMessage : Post, IReportable, IInternalUpdatable, IUniqueWebObject
    {
        /// <summary>
        /// Additional options that can be set when posting messages
        /// </summary>
        [Flags]
        public new enum Settings
        {
            /// <summary>
            /// If set, all URLs in the message are going to be parsed
            /// </summary>
            ParseUrl = 1,

            /// <summary>
            /// If set, the signature of the logged in user will be displayed beneath the message
            /// </summary>
            ShowSignature = 2,

            /// <summary>
            /// If set, every time a message will be send, a copy will be saved and stored somewhere
            /// </summary>
            SaveCopy = 3
        }

        /// <summary>
        /// Recipients of the message
        /// </summary>
        public List<User> Recipients { get; set; }

        /// <summary>
        /// If true, the message is marked as new message that wasn't read yet
        /// </summary>
        public bool Unread { get; set; }


        public PrivateMessage(int id)
            : this(id, new Content(), new List<User>())
        { }

        public PrivateMessage(User recipient, Content content, string title = null)
            : this(0, content, new List<User>() { recipient }, title)
        { }

        public PrivateMessage(List<User> recipients, Content content, string title = null)
            : this(0, content, recipients, title)
        { }

        public PrivateMessage(int id, Content content, List<User> recipients, string title = null)
            : base(id, content, title)
        {
            Recipients = recipients;
        }

        /// <summary>
        /// Sends a <c>PrivateMessage</c> using the given session
        /// </summary>
        /// <param name="session"> Session that is used for sending the request </param>
        /// <param name="settings"> Additional options that can be set </param>
        /// <remarks>
        /// The names of the recipients have to be given in order to send the message.
        /// Messages with a blank title will not be send. Therefore, '-' will be used as title if nothing was specified.
        /// Certain requirements must be fulfilled in order to send messages automatically without entering a captcha:
        /// - More than 20 posts OR the <c>User.Usergroup.Premium</c> rank OR the <c>User.Usergroup.EliteGoldTrader</c> rank
        /// </remarks>
        public void Send<TUser>(AuthenticatedSession<TUser> session, Settings settings = Settings.ParseUrl | Settings.ShowSignature) where TUser : User
        {
            session.ThrowIfInvalid();
            if (session.User.Posts <= 20 && !session.User.HasRank(Usergroup.Premium) && !session.User.HasRank(Usergroup.EliteGoldTrader))
                throw new InsufficientAccessException("More than 20 posts or the premium / elite*gold trader badge is required for sending private messages without captchas");

            foreach (var splittedRecipientList in Recipients.Split(5))
            {
                var recipients = String.Join(";", splittedRecipientList.Select(recipient => recipient.Name));

                session.Post("https://www.elitepvpers.com/forum/private.php?do=insertpm&pmid=",
                            new List<KeyValuePair<string, string>>()
                            {
                                new KeyValuePair<string, string>("recipients", recipients),
                                new KeyValuePair<string, string>("bccrecipients", String.Empty),
                                new KeyValuePair<string, string>("title", String.IsNullOrEmpty(Title) ? "-" : Title),
                                new KeyValuePair<string, string>("message", Content.ToString()),
                                new KeyValuePair<string, string>("wysiwyg", "0"),
                                new KeyValuePair<string, string>("iconid", "0"),
                                new KeyValuePair<string, string>("s", String.Empty),
                                new KeyValuePair<string, string>("securitytoken", session.SecurityToken),
                                new KeyValuePair<string, string>("do", "insertpm"),
                                new KeyValuePair<string, string>("pmid", String.Empty),
                                new KeyValuePair<string, string>("forward", String.Empty),
                                new KeyValuePair<string, string>("sbutton", "submit"),
                                new KeyValuePair<string, string>("savecopy", settings.HasFlag(Settings.SaveCopy) ? "1" : "0"),
                                new KeyValuePair<string, string>("signature", settings.HasFlag(Settings.ShowSignature) ? "1" : "0"),
                                new KeyValuePair<string, string>("parseurl", settings.HasFlag(Settings.ParseUrl) ? "1" : "0")
                            });
            }
        }

        /// <summary>
        /// Retrieves information about the messages such as title, content and sender
        /// </summary>
        /// <param name="session"> Session used for sending the request </param>
        public void Update<TUser>(AuthenticatedSession<TUser> session) where TUser : User
        {
            session.ThrowIfInvalid();
            if (ID == 0) throw new System.ArgumentException("ID must not be emtpy");

            var res = session.Get("https://www.elitepvpers.com/forum/private.php?do=showpm&pmid=" + ID.ToString());
            var doc = new HtmlDocument();
            doc.LoadHtml(res);

            new PrivateMessageParser.SenderParser(Sender).Execute(doc.GetElementbyId("post"));
            new PrivateMessageParser.ContentParser(this).Execute(doc.GetElementbyId("td_post_"));
        }

        /// <summary>
        /// Report the private message
        /// </summary>
        /// <param name="session"> The session which will be used for the report </param>
        /// <param name="reason"> The resion for the report </param>
        public void Report<TUser>(AuthenticatedSession<TUser> session, string reason) where TUser : User
        {
            session.ThrowIfInvalid();

            session.Post("https://www.elitepvpers.com/forum/private.php?do=sendemail",
                         new List<KeyValuePair<string, string>>()
                         {
                             new KeyValuePair<string, string>("s", String.Empty),
                             new KeyValuePair<string, string>("securitytoken", session.SecurityToken),
                             new KeyValuePair<string, string>("reason", reason),
                             new KeyValuePair<string, string>("pmid", ID.ToString()),
                             new KeyValuePair<string, string>("do", "sendemail"),
                             new KeyValuePair<string, string>("url", "https://www.elitepvpers.com/forum/private.php?do=showpm&pmid=" + ID.ToString())
                         });
        }

        /// <summary>
        /// Gets the url to the private message (Only accessable by administrators
        /// </summary>
        /// <returns> The url to the private message </returns>
        public string GetUrl()
        {
            return "https://www.elitepvpers.com/forum/private.php?do=showpm&pmid=" + ID;
        }
    }
}
