
namespace LorePath.Stormsign
{

    using System;
    using System.Collections.Generic;

    //using LorePath;
    //using DG.Tweening;

    public interface IDataObject { }
    
    public static class Blackboard
    {
        private static Dictionary<Type, IDataObject> _content = new();

        public static bool Has<T>() where T : class, IDataObject => _content.TryGetValue(typeof(T), out _); // apparently ContainsKey() is unreliable on some platforms :(

        public static void Set<T>(T data) where T : class, IDataObject
        {
            _content[typeof(T)] = data;
        }

        public static bool Get<T>(out T data) where T : class, IDataObject
        {
            data = default;
            if (_content.TryGetValue(typeof(T), out var content))
            {
                data = content as T;
                return true;
            }

            return false;

        }
    }


}


// q // cD // d
// Unity 6000.0.41f1
