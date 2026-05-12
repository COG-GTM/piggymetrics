package com.piggymetrics.account;

import com.piggymetrics.account.config.TestSecurityConfig;
import org.junit.jupiter.api.Test;
import org.springframework.boot.test.context.SpringBootTest;
import org.springframework.context.annotation.Import;

@SpringBootTest
@Import(TestSecurityConfig.class)
public class AccountServiceApplicationTests {

	@Test
	public void contextLoads() {

	}

}
