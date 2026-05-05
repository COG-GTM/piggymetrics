package com.piggymetrics.auth.config;

import java.util.HashMap;
import java.util.HashSet;
import java.util.Map;
import java.util.Set;

import jakarta.servlet.http.HttpServletRequest;

import org.springframework.lang.Nullable;
import org.springframework.security.core.Authentication;
import org.springframework.security.core.context.SecurityContextHolder;
import org.springframework.security.oauth2.core.endpoint.OAuth2ParameterNames;
import org.springframework.security.web.authentication.AuthenticationConverter;
import org.springframework.util.StringUtils;

public class ResourceOwnerPasswordAuthenticationConverter implements AuthenticationConverter {

    @Nullable
    @Override
    public Authentication convert(HttpServletRequest request) {
        String grantType = request.getParameter(OAuth2ParameterNames.GRANT_TYPE);
        if (!"password".equals(grantType)) {
            return null;
        }

        String username = request.getParameter(OAuth2ParameterNames.USERNAME);
        if (!StringUtils.hasText(username)) {
            return null;
        }

        String password = request.getParameter(OAuth2ParameterNames.PASSWORD);
        if (!StringUtils.hasText(password)) {
            return null;
        }

        String scope = request.getParameter(OAuth2ParameterNames.SCOPE);
        Set<String> requestedScopes = null;
        if (StringUtils.hasText(scope)) {
            requestedScopes = new HashSet<>(Set.of(scope.split(" ")));
        }

        Authentication clientPrincipal = SecurityContextHolder.getContext().getAuthentication();

        Map<String, Object> additionalParameters = new HashMap<>();
        request.getParameterMap().forEach((key, values) -> {
            if (!key.equals(OAuth2ParameterNames.GRANT_TYPE) &&
                    !key.equals(OAuth2ParameterNames.USERNAME) &&
                    !key.equals(OAuth2ParameterNames.PASSWORD) &&
                    !key.equals(OAuth2ParameterNames.SCOPE) &&
                    !key.equals(OAuth2ParameterNames.CLIENT_ID) &&
                    !key.equals(OAuth2ParameterNames.CLIENT_SECRET)) {
                additionalParameters.put(key, values[0]);
            }
        });

        return new ResourceOwnerPasswordAuthenticationToken(
                username, password, clientPrincipal, requestedScopes, additionalParameters);
    }
}
