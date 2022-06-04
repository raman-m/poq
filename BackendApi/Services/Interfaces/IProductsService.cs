﻿using Poq.BackendApi.Models;

namespace Poq.BackendApi.Services.Interfaces
{
    public interface IProductsService
    {
        Task<ICollection<Product>> AllAsync();
        Task<ICollection<Product>> FilterAsync(int? minPrice, int? maxPrice, Sizes? size);
    }
}