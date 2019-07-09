using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SDK
{
    public class AccountManager : Common.Singleton<AccountManager>
    {
        public USER_INFO UserInfo { get; set; }
    }
}
