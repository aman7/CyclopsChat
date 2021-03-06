﻿using System;
using System.Linq;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Windows.Media.Imaging;
using Cyclops.Core.Avatars;
using Cyclops.Core.CustomEventArgs;
using Cyclops.Core.Security;
using jabber;
using jabber.protocol;
using jabber.protocol.client;
using jabber.protocol.iq;

namespace Cyclops.Core.Resource.Avatars
{
    /// <summary>
    /// </summary>
    public class AvatarsManager : IAvatarsManager
    {
        private readonly IUserSession session;

        public AvatarsManager(IUserSession session)
        {
            this.session = session;
            var currentDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            AvatarsFolder = Path.Combine(currentDir, AvatarsFolder);
            defaultAvatar = FromFile(Path.Combine(AvatarsFolder, DefaultAvatar));
        }

        private static BitmapImage FromFile(string file)
        {
            try
            {
                return Image.FromFile(file).ToBitmapImage();
            }
            catch
            {
                return null;
            } 
        }

        public static string AvatarsFolder = @"Data\Avatars";
        public const string DefaultAvatar = "default.png";

        private BitmapImage defaultAvatar;

        #region Implementation of ISessionHolder

        public IUserSession Session
        {
            get { return session; }
        }

        #endregion

        #region Implementation of IAvatarsManager

        public bool DoesCacheContain(string hash)
        {
            string file = BuildPath(hash);
            return File.Exists(file);
        }

        public BitmapImage GetFromCache(string hash)
        {
            string file = BuildPath(hash);
            if (!File.Exists(file))
                return defaultAvatar;
            return FromFile(file);
        }
        
        public void SendAvatarRequest(IEntityIdentifier id)
        {
            var client = ((UserSession)Session).JabberClient;
            VCardIQ vcardIq = new VCardIQ(client.Document);
            vcardIq.To = (JID)id;
            vcardIq.Type = jabber.protocol.client.IQType.get;
            client.Tracker.BeginIQ(vcardIq, OnVcard, new object());
        }

        public event EventHandler<AvatarChangedEventArgs> AvatarChange = delegate { }; 

        private void OnVcard(object sender, IQ iq, object data)
        {
            if (iq.Query is VCard)
            {
                BitmapImage image = defaultAvatar;
                VCard vcard = iq.Query as VCard;

                if (vcard.Photo != null && vcard.Photo.Image != null)
                {
                    try
                    {
                        var file = BuildPath(ImageUtils.CalculateSha1HashOfAnImage(vcard.Photo.Image));
                        if (File.Exists(file))
                            try
                            {
                                File.Delete(file);}
                            catch //file is used by another process
                            {
                                return;
                            }
                        vcard.Photo.Image.Save(file, ImageFormat.Png);
                        image = vcard.Photo.Image.ToBitmapImage();
                    }
                    catch
                    {
                        //TODO: log an exception
                    }
                }

                AvatarChange(this, new AvatarChangedEventArgs(iq.From, image ?? defaultAvatar));
            }
        }

        private static string BuildPath(string hash)
        {
            return Path.Combine(AvatarsFolder, hash.ToLower() + ".png");
        }

        internal bool ProcessAvatarChangeHash(Presence pres, IEntityIdentifier conferenceId)
        {
            try
            {
                bool hasAvatar = false;
                var photoTagParent = pres.OfType<Element>().FirstOrDefault(i => i.Name == "x" && i["photo"] != null);
                if (photoTagParent != null)
                {
                    var from = pres.From.Equals(session.CurrentUserId) ? conferenceId : pres.From;

                    string sha1Hash = photoTagParent["photo"].InnerText;
                    if (!string.IsNullOrWhiteSpace(sha1Hash) && sha1Hash.Length == 40)
                    {

                        hasAvatar = true;
                        if (DoesCacheContain(sha1Hash))
                            AvatarChange(this, new AvatarChangedEventArgs(from, GetFromCache(sha1Hash)));

                        SendAvatarRequest(from);
                    }
                    else
                        AvatarChange(this, new AvatarChangedEventArgs(from, defaultAvatar));
                }

                return hasAvatar;
            }
            catch
            {
                //todo: log an exception
                return false;
            }
        }

        #endregion
    }
}
