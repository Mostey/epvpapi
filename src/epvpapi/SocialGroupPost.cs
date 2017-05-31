﻿using System;
using System.Collections.Generic;
using epvpapi.Connection;

namespace epvpapi
{
    public class SocialGroupPost : Post, IReasonableDeletable, IUniqueWebObject
    {
        /// <summary>
        /// Thread that contains the post
        /// </summary>
        public SocialGroupThread Thread { get; set; }

        public SocialGroupPost(int id, Content content)
            : base(id, content)
        { }

        public SocialGroupPost(int id, SocialGroupThread thread)
            : this(id, new Content())
        {
            Thread = thread;
        }

        /// <summary>
        /// Deletes the <c>SocialGroupPost</c>
        /// </summary>
        /// <param name="session"> Session that is used for sending the request </param>
        /// <param name="reason"> Reason for the deletion </param>
        public void Delete<TUser>(AuthenticatedSession<TUser> session, string reason) where TUser : User
        {
            if (session.User.GetHighestRank() < Usergroup.GlobalModerator && session.User != Thread.SocialGroup.Maintainer) throw new InsufficientAccessException("You don't have enough access rights to delete this social group post");
            session.ThrowIfInvalid();

            session.Post("https://www.elitepvpers.com/forum/group_inlinemod.php",
                        new List<KeyValuePair<string, string>>()
                        {
                            new KeyValuePair<string, string>("s", String.Empty),
                            new KeyValuePair<string, string>("securitytoken", session.SecurityToken),
                            new KeyValuePair<string, string>("groupid", Thread.SocialGroup.ID.ToString()),
                            new KeyValuePair<string, string>("messageids", ID.ToString()),
                            new KeyValuePair<string, string>("do", "doinlinedelete"),
                            new KeyValuePair<string, string>("url", "https://www.elitepvpers.com/forum/groups/t-d" + Thread.ID.ToString() + "--.html"),
                            new KeyValuePair<string, string>("inline_discussion", "0"),
                            new KeyValuePair<string, string>("deletetype", "1"),
                            new KeyValuePair<string, string>("deletereason", reason)
                        });
        }

        public string GetUrl()
        {
            return Thread.GetUrl() + "#gmessage" + ID;
        }
    }
}
