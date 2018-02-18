using AutoMapper;
using Export.Algorithm;
using Export.Entities;
using Export.PluginDataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Export.Plugin
{
    public static class PluginAutoMapperConfiguration
    {
        public static void Configure()
        {
            ConfigureEpisodeMapping();
            ConfigureDBManagerMapping();
        }

        private static void ConfigureEpisodeMapping()
        {
            Mapper.CreateMap<CalculatedInterval, Interval>();
            Mapper.CreateMap<Interval, CalculatedInterval>();
        }

        private static void ConfigureDBManagerMapping()
        {
            Mapper.CreateMap<ArtifactCountersModel, Interval>()
                .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => src.SampleFromDate))
                .ForMember(dest => dest.EndTime, opt => opt.MapFrom(src => src.SampleToDate));

            Mapper.CreateMap<ArtifactCountersExportedModel, Interval>()
                .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => src.SampleFromDate))
                .ForMember(dest => dest.EndTime, opt => opt.MapFrom(src => src.SampleToDate));

            Mapper.CreateMap<Interval, ArtifactCountersModel>()
                .ForMember(dest => dest.SampleFromDate, opt => opt.MapFrom(src => src.StartTime))
                .ForMember(dest => dest.SampleToDate, opt => opt.MapFrom(src => src.EndTime));
            Mapper.CreateMap<Interval, ArtifactCountersExportedModel>()

                .ForMember(dest => dest.SampleFromDate, opt => opt.MapFrom(src => src.StartTime))
                .ForMember(dest => dest.SampleToDate, opt => opt.MapFrom(src => src.EndTime));
        }
    }
}
