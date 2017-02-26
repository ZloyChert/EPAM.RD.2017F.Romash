using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using UserService.Events;
using UserService.Exceptions;
using UserService.IdCounters;

namespace UserService.Services
{
    [Serializable]
    public class UserServiceMaster : MarshalByRefObject, IUserServiceMaster, IDisposable
    {
        private readonly List<User> userList;
        private readonly ICounterId idCounter;
        public event EventHandler<AddUserEventArgs> AddUser = delegate { };
        public event EventHandler<DeleteUserEventArgs> DeleteUser = delegate { };
        public event EventHandler<AddUserEventArgs> AddUserOnSlaveCreating = delegate { };
        public int CountOfRemainingSlaves { get; private set;}

        /// <summary>
        /// Public constructor
        /// </summary>
        /// <param name="idCounter">Class that counts id</param>
        public UserServiceMaster(ICounterId idCounter)
        {
            if (idCounter == null)
                throw new ArgumentNullException();
            this.idCounter = idCounter;
            userList = new List<User>();
            Deserialize();

            int tempCount;
            if (!int.TryParse(ConfigurationManager.AppSettings["CountOfSlaves"], out tempCount))
                tempCount = 1;
            CountOfRemainingSlaves = tempCount;
        }

        public bool TryGetNextSlaveInstance(out IUserService slaveService)
        {
            int countOfSlaves = int.Parse(ConfigurationManager.AppSettings["CountOfSlaves"]);
            if (CountOfRemainingSlaves > 0)
            {
                AppDomain firstDomain = AppDomain.CreateDomain($"Domain_{countOfSlaves - CountOfRemainingSlaves + 1}");
                var instance = firstDomain.CreateInstanceAndUnwrap("UserService",
                    "UserService.Services.UserServiceSlave", true, BindingFlags.Default, null, new[] {(object) this},
                    CultureInfo.CurrentCulture, null);
                slaveService = (IUserService) instance;
                CountOfRemainingSlaves--;
                OnAddUserOnCreatingSlave(new AddUserEventArgs(userList));
                return true;
            }
            slaveService = null;
            return false;
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
            OnAddUser(new AddUserEventArgs(user));
        }

        public void Add(IEnumerable<User> users)
        {
            if (users == null)
                throw new ArgumentNullException();
            List<User> tempUsers = new List<User>();
            foreach (var user in users)
            {
                if (user?.FirstName == null || user.LastName == null)
                    continue;
                idCounter.CountId(user);
                if (userList.Exists(u => u.Id == user.Id))
                    continue;
                userList.Add(user);
                tempUsers.Add(user);
            }
            OnAddUser(new AddUserEventArgs(tempUsers));
        }

        /// <summary>
        /// Searching elements by predicate
        /// </summary>
        /// <param name="searchPredicate">Func that checks on each element</param>
        /// <returns>Sequence off elements</returns>
        public IEnumerable<User> Search(Func<User, bool> searchPredicate)
        {
            if (searchPredicate == null)
                throw new ArgumentNullException();
            return userList.Where(searchPredicate).ToList();
        }

        /// <summary>
        /// Deleting an element or elements frome the sequnce
        /// </summary>
        /// <param name="deletePredicate">Func that checks on each element</param>
        public void Delete(Func<User, bool> deletePredicate)
        {
            if (deletePredicate == null)
                throw new ArgumentNullException();
            foreach (var user in Search(deletePredicate))
            {
                userList.Remove(user);
            }
            OnDeleteUser(new DeleteUserEventArgs(deletePredicate));
        }

        protected virtual void OnAddUser(AddUserEventArgs e)
        {
            EventHandler<AddUserEventArgs> temp = AddUser;
            temp?.Invoke(this, e);
        }

        protected virtual void OnAddUserOnCreatingSlave(AddUserEventArgs e)
        {
            EventHandler<AddUserEventArgs> temp = AddUserOnSlaveCreating;
            temp?.Invoke(this, e);
        }

        protected virtual void OnDeleteUser(DeleteUserEventArgs e)
        {
            EventHandler<DeleteUserEventArgs> temp = DeleteUser;
            temp?.Invoke(this, e);
        }

        #region XML

        private void Serialize()
        {
            string fileName = ConfigurationManager.AppSettings["NameOfXmlFile"];
            var writer = new XmlTextWriter(fileName, Encoding.ASCII);
            WriteXml(writer);
            writer.Close();
        }

        private void Deserialize()
        {
            string fileName = ConfigurationManager.AppSettings["NameOfXmlFile"];
            var reader = new XmlTextReader(fileName);
            if (File.Exists(fileName))
                ReadXml(reader);
        }

        protected void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("UserStorage");
            foreach (var user in userList)
            {
                writer.WriteStartElement("User");
                writer.WriteAttributeString("FirstName", user.FirstName);
                writer.WriteAttributeString("LastName", user.LastName);
                writer.WriteAttributeString("Age", user.Age.ToString());
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }

        protected void ReadXml(XmlReader reader)
        {
            if (reader.MoveToContent() == XmlNodeType.Element && reader.LocalName == "UserStorage")
            {
                List<User> tempUsers = new List<User>();
                if (reader.ReadToDescendant("User"))
                {
                    while (reader.MoveToContent() == XmlNodeType.Element && reader.LocalName == "User")
                    {
                        User user = new User
                        {
                            FirstName = reader["FirstName"],
                            LastName = reader["LastName"],
                            Age = int.Parse(reader["Age"])
                        };
                        reader.Read();
                        tempUsers.Add(user);
                    }
                    Add(tempUsers);
                }
                reader.Read();
            }

        }

        #endregion
        #region Dispose
        public void Dispose()
        {
            Serialize();
            GC.SuppressFinalize(this);
        }

        ~UserServiceMaster()
        {
            Serialize();
        }
        #endregion
    }
}
