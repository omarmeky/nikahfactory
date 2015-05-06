using NikahFactory.APIModels;
using NikahFactory.Models;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using System.Web.Http;

namespace NikahFactory.APIControllers
{
    public class AccountController : ApiController
    {
        [HttpPost]
        public HttpResponseMessage Register([FromBody] RegisterRequest registerRequest)
        {
            using(NikahFactoryRepo _repo = new NikahFactoryRepo())
            {
                User u = _repo.GetUserByEmail(registerRequest.Email);
                if (u == null)
                {
                    u = _repo.GetUserByCreditCard(registerRequest.CreditCard);
                    if (u == null)
                    {
                        DateTime bday = new DateTime(registerRequest.Birthday);
                        DateTime today = DateTime.Today;
                        int age = today.Year - bday.Year;
                        if (bday > today.AddYears(-age)) age--;
                        if (15 < age && age < 71)
                        {
                            StripeClient stripeClient = new StripeClient(“token removed”);
                            var card = new CreditCard
                            {
                                Number = registerRequest.CreditCard,
                                ExpMonth = registerRequest.ExpirationMonth,
                                ExpYear = registerRequest.ExpirationYear,
                                Cvc = registerRequest.SecurityCode
                            };
                            dynamic response = stripeClient.CreateCustomer(card, null, registerRequest.Email, null, "base");
                            if (!response.IsError)
                            {
                                JsonObject stripeJSON = response as JsonObject;
                                string customerID = stripeJSON["id"].ToString();
                                User user = _repo.createUser(registerRequest, customerID);
                                AuthToken Token = _repo.createAuthToken(user);
                                return Request.CreateResponse(HttpStatusCode.Created, new { token = Token.Token });
                            }
                            else
                                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, new Exception("There was a problem processing your card. Please verify your credentials and try again."));
                        }
                        else
                            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, new Exception("Uh oh, it appears you do not meet the age requirement for Nikah Factory."));
                    }
                    else
                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, new Exception("It appears that card is already in use."));
                }
                else
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, new Exception("Looks like you're already registered. Try logging in or resetting your password."));
            }
        }

        [HttpPost]
        public HttpResponseMessage UpdatePayment(string token, [FromBody] UpdatePaymentRequest updatePaymentRequest)
        {
            using (NikahFactoryRepo _repo = new NikahFactoryRepo())
            {
                User u = _repo.GetUserByToken(token);
                if (u == null)
                    return Request.CreateErrorResponse(HttpStatusCode.Forbidden, new Exception("Please login to continue."));
                else if (_repo.GetUserByCreditCard(updatePaymentRequest.CreditCard) != null)
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, new Exception("It appears that card is already in use."));
                else
                {
                    StripeClient stripeClient = new StripeClient("sk_live_w31SFjS7TBwBdEhGDT6fRbEQ");
                    var card = new CreditCard
                    {
                        Number = updatePaymentRequest.CreditCard,
                        ExpMonth = updatePaymentRequest.ExpirationMonth,
                        ExpYear = updatePaymentRequest.ExpirationYear,
                        Cvc = updatePaymentRequest.SecurityCode
                    };
                    dynamic response = stripeClient.UpdateCustomer(u.CustomerId, card);
                    if (!response.IsError)
                    {
                        _repo.UpdatePayment(u, updatePaymentRequest.CreditCard);
                        response = stripeClient.UpdateCustomersSubscription(u.CustomerId, "base");
                        return Request.CreateResponse(HttpStatusCode.Created);
                    }
                    else
                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, new Exception("There was an error processing your request. Please verify your payment details."));
                }
            }
        }

        [HttpPost]
        public HttpResponseMessage Login([FromBody] LoginRequest loginRequest)
        {
            string email = loginRequest.Email;
            string password = loginRequest.Password;
            using(NikahFactoryRepo _repo = new NikahFactoryRepo())
            {
                User u = _repo.GetUserByEmail(email);
                if (u != null)
                    if (u.Password == password)
                    {
                        AuthToken Token = _repo.updateAuthToken(u);
                        return Request.CreateResponse(HttpStatusCode.Created, new { token = Token.Token });
                    } 
                    else
                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, new Exception("There might have been a typo in your password."));
                else
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, new Exception("Looks like you haven't registered yet!"));
            }
        }

        [HttpPost]
        public HttpResponseMessage ResetPassword(string email)
        {
            using(NikahFactoryRepo _repo = new NikahFactoryRepo())
            {
                User u = _repo.GetUserByEmail(email);
                if (u == null)
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, new Exception("Looks like that email is not registered yet!"));
                else
                {
                    string temporaryPassword = _repo.GetTemporaryPassword(u);
                    string emailMessage = "Assalamo Alaykum " + u.FirstName + ",\n\n" + "Here is your temporary password: " + temporaryPassword +"\nVisit the \"Settings\" page to change it.\n\nThank You,\nNikah Factory Support";
                    var client = new SmtpClient("outlook.office365.com")
                    {
                        Credentials = new NetworkCredential("info@nikahfactory.com", "email-486170"),
                        EnableSsl = true
                    };
                    client.Send("info@nikahfactory.com", u.Email, "Password Reset", emailMessage);
                    return Request.CreateResponse(HttpStatusCode.Created);
                }
            }
        }

        [HttpPost]
        public HttpResponseMessage ChangePassword(string token, [FromBody] ChangePasswordRequest changePasswordRequest)
        {
            using (NikahFactoryRepo _repo = new NikahFactoryRepo())
            {
                User u = _repo.GetUserByToken(token);
                if (u == null)
                    return Request.CreateErrorResponse(HttpStatusCode.Forbidden, new Exception("Please login to continue."));
                else
                {
                    if (u.Password == changePasswordRequest.OldPassword)
                    {
                        _repo.changePassword(u, changePasswordRequest.NewPassword);
                        return Request.CreateResponse(HttpStatusCode.Created);
                    }
                    else
                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, new Exception("Looks like you entered an incorrect password."));
                }
            }
        }

        [HttpPost]
        public HttpResponseMessage VerifyGuardianEmail(string token)
        {
            using (NikahFactoryRepo _repo = new NikahFactoryRepo())
            {
                if (_repo.verifyGuardianEmail(token))
                    return Request.CreateResponse(HttpStatusCode.OK);
                else
                    return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
        }

        [HttpGet]
        public HttpResponseMessage MyProfile(string token)
        {
            using (NikahFactoryRepo _repo = new NikahFactoryRepo())
            {
                User u = _repo.GetUserByToken(token);
                if (u == null)
                    return Request.CreateErrorResponse(HttpStatusCode.Forbidden, new Exception("Please login to continue."));
                else
                {
                    DateTime bday = u.Birthday;
                    DateTime today = DateTime.Today;
                    int age = today.Year - bday.Year;
                    if (bday > today.AddYears(-age)) age--;
                    return Request.CreateResponse(HttpStatusCode.OK, new { FirstName = u.FirstName, Gender = u.Gender, State = u.State, Country = u.Country, Age = age, Heading = u.Heading, Bio = u.Bio});
                }
            }
        }

        [HttpGet]
        public HttpResponseMessage ViewProfile(string token, int UserID)
        {
            using (NikahFactoryRepo _repo = new NikahFactoryRepo())
            {
                User u = _repo.GetUserByToken(token);
                if (u == null)
                    return Request.CreateErrorResponse(HttpStatusCode.Forbidden, new Exception("Please login to continue."));
                else
                {
                    User user = _repo.getUserByUserID(UserID);
                    DateTime bday = user.Birthday;
                    DateTime today = DateTime.Today;
                    int age = today.Year - bday.Year;
                    if (bday > today.AddYears(-age)) age--;
                    return Request.CreateResponse(HttpStatusCode.OK, new { FirstName = user.FirstName, Gender = user.Gender, State = user.State, Country = user.Country, Age = age, Heading = user.Heading, Bio = user.Bio });
                }
            }
        }

        [HttpPost]
        public HttpResponseMessage Alert(string token, int userid)
        {
            using (NikahFactoryRepo _repo = new NikahFactoryRepo())
            {
                User u = _repo.GetUserByToken(token);
                if (u == null)
                    return Request.CreateErrorResponse(HttpStatusCode.Forbidden, new Exception("Please login to continue."));
                else
                {
                    _repo.Alert(u, userid);
                    return Request.CreateResponse(HttpStatusCode.OK);
                }
            }
        }
        
        [HttpPost]
        public HttpResponseMessage SendMessage(string token, [FromBody] SendMessageRequest sendMessageRequest)
        {
            using (NikahFactoryRepo _repo = new NikahFactoryRepo())
            {
                User u = _repo.GetUserByToken(token);
                if (u == null)
                    return Request.CreateErrorResponse(HttpStatusCode.Forbidden, new Exception("Please login to continue."));
                else
                {
                    _repo.sendMessage(u, sendMessageRequest.UserID, sendMessageRequest.Message);
                    return Request.CreateResponse(HttpStatusCode.OK);
                }
            }
        }

        [HttpPost]
        public HttpResponseMessage Reply(string token, [FromBody] ReplyRequest replyRequest)
        {
            using (NikahFactoryRepo _repo = new NikahFactoryRepo())
            {
                User u = _repo.GetUserByToken(token);
                if (u == null)
                    return Request.CreateErrorResponse(HttpStatusCode.Forbidden, new Exception("Please login to continue."));
                else
                {
                    Message message = _repo.Reply(u.UserID, replyRequest.Receiver, replyRequest.ConversationId, replyRequest.Message);
                    MessageResponse messageResponse = new MessageResponse();
                    messageResponse.MessageID = message.MessageId;
                    messageResponse.Sender = u.FirstName;
                    messageResponse.Gender = u.Gender;
                    messageResponse.Me = true;
                    messageResponse.Body = replyRequest.Message;
                    return Request.CreateResponse(HttpStatusCode.OK, messageResponse);
                }
            }
        }

        [HttpPost]
        public HttpResponseMessage Search(string token, [FromBody] SearchRequest searchRequest)
        {
            using (NikahFactoryRepo _repo = new NikahFactoryRepo())
            {
                User u = _repo.GetUserByToken(token);
                if (u == null)
                    return Request.CreateErrorResponse(HttpStatusCode.Forbidden, new Exception("Please login to continue."));
                else
                {
                    string gender = "";
                    if (u.Gender == "Brother")
                        gender = "Sister";
                    else
                        gender = "Brother";
                    List<User> users = _repo.Search(gender, searchRequest.Countries, searchRequest.MinAge, searchRequest.MaxAge);
                    SearchResponse[] searchResponses = new SearchResponse[users.Count];
                    int i = 0;
                    DateTime bday = new DateTime();
                    DateTime today = DateTime.Now;
                    int age = 0;
                    foreach (User user in users)
                    {
                        searchResponses[i] = new SearchResponse();
                        searchResponses[i].UserID = user.UserID;
                        searchResponses[i].FirstName = user.FirstName;
                        searchResponses[i].Country = user.Country;
                        searchResponses[i].State = user.State;
                        searchResponses[i].Heading = user.Heading;
                        bday = user.Birthday;
                        age = today.Year - bday.Year;
                        if (bday > today.AddYears(-age)) age--;
                        searchResponses[i].Age = age;
                        searchResponses[i].Alerted = _repo.Alerted(u, user);
                        i++;
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, searchResponses);
                }
            }
        }

        [HttpGet]
        public HttpResponseMessage Alerts(string token)
        {
            using (NikahFactoryRepo _repo = new NikahFactoryRepo())
            {
                User u = _repo.GetUserByToken(token);
                if (u == null)
                    return Request.CreateErrorResponse(HttpStatusCode.Forbidden, new Exception("Please login to continue."));
                else
                {
                    List<Alert> alerts = _repo.Alerts(u);
                    AlertsResponse[] alertResponses = new AlertsResponse[alerts.Count];
                    int i = 0;
                    DateTime bday = new DateTime();
                    DateTime today = DateTime.Now;
                    int age = 0;
                    foreach (Alert alert in alerts)
                    {
                        alertResponses[i] = new AlertsResponse();
                        alertResponses[i].UserID = alert.Alerter.UserID;
                        alertResponses[i].FirstName = alert.Alerter.FirstName;
                        alertResponses[i].Country = alert.Alerter.Country;
                        alertResponses[i].State = alert.Alerter.State;
                        alertResponses[i].Heading = alert.Alerter.Heading;
                        bday = alert.Alerter.Birthday;
                        age = today.Year - bday.Year;
                        if (bday > today.AddYears(-age)) age--;
                        alertResponses[i].Age = age;
                        alertResponses[i].Alerted = _repo.Alerted(u, alert.Alerter);
                        alertResponses[i].New = alert.New;
                        i++;
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, alertResponses);
                }
            }
        }

        [HttpGet]
        public HttpResponseMessage Conversations(string token)
        {
            using (NikahFactoryRepo _repo = new NikahFactoryRepo())
            {
                User u = _repo.GetUserByToken(token);
                if (u == null)
                    return Request.CreateErrorResponse(HttpStatusCode.Forbidden, new Exception("Please login to continue."));
                else
                {
                    List<Conversation> conversations = _repo.Conversations(u);
                    List<Message> messages = new List<Message>();
                    ConversationResponse[] conversationResponse = new ConversationResponse[conversations.Count];
                    User otheruser = new User();
                    User sender = new User();
                    int i = 0;
                    foreach (Conversation conversation in conversations)
                    {
                        conversationResponse[i] = new ConversationResponse();
                        conversationResponse[i].ConversationID = conversation.ConversationId;
                        otheruser = _repo.getOtherConversationUser(u, conversation);
                        conversationResponse[i].UserID = otheruser.UserID;
                        conversationResponse[i].User = otheruser.FirstName;
                        conversationResponse[i].Gender = otheruser.Gender;
                        messages = _repo.getMessages(conversation);
                        conversationResponse[i].Messages = new MessageResponse[messages.Count];
                        int j = 0;
                        foreach (Message message in messages)
                        {
                            conversationResponse[i].Messages[j] = new MessageResponse();
                            conversationResponse[i].Messages[j].MessageID = message.MessageId;
                            conversationResponse[i].Messages[j].Body = message.Body;
                            sender = _repo.getSender(message);
                            conversationResponse[i].Messages[j].Sender = sender.FirstName;
                            conversationResponse[i].Messages[j].Gender = sender.Gender;
                            conversationResponse[i].Messages[j].Me = sender.UserID == u.UserID;
                            j++;
                        }
                        i++;
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, conversationResponse);
                }
            }
        }

        [HttpGet]
        public HttpResponseMessage NewAlerts(string token)
        {
            using (NikahFactoryRepo _repo = new NikahFactoryRepo())
            {
                User u = _repo.GetUserByToken(token);
                if (u == null)
                    return Request.CreateErrorResponse(HttpStatusCode.Forbidden, new Exception("Please login to continue."));
                else
                {
                    List<string> newalerts = _repo.NewAlerts(u);
                    int i = 0;
                    string[] alerters = new string[newalerts.Count];
                    foreach (string newalert in newalerts)
                    {
                        alerters[i] = newalert;
                        i++;
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, alerters);
                }
            }
        }

        [HttpGet]
        public HttpResponseMessage NewMessages(string token)
        {
            using (NikahFactoryRepo _repo = new NikahFactoryRepo())
            {
                User u = _repo.GetUserByToken(token);
                if (u == null)
                    return Request.CreateErrorResponse(HttpStatusCode.Forbidden, new Exception("Please login to continue."));
                else
                {
                    List<string> newmessages = _repo.NewMessages(u);
                    int i = 0;
                    string[] messagers = new string[newmessages.Count];
                    foreach (string newmessage in newmessages)
                    {
                        messagers[i] = newmessage;
                        i++;
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, messagers);
                }
            }
        }

        [HttpPost]
        public HttpResponseMessage ViewAlerts(string token)
        {
            using (NikahFactoryRepo _repo = new NikahFactoryRepo())
            {
                User u = _repo.GetUserByToken(token);
                if (u == null)
                    return Request.CreateErrorResponse(HttpStatusCode.Forbidden, new Exception("Please login to continue."));
                else
                {
                    _repo.ViewAlerts(u);
                    return Request.CreateResponse(HttpStatusCode.OK);
                }
            }
        }

        [HttpPost]
        public HttpResponseMessage ViewMessages(string token)
        {
            using (NikahFactoryRepo _repo = new NikahFactoryRepo())
            {
                User u = _repo.GetUserByToken(token);
                if (u == null)
                    return Request.CreateErrorResponse(HttpStatusCode.Forbidden, new Exception("Please login to continue."));
                else
                {
                    _repo.ViewMessages(u);
                    return Request.CreateResponse(HttpStatusCode.OK);
                }
            }
        }

        [HttpPost]
        public HttpResponseMessage SaveHeading(string token, string heading)
        {
            using (NikahFactoryRepo _repo = new NikahFactoryRepo())
            {
                User u = _repo.GetUserByToken(token);
                if (u == null)
                    return Request.CreateErrorResponse(HttpStatusCode.Forbidden, new Exception("Please login to continue."));
                else
                {
                    _repo.saveHeading(u, heading);
                    return Request.CreateResponse(HttpStatusCode.OK);
                }
            }
        }

        [HttpPost]
        public HttpResponseMessage SaveBio(string token, string bio)
        {
            using (NikahFactoryRepo _repo = new NikahFactoryRepo())
            {
                User u = _repo.GetUserByToken(token);
                if (u == null)
                    return Request.CreateErrorResponse(HttpStatusCode.Forbidden, new Exception("Please login to continue."));
                else
                {
                    _repo.saveBio(u, bio);
                    return Request.CreateResponse(HttpStatusCode.OK);
                }
            }
        }

        [HttpGet]
        public HttpResponseMessage ActiveUser(string token)
        {
            using(NikahFactoryRepo _repo = new NikahFactoryRepo())
            {
                if (_repo.activeUser(token))
                    return Request.CreateResponse(HttpStatusCode.OK);
                else
                    return Request.CreateErrorResponse(HttpStatusCode.PreconditionFailed, new Exception("Account Inactive"));
            }
        }

        [HttpGet]
        public HttpResponseMessage Paused(string token)
        {
            using (NikahFactoryRepo _repo = new NikahFactoryRepo())
            {
                User u = _repo.GetUserByToken(token);
                if (u == null)
                    return Request.CreateErrorResponse(HttpStatusCode.Forbidden, new Exception("Please login to continue."));
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { Paused = u.Paused });
                }
            }
        }

        [HttpPost]
        public HttpResponseMessage TogglePause(string token)
        {
            using (NikahFactoryRepo _repo = new NikahFactoryRepo())
            {
                User u = _repo.GetUserByToken(token);
                if (u == null)
                    return Request.CreateErrorResponse(HttpStatusCode.Forbidden, new Exception("Please login to continue."));
                else
                {
                    StripeClient stripeClient = new StripeClient("sk_live_w31SFjS7TBwBdEhGDT6fRbEQ");
                    if (u.Paused)
                    {
                        dynamic response = stripeClient.UpdateCustomersSubscription(u.CustomerId, "base");
                        _repo.unPause(u);
                    }
                    else
                    {
                        dynamic response = stripeClient.CancelCustomersSubscription(u.CustomerId, true);
                        _repo.Pause(u);
                    }
                    return Request.CreateResponse(HttpStatusCode.OK);
                }
            }
        }

        [HttpGet]
        public HttpResponseMessage ValidToken(string token)
        {
            using (NikahFactoryRepo _repo = new NikahFactoryRepo())
            {
                if (_repo.validToken(token))
                    return Request.CreateResponse(HttpStatusCode.OK);
                else
                    return Request.CreateErrorResponse(HttpStatusCode.PaymentRequired, new Exception("Invalid Token"));
            }
        }

        [HttpPost]
        public HttpResponseMessage Contact([FromBody] ContactRequest contactRequest)
        {
            var client = new SmtpClient("outlook.office365.com")
            {
                Credentials = new NetworkCredential("info@nikahfactory.com", "email-486170"),
                EnableSsl = true
            };
            client.Send("info@nikahfactory.com", "info@nikahfactory.com", "Contact Form", contactRequest.ToString());
            return Request.CreateResponse(HttpStatusCode.Created);
        }

        [HttpPost]
        public HttpResponseMessage Stripe([FromBody] dynamic stripeEvent)
        {
            string type = stripeEvent["type"];
            string customerID = stripeEvent["data"]["object"]["customer"];
            using (NikahFactoryRepo _repo = new NikahFactoryRepo())
            {
                if (type == "customer.subscription.deleted")
                    _repo.cancelUser(customerID);
            }
            return Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}