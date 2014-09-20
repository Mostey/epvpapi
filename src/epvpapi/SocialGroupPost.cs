﻿using System;
using System.Collections.Generic;
using System.Globalization;
using epvpapi.Connection;

namespace epvpapi
{
    public class SocialGroupPost : Post, IReasonableDeletable, IUniqueWebObject
    {
        private SocialGroupPost(uint id, string content = null)
            : base(id, content)
        {
        }

        public SocialGroupPost(uint id, SocialGroupThread thread)
            : this(id)
        {
            Thread = thread;
        }

        /// <summary>
        ///     Thread that contains the post
        /// </summary>
        private SocialGroupThread Thread { get; set; }

        /// <summary>
        ///     Deletes the <c>SocialGroupPost</c>
        /// </summary>
        /// <param name="session"> Session that is used for sending the request </param>
        /// <param name="reason"> Reason for the deletion </param>
        public void Delete<T>(ProfileSession<T> session, string reason) where T : User
        {
            if (session.User.GetHighestRank() < User.Rank.GlobalModerator &&
                session.User != Thread.SocialGroup.Maintainer)
                throw new InsufficientAccessException(
                    "You don't have enough access rights to delete this social group post");
            session.ThrowIfInvalid();

            session.Post("http://www.elitepvpers.com/forum/group_inlinemod.php",
                new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("s", String.Empty),
                    new KeyValuePair<string, string>("securitytoken", session.SecurityToken),
                    new KeyValuePair<string, string>("groupid",
                        Thread.SocialGroup.Id.ToString(CultureInfo.InvariantCulture)),
                    new KeyValuePair<string, string>("messageids", Id.ToString(CultureInfo.InvariantCulture)),
                    new KeyValuePair<string, string>("do", "doinlinedelete"),
                    new KeyValuePair<string, string>("url",
                        "http://www.elitepvpers.com/forum/groups/t-d" + Thread.Id + "--.html"),
                    new KeyValuePair<string, string>("inline_discussion", "0"),
                    new KeyValuePair<string, string>("deletetype", "1"),
                    new KeyValuePair<string, string>("deletereason", reason)
                });
        }

        public string GetUrl()
        {
            return Thread.GetUrl() + "#gmessage" + Id;
        }
    }
}