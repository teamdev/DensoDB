using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using denso.twitterlike.model;
using DeNSo.Meta.BSon;
using DeNSo.Meta;

namespace densodb.twitter.eventHandlers
{
  // In this example we assume the server is server-side or messages are dispatched to ALL nodes in the P2P mesh.

  // The command Attribute, let you intercept event messages BEFORE the Default event handlers. 
  // So you can customize event handlers logic and write your own server-side procedures. 
  [DeNSo.Meta.Command(Action = "Set", MessageType = typeof(Message), Method = "Set")]
  public static class MessageHandler
  {
    // A method that handles densodb events MUST have this delegate.
    public static void Set(IStore dbstore, BSonDoc command)
    {
      // IStore interface gives you a lowlevel access to DB Structure, 
      // Every action you take now will jump directly into DB without any event dispatching

      // The Istore is preloaded from densodb, and you should not have access to densodb internals.

      // Now deserialize message from Bson object. 
      // should be faster using BsonObject directly but this way is more clear. 
      var message = command.FromBSon<Message>();

      // Get the sender UserProfile
      var userprofile = dbstore.GetCollection("users").Where(d => d["UserName"].ToString() == message.From).FirstOrDefault().FromBSon<UserProfile>();
      
      if (userprofile != null)
      {
        // add message to user's messages
        var profilemessages = dbstore.GetCollection(string.Format("messages_{0}", userprofile.UserName));
        profilemessages.Set(command);

        // add message to user's wall 
        var profilewall = dbstore.GetCollection(string.Format("wall_{0}", userprofile.UserName));
        profilewall.Set(command);

        // Now i have user's follower. 
        foreach (var follower in userprofile.FollowedBy)
        {
          // Get followers's wall 
          var followerwall = dbstore.GetCollection(string.Format("wall_{0}", follower));

          // store the messages in follower's wall. 
          followerwall.Set(command);
        }
      }
    }
  }
}
