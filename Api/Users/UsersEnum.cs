using System.Collections;
using TourGuide.Users;

namespace TourGuide.Users
{
    public class UsersEnumerable : IEnumerable<User>
    {
        private User[] _usersEnum;
        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)GetEnumerator();
        }

        public UsersEnumerator GetEnumerator()
        {
            return new UsersEnumerator(_usersEnum);
        }

        IEnumerator<User> IEnumerable<User>.GetEnumerator()
        {
            return ((IEnumerable<User>)_usersEnum).GetEnumerator();
        }
    }
}

public class UsersEnumerator : IEnumerator
{
    public User[] _usersEnum;

    // Enumerators are positioned before the first element
    // until the first MoveNext() call.
    int position = -1;

    public UsersEnumerator(User[] list)
    {
        _usersEnum = list;
    }

    public bool MoveNext()
    {
        position++;
        return (position < _usersEnum.Length);
    }

    public void Reset()
    {
        position = -1;
    }

    object IEnumerator.Current
    {
        get
        {
            return Current;
        }
    }

    public User Current
    {
        get
        {
            try
            {
                return _usersEnum[position];
            }
            catch (IndexOutOfRangeException)
            {
                throw new InvalidOperationException();
            }
        }
    }
}