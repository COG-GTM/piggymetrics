package com.piggymetrics.observability;

import org.slf4j.MDC;
import org.springframework.http.server.reactive.ServerHttpRequest;
import org.springframework.web.server.ServerWebExchange;
import org.springframework.web.server.WebFilter;
import org.springframework.web.server.WebFilterChain;
import reactor.core.publisher.Mono;

/**
 * Reactive equivalent of {@link CorrelationIdFilter}. The ID is echoed on the response,
 * forwarded on the mutated request, stored as an exchange attribute and in the Reactor
 * context, and placed in the MDC while the filter chain is assembled.
 */
public class CorrelationIdWebFilter implements WebFilter {

	public static final String ATTRIBUTE = CorrelationIdWebFilter.class.getName() + ".correlationId";

	@Override
	public Mono<Void> filter(ServerWebExchange exchange, WebFilterChain chain) {
		String correlationId = CorrelationId.resolve(
				exchange.getRequest().getHeaders().getFirst(CorrelationId.HEADER));

		exchange.getResponse().getHeaders().set(CorrelationId.HEADER, correlationId);
		ServerHttpRequest request = exchange.getRequest().mutate()
				.header(CorrelationId.HEADER, correlationId)
				.build();
		ServerWebExchange mutated = exchange.mutate().request(request).build();
		mutated.getAttributes().put(ATTRIBUTE, correlationId);

		MDC.put(CorrelationId.MDC_KEY, correlationId);
		try {
			return chain.filter(mutated)
					.subscriberContext(context -> context.put(CorrelationId.MDC_KEY, correlationId));
		} finally {
			MDC.remove(CorrelationId.MDC_KEY);
		}
	}
}
