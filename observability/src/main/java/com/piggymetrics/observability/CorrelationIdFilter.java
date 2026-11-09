package com.piggymetrics.observability;

import org.slf4j.MDC;
import org.springframework.web.filter.OncePerRequestFilter;

import javax.servlet.FilterChain;
import javax.servlet.ServletException;
import javax.servlet.http.HttpServletRequest;
import javax.servlet.http.HttpServletRequestWrapper;
import javax.servlet.http.HttpServletResponse;
import java.io.IOException;
import java.util.Collections;
import java.util.Enumeration;
import java.util.LinkedHashSet;
import java.util.Set;

/**
 * Reads or creates the {@value CorrelationId#HEADER} header, stores it in the MDC for the
 * duration of the request and echoes it on the response.
 */
public class CorrelationIdFilter extends OncePerRequestFilter {

	@Override
	protected void doFilterInternal(HttpServletRequest request, HttpServletResponse response, FilterChain chain)
			throws ServletException, IOException {
		String correlationId = CorrelationId.resolve(request.getHeader(CorrelationId.HEADER));
		MDC.put(CorrelationId.MDC_KEY, correlationId);
		response.setHeader(CorrelationId.HEADER, correlationId);
		try {
			chain.doFilter(new CorrelationIdRequest(request, correlationId), response);
		} finally {
			MDC.remove(CorrelationId.MDC_KEY);
		}
	}

	/**
	 * Exposes the resolved correlation ID as the request header, so code that copies
	 * incoming headers onto outgoing calls (e.g. a proxying gateway) forwards it.
	 */
	static class CorrelationIdRequest extends HttpServletRequestWrapper {

		private final String correlationId;

		CorrelationIdRequest(HttpServletRequest request, String correlationId) {
			super(request);
			this.correlationId = correlationId;
		}

		@Override
		public String getHeader(String name) {
			if (CorrelationId.HEADER.equalsIgnoreCase(name)) {
				return correlationId;
			}
			return super.getHeader(name);
		}

		@Override
		public Enumeration<String> getHeaders(String name) {
			if (CorrelationId.HEADER.equalsIgnoreCase(name)) {
				return Collections.enumeration(Collections.singletonList(correlationId));
			}
			return super.getHeaders(name);
		}

		@Override
		public Enumeration<String> getHeaderNames() {
			Set<String> names = new LinkedHashSet<>();
			Enumeration<String> original = super.getHeaderNames();
			if (original != null) {
				while (original.hasMoreElements()) {
					String name = original.nextElement();
					if (!CorrelationId.HEADER.equalsIgnoreCase(name)) {
						names.add(name);
					}
				}
			}
			names.add(CorrelationId.HEADER);
			return Collections.enumeration(names);
		}
	}
}
