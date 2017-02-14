﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserService
{
    class CounterId : ICounterId
    {
        /// <summary>
        /// Method that gives algorithm of counting id of user
        /// </summary>
        /// <param name="user">User to set id</param>
        public void CountId(User user)
        {
            user.Id =  LyHash(user.FirstName) + LyHash(user.LastName) + LyHash(user.Age.ToString());
        }

        /// <summary>
        /// Hash Function
        /// </summary>
        /// <param name="str">string to find hash</param>
        /// <returns>32-bit hash</returns>
        private static int LyHash(string str)
        {
            return str.Aggregate(0, (current, t) => current * 1664525 + t + 1013904223);
        }
    }
}