using AutoMapper;
using DzenCode.Common.Entities;
using DzenCode.Common.Entities.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DzenCode.Common.Helpers
{
    //mapper for comments entity and dto cause i lazy to write it every time
    public class CommentProfile : Profile
    {
        public CommentProfile()
        {
            CreateMap<CommentDto, Comment>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Parent, opt => opt.Ignore())
                .ForMember(dest => dest.Replies, opt => opt.Ignore())
                .ForMember(dest => dest.ImagePath, opt => opt.Ignore())
                .ForMember(dest => dest.TextFilePath, opt => opt.Ignore());

            CreateMap<Comment, CommentResponseDto>()
                .ForMember(dest => dest.Replies, opt => opt.MapFrom(src => src.Replies));
        }
    }
}
