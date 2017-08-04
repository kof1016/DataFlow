using System;

using Utility;

namespace DataDefine
{
    public interface IAccountFinder
    {
        Value<Account> FindAccountByName(string id);

        Value<Account> FindAccountById(Guid account_id);
    }
}