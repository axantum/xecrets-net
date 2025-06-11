using AxCrypt.Api.Model.SecuredMessenger;
using AxCrypt.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AxCrypt.Core.SecuredMessenger
{
    public class SecuredMessenger
    {
        public SecuredMessenger()
        {
            
        }
        //public SecuredMessenger(IEnumerable<MessengerReceiverApiModel> description, string userName, string theMessage, SecuredMessengerVisibility visibility, DateTime visibleUntil)
        //{
        //    ReceiverList = description?.ToList() ?? new List<MessengerReceiverApiModel>();
        //    Username = userName;
        //    TheMessage = theMessage;
        //    Visibility = visibility;
        //    VisibleUntil = visibleUntil;
        //}

        public SecuredMessengerVisibility Visibility { get; set; }

        public DateTime VisibleUntil { get; set; }

        public IEnumerable<MessengerReceiverApiModel> ReceiverList
        {
            get; set;
        } = [];

        private string? _username;

        public string Username
        {
            get { return _username ?? String.Empty; }
            set { _username = value; }
        }

        private string? _theMessage;

        /// <summary>
        /// The (short) actual secret - it may be any text
        /// </summary>
        public string TheMessage
        {
            get { return _theMessage ?? String.Empty; }
            set { _theMessage = value; }
        }
    }
}
