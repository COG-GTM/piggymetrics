package com.piggymetrics.auth.service.security;

import com.piggymetrics.auth.domain.User;
import com.piggymetrics.auth.repository.UserRepository;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.extension.ExtendWith;
import org.mockito.InjectMocks;
import org.mockito.Mock;
import org.mockito.junit.jupiter.MockitoExtension;
import org.springframework.security.core.userdetails.UserDetails;
import org.springframework.security.core.userdetails.UsernameNotFoundException;

import java.util.Optional;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertThrows;
import static org.mockito.ArgumentMatchers.any;
import static org.mockito.Mockito.when;

@ExtendWith(MockitoExtension.class)
class MongoUserDetailsServiceTest {

	@InjectMocks
	private MongoUserDetailsService service;

	@Mock
	private UserRepository repository;

	@Test
	void shouldLoadByUsernameWhenUserExists() {

		final User user = new User();

		when(repository.findById(any())).thenReturn(Optional.of(user));
		UserDetails loaded = service.loadUserByUsername("name");

		assertEquals(user, loaded);
	}

	@Test
	void shouldFailToLoadByUsernameWhenUserNotExists() {
		when(repository.findById(any())).thenReturn(Optional.empty());
		assertThrows(UsernameNotFoundException.class, () -> service.loadUserByUsername("name"));
	}
}
