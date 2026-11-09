package com.piggymetrics.observability;

import org.junit.Test;
import org.slf4j.MDC;
import org.springframework.mock.http.server.reactive.MockServerHttpRequest;
import org.springframework.mock.web.server.MockServerWebExchange;
import org.springframework.web.server.ServerWebExchange;
import reactor.core.publisher.Mono;

import java.util.UUID;
import java.util.concurrent.atomic.AtomicReference;

import static org.junit.Assert.assertEquals;
import static org.junit.Assert.assertNull;

public class CorrelationIdWebFilterTest {

	private final CorrelationIdWebFilter filter = new CorrelationIdWebFilter();

	@Test
	public void shouldReuseIncomingCorrelationId() {
		MockServerWebExchange exchange = MockServerWebExchange.from(
				MockServerHttpRequest.get("/").header(CorrelationId.HEADER, "abc-123").build());
		AtomicReference<ServerWebExchange> seen = new AtomicReference<>();
		AtomicReference<String> mdc = new AtomicReference<>();
		AtomicReference<String> context = new AtomicReference<>();

		filter.filter(exchange, ex -> {
			seen.set(ex);
			mdc.set(MDC.get(CorrelationId.MDC_KEY));
			return Mono.subscriberContext()
					.doOnNext(ctx -> context.set(ctx.get(CorrelationId.MDC_KEY)))
					.then();
		}).block();

		assertEquals("abc-123", exchange.getResponse().getHeaders().getFirst(CorrelationId.HEADER));
		assertEquals("abc-123", seen.get().getRequest().getHeaders().getFirst(CorrelationId.HEADER));
		assertEquals("abc-123", seen.get().getAttribute(CorrelationIdWebFilter.ATTRIBUTE));
		assertEquals("abc-123", mdc.get());
		assertEquals("abc-123", context.get());
		assertNull(MDC.get(CorrelationId.MDC_KEY));
	}

	@Test
	public void shouldCreateCorrelationIdWhenMissing() {
		MockServerWebExchange exchange = MockServerWebExchange.from(MockServerHttpRequest.get("/").build());
		AtomicReference<ServerWebExchange> seen = new AtomicReference<>();

		filter.filter(exchange, ex -> {
			seen.set(ex);
			return Mono.empty();
		}).block();

		String generated = exchange.getResponse().getHeaders().getFirst(CorrelationId.HEADER);
		UUID.fromString(generated);
		assertEquals(generated, seen.get().getRequest().getHeaders().getFirst(CorrelationId.HEADER));
	}

	@Test
	public void shouldReplaceUnsafeCorrelationId() {
		MockServerWebExchange exchange = MockServerWebExchange.from(
				MockServerHttpRequest.get("/").header(CorrelationId.HEADER, "<script>").build());

		filter.filter(exchange, ex -> Mono.empty()).block();

		UUID.fromString(exchange.getResponse().getHeaders().getFirst(CorrelationId.HEADER));
	}
}
