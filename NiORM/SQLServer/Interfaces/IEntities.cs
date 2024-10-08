﻿using System.Linq.Expressions;

namespace NiORM.SQLServer.Interfaces
{
    public interface IEntities<T> where T : ITable
    {
        List<T> ToList();
        List<T> ToList(string whereQuery);
        T AddReturn(T entity);
        void Add(T entity); 
        void Edit(T entity);
        void Remove(T entity);

        List<T> Where((string, string) Predict);
        List<T> Where(Expression<Func<T, bool>> predicate);
        List<T> Query(string Query);
        T Find(int firstId, string secondId);
        T Find(int id);
        T Find(string id);
        T FirstOrDefault(string Query);
        T FirstOrDefault();
        void Execute(string Query);
        List<T> List();
        List<T> List(string Query);

    }
}