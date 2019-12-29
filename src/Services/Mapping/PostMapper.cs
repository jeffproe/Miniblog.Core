using System.Linq;
using AutoMapper;
using Miniblog.Core.Database.Models;
using Miniblog.Core.Models;

namespace src.Services.Mapping
{
	public class PostMapper : Profile
	{
		public PostMapper()
		{
			CreateMap<Post, PostVM>()
				.ForMember(dest => dest.Comments, opt => opt.MapFrom(src => src.Comments))
				.ForMember(dest => dest.Categories, opt => opt.MapFrom(src => src.PostCategories.Select(x => x.Category.Name).ToList()))
				.ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
				.ReverseMap();

			CreateMap<Comment, CommentVM>()
				.ReverseMap();
		}
	}
}