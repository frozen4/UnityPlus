using System;
using System.Collections.Generic;

namespace Hoba.ObjectPool
{
     public class ObjectPool<T> where T : PooledObject
    {
        public class ObjectPoolDiagnostics
        {
            #region Public Properties and backing fields

            /// gets the total count of live instances, both in the pool and in use.
            public int TotalLiveInstancesCount
            {
                get { return _TotalInstancesCreated - _TotalInstancesDestroyed; }
            }

            internal int _ObjectResetFailedCount;
            /// gets the count of object reset failures occured while the pool tried to re-add the object into the pool.
            public int ObjectResetFailedCount
            {
                get { return _ObjectResetFailedCount; }
            }

            internal int _ReturnedToPoolByRessurectionCount;
            /// gets the total count of object that has been picked up by the GC, and returned to pool. 
            public int ReturnedToPoolByRessurectionCount
            {
                get { return _ReturnedToPoolByRessurectionCount; }
            }

            internal int _PoolObjectHitCount;
            /// gets the total count of successful accesses. The pool had a spare object to provide to the user without creating it on demand.
            public int PoolObjectHitCount
            {
                get { return _PoolObjectHitCount; }
            }

            internal int _PoolObjectMissCount;
            /// gets the total count of unsuccessful accesses. The pool had to create an object in order to satisfy the user request. If the number is high, consider increasing the object minimum limit.
            public int PoolObjectMissCount
            {
                get { return _PoolObjectMissCount; }
            }

            internal int _TotalInstancesCreated;
            /// gets the total number of pooled objected created
            public int TotalInstancesCreated
            {
                get { return _TotalInstancesCreated; }
            }

            internal int _TotalInstancesDestroyed;
            /// gets the total number of objects destroyes, both in case of an pool overflow, and state corruption.
            public int TotalInstancesDestroyed
            {
                get { return _TotalInstancesDestroyed; }
            }

            internal int _PoolOverflowCount;
            /// gets the number of objects been destroyed because the pool was full at the time of returning the object to the pool.
            public int PoolOverflowCount
            {
                get { return _PoolOverflowCount; }
            }

            internal int _ReturnedToPoolCount;
            /// gets the total count of objects that been successfully returned to the pool
            public int ReturnedToPoolCount
            {
                get { return _ReturnedToPoolCount; }
            }
            #endregion

            #region Internal Methods for incrementing the counters
            internal void IncrementObjectsCreatedCount()
            {
                _TotalInstancesCreated++;
            }

            internal void IncrementObjectsDestroyedCount()
            {
                _TotalInstancesDestroyed++;
            }

            internal void IncrementPoolObjectHitCount()
            {
                _PoolObjectHitCount++;
            }

            internal void IncrementPoolObjectMissCount()
            {
                _PoolObjectMissCount++;
            }

            internal void IncrementPoolOverflowCount()
            {
                _PoolOverflowCount++;
            }

            internal void IncrementResetStateFailedCount()
            {
                _ObjectResetFailedCount++;
            }

            internal void IncrementObjectRessurectionCount()
            {
                _ReturnedToPoolByRessurectionCount++;
            }

            internal void IncrementReturnedToPoolCount()
            {
                _ReturnedToPoolCount++;
            }
            #endregion
        }

        #region Private Members
        private Queue<T> PooledObjects { get; set; }
        private int _MinimumPoolSize = 0;
        private int _MaximumPoolSize = 0;
        private Func<T> _FactoryMethod = null;
        private Action<PooledObject, bool> _ReturnToPoolAction = null;
        #endregion

        public ObjectPoolDiagnostics Diagnostics { get; private set; }

        #region C'tor and Initialization code
        public ObjectPool(int minimumPoolSize, int maximumPoolSize, Func<T> factoryMethod)
        {
            InitializePool(minimumPoolSize, maximumPoolSize, factoryMethod);
        }

        private void InitializePool(int minimumPoolSize, int maximumPoolSize, Func<T> factoryMethod)
        {
            // Validating pool limits, exception is thrown if invalid
            ValidatePoolLimits(minimumPoolSize, maximumPoolSize);

            // Assigning properties
            _FactoryMethod = factoryMethod;
            _MaximumPoolSize = maximumPoolSize;
            _MinimumPoolSize = minimumPoolSize;

            // Initializing the internal pool data structure
            PooledObjects = new Queue<T>();

            // Creating a new instnce for the Diagnostics class
            Diagnostics = new ObjectPoolDiagnostics();

            // Setting the action for returning to the pool to be integrated in the pooled objects
            _ReturnToPoolAction = ReturnObjectToPool;

            // Initilizing objects in pool
            AdjustPoolSizeToBounds();
        }

        #endregion

        #region Private Methods

        private void ValidatePoolLimits(int minimumPoolSize, int maximumPoolSize)
        {
            if (minimumPoolSize < 0)
            {
                throw new ArgumentException("Minimum pool size must be greater or equals to zero.");
            }

            if (maximumPoolSize < 1)
            {
                throw new ArgumentException("Maximum pool size must be greater than zero.");
            }

            if (minimumPoolSize > maximumPoolSize)
            {
                throw new ArgumentException("Maximum pool size must be greater than the maximum pool size.");
            }
        }

