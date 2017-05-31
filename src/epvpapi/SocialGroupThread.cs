﻿using System;
using System.Collections.Generic;
using System.Linq;
using epvpapi.Connection;

namespace epvpapi
{
    public class SocialGroupThread : Thread, IReasonableDeletable, IUniqueWebObject
    {
        /// <summary>
        /// <c>SocialGroup</c> under which the thread is listed
        /// </summary>
        public SocialGroup SocialGroup { get; set; }

        /// <summary>
        /// List of all posts in the thread
        /// </summary>
        public List<SocialGroupPost> Posts { get; set; }

        public SocialGroupThread(int id, SocialGroup socialGroup)
            : base(id)
        {
            SocialGroup = socialGroup;
            Posts = new List<SocialGroupPost>();
        }

        /// <summary>
        /// Creates a <c>SocialGroupThread</c>
        /// </summary>
        /// <param name="session"> Session that is used for sending the request </param>
        /// <param name="socialGroup"> SocialGroup where to create the <c>SocialGroupThread</c></param>
        /// <param name="startPost"> Represents the content and title of the <c>SocialGroupThread</c> </param>
        /// <param name="settings"> Additional options that can be set </param>
        /// <returns> Freshly created <c>SocialGroupThread</c></returns>
        public static SocialGroupThread Create<TUser>(AuthenticatedSession<TUser> session, SocialGroup socialGroup, SocialGroupPost startPost,
                                                     Message.Settings settings = Message.Settings.ParseUrl)
                                                     where TUser : User
        {
            session.ThrowIfInvalid();

            session.Post("https://www.elitepvpers.com/forum/group.php?do=message",
                        new List<KeyValuePair<string, string>>()
                        {
                            new KeyValuePair<string, string>("subject", startPost.Title),
                            new KeyValuePair<string, string>("message", startPost.Content.ToString()),
                            new KeyValuePair<string, string>("wysiwyg", "0"),
                            new KeyValuePair<string, string>("s", String.Empty),
                            new KeyValuePair<string, string>("securitytoken", session.SecurityToken),
                            new KeyValuePair<string, string>("do", "message"),
                            new KeyValuePair<string, string>("gmid", String.Empty),
                            new KeyValuePair<string, string>("posthash", String.Empty),
                            new KeyValuePair<string, string>("loggedinuser", session.User.ID.ToString()),
                            new KeyValuePair<string, string>("groupid", socialGroup.ID.ToString()),
                            new KeyValuePair<string, string>("discussionid", String.Empty),
                            new KeyValuePair<string, string>("sbutton", "Nachricht+speichern"),
                            new KeyValuePair<string, string>("parseurl", settings.HasFlag(Message.Settings.ParseUrl) ? "1" : "0"),
                            new KeyValuePair<string, string>("parseame", "1"),
                        });

            var socialGroupThread = new SocialGroupThread(0, socialGroup) { Creator = session.User, Deleted = false };
            socialGroupThread.Posts.Insert(0, startPost);
            return socialGroupThread;
        }

        /// <summary>
        /// Deletes the <c>SocialGroupThread</c>
        /// </summary>
        /// <param name="session"> Session that is used for sending the request </param>
        /// <param name="reason"> Reason for the deletion </param>
        public void Delete<TUser>(AuthenticatedSession<TUser> session, string reason) where TUser : User
        {
            if (session.User.GetHighestRank() < Usergroup.GlobalModerator && session.User != SocialGroup.Maintainer) throw new InsufficientAccessException("You don't have enough access rights to delete this social group post");
            if (ID == 0) throw new ArgumentException("ID must not be empty");
            session.ThrowIfInvalid();

            session.Post("https://www.elitepvpers.com/forum/group_inlinemod.php?gmids=",
                        new List<KeyValuePair<string, string>>()
                        {
                            new KeyValuePair<string, string>("securitytoken", session.SecurityToken),
                            new KeyValuePair<string, string>("groupid", SocialGroup.ID.ToString()),
                            new KeyValuePair<string, string>("messageids", ID.ToString()),
                            new KeyValuePair<string, string>("do", "doinlinedelete"),
                            new KeyValuePair<string, string>("url", "https://www.elitepvpers.com/forum/groups/" + SocialGroup.ID.ToString() + "--.html"),
                            new KeyValuePair<string, string>("inline_discussion", "1"),
                            new KeyValuePair<string, string>("deletetype", "1"),
                            new KeyValuePair<string, string>("deletereason", reason)
                        });
        }

        /// <summary>
        /// Replies to the <c>SocialGroupThread</c>
        /// </summary>
        /// <param name="session"> Session that is used for sending the request </param>
        /// <param name="settings"> Additional options that can be set </param>
        /// <param name="post"> Reply to post </param>
        public void Reply<TUser>(AuthenticatedSession<TUser> session, SocialGroupPost post, Message.Settings settings = Message.Settings.ParseUrl) where TUser : User
        {
            session.ThrowIfInvalid();

            session.Post("https://www.elitepvpers.com/forum/group.php?do=message",
                        new List<KeyValuePair<string, string>>()
                        {
                            new KeyValuePair<string, string>("message", post.Content.ToString()),
                            new KeyValuePair<string, string>("wysiwyg", "0"),
                            new KeyValuePair<string, string>("s", String.Empty),
                            new KeyValuePair<string, string>("securitytoken", session.SecurityToken),
                            new KeyValuePair<string, string>("do", "message"),
                            new KeyValuePair<string, string>("gmid", String.Empty),
                            new KeyValuePair<string, string>("posthash", String.Empty),
                            new KeyValuePair<string, string>("loggedinuser", session.User.ID.ToString()),
                            new KeyValuePair<string, string>("groupid", SocialGroup.ID.ToString()),
                            new KeyValuePair<string, string>("discussionid", ID.ToString()),
                            new KeyValuePair<string, string>("sbutton", "Post+Message"),
                            new KeyValuePair<string, string>("parseurl", (settings & Message.Settings.ParseUrl).ToString()),
                            new KeyValuePair<string, string>("parseame", "1"),
                        });
        }

        /// <summary>
        /// Gets the url of the group thread
        /// </summary>
        /// <returns> The url of the group thread </returns>
        public string GetUrl()
        {
            return String.Format("https://www.elitepvpers.com/forum/groups/{0}-{1}-d{2}-{3}.html",
                                 SocialGroup.ID, SocialGroup.Name.UrlEscape(), ID, Posts.First().Title.UrlEscape());
        }
    }
}
