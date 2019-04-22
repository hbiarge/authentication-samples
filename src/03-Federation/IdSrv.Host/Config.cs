using IdentityServer4.Models;
using System.Collections.Generic;
using IdentityServer4;

namespace IdSrv.Host
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new IdentityResource[]
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResources.Email(),
                new IdentityResources.Address(),
                new IdentityResources.Phone()
            };
        }

        public static IEnumerable<ApiResource> GetApis()
        {
            return new ApiResource[]
            {
                new ApiResource("api1", "Api with IdSrv Bearer Token")
                {
                    ApiSecrets = new []
                    {
                        new Secret("secret".Sha256())
                    }
                }
            };
        }

        public static IEnumerable<Client> GetClients()
        {
            return new[]
            {
                // WPF public client with PKCE and SystemBrowser
                new Client
                {
                    ClientId = "public.hybrid.pkce",
                    ClientName = "WPF public client with PKCE",
                    RequireClientSecret = false,
                    
                    AllowedGrantTypes = GrantTypes.Hybrid,
                    RequirePkce = true,

                    RedirectUris =
                    {
                        "http://127.0.0.1"
                    },

                    AllowOfflineAccess = true,

                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "api1"
                    },

                    IdentityProviderRestrictions =
                    {
                        "aad"
                    }
                },

                // WPF public client with PKCE and WpfEmbeddedBrowser
                new Client
                {
                    ClientId = "public.code.pkce",
                    ClientName = "WPF public client with PKCE",
                    RequireClientSecret = false,

                    AllowedGrantTypes = GrantTypes.Code,
                    RequirePkce = true,

                    RedirectUris =
                    {
                        "http://127.0.0.1/sample-wpf-app"
                    },

                    AllowOfflineAccess = true,

                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "api1"
                    },

                    IdentityProviderRestrictions =
                    {
                        "aad"
                    }
                },

                // UWP public client with PKCE
                new Client
                {
                    ClientId = "public.uwp.hybrid.pkce",
                    ClientName = "UWP public client with PKCE",
                    RequireClientSecret = false,

                    AllowedGrantTypes = GrantTypes.Hybrid,
                    RequirePkce = true,

                    RedirectUris = { "http://uwpidsrv" },

                    AllowOfflineAccess = true,

                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "api1"
                    },

                    IdentityProviderRestrictions =
                    {
                        "aad"
                    }
                },

                // MVC client using hybrid flow
                new Client
                {
                    ClientId = "mvc",
                    ClientName = "MVC Client",
                    RequireConsent = true,

                    AllowedGrantTypes = GrantTypes.Hybrid,
                    
                    ClientSecrets =
                    {
                        new Secret("373f4671-0c18-48d6-9da3-962b1c81299a".Sha256())
                    },

                    RedirectUris =
                    {
                        "https://localhost:5011/signin-oidc"
                    },
                    PostLogoutRedirectUris =
                    {
                        "https://localhost:5011/signout-callback-oidc"
                    },
                    FrontChannelLogoutUri = "https://localhost:5011/signout-oidc",
                    
                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "api1"
                    },
                    AllowOfflineAccess = true
                },

                // SPA client using implicit flow
                new Client
                {
                    ClientId = "spa",
                    ClientName = "SPA Client",
                    ClientUri = "http://identityserver.io",

                    AllowedGrantTypes = GrantTypes.Implicit,
                    AllowAccessTokensViaBrowser = true,

                    RedirectUris =
                    {
                        "http://localhost:5002/index.html",
                        "http://localhost:5002/callback.html",
                        "http://localhost:5002/silent.html",
                        "http://localhost:5002/popup.html",
                    },

                    PostLogoutRedirectUris = { "http://localhost:5002/index.html" },
                    AllowedCorsOrigins = { "http://localhost:5002" },

                    AllowedScopes = { "openid", "profile", "api1" }
                }
            };
        }
    }
}