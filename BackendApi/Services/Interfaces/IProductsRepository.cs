﻿using Poq.BackendApi.Models;

namespace Poq.BackendApi.Services.Interfaces
{
    public interface IProductsRepository
    {
        Task<int> CountAsync();
        Task<Product> GetAsync(int id);
        Task<List<Product>> SelectAsync();
        Task<List<Product>> SelectAsync(Func<Product, bool> predicate);
        Task<bool> WarmupAsync();
    }
}
