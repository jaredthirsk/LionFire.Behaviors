#if NoDeps
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace LionFire
{
    public static class Singleton<T>
      where T : class, new()
    {
        //static Singleton()
        //{
        //}

        public static T Instance
        {
            get
            {
                return lazyInstance.Value;
            }
        }
        private static readonly Lazy<T> lazyInstance = new Lazy<T>(() => new T());

        //public static readonly T Instance =
        //  typeof(T).InvokeMember(typeof(T).Name,
        //                         BindingFlags.CreateInstance |
        //                         BindingFlags.Instance |
        //                         BindingFlags.NonPublic | BindingFlags.Public,
        //                         null, null, null) as T;
    }

    // Better?
  //  public static class Singleton<T>
  //where T : class
  //  {
  //      static volatile T _instance;
  //      static object _lock = new object();

  //      static Singleton()
  //      {
  //      }

  //      public static T Instance
  //      {
  //          get
  //          {
  //              try
  //              {
  //                  if (_instance == null)
  //                      lock (_lock)
  //                      {
  //                          if (_instance == null)
  //                              _instance = typeof(T).InvokeMember(typeof(T).Name,
  //                                 BindingFlags.CreateInstance | BindingFlags.Instance |
  //                                 BindingFlags.NonPublic, null, null, null) as T;
  //                      }

  //                  return _instance;
  //              }
  //              catch (Exception exception)
  //              {
  //                  throw new SingletonException(exception);
  //              }
  //          }
  //      }
  //  }
}
#endif