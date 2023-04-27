using Microsoft.AspNetCore.Mvc;

namespace Wbtb.Core.Web
{
    public class Responses
    {
        #region METHODS

        /// <summary>
        /// Generic 404 JSON response
        /// </summary>
        /// <param name="description"></param>
        /// <returns></returns>
        public static JsonResult NotFoundError(string description)
        {
            return new JsonResult(new
            {
                statusCode = 404,
                error = new
                {
                    description = description
                }
            });
        }

        public static JsonResult UnknownError(string description, int code)
        {
            return new JsonResult(new
            {
                statusCode = 400,
                error = new
                {
                    code = code,
                    description = description
                }
            });
        }

        #endregion
    }
}
