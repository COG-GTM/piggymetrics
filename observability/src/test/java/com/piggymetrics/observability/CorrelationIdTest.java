package com.piggymetrics.observability;

import org.junit.Test;

import static org.junit.Assert.assertEquals;
import static org.junit.Assert.assertFalse;
import static org.junit.Assert.assertTrue;

public class CorrelationIdTest {

	@Test
	public void shouldAcceptSafeIdentifiers() {
		assertTrue(CorrelationId.isValid("3f2a1c9e-1b2c-4d5e-8f90-123456789abc"));
		assertTrue(CorrelationId.isValid("req_01.service:42"));
		assertEquals("abc-123", CorrelationId.resolve("abc-123"));
	}

	@Test
	public void shouldRejectMissingOrUnsafeIdentifiers() {
		assertFalse(CorrelationId.isValid(null));
		assertFalse(CorrelationId.isValid(""));
		assertFalse(CorrelationId.isValid("has space"));
		assertFalse(CorrelationId.isValid("line\r\nbreak"));
		assertFalse(CorrelationId.isValid("{\"json\":true}"));
	}
}
