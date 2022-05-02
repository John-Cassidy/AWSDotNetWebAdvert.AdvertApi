using AdvertApi.Entities;
using AdvertApi.Models;
using AutoMapper;

namespace AdvertApi.Mapper {
    public class AdvertProfile : Profile {
        public AdvertProfile() {
            CreateMap<Advert, AdvertModel>().ReverseMap();
        }
    }
}
