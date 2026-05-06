package com.piggymetrics.account;

import org.junit.jupiter.api.Test;
import org.springframework.boot.test.context.SpringBootTest;
import org.springframework.boot.test.mock.mockito.MockBean;
import org.springframework.security.oauth2.jwt.JwtDecoder;

@SpringBootTest
public class AccountServiceApplicationTests {

	@MockBean
	private JwtDecoder jwtDecoder;

	@Test
	public void contextLoads() {

	}

}
