﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using epvpapi.Connection;

namespace epvpapi
{
    public class Blog : UniqueObject, IUniqueWebObject
    {
        public Blog(User owner) :
            base(owner.Id)
        {
            Entries = new List<Entry>();
            Owner = owner;
        }

        public List<Entry> Entries { get; set; }
        public DateTime LastEntry { get; set; }
        private User Owner { get; set; }

        public string GetUrl()
        {
            return "http://www.elitepvpers.com/forum/blogs/" + Owner.Id + "-" + Owner.Name.UrlEscape() + ".html";
        }

        public class Entry : Post
        {
            /// <summary>
            ///     Additional options that can be set when posting messages
            /// </summary>
            [Flags]
            public new enum Settings
            {
                /// <summary>
                ///     If set, all URLs in the message are going to be parsed
                /// </summary>
                ParseUrl = 1,

                /// <summary>
                ///     Users may reply to the entry using the built-in comment feature if this flag has been set
                /// </summary>
                AllowComments = 3,

                /// <summary>
                ///     If set, comments need to be approved first before they are shown to the public.
                ///     While they are not approved, only the staff and you, the owner, can see the comments
                /// </summary>
                ModerateComments = 4,

                /// <summary>
                ///     If set, the blog entry is only visible to the staff and yourself
                /// </summary>
                Private = 5
            }

            private Entry(uint id, string content, string title = null)
                : base(id, content, title)
            {
                Tags = new List<string>();
            }

            public Entry(string content, string title = null)
                : this(0, content, title)
            {
            }

            public Entry(uint id)
                : this(id, null)
            {
            }

            private List<string> Tags { get; set; }
            private Blog Blog { get; set; }

            /// <summary>
            ///     Publishes the <c>Entry</c> in the logged-in user's blog
            /// </summary>
            /// <typeparam name="T"> Type of User </typeparam>
            /// <param name="session"> Session that is used for sending the request </param>
            /// <param name="settings"> Additional options that can be set </param>
            public void Publish<T>(ProfileSession<T> session,
                Settings settings = Settings.ParseUrl | Settings.AllowComments) where T : User
            {
                Publish(session, DateTime.Now, settings);
            }

            /// <summary>
            ///     Publishes the <c>Entry</c> in the logged-in user's blog at the given time (automatically)
            /// </summary>
            /// <typeparam name="T"> Type of User </typeparam>
            /// <param name="session"> Session that is used for sending the request </param>
            /// <param name="publishDate"> Date and time when the entry will go live </param>
            /// <param name="settings"> Additional options that can be set </param>
            private void Publish<T>(ProfileSession<T> session, DateTime publishDate,
                Settings settings = Settings.ParseUrl | Settings.AllowComments) where T : User
            {
                session.ThrowIfInvalid();

                string tags = "";
                foreach (string tag in Tags)
                {
                    tags += tag;
                    if (Tags.Last() != tag)
                        tags += ",";
                }

                Date = publishDate;

                session.Post("http://www.elitepvpers.com/forum/blog_post.php?do=updateblog&blogid=",
                    new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string, string>("title", Title),
                        new KeyValuePair<string, string>("message", Content),
                        new KeyValuePair<string, string>("wysiwyg", "0"),
                        new KeyValuePair<string, string>("s", String.Empty),
                        new KeyValuePair<string, string>("securitytoken", session.SecurityToken),
                        new KeyValuePair<string, string>("do", "updateblog"),
                        new KeyValuePair<string, string>("b", String.Empty),
                        new KeyValuePair<string, string>("posthash", String.Empty),
                        new KeyValuePair<string, string>("poststarttime",
                            DateTime.Now.UnixTimestamp().ToString(CultureInfo.InvariantCulture)),
                        new KeyValuePair<string, string>("loggedinuser",
                            session.User.Id.ToString(CultureInfo.InvariantCulture)),
                        new KeyValuePair<string, string>("u", String.Empty),
                        new KeyValuePair<string, string>("taglist", tags),
                        new KeyValuePair<string, string>("allowcomments",
                            Convert.ToUInt32(settings.HasFlag(Settings.AllowComments))
                                .ToString(CultureInfo.InvariantCulture)),
                        new KeyValuePair<string, string>("moderatecomments",
                            Convert.ToUInt32(settings.HasFlag(Settings.ModerateComments))
                                .ToString(CultureInfo.InvariantCulture)),
                        new KeyValuePair<string, string>("private",
                            Convert.ToUInt32(settings.HasFlag(Settings.Private)).ToString(CultureInfo.InvariantCulture)),
                        new KeyValuePair<string, string>("status",
                            (publishDate.Compare(DateTime.Now)) ? "publish_now" : "publish_on"),
                        new KeyValuePair<string, string>("publish[month]",
                            Date.Month.ToString(CultureInfo.InvariantCulture)),
                        new KeyValuePair<string, string>("publish[day]", Date.Day.ToString(CultureInfo.InvariantCulture)),
                        new KeyValuePair<string, string>("publish[year]",
                            Date.Year.ToString(CultureInfo.InvariantCulture)),
                        new KeyValuePair<string, string>("publish[hour]",
                            Date.Hour.ToString(CultureInfo.InvariantCulture)),
                        new KeyValuePair<string, string>("publish[minute]",
                            Date.Minute.ToString(CultureInfo.InvariantCulture)),
                        new KeyValuePair<string, string>("parseurl",
                            Convert.ToUInt32(settings.HasFlag(Settings.ParseUrl)).ToString(CultureInfo.InvariantCulture)),
                        new KeyValuePair<string, string>("parseame", "1"),
                        new KeyValuePair<string, string>("emailupdate", "none"),
                        new KeyValuePair<string, string>("sbutton", "Submit")
                    });
            }

            public string GetUrl()
            {
                return "http://www.elitepvpers.com/forum/blogs/" + Blog.Owner.Id + "-" + Blog.Owner.Name.UrlEscape() +
                       "/" + Id + "-" + Title.UrlEscape() + ".html";
            }
        }
    }
}