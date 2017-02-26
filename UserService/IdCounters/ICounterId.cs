namespace UserService.IdCounters
{
    public interface ICounterId
    {
        /// <summary>
        /// Method that gives algorithm of counting id of user
        /// </summary>
        /// <param name="user">User to set id</param>
        void CountId(User user);
    }
}
