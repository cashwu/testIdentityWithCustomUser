﻿﻿/*
 * Copyright 2015 Dominick Baier, Brock Allen
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

 using System;
 using System.IdentityModel.Tokens;
 using System.Linq;
 using System.Threading;
 using Microsoft.IdentityModel.Tokens;
 using Microsoft.Owin.Extensions;
 using Microsoft.Owin.Logging;
 using Microsoft.Owin.Security.Jwt;
 using Microsoft.Owin.Security.OAuth;
 using Owin;
 using testMvc.IdentityServer3.AccessTokenValidation.Plumbing;
 using SecurityKey = System.IdentityModel.Tokens.SecurityKey;
 using SecurityToken = System.IdentityModel.Tokens.SecurityToken;

 namespace testMvc.IdentityServer3.AccessTokenValidation
{
    /// <summary>
    /// AppBuilder extensions for identity server token validation
    /// </summary>
    public static class IdentityServerBearerTokenValidationAppBuilderExtensions
    {
        /// <summary>
        /// Add identity server token authentication to the pipeline.
        /// </summary>
        /// <param name="app">The application.</param>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        public static IAppBuilder UseIdentityServerBearerTokenAuthentication(this IAppBuilder app, IdentityServerBearerTokenAuthenticationOptions options)
        {
            if (app == null) throw new ArgumentNullException("app");
            if (options == null) throw new ArgumentNullException("options");

            var loggerFactory = app.GetLoggerFactory();
            var middlewareOptions = new IdentityServerOAuthBearerAuthenticationOptions();

            switch (options.ValidationMode)
            {
                case ValidationMode.Local:
                    middlewareOptions.LocalValidationOptions = ConfigureLocalValidation(options, loggerFactory);
                    break;
                case ValidationMode.ValidationEndpoint:
                    middlewareOptions.EndpointValidationOptions = ConfigureEndpointValidation(options, loggerFactory);
                    break;
                case ValidationMode.Both:
                    middlewareOptions.LocalValidationOptions = ConfigureLocalValidation(options, loggerFactory);
                    middlewareOptions.EndpointValidationOptions = ConfigureEndpointValidation(options, loggerFactory);
                    break;
                default:
                    throw new Exception("ValidationMode has invalid value");
            }

            if (!options.DelayLoadMetadata)
            {
                // evaluate the lazy members so that they can do their job

                if (middlewareOptions.LocalValidationOptions != null)
                {
                    var ignore = middlewareOptions.LocalValidationOptions.Value;
                }

                if (middlewareOptions.EndpointValidationOptions != null)
                {
                    var ignore = middlewareOptions.EndpointValidationOptions.Value;
                }
            }

            if (options.TokenProvider != null)
            {
                middlewareOptions.TokenProvider = options.TokenProvider;
            }

            app.Use<IdentityServerBearerTokenValidationMiddleware>(app, middlewareOptions, loggerFactory);

            if (options.RequiredScopes.Any())
            {
                var scopeOptions = new ScopeRequirementOptions
                {
                    AuthenticationType = options.AuthenticationType,
                    RequiredScopes = options.RequiredScopes
                };

                app.Use<ScopeRequirementMiddleware>(scopeOptions);
            }

            if (options.PreserveAccessToken)
            {
                app.Use<PreserveAccessTokenMiddleware>();
            }

            app.UseStageMarker(PipelineStage.Authenticate);

            return app;
        }

        private static Lazy<OAuthBearerAuthenticationOptions> ConfigureEndpointValidation(IdentityServerBearerTokenAuthenticationOptions options, ILoggerFactory loggerFactory)
        {
            return new Lazy<OAuthBearerAuthenticationOptions>(() => 
            {
                if (options.EnableValidationResultCache)
                {
                    if (options.ValidationResultCache == null)
                    {
                        options.ValidationResultCache = new InMemoryValidationResultCache(options);
                    }
                }

                var bearerOptions = new OAuthBearerAuthenticationOptions
                {
                    AuthenticationMode = options.AuthenticationMode,
                    AuthenticationType = options.AuthenticationType,
                    Provider = new ContextTokenProvider(options.TokenProvider),
                };

                if (!string.IsNullOrEmpty(options.ClientId) || options.IntrospectionHttpHandler != null)
                {
                    bearerOptions.AccessTokenProvider = new IntrospectionEndpointTokenProvider(options, loggerFactory);
                }
                else
                {
                    bearerOptions.AccessTokenProvider = new ValidationEndpointTokenProvider(options, loggerFactory);
                }

                return bearerOptions;

            }, true);
        }

        internal static Lazy<OAuthBearerAuthenticationOptions> ConfigureLocalValidation(IdentityServerBearerTokenAuthenticationOptions options, ILoggerFactory loggerFactory)
        {
            return new Lazy<OAuthBearerAuthenticationOptions>(() => 
            {
                JwtFormat tokenFormat = null;

                // use static configuration
                if (!string.IsNullOrWhiteSpace(options.IssuerName) &&
                    options.SigningCertificate != null)
                {
                    var audience = options.IssuerName.EnsureTrailingSlash();
                    audience += "resources";

                    var valParams = new TokenValidationParameters
                    {
                        ValidIssuer = options.IssuerName,
                        ValidAudience = audience,
//                        IssuerSigningToken = new X509SecurityToken(options.SigningCertificate),

                        NameClaimType = options.NameClaimType,
                        RoleClaimType = options.RoleClaimType,
                    };

                    tokenFormat = new JwtFormat(valParams);
                }
                else
                {
                    // use discovery endpoint
                    if (string.IsNullOrWhiteSpace(options.Authority))
                    {
                        throw new Exception("Either set IssuerName and SigningCertificate - or Authority");
                    }

                    var discoveryEndpoint = options.Authority.EnsureTrailingSlash();
                    discoveryEndpoint += ".well-known/openid-configuration";

                    var issuerProvider = new DiscoveryDocumentIssuerSecurityTokenProvider(
                        discoveryEndpoint,
                        options,
                        loggerFactory);

                    var valParams = new TokenValidationParameters
                    {
                        ValidAudience = issuerProvider.Audience,
                        NameClaimType = options.NameClaimType,
                        RoleClaimType = options.RoleClaimType
                    };

                    if (options.IssuerSigningKeyResolver != null)
                    {
                        valParams.IssuerSigningKeyResolver = options.IssuerSigningKeyResolver;
                    }
                    else
                    {
//                        valParams.IssuerSigningKeyResolver = ResolveRsaKeys;
                    }

//                    tokenFormat = new JwtFormat(valParams, issuerProvider);
                }


                var bearerOptions = new OAuthBearerAuthenticationOptions
                {
                    AccessTokenFormat = tokenFormat,
                    AuthenticationMode = options.AuthenticationMode,
                    AuthenticationType = options.AuthenticationType,
                    Provider = new ContextTokenProvider(options.TokenProvider)
                };

                return bearerOptions;

            }, LazyThreadSafetyMode.PublicationOnly);
        }

//        private static SecurityKey ResolveRsaKeys(
//            string token, 
//            SecurityToken securityToken, 
//            SecurityKeyIdentifier keyIdentifier, 
//            TokenValidationParameters validationParameters)
//        {
//            string id = null;
//            foreach (var keyId in keyIdentifier)
//            {
//                var nk = keyId as NamedKeySecurityKeyIdentifierClause;
//                if (nk != null)
//                {
//                    id = nk.Id;
//                    break;
//                }
//            }
//
//            if (id == null) return null;
//
//            var issuerToken = validationParameters.IssuerSigningTokens.FirstOrDefault(it => it.Id == id);
//            if (issuerToken == null) return null;
//
//            return issuerToken.SecurityKeys.FirstOrDefault();
//        }
    }
}