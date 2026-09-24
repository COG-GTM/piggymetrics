package com.piggymetrics.auth.config;

import org.junit.jupiter.api.Test;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.test.autoconfigure.web.servlet.AutoConfigureMockMvc;
import org.springframework.boot.test.context.SpringBootTest;
import org.springframework.test.web.servlet.MockMvc;

import static org.springframework.security.test.web.servlet.request.SecurityMockMvcRequestPostProcessors.httpBasic;
import static org.springframework.test.web.servlet.request.MockMvcRequestBuilders.get;
import static org.springframework.test.web.servlet.request.MockMvcRequestBuilders.post;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.jsonPath;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.status;

@SpringBootTest
@AutoConfigureMockMvc
public class AuthorizationServerIntegrationTest {

	@Autowired
	private MockMvc mockMvc;

	@Test
	public void shouldIssueAccessTokenForServiceClient() throws Exception {
		mockMvc.perform(post("/oauth2/token")
						.param("grant_type", "client_credentials")
						.param("scope", "server")
						.with(httpBasic("account-service", "test-account-password")))
				.andExpect(status().isOk())
				.andExpect(jsonPath("$.access_token").exists())
				.andExpect(jsonPath("$.token_type").value("Bearer"));
	}

	@Test
	public void shouldExposeJwkSet() throws Exception {
		mockMvc.perform(get("/oauth2/jwks"))
				.andExpect(status().isOk())
				.andExpect(jsonPath("$.keys[0].kty").value("RSA"));
	}

	@Test
	public void shouldRejectUnauthenticatedTokenRequest() throws Exception {
		mockMvc.perform(post("/oauth2/token").param("grant_type", "client_credentials"))
				.andExpect(status().isUnauthorized());
	}
}
