package com.piggymetrics.observability;

import java.util.UUID;
import java.util.regex.Pattern;

/**
 * Correlation ID conventions shared by the servlet and reactive filters.
 */
public final class CorrelationId {

	public static final String HEADER = "X-Correlation-Id";

	public static final String MDC_KEY = "correlationId";

	static final int MAX_LENGTH = 128;

	private static final Pattern ALLOWED = Pattern.compile("[A-Za-z0-9._:-]+");

	private CorrelationId() {
	}

	/**
	 * Returns the incoming value when it is a safe identifier, otherwise a new random one.
	 * Unsafe values are replaced so that untrusted input cannot inject content into logs or headers.
	 */
	public static String resolve(String incoming) {
		if (isValid(incoming)) {
			return incoming;
		}
		return UUID.randomUUID().toString();
	}

	static boolean isValid(String value) {
		return value != null
				&& !value.isEmpty()
				&& value.length() <= MAX_LENGTH
				&& ALLOWED.matcher(value).matches();
	}
}
