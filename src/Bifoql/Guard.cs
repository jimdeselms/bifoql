namespace Bifoql
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    /// You can create custom functions that take up to three parametrs.
    public static class Guard
    {
        public static void ArgumentNotNull(object argument, string name)
        {
            if (argument == null)
            {
                throw new ArgumentNullException(name);
            }
        }
    }
}