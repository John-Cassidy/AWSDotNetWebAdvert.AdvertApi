using AdvertApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvertApi.Services
{
    public interface IAdvertStorageService
    {
        Task<string> AddAsync(AdvertModel model);

        Task ConfirmAsync(ConfirmAdvertModel model);

        Task<bool> CheckHealthAsync();
    }
}
