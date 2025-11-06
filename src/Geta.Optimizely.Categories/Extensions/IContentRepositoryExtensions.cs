// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using EPiServer;
using EPiServer.Core;
using EPiServer.Core.Internal;
using EPiServer.DataAccess;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using EPiServer.Web;

namespace Geta.Optimizely.Categories.Extensions
{
    public static class ContentRepositoryExtensions
    {
        private static Injected<CategorySettings> CategorySettings;
        public static ContentReference GetOrCreateGlobalCategoriesRoot(this IContentRepository contentRepository)
        {
            var siteAssetsExists = SiteDefinition.Current.GlobalAssetsRoot != SiteDefinition.Current.SiteAssetsRoot;
            var name = siteAssetsExists ? "For All Sites" : "Categories";
            var routeSegment = siteAssetsExists ? "global-categories" : "categories";
            return contentRepository.GetOrCreateCategoriesRoot(SiteDefinition.Current.GlobalAssetsRoot, name, routeSegment);
        }

        public static ContentReference GetOrCreateSiteCategoriesRoot(this IContentRepository contentRepository)
        {
            var siteAssetsExists = SiteDefinition.Current.GlobalAssetsRoot != SiteDefinition.Current.SiteAssetsRoot;
            var name = siteAssetsExists ? "For This Site" : "Categories";
            var routeSegment = "categories";
            return contentRepository.GetOrCreateCategoriesRoot(SiteDefinition.Current.SiteAssetsRoot, name, routeSegment);
        }
        //Added for categories fix
        private static ContentReference GetOrCreateCategoriesRoot(this IContentRepository contentRepository, ContentReference parentLink, string name, string routeSegment)
        {
            var rootCategory = contentRepository.GetCategoryRoots(parentLink).FirstOrDefaultActiveCategoryRoot();
            if (rootCategory != null)
            {
                return rootCategory.ContentLink;
            }

            rootCategory = contentRepository.GetDefault<CategoryRoot>(parentLink);
            rootCategory.Name = name;
            rootCategory.RouteSegment = routeSegment;
            return contentRepository.Save(rootCategory, SaveAction.Publish, AccessLevel.NoAccess);
        }
        //Added for categories fix

        /// <remarks>Load the existing Category Roots.</remarks>
        public static IEnumerable<CategoryRoot> GetCategoryRoots(this IContentRepository contentRepository, ContentReference parentLink)
        {
            IEnumerable<CategoryRoot> categoryRoots;
            var loaderOptions = new LoaderOptions
            {
                LanguageLoaderOption.FallbackWithMaster()
            };

            var useAlternativeLogic = CategorySettings.Service.UseAlternativeCategoryRootLogic;
            if (useAlternativeLogic)
            {
                // GetDescendents returns all children (direct and non-direct) of the specified parent
                var descendents = contentRepository.GetDescendents(parentLink);
                categoryRoots = contentRepository.GetItems(descendents, loaderOptions).OfType<CategoryRoot>();
            }
            else
            {
                // GetChildren returns the direct children of the specified parent
                categoryRoots = contentRepository.GetChildren<CategoryRoot>(parentLink, loaderOptions);
            }

            return categoryRoots;
        }
        //Added for categories fix
        /// <remarks>Find the first active Category Root or fallback to the first Category Root (if any).</remarks>
        public static CategoryRoot FirstOrDefaultActiveCategoryRoot(this IEnumerable<CategoryRoot> categories)
        {
            var firstActive = (CategoryRoot)null;
            var first = (CategoryRoot)null;

            foreach (var categoryRoot in categories)
            {
                if (first == null)
                {
                    first = categoryRoot;
                }

                if (!categoryRoot.IsActive)
                {
                    continue;
                }

                firstActive = categoryRoot;
                break;
            }

            return firstActive ?? first;
        }
    }
}
