using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using UserService.Exceptions;
using UserService.IdCounters;

namespace UserService.Tests
{
    [TestFixture]
    public class UserServiceTests
    {
        [Test]
        public void Add_ArgumentNullExceptionExpected()
        {
            CounterId counter = new CounterId();
            Services.UserServiceMaster us = new Services.UserServiceMaster(counter);
            Assert.Throws(typeof(ArgumentNullException), () => us.Add(null));
        }

        [Test]
        public void Add_EmptyUserExceptionExpected()
        {
            CounterId counter = new CounterId();
            Services.UserServiceMaster us = new Services.UserServiceMaster(counter);
            User user = new User {FirstName = "Pavel"};
            Assert.Throws(typeof(EmptyUserException), () => us.Add(user));
        }

        [Test]
        public void Add_UserExistsExceptionExpected()
        {
            CounterId counter = new CounterId();
            Services.UserServiceMaster us = new Services.UserServiceMaster(counter);
            User user = new User
            {
                FirstName = "Pavel",
                LastName = "Romash"
            };
            us.Add(new User
            {
                FirstName = "Pavel",
                LastName = "Romash"
            });
            Assert.Throws(typeof(UserExistsException), () => us.Add(user));
        }
    }
}
