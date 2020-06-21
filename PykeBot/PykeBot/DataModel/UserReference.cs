using DevExpress.Xpo;
using System;
using System.Data;

namespace Pyke_Bot.DataModel
{
    class UserReference : XPLiteObject
    {
        public UserReference(Session session) : base(session)
        {
        }

        private int id;
        [Key(true)]
        public int Id
        {
            get { return id; }
            set { SetPropertyValue(nameof(Id), ref id, value); }
        }

        ulong discordID;
        [Indexed(Unique = true)]
        public ulong DiscordId
        {
            get { return discordID; }
            set { SetPropertyValue(nameof(DiscordId), ref discordID, value); }
        }

        string riotId;
        [Size(64)]
        public string RiotId
        {
            get { return riotId; }
            set { SetPropertyValue(nameof(riotId), ref riotId, value); }
        }

        string region;
        [Size(6)]
        public string Region
        {
            get { return region; }
            set { SetPropertyValue(nameof(Region), ref region, value); }
        }

        DateTime date;
        public DateTime Date
        {
            get { return date; }
            set { SetPropertyValue(nameof(Date), ref date, value); }
        }
    }
}
