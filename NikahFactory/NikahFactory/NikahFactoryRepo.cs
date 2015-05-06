using NikahFactory.APIModels;
using NikahFactory.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web;

namespace NikahFactory
{
    public class NikahFactoryRepo : IDisposable
    {
        private readonly NikahFactoryContext _ctx = new NikahFactoryContext();
        public IEnumerable<User> GetUsers()
        {
            return _ctx.Users.ToList();
        }
        public User getUserByUserID(int UserID)
        {
            return _ctx.Users.Where(u => u.UserID == UserID).Single();
        }
        public User GetUserByEmail(string email)
        {
            return _ctx.Users.FirstOrDefault(u => u.Email == email);
        }
        public User GetUserByCreditCard(string creditcard)
        {
            return _ctx.Users.FirstOrDefault(u => u.CreditCard == creditcard);
        }
        public User GetUserByToken(string token)
        {
            if (validToken(token))
            {
                return _ctx.AuthTokens.Include("User").Single(a => a.Token == token).User;
            }
            else
                return null;
        }
        public void changePassword(User u, string password)
        {
            u.Password = password;
            _ctx.SaveChanges();
        }
        public string GetTemporaryPassword(User u)
        {
            User user = _ctx.Users.Single(x => x.UserID == u.UserID);
            Random random = new Random();
            user.Password = random.Next(100000, 999999).ToString();
            _ctx.SaveChanges();
            return user.Password;
        }
        public User createUser(RegisterRequest registerRequest, string customerID)
        {
            User user = new User();
            user.Bio = "";
            user.Birthday = new DateTime(registerRequest.Birthday);
            user.CustomerId = customerID;
            user.CreditCard = registerRequest.CreditCard;
            user.Country = registerRequest.Country;
            user.Email = registerRequest.Email;
            user.FirstName = registerRequest.FirstName;
            user.Gender = registerRequest.Gender;
            if (user.Gender == "Sister")
            {
                Guardian guardian = new Guardian();
                guardian.Email = registerRequest.GuardianEmail;
                Random random = new Random();
                var token = random.Next(100000, 999999).ToString();
                while (_ctx.Guardians.Where(g => g.Token == token).FirstOrDefault() != null)
                    token = random.Next(100000, 999999).ToString();
                guardian.Token = token;
                guardian.Verified = false;
                var client = new SmtpClient("outlook.office365.com")
                {
                    Credentials = new NetworkCredential("info@nikahfactory.com", "email-486170"),
                    EnableSsl = true
                };
                client.Send("info@nikahfactory.com", registerRequest.GuardianEmail, "Guardian Email Verification", "Assalamo Alaykum,\n\nWe would like to verify that you are a guardian for " + registerRequest.FirstName + " " + registerRequest.LastName + ". Please visit the following link to verify your email: http://www.nikahfactory.com/#/verifyguardianemail/" + guardian.Token + "\n\nThank you,\nNikah Factory");
                user.Guardian = guardian;
            }
            else
            {
                user.Guardian = null;
            }
            user.Active = true;
            user.Heading = "Assalamo Alaykum!";
            user.Bio = "I am looking to get married.";
            user.LastName = registerRequest.LastName;
            user.Password = registerRequest.Password;
            user.State = registerRequest.State;
            _ctx.Users.Add(user);
            _ctx.SaveChanges();
            return user;
        }
        public void UpdatePayment(User u, string creditcard)
        {
            u.CreditCard = creditcard;
            u.Active = true;
            if (u.Unpaid)
            {
                var client = new SmtpClient("outlook.office365.com")
                {
                    Credentials = new NetworkCredential("info@nikahfactory.com", "email-486170"),
                    EnableSsl = true
                };
                client.Send("info@nikahfactory.com", "info@nikahfactory.com", "Unpaid user updated payment", "UserID: " + u.CustomerId);
                u.Unpaid = false;
            }
            _ctx.SaveChanges();
        }
        public void saveHeading(User u, string heading)
        {
            u.Heading = heading;
            _ctx.SaveChanges();
        }
        public void saveBio(User u, string bio)
        {
            u.Bio = bio;
            _ctx.SaveChanges();
        }
        public void saveAuthToken(AuthToken authToken)
        {
            AuthToken currentAuthToken = _ctx.AuthTokens.SingleOrDefault(a => a.User.UserID == authToken.User.UserID);
            if (currentAuthToken != null)
            {
                currentAuthToken.Token = authToken.Token;
                currentAuthToken.Expiration = authToken.Expiration;
            }
            else
                _ctx.AuthTokens.Add(authToken);
            _ctx.SaveChanges();
        }
        public AuthToken createAuthToken(User user)
        {
            Random random = new Random();
            var token = random.Next(100000000, 999999999).ToString();
            while (_ctx.AuthTokens.Where(a => a.Token == token).FirstOrDefault() != null)
                token = random.Next(100000000, 999999999).ToString();
            var authToken = new AuthToken()
            {
                Token = token,
                Expiration = DateTime.UtcNow.AddDays(1),
                User = user
            };
            _ctx.AuthTokens.Add(authToken);
            _ctx.SaveChanges();
            return authToken;
        }
        public AuthToken updateAuthToken(User user)
        {
            AuthToken Token = _ctx.AuthTokens.Where(a => a.User.UserID == user.UserID).Single();
            Token.Expiration = DateTime.UtcNow.AddDays(1);
            _ctx.SaveChanges();
            return Token;
        }
        public bool verifyGuardianEmail(string token)
        {
            Guardian guardian = _ctx.Guardians.FirstOrDefault(g => g.Token == token);
            if (guardian != null)
            {
                guardian.Verified = true;
                _ctx.SaveChanges();
                return true;
            }
            else
                return false;
        }
        public bool activeUser(string token)
        {
            User user = _ctx.AuthTokens.Include("User").Single(a => a.Token == token).User;
            Guardian g = _ctx.Users.Include("Guardian").Single(u => u.UserID == user.UserID).Guardian;
            return user.Active && (g == null || g.Verified);
        }
        public void Pause(User u)
        {
            u.Paused = true;
            _ctx.SaveChanges();
        }
        public void unPause(User u)
        {
            u.Paused = false;
            _ctx.SaveChanges();
        }
        public bool validToken(string token)
        {
            AuthToken authToken = _ctx.AuthTokens.SingleOrDefault(a => a.Token == token);
            return authToken != null && authToken.Expiration > DateTime.Now;
        }
        public List<User> Search (string Gender, string[] Countries, int MinAge, int MaxAge)
        {
            DateTime now = DateTime.Now;
            DateTime MinDate = now.AddYears(MinAge * -1); 
            DateTime MaxDate = now.AddYears(MaxAge * -1); 
            return _ctx.Users.Where(u => u.Active && u.Gender == Gender && Countries.Contains(u.Country) && u.Birthday <= MinDate && u.Birthday >= MaxDate).ToList();
        }
        public List<Alert> Alerts (User alerted)
        {
            return _ctx.Alerts.Include("Alerter").Where(a => a.Alerted.UserID == alerted.UserID).OrderByDescending(a => a.AlertDateTime).ToList();
        }
        public List<Conversation> Conversations(User user)
        {
            return _ctx.Users.Include("Conversations").Single(u => u.UserID == user.UserID).Conversations.OrderByDescending(c => c.Last).ToList();
        }
        public User getOtherConversationUser(User user, Conversation conversation)
        {
            Message[] messages = _ctx.Conversations.Include("Messages").Include("Users").Where(c => c.ConversationId == conversation.ConversationId).Single().Messages.ToArray();
            Message message = messages[0];
            int otheruser;
            if (message.Sender.UserID == user.UserID)
                otheruser = message.Receiver.UserID;
            else
                otheruser = message.Sender.UserID;
            return _ctx.Users.Single(u => u.UserID == otheruser);
        }
        public List<Message> getMessages(Conversation conversation)
        {
            return _ctx.Conversations.Include("Messages").Single(c => c.ConversationId == conversation.ConversationId).Messages.OrderBy(m => m.DateTime).ToList();
        }
        public User getSender(Message message)
        {
            return _ctx.Messages.Include("Sender").Single(m => m.MessageId == message.MessageId).Sender;
        }
        public List<string> NewAlerts (User alerted)
        {
            return _ctx.Alerts.Include("Alerter").Where(a => a.Alerted.UserID == alerted.UserID && a.New).OrderByDescending(a => a.AlertDateTime).Select(a => a.Alerter.FirstName).ToList();
        }
        public List<string> NewMessages(User messaged)
        {
            List<Conversation> conversations = new List<Conversation>();
            List<Message> messages = new List<Message>();
            List<string> newmessages = new List<string>();
            try {
                conversations = _ctx.Users.Include("Conversations").Single(u => u.UserID == messaged.UserID).Conversations.ToList();
            }
            catch (Exception e) { }
            if (conversations == null)
                return new List<string>();
            foreach (Conversation conversation in conversations)
            {
                messages = _ctx.Conversations.Include("Messages").Include("Users").Single(c => c.ConversationId == conversation.ConversationId).Messages.ToList();
                foreach (Message message in messages)
                    if (message.Unread && message.Receiver.UserID == messaged.UserID)
                        newmessages.Add(message.Sender.FirstName);
            }
            if (newmessages == null)
                return new List<string>();
            else
                return newmessages;
        }
        public void ViewMessages(User messaged)
        {
            List<Conversation> conversations = new List<Conversation>();
            List<Message> messages = new List<Message>();
            List<string> newmessages = new List<string>();
            try
            {
                conversations = _ctx.Users.Include("Conversations").Single(u => u.UserID == messaged.UserID).Conversations.ToList();
            }
            catch (Exception e) { }
            if (conversations == null)
                return;
            foreach (Conversation conversation in conversations)
            {
                messages = _ctx.Conversations.Include("Messages").Include("Users").Single(c => c.ConversationId == conversation.ConversationId).Messages.ToList();
                foreach (Message message in messages)
                    if (message.Unread && message.Receiver.UserID == messaged.UserID)
                        message.Unread = false;
            }
            if (newmessages == null)
                return;
            else
                _ctx.SaveChanges();
        }
        public void ViewAlerts (User alerted)
        {
            List<Alert> alerts = _ctx.Alerts.Where(a => a.Alerted.UserID == alerted.UserID).ToList();
            foreach (Alert alert in alerts)
            {
                alert.New = false;
            }
            _ctx.SaveChanges();
        }
        public bool Alerted (User alerter, User alerted)
        {
            return _ctx.Alerts.Where(a => a.Alerter.UserID == alerter.UserID && a.Alerted.UserID == alerted.UserID).SingleOrDefault() != null;
        }
        public void Alert (User alerter, int userid)
        {
            User alerted = _ctx.Users.Where(u => u.UserID == userid).Single();
            Alert alert = new Alert();
            alert.New = true; 
            alert.Alerter = alerter;
            alert.Alerted = alerted;
            alert.AlertDateTime = DateTime.Now;
            _ctx.Alerts.Add(alert);
            _ctx.SaveChanges();
            var client = new SmtpClient("outlook.office365.com")
            {
                Credentials = new NetworkCredential("info@nikahfactory.com", "email-486170"),
                EnableSsl = true
            };
            client.Send("info@nikahfactory.com", alerted.Email, "New Alert", "Assalamo Alaykum " + alerted.FirstName + ",\n\nYou have received a new alert! Visit nikahfactory.com to check it out!");
        }
        public void sendMessage(User sender, int receiverUserID, string messageBody)
        {
            User receiver = _ctx.Users.Include("Conversations").Include("Guardian").Single(u => u.UserID == receiverUserID);
            sender = _ctx.Users.Include("Conversations").Include("Guardian").Single(u => u.UserID == sender.UserID);
            int conversationID = 0;
            try
            {
                foreach (Conversation conversation in receiver.Conversations)
                {
                    if (sender.Conversations.Where(c => c.ConversationId == conversation.ConversationId).SingleOrDefault() != null)
                    {
                        conversationID = conversation.ConversationId;
                        break;
                    }
                }
            }
            catch (Exception e) { }
            Conversation convo;
            if (conversationID != 0)
                convo = _ctx.Conversations.Include("Messages").Where(c => c.ConversationId == conversationID).Single();
            else
            {
                convo = new Conversation();
                try
                {
                    sender.Conversations.Add(convo);
                    receiver.Conversations.Add(convo);
                }
                catch (Exception e)
                {
                    sender.Conversations = new List<Conversation>();
                    receiver.Conversations = new List<Conversation>();
                    sender.Conversations.Add(convo);
                    receiver.Conversations.Add(convo);
                }
            }
            Message message = new Message();
            message.Sender = sender;
            message.Receiver = receiver;
            message.DateTime = DateTime.Now;
            message.Body = messageBody;
            message.Unread = true;
            try
            {
                convo.Messages.Add(message);
            }
            catch (Exception e)
            {
                convo.Messages = new List<Message>();
                convo.Messages.Add(message);
            }
            convo.Last = DateTime.Now;
            _ctx.SaveChanges();
            var client = new SmtpClient("outlook.office365.com")
            {
                Credentials = new NetworkCredential("info@nikahfactory.com", "email-486170"),
                EnableSsl = true
            };
            client.Send("info@nikahfactory.com", receiver.Email, "New Message", "Assalamo Alaykum " + receiver.FirstName + ",\n\nYou have received a new message! Visit nikahfactory.com to check it out!");
            if (sender.Guardian != null)
            {
                client.Send("info@nikahfactory.com", sender.Guardian.Email, sender.FirstName + " has sent a new message!", "Assalamo Alaykum,\n\n" + sender.FirstName + " has sent the following message to " + receiver.FirstName + ":\n\n" + messageBody);
            }
            else
            {
                client.Send("info@nikahfactory.com", receiver.Guardian.Email, receiver.FirstName + " has received a new message!", "Assalamo Alaykum,\n\n" + receiver.FirstName + " has received the following message from " + sender.FirstName + ":\n\n" + messageBody);
            }
        }
        public Message Reply(int Sender, int Receiver, int ConversationId, string Message)
        {
            User receiver = _ctx.Users.Include("Conversations").Include("Guardian").Single(u => u.UserID == Receiver);
            User sender = _ctx.Users.Include("Conversations").Include("Guardian").Single(u => u.UserID == Sender);
            Conversation conversation = _ctx.Conversations.Include("Messages").Where(c => c.ConversationId == ConversationId).Single();
            Message message = new Message();
            message.Sender = sender;
            message.Receiver = receiver;
            message.DateTime = DateTime.Now;
            message.Body = Message;
            message.Unread = true;
            conversation.Messages.Add(message);
            conversation.Last = DateTime.Now;
            _ctx.SaveChanges();
            var client = new SmtpClient("outlook.office365.com")
            {
                Credentials = new NetworkCredential("info@nikahfactory.com", "email-486170"),
                EnableSsl = true
            };
            client.Send("info@nikahfactory.com", receiver.Email, "New Message", "Assalamo Alaykum " + receiver.FirstName + ",\n\nYou have received a new message! Visit nikahfactory.com to check it out!");
            if (sender.Guardian != null)
            {
                client.Send("info@nikahfactory.com", sender.Guardian.Email, sender.FirstName + " has sent a new message!", "Assalamo Alaykum,\n\n" + sender.FirstName + " has sent the following message to " + receiver.FirstName + ":\n\n" + Message);
            }
            else
            {
                client.Send("info@nikahfactory.com", receiver.Guardian.Email, receiver.FirstName + " has received a new message!", "Assalamo Alaykum,\n\n" + receiver.FirstName + " has received the following message from " + sender.FirstName + ":\n\n" + Message);
            }
            return message;
        }
        public void cancelUser(string customerID)
        {
            User user = _ctx.Users.Single(u => u.CustomerId == customerID);
            user.Active = false;
            try
            {
                if (!user.Paused)
                    user.Unpaid = true;
            }
            catch (Exception e) { }
            _ctx.SaveChanges();
        }
        public void Dispose()
        {
            if (_ctx != null)
                _ctx.Dispose();
        }
    }
}