﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using epvpapi.Connection;
using HtmlAgilityPack;

namespace epvpapi.TBM
{
    /// <summary>
    ///     Represents a treasure containing user-defined content that can be purchased with elite*gold
    /// </summary>
    public class Treasure : UniqueObject, IUniqueWebObject, IDefaultUpdatable, IDeletable
    {
        public enum Query
        {
            /// <summary>
            ///     If the <c>Treasure</c> was bought, basically the same as <c>SoldListed</c>
            /// </summary>
            Bought,

            /// <summary>
            ///     If the <c>Treasure</c> was sold and/or listed
            /// </summary>
            SoldListed
        }

        public Treasure(string title, string content, uint cost) :
            this(0, title, content, cost)
        {
        }


        public Treasure(uint id = 0, string title = null, string content = null, uint cost = 0) :
            base(id)
        {
            Title = title;
            Content = content;
            Cost = cost;
            Seller = new User();
            Buyer = new User();
            CreationDate = new DateTime();
            PurchaseDate = new DateTime();
            Available = true;
        }

        /// <summary>
        ///     Title of the treasure, visible for everyone
        /// </summary>
        /// <remarks>
        ///     The title must be at least 4 characters long to be accepted by the system
        /// </remarks>
        public string Title { private get; set; }

        /// <summary>
        ///     User-defined content that will be visible once the <c>Treasure</c> has been bought
        ///     <remarks>
        ///         The content must be at least 4 characters long to be accepted by the system
        ///         Content is only visible for the buyer
        ///     </remarks>
        /// </summary>
        private string Content { get; set; }

        /// <summary>
        ///     <c>User</c> that sells/sold the <c>Treasure</c>
        /// </summary>
        public User Seller { get; set; }

        /// <summary>
        ///     <c>User</c> that bought the <c>Treasure</c>
        /// </summary>
        public User Buyer { get; set; }

        /// <summary>
        ///     Treasure purchase cost given in elite*gold
        /// </summary>
        /// <remarks>
        ///     The cost must amount to 1 elite*gold or higher
        /// </remarks>
        public uint Cost { private get; set; }

        /// <summary>
        ///     Date and time when the <c>Treasure</c> was created
        /// </summary>
        private DateTime CreationDate { get; set; }

        /// <summary>
        ///     Date and time when the <c>Treasure</c> was bought
        /// </summary>
        private DateTime PurchaseDate { get; set; }

        /// <summary>
        ///     If the <c>Treasure</c> was already sold to someone, this value is set to fakse
        /// </summary>
        public bool Available { get; set; }

        /// <summary>
        ///     Updates the <c>Treasure</c> by requesting the treasure page
        /// </summary>
        /// <param name="session"> Session used for sending the request </param>
        public void Update(Session session)
        {
            session.ThrowIfInvalid();
            if (Id == 0) throw new ArgumentException("ID must not be zero");

            Response res = session.Get(GetUrl());
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(res.ToString());

            HtmlNode rootFormNode = htmlDocument.GetElementbyId("contentbg");
            if (rootFormNode == null) return;

            rootFormNode = rootFormNode.SelectSingleNode("table[1]/tr[1]/td[1]/table[1]/tr[2]/td[1]");
            if (rootFormNode == null) return;

            HtmlNode treasureInfoNode = rootFormNode.SelectSingleNode("div[1]/div[3]/table[1]/tr[1]/td[1]/table[1]");
            if (treasureInfoNode != null)
            {
                foreach (HtmlNode treasureAttributeNode in treasureInfoNode.GetElementsByTagName("tr"))
                {
                    HtmlNode keyNode = treasureAttributeNode.SelectSingleNode("td[1]");
                    HtmlNode valueNode = treasureAttributeNode.SelectSingleNode("td[2]");
                    if (keyNode == null || valueNode == null) continue;
                    string key = keyNode.InnerText.Strip();

                    switch (key)
                    {
                        case "Title:":
                            Title = valueNode.InnerText;
                            break;
                        case "Buyer:":
                        case "Seller:":
                        {
                            HtmlNode userRefNode = valueNode.SelectSingleNode("a[1]");
                            if (userRefNode != null)
                            {
                                if (key == "Seller:")
                                    Seller = new User(userRefNode.InnerText,
                                        userRefNode.Attributes.Contains("href")
                                            ? User.FromUrl(userRefNode.Attributes["href"].Value)
                                            : 0);
                                else if (key == "Buyer:")
                                    Buyer = new User(userRefNode.InnerText,
                                        userRefNode.Attributes.Contains("href")
                                            ? User.FromUrl(userRefNode.Attributes["href"].Value)
                                            : 0);
                            }
                        }
                            break;
                        case "Cost:":
                        {
                            Match match = new Regex(@"([0-9]+) eg").Match(valueNode.InnerText);
                            if (match.Groups.Count > 1)
                                Cost = Convert.ToUInt32(match.Groups[1].Value);
                        }
                            break;
                        case "Creation date:":
                            CreationDate = valueNode.InnerText.ToElitepvpersDateTime();
                            break;
                        case "Purchase date:":
                        {
                            Available = false;
                            HtmlNode countdownNode = valueNode.SelectSingleNode("div[1]");
                            PurchaseDate = (countdownNode != null)
                                ? countdownNode.InnerText.ToElitepvpersDateTime()
                                : new DateTime();
                        }
                            break;
                    }
                }
            }

            HtmlNode treasureContentNode = rootFormNode.SelectSingleNode("div[2]/div[3]");
            Content = (treasureContentNode != null) ? treasureContentNode.InnerText.Strip() : "";
        }

        /// <summary>
        ///     Deletes the <c>Treasure</c> permanently
        /// </summary>
        /// <param name="session"> Session used for sending the request </param>
        public void Delete<T>(ProfileSession<T> session) where T : User
        {
            session.ThrowIfInvalid();
            if (Id == 0) throw new ArgumentException("ID must not be zero");

            Response res = session.Post(GetUrl(),
                new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("deletetreasure", "1")
                });
        }


        public string GetUrl()
        {
            return "http://www.elitepvpers.com/theblackmarket/treasure/" + Id;
        }

        /// <summary>
        ///     Creates the <c>Treasure</c> and makes it public
        /// </summary>
        /// <param name="session"> Session used for sending the request </param>
        public void Create<TUser>(ProfileSession<TUser> session) where TUser : User
        {
            session.ThrowIfInvalid();
            if (Content.Length < 4) throw new ArgumentException("The content is too short (4 characters minimum)");
            if (Title.Length < 4) throw new ArgumentException("The title is too short (4 characters minimum)");
            if (Cost < 1) throw new ArgumentException("The price is too low (at least 1 elite*gold)");

            session.Post("http://www.elitepvpers.com/theblackmarket/treasures/",
                new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("title", Title),
                    new KeyValuePair<string, string>("content", Content),
                    new KeyValuePair<string, string>("cost", Cost.ToString()),
                    new KeyValuePair<string, string>("createtreasure", "Submit")
                });

            CreationDate = DateTime.Now;
            Seller = session.User;
        }
    }
}