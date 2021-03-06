﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;
using NLog;
using UserService.AOP;
using UserService.Events;
using UserService.Exceptions;
using UserService.IdCounters;
using UserService.TCP;

namespace UserService.Services
{
    [Serializable]
    public class UserServiceMaster : MarshalByRefObject, IUserServiceMaster, IDisposable
    {
        
        private static readonly ReaderWriterLock Rwl = new ReaderWriterLock();
        private static readonly int time = 10000;
        private readonly List<User> userList;
        private readonly ICounterId idCounter;
        public event EventHandler<UserEventArgs> AddUser = delegate { };
        public event EventHandler<UserEventArgs> DeleteUser = delegate { };
        public event EventHandler<UserEventArgs> AddUserOnSlaveCreating = delegate { };

        [LogMethods]
        public void ConnectToTcpServer(ServerTcp server)
        {
            if ( server == null)
                throw new ArgumentNullException();
            server.ConnectToMasterService(this);
        }

        /// <summary>
        /// Public constructor
        /// </summary>
        /// <param name="idCounter">Class that counts id</param>
        [LogMethods]
        public UserServiceMaster(ICounterId idCounter)
        {
            if (idCounter == null)
                throw new ArgumentNullException();
            this.idCounter = idCounter;
            userList = new List<User>();
            Deserialize();
        }

        /// <summary>
        /// Add an element in service
        /// </summary>
        /// <param name="user">The element to add</param>
        [LogMethods]
        public void Add(User user)
        {
            if (user == null)
                throw new ArgumentNullException();
            if (user.FirstName == null || user.LastName == null)
                throw new EmptyUserException();
            idCounter.CountId(user);
            if (userList.Exists(u => u.Id == user.Id))
                throw new UserExistsException();
            Rwl.AcquireWriterLock(time);
            userList.Add(user);
            Rwl.ReleaseWriterLock();
            OnAddUser(new UserEventArgs(user));
        }

        [LogMethods]
        public void Add(IEnumerable<User> users)
        {
            if (users == null)
                throw new ArgumentNullException();
            List<User> tempUsers = new List<User>();
            Rwl.AcquireWriterLock(time);
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
            Rwl.ReleaseWriterLock();
            OnAddUser(new UserEventArgs(tempUsers));
        }

        /// <summary>
        /// Searching elements by predicate
        /// </summary>
        /// <param name="searchPredicate">Func that checks on each element</param>
        /// <returns>Sequence off elements</returns>
        [LogMethods]
        public IEnumerable<User> Search(Func<User, bool> searchPredicate)
        {
            if (searchPredicate == null)
                throw new ArgumentNullException();
            Rwl.AcquireReaderLock(time);
            var listUsers = userList.Where(searchPredicate).ToList();
            Rwl.ReleaseReaderLock();
            return listUsers;
        }

        /// <summary>
        /// Deleting an element or elements frome the sequnce
        /// </summary>
        /// <param name="deletePredicate">Func that checks on each element</param>
        [LogMethods]
        public void Delete(Func<User, bool> deletePredicate)
        {
            if (deletePredicate == null)
                throw new ArgumentNullException();
            List<User> tempUsers = Search(deletePredicate).ToList();
            Rwl.AcquireWriterLock(time);
            foreach (var user in Search(deletePredicate))
            {
                userList.Remove(user);
            }           
            Rwl.ReleaseWriterLock();
            OnDeleteUser(new UserEventArgs(tempUsers));
        }

        [LogMethods]
        protected virtual void OnAddUser(UserEventArgs e)
        {
            EventHandler<UserEventArgs> temp = AddUser;
            temp?.Invoke(this, e);
        }

        [LogMethods]
        protected virtual void OnAddUserOnCreatingSlave()
        {
            EventHandler<UserEventArgs> temp = AddUserOnSlaveCreating;
            temp?.Invoke(this, new UserEventArgs(userList));
        }

        [LogMethods]
        protected virtual void OnDeleteUser(UserEventArgs e)
        {
            EventHandler<UserEventArgs> temp = DeleteUser;
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
