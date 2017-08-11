using System;
using System.Linq;

using Library.TypeHelper;

namespace DataDefine
{
    public class Account
    {
        public enum COMPETENCE
        {
            [EnumDescription("帳號管理")]
            ACCOUNT_MANAGER,

            [EnumDescription("遊戲體驗")]
            GAME_PLAYER,

            [EnumDescription("帳號查詢")]
            ACCOUNT_FINDER,

            ACCOUNT_MANAGER2
        }

        public Guid Id;

        public string Name;

        public string Password;

        public Flag<COMPETENCE> Competences { get; set; }

        public Account()
        {
            Id = Guid.NewGuid();
            Name = Id.ToString();
            Password = Id.ToString();
        }

        public bool IsPassword(string password)
        {
            return Password == password;
        }

        public bool IsPlayer()
        {
            return _HasCompetence(COMPETENCE.GAME_PLAYER);
        }

        private bool _HasCompetence(COMPETENCE competence)
        {
            return Competences[competence];
        }

        public bool HasCompetence(COMPETENCE competence)
        {
            return _HasCompetence(competence);
        }

        public static Flag<COMPETENCE> AllCompetence()
        {
            var flags = EnumHelper.GetEnums<COMPETENCE>().ToArray();
            return new Flag<COMPETENCE>(flags);
        }
    }
}
