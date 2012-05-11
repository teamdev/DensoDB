using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace denso.twitterlike.model
{
  public class UserProfile
  {
    public string UserName { get; set; }
    public string UserFullName { get; set; }
    public string Description { get; set; }

    public List<string> Following { get; set; }
    public List<string> FollowedBy { get; set; }

  }
}
