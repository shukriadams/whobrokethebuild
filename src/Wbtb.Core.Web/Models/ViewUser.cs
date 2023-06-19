using Wbtb.Core.Common;

namespace Wbtb.Core.Web
{
	public class ViewUser : User
	{
		public static ViewUser Copy(User user)
		{
			if (user == null)
				return null;

			return new ViewUser
			{
				AuthPlugin = user.AuthPlugin,
				Description = user.Description,
				Enable = user.Enable,
				Id = user.Id,
				Image = user.Image,
				Key = user.Key,
				KeyPrev = user.KeyPrev,
				Message = user.Message,
				Name = user.Name,
				SourceServerIdentities = user.SourceServerIdentities
			};
		}
	}
}
