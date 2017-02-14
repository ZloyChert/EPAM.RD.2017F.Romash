using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserService.Exceptions;

namespace UserService
{
    public class UserService
    {
        private readonly List<User> userList;
        private readonly ICounterId idCounter;

        /// <summary>
        /// Public constructor
        /// </summary>
        /// <param name="idCounter">Class that counts id</param>
        public UserService(ICounterId idCounter)
        {
            this.idCounter = idCounter;
            userList = new List<User>();
        }

        /// <summary>
        /// Add an element in service
        /// </summary>
        /// <param name="user">The element to add</param>
        public void Add(User user)
        {
            if (user == null)
                throw new ArgumentNullException();
            if (user.FirstName == null || user.LastName == null)
                throw new EmptyUserException();
            idCounter.CountId(user);
            if (userList.Exists(u => u.Id == user.Id))
                throw new UserExistsException();
            userList.Add(user);
        }

        /// <summary>
        /// Searching elements by predicate
        /// </summary>
        /// <param name="searchPredicate">Func that checks on each element</param>
        /// <returns>Sequence off elements</returns>
        public IEnumerable<User> Search(Func<User, bool> searchPredicate)
        {
            return userList.Where(searchPredicate);
        }

        /// <summary>
        /// Deleting an element or elements frome the sequnce
        /// </summary>
        /// <param name="deletePredicate">Func that checks on each element</param>
        /// <returns>True if deleting was ok, else if there was an error</returns>
        public bool TryDelete(Func<User, bool> deletePredicate)
        {
            try
            {
                foreach (var user in Search(deletePredicate))
                {
                    userList.Remove(user);
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
