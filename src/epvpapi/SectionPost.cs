﻿using System;
using System.Collections.Generic;
using epvpapi.Connection;

namespace epvpapi
{
    public class SectionPost : Post, IReportable, IReasonableDeletable, IUniqueWebObject
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
            ShowSignature = 2
        }

        /// <summary>
        /// Icon associated with the post
        /// </summary>
        public short Icon { get; set; }

        public SectionThread Thread { get; set; }

        public SectionPost(Content content, string title = null)
            : this(0, new SectionThread(new Section(0, "")), content, title)
        { }

        public SectionPost(int id, SectionThread thread, string title = null)
            : this(id, thread, new Content(), title)
        {
            Thread = thread;
        }

        public SectionPost(int id, SectionThread thread, Content content, string title = null)
            : base(id, content, title)
        {
            Thread = thread;
        }

        public string GetUrl()
        {
            return "https://www.elitepvpers.com/forum/joining-e-pvp/" + Thread.ID + "-" + Thread.InitialPost.Title.UrlEscape() + ".html";
        }

        /// <summary>
        /// Reports the <c>SectionPost</c> using the built-in report function
        /// </summary>
        /// <param name="session"> Session that is used for sending the request </param>
        /// <param name="reason"> Reason of the report </param>
        /// <remarks>
        /// The ID of the <c>SectionPost</c> has to be given in order to report the post
        /// </remarks>
        public void Report<TUser>(AuthenticatedSession<TUser> session, string reason) where TUser : User
        {
            if (ID == 0) throw new System.ArgumentException("ID must not be empty");
            session.ThrowIfInvalid();

            session.Post("https://www.elitepvpers.com/forum/report.php?do=sendemail",
                        new List<KeyValuePair<string, string>>()
                        {
                            new KeyValuePair<string, string>("securitytoken", session.SecurityToken),
                            new KeyValuePair<string, string>("reason", reason),
                            new KeyValuePair<string, string>("postid", ID.ToString()),
                            new KeyValuePair<string, string>("do", "sendemail"),
                            new KeyValuePair<string, string>("url", "showthread.php?p=" + ID.ToString() + "#post" + ID.ToString())                
                        });
        }

        /// <summary>
        /// Deletes the <c>SectionPost</c>
        /// </summary>
        /// <param name="session"> Session that is used for sending the request </param>
        /// <param name="reason"> Reason for the deletion </param>
        /// <remarks>
        /// Not tested yet!
        /// </remarks>
        public void Delete<TUser>(AuthenticatedSession<TUser> session, string reason) where TUser : User
        {
            if (ID == 0) throw new ArgumentException("ID must not be empty");
            session.ThrowIfInvalid();

            session.Post("https://www.elitepvpers.com/forum/editpost.php",
                        new List<KeyValuePair<string, string>>()
                        {
                            new KeyValuePair<string, string>("do", "deletepost"),
                            new KeyValuePair<string, string>("s", String.Empty),
                            new KeyValuePair<string, string>("securitytoken", session.SecurityToken),
                            new KeyValuePair<string, string>("postid", ID.ToString()),
                            new KeyValuePair<string, string>("deletepost", "delete"),
                            new KeyValuePair<string, string>("reason", reason),
                        });

            Deleted = true;
        }
    }
}
