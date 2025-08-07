
namespace LorePath
{ 
    using UnityEngine;
    using System.Collections;

    /// <summary>
    /// A singleton supporting safe referencing, runtime generation, and pre-existing instances,
    /// with conflict resolution / duplicate self-destruction, and custom initialization.
    /// </summary>    
    public abstract class PSingleton<T> : MonoBehaviour where T : PSingleton<T>
    {
        public static bool IsQuitting => _isQuitting;
        protected static bool _isQuitting = false;
        
        private static T _instanceBacker = null;

        protected static T _instance
        {
            get
            {
                // if the quit flag is true whether because we haven't yet reset it, or 
                // because we're actually quitting, we need to find the instance in a non-instantiating manner.
                if (_isQuitting)
                {
                    string type = typeof(T).ToString();
                    //if (_safeInstance == null) Debug.LogWarning($"{nameof(T)} singleton accessed while quitting could not be found.");
                    return _safeInstance;
                }
                
                // The first time the instance is required, we find it.
                if (_instanceBacker == null)
                {
                    _instanceBacker = Object.FindObjectOfType<T>();
                    if (_instanceBacker == null)
                    {
                        // if we could not find it, we make it.                        
                        _instanceBacker = new GameObject("[Singleton]" + nameof(T), typeof(T)).GetComponent<T>();
                        Debug.Log($"{nameof(T)} singleton generated dynamically.", _instanceBacker);
                        _instanceBacker._wasGenerated = true;
                        _instanceBacker.OnGenerateSingleton();
                    }
                    _instanceBacker.InitSingleton();
                }
                return _instanceBacker;
            }
        }


        /// <summary>
        /// Returns the instance or finds one in the scene, but will not generate one dynamically.
        /// </summary>
        protected static T _safeInstance
        {
            get
            {
                if (_instanceBacker == null)
                {
                    _instanceBacker = Object.FindObjectOfType<T>();
                    if (_instanceBacker != null && !_isQuitting)
                    {
                        _instanceBacker.InitSingleton();
                    }
                }
                return _instanceBacker;
            }
        }

        /// <summary>
        /// the cheapest way to see if the singleton has been initialized, in case 
        /// auto-generation is not supported. 
        /// </summary>
        public static bool HasInstance => _instanceBacker != null;

        /// <summary>
        /// Returns true if we are not quitting and the instance exists in the scene.
        /// does not generate a new instance, but will find one if possible.
        /// </summary>
        public static bool Exists
        {
            get { return !_isQuitting && _safeInstance != null; }
        }



        protected bool _initializedAsInstance = false;
        protected bool _wasGenerated = false;


        protected virtual void Awake()
        {
            _isQuitting = false;
            if (VerifySingleton())
            {
                InitSingleton();
            }
        }


        /// <summary>
        /// A more reliable 'Awake' called only on the instance backer, either when it is fetched on demand or in Awake if not fetched.        
        /// </summary>
        private void InitSingleton() 
        {
            if (!_initializedAsInstance)
            {
                if (_instanceBacker == null) _instanceBacker = (T)this;
                Debug.Assert(_instanceBacker == this, $"InitSingleton called on {nameof(T)} that wasn't the instance ({this.name})" +
                    $". If you are overriding Awake(), make sure to call base.Awake() or otherwise VerifySingleton() to prevent duplicates.");                
                _initializedAsInstance = true;
                OnInitializeSingleton();
            }
        }


        /// <summary>
        /// Called as the throughput of InitSingleton() in the case of new initialization.        
        /// It will only be called once, and always called before the instance reference is used.
        /// unlike Awake, this is guaranteed to be called before any object uses the singleton
        /// </summary>        
        protected virtual void OnInitializeSingleton() { }
        
        /// <summary>
        /// Called if the singleton was generated on-demand due to not being in the scene.
        /// this can be used to set up components or throw an error if the singleton doesn't fully
        /// support runtime generation.
        /// </summary>
        protected virtual void OnGenerateSingleton() { }


       /// <summary>
       /// Ensures only one instance of the singleton exists at any time, destroying duplicates.
       /// called in awake for each instance.
       /// </summary>       
        protected bool VerifySingleton(bool destroyObject = true)
        {            
            if (_instanceBacker != null && _instanceBacker != this)
            {
                string destroyType = destroyObject ? "GameObject" : "script";                
                if (!this.ResolveSingletonConflict())
                {
                    Debug.LogWarning($"{this.GetType().Name} Singleton conflict; " +
                    $"{this.name} was initialized but instance exists ({_instance.name}). This {destroyType} will be destroyed.");
                    if (destroyObject) Destroy(this.gameObject);
                    else Destroy(this);
                }
                return false;
            }
            return true;
        }


        protected virtual void Log(string src, string msg = "", int threat = 0, Object context = null)
        {
            // usually this kicks back to a logger with log settings comparison but for now we just use the threat value
            switch (threat)
            {
                case 0:
                case 1:
                    Debug.Log($"[{this.GetType().Name}] {src}:{msg}", context);
                    break;
                case 2:
                    Debug.LogWarning($"[{this.GetType().Name}] {src}:{msg}", context);
                    break;
                default:
                    Debug.LogError($"[{this.GetType().Name}] {src}:{msg}", context);
                    break;
            }
            
        }

        /// <summary>
        /// This is called on any 'extra instance' of the singleton before it is destroyed and after Instance is defined.
        /// most scripts won't need it but it can be used to consolidate or clean up elements 
        /// that may have been accidentally spread accross multiple 'instances' in the scene.
        /// it is NOT called on the true instance, only on the duplicates in a conflict.
        /// </summary>
        /// <returns>
        /// returns true if the duplicate will resolve the conflict on its own, otherwise false.
        /// if false, the script or game object will be destroyed.
        /// </returns>
        protected virtual bool ResolveSingletonConflict() => false;


        protected virtual void OnApplicationQuit()
        {
            _isQuitting = true;
        }

        protected virtual void OnDestroy()
        {            
            if (_instanceBacker == this)
            {
                CleanUp();
                _instanceBacker = null;
            }
        }

        /// <summary>
        /// Cleanup is called only on the proper instance when it is destroyed, not on duplicates.
        /// </summary>
        protected virtual void CleanUp() { }
        
    }
}