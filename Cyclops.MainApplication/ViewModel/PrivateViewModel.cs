﻿using System.Collections.ObjectModel;
using Cyclops.Core;
using Cyclops.Core.Resource;
using Cyclops.MainApplication.Controls;
using Cyclops.MainApplication.View.Popups;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace Cyclops.MainApplication.ViewModel
{
    public class PrivateViewModel : ChatAreaViewModel
    {
        private string currentlyTypedMessage;
        private ObservableCollection<MessageViewModel> messages;

        public PrivateViewModel(IChatAreaView view) : base(view)
        {
            Messages = new ObservableCollection<MessageViewModel>();
            Messages.CollectionChanged += MessagesCollectionChanged;
        }

        void MessagesCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (!IsActive)
            {
                UnreadMessagesCount++;
            }

            ApplicationSounds.PlayOnIcomingPrivate(this);
        }


        protected override void CloseAction()
        {
            ApplicationContext.Current.MainViewModel.PrivateViewModels.Remove(this);
        }

        public IEntityIdentifier Participant { get; set; }
        
        public IConference Conference { get; set; }

        public ObservableCollection<MessageViewModel> Messages
        {
            get { return messages; }
            set
            {
                messages = value;
                RaisePropertyChanged("Messages");
            }
        }
        
        protected override void OnSendMessage()
        {
            if (string.IsNullOrEmpty(CurrentlyTypedMessage))
                return;


            ChatObjectFactory.GetSession().SendPrivate(Participant, CurrentlyTypedMessage);

            Messages.Add(new MessageViewModel(new PrivateMessage
                                                  {
                                                      AuthorNick = Localization.Conference.Me, 
                                                      IsSelfMessage = true,
                                                      Body = RemoveEndNewLineSymbol(CurrentlyTypedMessage)
                                                  }));
            CurrentlyTypedMessage = string.Empty;
        }

        public override bool IsPrivate
        {
            get { return true; }
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(Participant.Resource))
                return Participant.User;
            return string.Format("{0} ({1})", Participant.Resource, Participant.User);
        }
    }
}