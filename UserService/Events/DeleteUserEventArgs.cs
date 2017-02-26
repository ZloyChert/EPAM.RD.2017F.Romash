using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserService.Events
{
    [Serializable]
    public class DeleteUserEventArgs
    {
        public DeleteUserEventArgs(Func<User, bool> deletePredicate)
        {
            DeletePredicate = deletePredicate;
        }

        public Func<User, bool> DeletePredicate { get; }
    }

}

