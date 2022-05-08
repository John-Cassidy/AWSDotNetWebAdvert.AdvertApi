using AdvertApi.Entities;
using AdvertApi.Models;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AdvertApi.Services {
    public class DynamoDBAdvertStorage : IAdvertStorageService {

        private readonly IMapper _mapper;

        public DynamoDBAdvertStorage(IMapper mapper) {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<string> AddAsync(AdvertModel model) {
            var dbModel = _mapper.Map<Advert>(model);

            dbModel.Id = Guid.NewGuid().ToString();
            dbModel.CreationDateTime = DateTime.UtcNow;
            dbModel.Status = AdvertStatus.Pending;

            using (var client = new AmazonDynamoDBClient()) {
                var table = await client.DescribeTableAsync("Adverts");

                using (var context = new DynamoDBContext(client)) {
                    await context.SaveAsync(dbModel);
                }
            }
            return dbModel.Id;
        }

        public async Task<bool> CheckHealthAsync() {  
            using (var client = new AmazonDynamoDBClient()) {
                var tableData = await client.DescribeTableAsync("Adverts");
                return string.Compare(tableData.Table.TableStatus, "active", true) == 0;
            }
        }

        public async Task ConfirmAsync(ConfirmAdvertModel model) {
            using (var client = new AmazonDynamoDBClient()) {
                using (var context = new DynamoDBContext(client)) {
                    var record = await context.LoadAsync<Advert>(model.Id);
                    if (record == null)
                        throw new KeyNotFoundException($"A record with ID={model.Id} was not found.");
                    if (model.Status == AdvertStatus.Active) {                        
                        record.Status = AdvertStatus.Active;
                        await context.SaveAsync(record);
                    } else {
                        await context.DeleteAsync(record);
                    }
                }
            }
        }

        public async Task<AdvertModel> GetByIdAsync(string id) {
            using (var client = new AmazonDynamoDBClient()) {
                using (var context = new DynamoDBContext(client)) {
                    var dbModel = await context.LoadAsync<Advert>(id);
                    if (dbModel != null)
                        return _mapper.Map<AdvertModel>(dbModel);
                }
            }

            throw new KeyNotFoundException();
        }
    }
}
