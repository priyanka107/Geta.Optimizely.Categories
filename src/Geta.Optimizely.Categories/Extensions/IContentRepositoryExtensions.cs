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

        private static ContentReference GetOrCreateCategoriesRoot(this IContentRepository contentRepository, ContentReference parentLink, string name, string routeSegment)
        {
            var loaderOptions = new LoaderOptions
            {
                LanguageLoaderOption.FallbackWithMaster()
            };

            var rootCategory = contentRepository.GetChildren<CategoryRoot>(parentLink, loaderOptions).FirstOrDefault();

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
        public static IEnumerable<CategoryRoot> GetCategoryRoots(this IContentRepository contentRepository, ContentReference parentLink)
        {
            //IL_0000: Unknown result type (might be due to invalid IL or missing references)
            //IL_0005: Unknown result type (might be due to invalid IL or missing references)
            //IL_0011: Expected O, but got Unknown
            //IL_0013: Expected O, but got Unknown
            var val = new LoaderOptions();
            ((OptionsBase<LoaderOptions, LoaderOption>)val).Add<LanguageLoaderOption>(LanguageLoaderOption.FallbackWithMaster((CultureInfo)null));
            var val2 = val;
            if (CategorySettings.Service.UseAlternativeCategoryRootLogic)
            {
                var descendents = ((IContentLoader)contentRepository).GetDescendents(parentLink);
                return ((IContentLoader)contentRepository).GetItems(descendents, val2).OfType<CategoryRoot>();
            }
            return ((IContentLoader)contentRepository).GetChildren<CategoryRoot>(parentLink, val2);
        }
        //Added for categories fix
        public static CategoryRoot FirstOrDefaultActiveCategoryRoot(this IEnumerable<CategoryRoot> categories)
        {
            CategoryRoot categoryRoot = null;
            CategoryRoot categoryRoot2 = null;
            foreach (var category in categories)
            {
                categoryRoot2 ??= category;
                if (category.IsActive)
                {
                    categoryRoot = category;
                    break;
                }
            }
            return categoryRoot ?? categoryRoot2;
        }
    }
}
