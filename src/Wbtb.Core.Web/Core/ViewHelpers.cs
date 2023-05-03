using Humanizer;
using Microsoft.AspNetCore.Html;
using System;
using System.Web;
using Wbtb.Core.Common;
using Wbtb.Core.Common.Plugins;

namespace Wbtb.Core.Web
{
    public static class ViewHelpers
    {
        public static HtmlString PreserveLineBreaks(string content)
        { 
            if (content == null)
                content = string.Empty;

            content = content
                .Replace("\r\n", "<br/>")
                .Replace("\r", "<br/>");

            return new HtmlString(content);
        }

        /// <summary>
        /// Converts build status to siumpler css classes. This is a soft conversation - it can handle null builds for less logic in view, and returns empty string in that case.
        /// </summary>
        /// <param name="build"></param>
        /// <returns></returns>
        public static string BuildStatusToCSSClass(Build build)
        {
            if (build == null)
                return string.Empty;

            if (build.Status == BuildStatus.Failed)
                return "broken";

            if (build.Status == BuildStatus.Passed)
                return "passing";
            
            return "build";
        }
        
        /// <summary>
        /// Humanizer shorthand
        /// </summary>
        /// <param name="ts"></param>
        /// <returns></returns>
        public static string Hmz(TimeSpan? ts)
        { 
            return ts.HasValue ? ts.Value.Humanize() : string.Empty;
        }

        /// <summary>
        /// Generates best possible user page link from build involveement. Involvement can contain no user info at all, a user string name from source control which isn't directly 
        /// associated with a known user, or a mapped user that is known.
        /// </summary>
        /// <param name="involvement"></param>
        /// <returns></returns>
        public static HtmlString BuildInvolvementUserLink(BuildInvolvement involvement)
        { 
            if (involvement == null)
                return new HtmlString(string.Empty);

            SimpleDI di = new SimpleDI();
            PluginProvider pluginProvider = di.Resolve<PluginProvider>();

            if (!string.IsNullOrEmpty(involvement.MappedUserId)){
                IDataLayerPlugin dataLayer = pluginProvider.GetFirstForInterface<IDataLayerPlugin>();
                User user = dataLayer.GetUserById(involvement.MappedUserId);
                return new HtmlString($"<a href=\"/user/{involvement.MappedUserId}\">{user.Name}</a>");
            }

            return new HtmlString($"<span class=\"\">User not resolved</span>");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="involvement"></param>
        /// <returns></returns>
        public static HtmlString BuildInvolvementUserLink(ViewBuildInvolvement involvement)
        {
            if (involvement == null)
                return new HtmlString(string.Empty);

            SimpleDI di = new SimpleDI();
            PluginProvider pluginProvider = di.Resolve<PluginProvider>();

            if (!string.IsNullOrEmpty(involvement.MappedUserId))
            {
                IDataLayerPlugin dataLayer = pluginProvider.GetFirstForInterface<IDataLayerPlugin>();
                User user = dataLayer.GetUserById(involvement.MappedUserId);
                return new HtmlString($"<a href=\"/user/{involvement.MappedUserId}\">{user.Name}</a>");
            }

            if (involvement.Revision != null) 
                return new HtmlString($"<span class=\"\">{involvement.Revision.User} (user unresolved)</span>");

            return new HtmlString($"<span class=\"\">revision data unresolved</span>");
        }

        public static HtmlString BuildLink(Build build)
        { 
            if (build == null)
                return new HtmlString(string.Empty);

            return new HtmlString($"<a href=\"/build/{build.Id}\">{build.Identifier}</a>");
        }

        public static HtmlString JobLink(Job job)
        {
            if (job == null)
                return new HtmlString(string.Empty);

            return new HtmlString($"<a href=\"/job/{job.Id}\">{job.Name}</a>");
        }

        public static HtmlString BuildHostLink(Build build)
        {
            if (build == null)
                return new HtmlString(string.Empty);

            return new HtmlString($"<a href=\"/buildhost/{HttpUtility.UrlEncode(build.Hostname)}\">{build.Hostname}</a>");
        }


        /// <summary>
        /// Shorthand to force html string something
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static HtmlString String(string html)
        {
            return new HtmlString(html);
        }


        /// <summary>
        /// Translates build delta into a user-friendly string that clearly communicates status of build without assuming deep technical knowledge.
        /// </summary>
        /// <param name="build"></param>
        /// <returns></returns>
        public static string TranslateBuildStatus(Build build)
        { 
            // first try to use build delta 
            string result = string.Empty;

            switch(build.Delta)
            { 
                case BuildDelta.Pass:
                    result = "Passing";
                    break;

                case BuildDelta.Restore:
                    result = "Fixed build";
                    break;

                case BuildDelta.Broke:
                    result = "Broke build";
                    break;

                case BuildDelta.ContinuedBreak:
                    result = "Already broken";
                    break;
            }

            if (result == string.Empty)
                switch(build.Status)
                { 
                    case BuildStatus.Failed:
                        result = "Broken";
                        break;

                    case BuildStatus.Passed:
                        result = "Passing";
                        break;

                    case BuildStatus.InProgress:
                        result = "Building";
                        break;

                }

            if (result == string.Empty)
                result = "Unknown";

            return result;
        }

        public static string GistOf(string text, int length, string overflow = "...")
        { 
            if (text.Length < length + overflow.Length)
                return text;
            
            return $"{text.Substring(0, length)}{overflow}";
        }

        public static HtmlString PagerBar<T>(string baseUrl, PageableData<T> data, Config config)
        {
            Pager pager = new Pager();
            return new HtmlString(pager.Render(data, config.PagesPerPageGroup, baseUrl, "page"));
        }

        public static string Duration(Build build)
        { 
            if (!build.EndedUtc.HasValue)
                return string.Empty;
            
            TimeSpan duration = build.EndedUtc.Value - build.StartedUtc;
            return duration.Humanize();
        }

        public static string Duration(DateTime after, DateTime before)
        {
            TimeSpan duration = before - after;
            return duration.Humanize();
        }

        public static string TranslateBuildInvolvement(BuildInvolvement involvement)
        {
            switch(involvement.Blame)
            {
                case Blame.Innocent:
                    return "Not responsible";
                case Blame.Guilty:
                    return "Broke build";
                default:
                    return "Undermined";
            }
        }
    }
}