        private void AdjustPoolSizeToBounds()
        {
            // If we reached this point, we've set the AdjustPoolSizeIsInProgressCASFlag to 1 (true) - using the above CAS function
            // We can now safely adjust the pool size without interferences

            // Adjusting...
            while (PooledObjects.Count < _MinimumPoolSize)
            {
                PooledObjects.Enqueue(CreatePooledObject());
            }

            while (PooledObjects.Count > _MaximumPoolSize)
            {
                T dequeuedObjectToDestroy = PooledObjects.Dequeue();

                if (dequeuedObjectToDestroy != null)
                {
                    // Diagnostics update
                    Diagnostics.IncrementPoolOverflowCount();

                    DestroyPooledObject(dequeuedObjectToDestroy);
                }
            }
        }

        private T CreatePooledObject()
        {
            T newObject;
            if (_FactoryMethod != null)
            {
                newObject = _FactoryMethod();
            }
            else
            {
                // Throws an exception if the type doesn't have default ctor - on purpose! I've could've add a generic constraint with new (), but I didn't want to limit the user and force a parameterless c'tor
                newObject = (T)Activator.CreateInstance(typeof(T));
            }

            // Diagnostics update
            Diagnostics.IncrementObjectsCreatedCount();

            // Setting the 'return to pool' action in the newly created pooled object
            newObject.ReturnToPool = (Action<PooledObject, bool>)_ReturnToPoolAction;
            return newObject;
        }

        private void DestroyPooledObject(PooledObject objectToDestroy)
        {
            if(objectToDestroy == null) return;

            // Making sure that the object is only disposed once (in case of application shutting down and we don't control the order of the finalization)
            if (!objectToDestroy.Disposed)
            {
                // Deterministically release object resources, nevermind the result, we are destroying the object
                objectToDestroy.ReleaseResources();
                objectToDestroy.Disposed = true;

                // Diagnostics update
                Diagnostics.IncrementObjectsDestroyedCount();
            }

            // The object is being destroyed, resources have been already released deterministically, so we di no need the finalizer to fire
            GC.SuppressFinalize(objectToDestroy);
        }
        #endregion

        #region Pool Operations

        /// Get a monitored object from the pool. 
        public T GetObject()
        {
            T dequeuedObject = (PooledObjects.Count == 0) ? null : PooledObjects.Dequeue();
            
            if (dequeuedObject != null)
            {
                AdjustPoolSizeToBounds();

                // Diagnostics update
                Diagnostics.IncrementPoolObjectHitCount();

                return dequeuedObject;
            }
            else
            {
                // This should not happen normally, but could be happening when there is stress on the pool
                // No available objects in pool, create a new one and return it to the caller
                //Debug.log("Object pool failed to return a pooled object. pool is empty. consider increasing the number of minimum pooled objects.");

                // Diagnostics update
                Diagnostics.IncrementPoolObjectMissCount();

                return CreatePooledObject();
            }
        }

        internal void ReturnObjectToPool(PooledObject objectToReturnToPool, bool reRegisterForFinalization)
        {
            T returnedObject = (T)objectToReturnToPool;

            // Diagnostics update
            if (reRegisterForFinalization) Diagnostics.IncrementObjectRessurectionCount();

            // Checking that the pool is not full
            if (PooledObjects.Count < _MaximumPoolSize)
            {
                // Reset the object state (if implemented) before returning it to the pool. If reseting the object have failed, destroy the object
                if (!returnedObject.Reset())
                {
                    // Diagnostics update
                    Diagnostics.IncrementResetStateFailedCount();

                    DestroyPooledObject(returnedObject);
                    return;
                }

                // re-registering for finalization - in case of resurrection (called from Finalize method)
                if (reRegisterForFinalization)
                {
                    GC.ReRegisterForFinalize(returnedObject);
                }

                // Diagnostics update
                Diagnostics.IncrementReturnedToPoolCount();

                // Adding the object back to the pool 
                PooledObjects.Enqueue(returnedObject);
            }
            else
            {
                // Diagnostics update
                Diagnostics.IncrementPoolOverflowCount();

                //The Pool's upper limit has exceeded, there is no need to add this object back into the pool and we can destroy it.
                DestroyPooledObject(returnedObject);
            }
        }

        #endregion


        #region DiagnosticsInfo

        public void ShowDiagnostics(string tagName)
        {
            Common.HobaDebuger.LogWarningFormat("{8} Diagnostics: TotalLiveInstancesCount = {0}, ReturnedToPoolByRessurectionCount = {1}, PoolObjectHitCount = {2}, PoolObjectMissCount = {3}, TotalInstancesCreated = {4}, TotalInstancesDestroyed = {5}, PoolOverflowCount = {6}, ReturnedToPoolCount = {7}",
                Diagnostics.TotalLiveInstancesCount,
                Diagnostics.ReturnedToPoolByRessurectionCount,
                Diagnostics.PoolObjectHitCount,
                Diagnostics.PoolObjectMissCount,
                Diagnostics.TotalInstancesCreated,
                Diagnostics.TotalInstancesDestroyed,
                Diagnostics.PoolOverflowCount,
                Diagnostics.ReturnedToPoolCount,
                tagName);
        }
        #endregion

        #region Finalizer

        ~ObjectPool()
        {
            // The pool is going down, releasing the resources for all objects in pool
            foreach (var item in PooledObjects)
            {
                DestroyPooledObject(item);
            }
        }
        #endregion
    }
}


