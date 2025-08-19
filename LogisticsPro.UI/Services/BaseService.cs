using System;
using System.Collections.Generic;
using System.Linq;

namespace LogisticsPro.UI.Services
{
    /// <summary>
    /// Generic base service to provide common CRUD operations for models
    /// </summary>
    /// <typeparam name="T">Model type with an Id property</typeparam>
    public abstract class BaseService<T> where T : class
    {
        protected readonly List<T> MockData;
        protected readonly Func<T, int> GetId;
        protected readonly Action<T, int> SetId;
        
        /// <summary>
        /// Constructor for BaseService
        /// </summary>
        /// <param name="mockData">Initial mock data</param>
        /// <param name="getId">Function to get the ID from an entity</param>
        /// <param name="setId">Action to set the ID on an entity</param>
        protected BaseService(List<T> mockData, Func<T, int> getId, Action<T, int> setId)
        {
            MockData = mockData;
            GetId = getId;
            SetId = setId;
        }
        
        /// <summary>
        /// Get all entities
        /// </summary>
        /// <returns>List of all entities</returns>
        public virtual List<T> GetAll()
        {
            return MockData;
        }
        
        /// <summary>
        /// Get entity by ID
        /// </summary>
        /// <param name="id">Entity ID</param>
        /// <returns>Found entity or null</returns>
        public virtual T GetById(int id)
        {
            return MockData.FirstOrDefault(e => GetId(e) == id);
        }
        
        /// <summary>
        /// Add a new entity
        /// </summary>
        /// <param name="entity">Entity to add</param>
        public virtual void Add(T entity)
        {
            // Generate a new ID
            var newId = MockData.Count > 0 ? MockData.Max(e => GetId(e)) + 1 : 1;
            SetId(entity, newId);
            
            MockData.Add(entity);
        }
        
        /// <summary>
        /// Update an existing entity
        /// </summary>
        /// <param name="entity">Entity with updated values</param>
        /// <returns>True if updated, false if not found</returns>
        public virtual bool Update(T entity)
        {
            var id = GetId(entity);
            var existingIndex = MockData.FindIndex(e => GetId(e) == id);
            
            if (existingIndex >= 0)
            {
                MockData[existingIndex] = entity;
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// Delete an entity by ID
        /// </summary>
        /// <param name="id">Entity ID</param>
        /// <returns>True if deleted, false if not found</returns>
        public virtual bool Delete(int id)
        {
            var entity = GetById(id);
            if (entity != null)
            {
                return MockData.Remove(entity);
            }
            
            return false;
        }
    }
}