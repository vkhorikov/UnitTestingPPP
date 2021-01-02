using System;

namespace Book.Chapter11.Time
{
    public static class DateTimeServer
    {
        private static Func<DateTime> _func;
        public static DateTime Now => _func();

        public static void Init(Func<DateTime> func)
        {
            _func = func;
        }
    }

    /*
    // Initialization code for production
    DateTimeServer.Init(() => DateTime.UtcNow);

    // Initialization code for unit tests
    DateTimeServer.Init(() => new DateTime(2016, 5, 3));
    */
}
