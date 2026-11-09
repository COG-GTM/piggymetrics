package com.piggymetrics.observability;

import org.junit.Test;
import org.slf4j.MDC;
import org.springframework.mock.web.MockHttpServletRequest;
import org.springframework.mock.web.MockHttpServletResponse;

import javax.servlet.FilterChain;
import javax.servlet.http.HttpServletRequest;
import java.util.Collections;
import java.util.UUID;
import java.util.concurrent.atomic.AtomicReference;

import static org.junit.Assert.assertEquals;
import static org.junit.Assert.assertNotEquals;
import static org.junit.Assert.assertNotNull;
import static org.junit.Assert.assertNull;
import static org.junit.Assert.assertTrue;

public class CorrelationIdFilterTest {

	private final CorrelationIdFilter filter = new CorrelationIdFilter();

	@Test
	public void shouldReuseIncomingCorrelationId() throws Exception {
		MockHttpServletRequest request = new MockHttpServletRequest("GET", "/");
		request.addHeader(CorrelationId.HEADER, "abc-123");
		MockHttpServletResponse response = new MockHttpServletResponse();
		AtomicReference<String> mdcDuringRequest = new AtomicReference<>();

		filter.doFilter(request, response, (req, res) -> mdcDuringRequest.set(MDC.get(CorrelationId.MDC_KEY)));

		assertEquals("abc-123", mdcDuringRequest.get());
		assertEquals("abc-123", response.getHeader(CorrelationId.HEADER));
	}

	@Test
	public void shouldCreateCorrelationIdWhenMissing() throws Exception {
		MockHttpServletRequest request = new MockHttpServletRequest("GET", "/");
		MockHttpServletResponse response = new MockHttpServletResponse();
		AtomicReference<String> mdcDuringRequest = new AtomicReference<>();

		filter.doFilter(request, response, (req, res) -> mdcDuringRequest.set(MDC.get(CorrelationId.MDC_KEY)));

		String generated = response.getHeader(CorrelationId.HEADER);
		assertNotNull(generated);
		UUID.fromString(generated);
		assertEquals(generated, mdcDuringRequest.get());
	}

	@Test
	public void shouldReplaceUnsafeCorrelationId() throws Exception {
		MockHttpServletRequest request = new MockHttpServletRequest("GET", "/");
		request.addHeader(CorrelationId.HEADER, "bad\nvalue");
		MockHttpServletResponse response = new MockHttpServletResponse();

		filter.doFilter(request, response, (req, res) -> { });

		String echoed = response.getHeader(CorrelationId.HEADER);
		assertNotEquals("bad\nvalue", echoed);
		UUID.fromString(echoed);
	}

	@Test
	public void shouldReplaceOverlongCorrelationId() throws Exception {
		String overlong = String.join("", Collections.nCopies(CorrelationId.MAX_LENGTH + 1, "a"));
		MockHttpServletRequest request = new MockHttpServletRequest("GET", "/");
		request.addHeader(CorrelationId.HEADER, overlong);
		MockHttpServletResponse response = new MockHttpServletResponse();

		filter.doFilter(request, response, (req, res) -> { });

		UUID.fromString(response.getHeader(CorrelationId.HEADER));
	}

	@Test
	public void shouldExposeResolvedCorrelationIdToDownstreamRequestHeaders() throws Exception {
		MockHttpServletRequest request = new MockHttpServletRequest("GET", "/");
		request.addHeader("Accept", "application/json");
		MockHttpServletResponse response = new MockHttpServletResponse();
		AtomicReference<HttpServletRequest> seen = new AtomicReference<>();

		filter.doFilter(request, response, (req, res) -> seen.set((HttpServletRequest) req));

		String generated = response.getHeader(CorrelationId.HEADER);
		assertEquals(generated, seen.get().getHeader(CorrelationId.HEADER));
		assertEquals(generated, seen.get().getHeaders(CorrelationId.HEADER).nextElement());
		assertTrue(Collections.list(seen.get().getHeaderNames()).contains(CorrelationId.HEADER));
		assertEquals("application/json", seen.get().getHeader("Accept"));
	}

	@Test
	public void shouldClearMdcAfterRequestEvenWhenChainFails() throws Exception {
		MockHttpServletRequest request = new MockHttpServletRequest("GET", "/");
		MockHttpServletResponse response = new MockHttpServletResponse();
		FilterChain failing = (req, res) -> {
			throw new IllegalStateException("boom");
		};

		try {
			filter.doFilter(request, response, failing);
		} catch (IllegalStateException expected) {
			// expected
		}

		assertNull(MDC.get(CorrelationId.MDC_KEY));
		assertNotNull(response.getHeader(CorrelationId.HEADER));
	}
}
